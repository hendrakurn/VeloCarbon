-- VeloCarbon: PostgreSQL 15+; jalankan pada database kosong.
-- Rancangan fisik, bukan integrasi EF Core. Batas validasi: docs/database/erd.md.
-- UUID dibuat PostgreSQL atau Guid dari aplikasi. Semua FK memakai ON UPDATE NO ACTION.
BEGIN;

-- UserProfile: Profil individu; tidak menyimpan akun daring atau kata sandi.
CREATE TABLE user_profile (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL CHECK (btrim(name) <> ''),
    unit_preference VARCHAR(10) NOT NULL DEFAULT 'Metric' CHECK (unit_preference IN ('Metric')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Vehicle: Kendaraan pribadi milik satu profil.
CREATE TABLE vehicle (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_profile_id UUID NOT NULL REFERENCES user_profile(id) ON DELETE RESTRICT,
    name VARCHAR(100) NOT NULL CHECK (btrim(name) <> ''),
    vehicle_type VARCHAR(16) NOT NULL CHECK (vehicle_type IN ('Motorcycle', 'Car')),
    fuel_type VARCHAR(10) NOT NULL CHECK (fuel_type IN ('Petrol', 'Diesel'))
);

-- VehicleMonthlyInput: Pilihan metode mobilitas untuk satu kendaraan pada satu bulan.
CREATE TABLE vehicle_monthly_input (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    vehicle_id UUID NOT NULL REFERENCES vehicle(id) ON DELETE CASCADE,
    month DATE NOT NULL CHECK (isfinite(month) AND EXTRACT(DAY FROM month) = 1),
    method VARCHAR(16) NOT NULL CHECK (method IN ('Distance', 'FuelPurchase')),
    UNIQUE (vehicle_id, month)
);

-- ActivityRecord: Atribut bersama semua aktivitas, dengan discriminator activity_type.
CREATE TABLE activity_record (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_profile_id UUID NOT NULL REFERENCES user_profile(id) ON DELETE RESTRICT,
    name VARCHAR(100) NOT NULL CHECK (btrim(name) <> ''),
    activity_type VARCHAR(16) NOT NULL CHECK (activity_type IN ('Mobility', 'FuelPurchase', 'Electricity')),
    unit VARCHAR(10) NOT NULL CHECK (unit IN ('Kilometer', 'Liter', 'KWh')),
    activity_date DATE NOT NULL CHECK (isfinite(activity_date)),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CHECK ((activity_type = 'Mobility' AND unit = 'Kilometer') OR (activity_type = 'FuelPurchase' AND unit = 'Liter') OR (activity_type = 'Electricity' AND unit = 'KWh')),
    CHECK (activity_type <> 'Electricity' OR EXTRACT(DAY FROM activity_date) = 1)
);

-- MobilityActivity: Detail perjalanan berdasarkan jarak dan frekuensi.
CREATE TABLE mobility_activity (
    id UUID PRIMARY KEY REFERENCES activity_record(id) ON DELETE CASCADE,
    vehicle_id UUID REFERENCES vehicle(id) ON DELETE RESTRICT,
    transport_mode VARCHAR(16) NOT NULL CHECK (transport_mode IN ('Motorcycle', 'Car', 'Bus')),
    distance_km NUMERIC(12,3) NOT NULL CHECK (distance_km <> 'NaN'::numeric AND distance_km > 0),
    frequency INTEGER NOT NULL CHECK (frequency BETWEEN 1 AND 10000),
    CHECK ((transport_mode = 'Bus' AND vehicle_id IS NULL) OR (transport_mode IN ('Motorcycle', 'Car') AND vehicle_id IS NOT NULL))
);

-- FuelPurchaseActivity: Detail pembelian bahan bakar; estimasi berdasarkan liter yang dibeli.
CREATE TABLE fuel_purchase_activity (
    id UUID PRIMARY KEY REFERENCES activity_record(id) ON DELETE CASCADE,
    vehicle_id UUID NOT NULL REFERENCES vehicle(id) ON DELETE RESTRICT,
    fuel_type VARCHAR(10) NOT NULL CHECK (fuel_type IN ('Petrol', 'Diesel')),
    liters_purchased NUMERIC(12,3) NOT NULL CHECK (liters_purchased <> 'NaN'::numeric AND liters_purchased > 0),
    total_cost NUMERIC(14,2) CHECK (total_cost <> 'NaN'::numeric AND total_cost >= 0)
);

-- ElectricityUsage: Satu bulan listrik; tanggal awal disimpan di activity_record.activity_date.
CREATE TABLE electricity_usage (
    id UUID PRIMARY KEY REFERENCES activity_record(id) ON DELETE CASCADE,
    input_method VARCHAR(16) NOT NULL CHECK (input_method IN ('Meter', 'Appliances')),
    usage_kwh NUMERIC(12,3) CHECK (usage_kwh <> 'NaN'::numeric AND usage_kwh >= 0),
    CHECK ((input_method = 'Meter' AND usage_kwh IS NOT NULL) OR (input_method = 'Appliances' AND usage_kwh IS NULL))
);

-- ApplianceUsage: Detail alat untuk estimasi listrik.
CREATE TABLE appliance_usage (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    electricity_usage_id UUID NOT NULL REFERENCES electricity_usage(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL CHECK (btrim(name) <> ''),
    power_watt NUMERIC(9,3) NOT NULL CHECK (power_watt <> 'NaN'::numeric AND power_watt > 0),
    quantity SMALLINT NOT NULL CHECK (quantity BETWEEN 1 AND 1000),
    hours_per_day NUMERIC(4,2) NOT NULL CHECK (hours_per_day <> 'NaN'::numeric AND hours_per_day > 0 AND hours_per_day <= 24),
    days_used SMALLINT NOT NULL CHECK (days_used BETWEEN 1 AND 31),
    UNIQUE (id, electricity_usage_id)
);

-- EmissionFactor: Faktor emisi berversi beserta sumber dan masa berlakunya.
CREATE TABLE emission_factor (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category VARCHAR(16) NOT NULL CHECK (category IN ('Mobility', 'Electricity')),
    factor_key VARCHAR(16) NOT NULL CHECK (factor_key IN ('Motorcycle', 'Car', 'Bus', 'Petrol', 'Diesel', 'Electricity')),
    value NUMERIC(18,9) NOT NULL CHECK (value <> 'NaN'::numeric AND value >= 0),
    unit VARCHAR(10) NOT NULL CHECK (unit IN ('Kilometer', 'Liter', 'KWh')),
    source TEXT NOT NULL CHECK (btrim(source) <> ''),
    version VARCHAR(30) NOT NULL CHECK (btrim(version) <> ''),
    valid_from DATE NOT NULL CHECK (isfinite(valid_from)),
    valid_until DATE CHECK (isfinite(valid_until)),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    UNIQUE (factor_key, unit, version),
    CHECK (valid_until IS NULL OR valid_until >= valid_from),
    CHECK ((category = 'Mobility' AND factor_key IN ('Motorcycle', 'Car', 'Bus') AND unit = 'Kilometer') OR (category = 'Mobility' AND factor_key IN ('Petrol', 'Diesel') AND unit = 'Liter') OR (category = 'Electricity' AND factor_key = 'Electricity' AND unit = 'KWh'))
);

-- EmissionCalculation: Satu hasil aktif per aktivitas beserta faktor yang digunakan.
CREATE TABLE emission_calculation (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    activity_record_id UUID NOT NULL UNIQUE REFERENCES activity_record(id) ON DELETE CASCADE,
    emission_factor_id UUID NOT NULL REFERENCES emission_factor(id) ON DELETE RESTRICT,
    activity_value NUMERIC(24,9) NOT NULL CHECK (activity_value <> 'NaN'::numeric AND activity_value >= 0),
    result_kg_co2e NUMERIC(24,6) NOT NULL CHECK (result_kg_co2e <> 'NaN'::numeric AND result_kg_co2e >= 0),
    calculated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Scenario: Rancangan pengurangan aktivitas untuk satu bulan baseline.
CREATE TABLE scenario (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_profile_id UUID NOT NULL REFERENCES user_profile(id) ON DELETE RESTRICT,
    name VARCHAR(100) NOT NULL CHECK (btrim(name) <> ''),
    baseline_start DATE NOT NULL CHECK (isfinite(baseline_start) AND EXTRACT(DAY FROM baseline_start) = 1),
    baseline_end DATE NOT NULL CHECK (isfinite(baseline_end)),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CHECK (baseline_end = (baseline_start + INTERVAL '1 month' - INTERVAL '1 day')::date)
);

-- ScenarioChange: Perubahan persentase yang menarget satu aktivitas atau rincian alat.
CREATE TABLE scenario_change (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    scenario_id UUID NOT NULL REFERENCES scenario(id) ON DELETE CASCADE,
    change_type VARCHAR(32) NOT NULL CHECK (change_type IN ('ReduceDistancePercent', 'ReduceFrequencyPercent', 'ReduceFuelPercent', 'ReduceElectricityPercent', 'ReduceApplianceHoursPercent')),
    target_activity_record_id UUID NOT NULL REFERENCES activity_record(id) ON DELETE RESTRICT,
    target_appliance_usage_id UUID,
    change_value NUMERIC(5,2) NOT NULL CHECK (change_value <> 'NaN'::numeric AND change_value >= 0 AND change_value <= 100),
    FOREIGN KEY (target_appliance_usage_id, target_activity_record_id) REFERENCES appliance_usage(id, electricity_usage_id) ON DELETE RESTRICT,
    CHECK ((change_type = 'ReduceApplianceHoursPercent' AND target_appliance_usage_id IS NOT NULL) OR (change_type <> 'ReduceApplianceHoursPercent' AND target_appliance_usage_id IS NULL))
);

-- ScenarioResult: Hasil terbaru satu skenario; hasil dihapus/dihitung ulang jika baseline berubah.
CREATE TABLE scenario_result (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    scenario_id UUID NOT NULL UNIQUE REFERENCES scenario(id) ON DELETE CASCADE,
    baseline_kg_co2e NUMERIC(24,6) NOT NULL CHECK (baseline_kg_co2e <> 'NaN'::numeric AND baseline_kg_co2e >= 0),
    scenario_kg_co2e NUMERIC(24,6) NOT NULL CHECK (scenario_kg_co2e <> 'NaN'::numeric AND scenario_kg_co2e >= 0),
    reduction_kg_co2e NUMERIC(24,6) GENERATED ALWAYS AS (baseline_kg_co2e - scenario_kg_co2e) STORED NOT NULL,
    calculated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CHECK (scenario_kg_co2e <= baseline_kg_co2e)
);

-- Recommendation: Saran dari hasil skenario, diurutkan berdasarkan estimasi dampak.
CREATE TABLE recommendation (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    scenario_result_id UUID NOT NULL REFERENCES scenario_result(id) ON DELETE CASCADE,
    title VARCHAR(150) NOT NULL CHECK (btrim(title) <> ''),
    description TEXT NOT NULL CHECK (btrim(description) <> ''),
    target_category VARCHAR(16) NOT NULL CHECK (target_category IN ('Mobility', 'Electricity')),
    recommended_action TEXT NOT NULL CHECK (btrim(recommended_action) <> ''),
    estimated_reduction_kg_co2e NUMERIC(24,6) NOT NULL CHECK (estimated_reduction_kg_co2e <> 'NaN'::numeric AND estimated_reduction_kg_co2e > 0),
    priority INTEGER NOT NULL CHECK (priority >= 1),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (scenario_result_id, priority)
);

-- Satu catatan listrik per profil/bulan; activity_date wajib hari pertama.
CREATE UNIQUE INDEX uq_electricity_profile_month
    ON activity_record(user_profile_id, activity_date)
    WHERE activity_type = 'Electricity';

CREATE INDEX ix_activity_profile_date ON activity_record(user_profile_id, activity_date);
CREATE INDEX ix_vehicle_profile ON vehicle(user_profile_id);
CREATE INDEX ix_mobility_vehicle ON mobility_activity(vehicle_id);
CREATE INDEX ix_fuel_vehicle ON fuel_purchase_activity(vehicle_id);
CREATE INDEX ix_appliance_electricity ON appliance_usage(electricity_usage_id);
CREATE INDEX ix_calculation_factor ON emission_calculation(emission_factor_id);
CREATE INDEX ix_factor_selection ON emission_factor(factor_key, unit, valid_from) WHERE is_active;
CREATE INDEX ix_scenario_profile_month ON scenario(user_profile_id, baseline_start);
CREATE INDEX ix_change_scenario ON scenario_change(scenario_id);
CREATE INDEX ix_change_activity ON scenario_change(target_activity_record_id);
CREATE INDEX ix_change_appliance ON scenario_change(target_appliance_usage_id);
-- Mencegah dua perubahan total atau dua perubahan pada alat yang sama.
-- Campuran perubahan total + alat masih divalidasi service di dalam transaksi.
CREATE UNIQUE INDEX uq_change_whole_activity
    ON scenario_change(scenario_id, target_activity_record_id)
    WHERE target_appliance_usage_id IS NULL;
CREATE UNIQUE INDEX uq_change_appliance
    ON scenario_change(scenario_id, target_appliance_usage_id)
    WHERE target_appliance_usage_id IS NOT NULL;

-- Total/disjoint specialization: setiap induk harus punya tepat satu detail sesuai tipe.
-- Ditunda sampai COMMIT agar insert induk lalu detail dapat dilakukan dalam satu transaksi.
CREATE FUNCTION validate_activity_detail() RETURNS trigger
LANGUAGE plpgsql AS $$
DECLARE
    record_id uuid;
    expected_type varchar(16);
    detail_count integer;
    actual_type varchar(16);
BEGIN
    IF TG_OP = 'DELETE' THEN
        record_id := OLD.id;
    ELSE
        record_id := NEW.id;
    END IF;
    IF TG_OP = 'UPDATE' AND OLD.id <> NEW.id THEN
        RAISE EXCEPTION 'ID aktivitas/detail tidak boleh diubah';
    END IF;
    SELECT activity_type INTO expected_type
        FROM activity_record WHERE id = record_id FOR UPDATE;
    IF NOT FOUND THEN
        RETURN NULL; -- induk sudah dihapus; detail ikut CASCADE
    END IF;
    SELECT count(*), min(kind) INTO detail_count, actual_type
    FROM (
        SELECT 'Mobility'::varchar AS kind FROM mobility_activity WHERE id = record_id
        UNION ALL
        SELECT 'FuelPurchase'::varchar FROM fuel_purchase_activity WHERE id = record_id
        UNION ALL
        SELECT 'Electricity'::varchar FROM electricity_usage WHERE id = record_id
    ) details;
    IF detail_count <> 1 OR actual_type <> expected_type THEN
        RAISE EXCEPTION 'Aktivitas % harus mempunyai tepat satu detail bertipe %',
            record_id, expected_type USING ERRCODE = '23514';
    END IF;
    RETURN NULL;
END;
$$;

CREATE CONSTRAINT TRIGGER ct_activity_record_detail
    AFTER INSERT OR UPDATE OR DELETE ON activity_record
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION validate_activity_detail();

CREATE CONSTRAINT TRIGGER ct_mobility_activity_detail
    AFTER INSERT OR UPDATE OR DELETE ON mobility_activity
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION validate_activity_detail();

CREATE CONSTRAINT TRIGGER ct_fuel_purchase_activity_detail
    AFTER INSERT OR UPDATE OR DELETE ON fuel_purchase_activity
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION validate_activity_detail();

CREATE CONSTRAINT TRIGGER ct_electricity_usage_detail
    AFTER INSERT OR UPDATE OR DELETE ON electricity_usage
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION validate_activity_detail();

COMMIT;
