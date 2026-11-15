BEGIN;

CREATE TABLE public.user_profiles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name varchar(100) NOT NULL CHECK (length(trim(name)) > 0),
    unit_preference varchar(20) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE public.vehicles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_profile_id uuid NOT NULL REFERENCES public.user_profiles(id),
    name varchar(100) NOT NULL CHECK (length(trim(name)) > 0),
    vehicle_type varchar(30) NOT NULL,
    fuel_type varchar(20)
);

CREATE TABLE public.vehicle_monthly_inputs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    vehicle_id uuid NOT NULL REFERENCES public.vehicles(id),
    month date NOT NULL CHECK (extract(day FROM month) = 1),
    method varchar(20) NOT NULL CHECK (method IN ('Distance', 'FuelPurchase')),
    CONSTRAINT uq_vehicle_month UNIQUE (vehicle_id, month)
);

-- Table-per-type mapping for ActivityRecord and its three concrete children.
CREATE TABLE public.activity_records (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_profile_id uuid NOT NULL REFERENCES public.user_profiles(id),
    activity_type varchar(20) NOT NULL
        CHECK (activity_type IN ('Mobility', 'FuelPurchase', 'Electricity')),
    unit varchar(20) NOT NULL CHECK (unit IN ('km', 'liter', 'kWh')),
    CONSTRAINT uq_activity_profile UNIQUE (id, user_profile_id),
    CONSTRAINT ck_activity_type_unit CHECK (
        (activity_type = 'Mobility' AND unit = 'km') OR
        (activity_type = 'FuelPurchase' AND unit = 'liter') OR
        (activity_type = 'Electricity' AND unit = 'kWh')
    )
);

CREATE TABLE public.mobility_activities (
    activity_record_id uuid PRIMARY KEY
        REFERENCES public.activity_records(id) ON DELETE CASCADE,
    vehicle_id uuid REFERENCES public.vehicles(id),
    activity_date date NOT NULL,
    transport_mode varchar(30) NOT NULL,
    distance_km numeric(10,2) NOT NULL CHECK (distance_km > 0),
    frequency integer NOT NULL CHECK (frequency > 0)
);

CREATE TABLE public.fuel_purchase_activities (
    activity_record_id uuid PRIMARY KEY
        REFERENCES public.activity_records(id) ON DELETE CASCADE,
    vehicle_id uuid NOT NULL REFERENCES public.vehicles(id),
    purchase_date date NOT NULL,
    fuel_type varchar(20) NOT NULL,
    liters_purchased numeric(10,3) NOT NULL CHECK (liters_purchased > 0),
    total_cost numeric(14,2) CHECK (total_cost >= 0)
);

CREATE TABLE public.electricity_usages (
    activity_record_id uuid PRIMARY KEY,
    user_profile_id uuid NOT NULL,
    input_method varchar(20) NOT NULL
        CHECK (input_method IN ('MeterKWh', 'ApplianceEstimate')),
    usage_kwh numeric(12,3) CHECK (usage_kwh >= 0),
    period_start date NOT NULL CHECK (extract(day FROM period_start) = 1),
    period_end date NOT NULL,
    CONSTRAINT fk_electricity_activity_profile
        FOREIGN KEY (activity_record_id, user_profile_id)
        REFERENCES public.activity_records(id, user_profile_id) ON DELETE CASCADE,
    CONSTRAINT uq_electricity_profile_month UNIQUE (user_profile_id, period_start),
    CONSTRAINT ck_electricity_full_month CHECK (
        period_end = (period_start + interval '1 month - 1 day')::date
    ),
    CONSTRAINT ck_meter_has_value CHECK (
        input_method <> 'MeterKWh' OR usage_kwh IS NOT NULL
    )
);

CREATE TABLE public.appliance_usages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    electricity_usage_id uuid NOT NULL
        REFERENCES public.electricity_usages(activity_record_id) ON DELETE CASCADE,
    name varchar(100) NOT NULL CHECK (length(trim(name)) > 0),
    power_watt numeric(10,2) NOT NULL CHECK (power_watt > 0),
    quantity integer NOT NULL CHECK (quantity > 0),
    hours_per_day numeric(4,2) NOT NULL
        CHECK (hours_per_day > 0 AND hours_per_day <= 24),
    days_used smallint NOT NULL CHECK (days_used BETWEEN 1 AND 31)
);

CREATE TABLE public.emission_factors (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    category varchar(30) NOT NULL,
    subcategory varchar(50) NOT NULL,
    value numeric(16,8) NOT NULL CHECK (value >= 0),
    unit varchar(30) NOT NULL
        CHECK (unit IN ('kgCO2e/km', 'kgCO2e/liter', 'kgCO2e/kWh')),
    source text NOT NULL CHECK (length(trim(source)) > 0),
    version varchar(50) NOT NULL,
    valid_from date NOT NULL,
    valid_until date,
    is_active boolean NOT NULL DEFAULT true,
    CONSTRAINT ck_factor_valid_dates
        CHECK (valid_until IS NULL OR valid_until >= valid_from)
);

CREATE TABLE public.emission_calculations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    activity_record_id uuid NOT NULL UNIQUE
        REFERENCES public.activity_records(id) ON DELETE CASCADE,
    emission_factor_id uuid NOT NULL REFERENCES public.emission_factors(id),
    activity_value numeric(14,4) NOT NULL CHECK (activity_value >= 0),
    result_kg_co2e numeric(16,6) NOT NULL CHECK (result_kg_co2e >= 0),
    calculated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE public.scenarios (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_profile_id uuid NOT NULL REFERENCES public.user_profiles(id),
    name varchar(100) NOT NULL CHECK (length(trim(name)) > 0),
    baseline_start date NOT NULL CHECK (extract(day FROM baseline_start) = 1),
    baseline_end date NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_scenario_full_month CHECK (
        baseline_end = (baseline_start + interval '1 month - 1 day')::date
    )
);

CREATE TABLE public.scenario_changes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    scenario_id uuid NOT NULL REFERENCES public.scenarios(id) ON DELETE CASCADE,
    change_type varchar(40) NOT NULL CHECK (change_type IN (
        'ReduceDistancePercent', 'ReduceFrequencyPercent',
        'ReduceFuelPercent', 'ReduceElectricityPercent',
        'ReduceApplianceHoursPercent'
    )),
    target_activity_record_id uuid NOT NULL REFERENCES public.activity_records(id),
    target_appliance_usage_id uuid REFERENCES public.appliance_usages(id),
    change_value numeric(5,2) NOT NULL CHECK (change_value BETWEEN 0 AND 100),
    CONSTRAINT ck_appliance_change_target CHECK (
        (change_type = 'ReduceApplianceHoursPercent' AND
            target_appliance_usage_id IS NOT NULL) OR
        (change_type <> 'ReduceApplianceHoursPercent' AND
            target_appliance_usage_id IS NULL)
    )
);

CREATE TABLE public.scenario_results (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    scenario_id uuid NOT NULL UNIQUE REFERENCES public.scenarios(id) ON DELETE CASCADE,
    baseline_kg_co2e numeric(16,6) NOT NULL CHECK (baseline_kg_co2e >= 0),
    scenario_kg_co2e numeric(16,6) NOT NULL CHECK (scenario_kg_co2e >= 0),
    reduction_kg_co2e numeric(16,6) NOT NULL,
    calculated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_result_reduction CHECK (
        reduction_kg_co2e = baseline_kg_co2e - scenario_kg_co2e
    )
);

CREATE TABLE public.recommendations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    scenario_result_id uuid NOT NULL
        REFERENCES public.scenario_results(id) ON DELETE CASCADE,
    title varchar(160) NOT NULL CHECK (length(trim(title)) > 0),
    description text NOT NULL,
    target_category varchar(30) NOT NULL,
    recommended_action text NOT NULL,
    estimated_reduction_kg_co2e numeric(16,6) NOT NULL
        CHECK (estimated_reduction_kg_co2e >= 0),
    priority integer NOT NULL CHECK (priority > 0),
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_activity_records_profile ON public.activity_records(user_profile_id);
CREATE INDEX ix_mobility_date ON public.mobility_activities(activity_date);
CREATE INDEX ix_fuel_date ON public.fuel_purchase_activities(purchase_date);
CREATE INDEX ix_factors_lookup ON public.emission_factors(category, subcategory, valid_from);
CREATE INDEX ix_scenarios_profile ON public.scenarios(user_profile_id);
CREATE INDEX ix_scenario_changes_scenario ON public.scenario_changes(scenario_id);

-- Tables created in SQL Editor would otherwise be exposed through Supabase Data API.
-- No client-side policies are created: the backend connects to PostgreSQL instead.
ALTER TABLE public.user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.vehicles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.vehicle_monthly_inputs ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.activity_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.mobility_activities ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.fuel_purchase_activities ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.electricity_usages ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.appliance_usages ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.emission_factors ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.emission_calculations ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.scenarios ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.scenario_changes ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.scenario_results ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.recommendations ENABLE ROW LEVEL SECURITY;

COMMIT;
