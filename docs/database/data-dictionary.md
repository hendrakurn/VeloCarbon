# Kamus data VeloCarbon

Rancangan PostgreSQL untuk 14 entitas dan 84 atribut. Nama tabel memakai `snake_case`; kolom **Model C#** pada setiap bagian menunjukkan padanannya. Diagram dan aturan relasi ada di [ERD](erd.md), implementasi di [schema.sql](../../database/schema.sql).

## Konvensi tipe, ukuran, dan nilai

- `UUID`: 128 bit (16 byte), dipetakan ke `Guid`.
- `VARCHAR(n)`: maksimal **n karakter**, bukan n byte; `TEXT`: panjang bervariasi tanpa batas aplikasi yang ditetapkan. Nama/teks wajib ditolak jika hanya spasi. String dipetakan ke `string`.
- `NUMERIC(p,s)`: p digit total, s digit desimal; p−s digit bagian bulat. Penyimpanan fisik bervariasi, bukan p byte. Dipetakan ke `decimal`; aplikasi menangani overflow dan memakai pembulatan `MidpointRounding.AwayFromZero` saat menyimpan.
- `SMALLINT`: 2 byte (−32768 sampai 32767); `INTEGER`: 4 byte (−2147483648 sampai 2147483647). Batas bisnis yang lebih sempit dijelaskan per atribut. Domain dapat tetap memakai `int` dengan konversi terperiksa untuk SMALLINT.
- `DATE`: 4 byte, tanpa jam; dipetakan ke `DateOnly` pada persistence atau `DateTime.Date` pada prototype. `TIMESTAMPTZ`: 8 byte, ketelitian mikrodetik; waktu absolut, ditampilkan menurut zona sesi, tidak menyimpan nama zona asal. Gunakan `DateTime` UTC pada persistence.
- `BOOLEAN`: 1 byte, dipetakan ke `bool`.
- PK = primary key; FK = foreign key; UK = unique key. Nullable berarti SQL mengizinkan NULL; atribut bersyarat tetap mengikuti CHECK.
- UUID mandiri memakai default `gen_random_uuid()`; ID tabel detail harus memakai ID induk. Waktu memakai `CURRENT_TIMESTAMP`; tidak ada mekanisme otomatis memperbarui waktu saat UPDATE.
- Enum menggunakan VARCHAR + CHECK agar nilai mudah dibaca dan sesuai nama enum C#. `unit_preference` baru mendukung Metric; dukungan unit lain memerlukan perubahan kontrak dan konversi.
- Semua besaran fisik menggunakan satuan tetap: km, liter, watt, jam, kWh, kgCO2e, persen, dan rupiah. Nilai numeric NaN ditolak; tanggal aktivitas/periode/faktor harus finite.
- Presisi penyimpanan: input sampai 3 desimal (jam 2), nilai aktivitas 9, faktor 9, hasil emisi 6. Hasil perkalian dijumlahkan dengan decimal sebelum pembulatan penyimpanan; tampilan memakai 2 desimal. Presisi hasil 24 digit memberi ruang lebih besar daripada rencana awal NUMERIC(18,6); nilai di luar kapasitas ditolak, bukan dipotong.

Dasar tipe dan ukuran: [PostgreSQL numeric](https://www.postgresql.org/docs/current/datatype-numeric.html), [character](https://www.postgresql.org/docs/current/datatype-character.html), [date/time](https://www.postgresql.org/docs/current/datatype-datetime.html), [UUID](https://www.postgresql.org/docs/current/datatype-uuid.html), dan [boolean](https://www.postgresql.org/docs/current/datatype-boolean.html).

## 1. user_profile

**Model C#:** `UserProfile`. Profil individu; tidak menyimpan akun daring atau kata sandi.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `name` | `VARCHAR(100)` | 100 karakter maks. | Tidak | — | Nama tampilan, wajib berisi teks. |
| `unit_preference` | `VARCHAR(10)` | 10 karakter maks. | Tidak | — | Preferensi tampilan MVP: km, liter, kWh; seluruh input memakai satuan ini. Nilai: `Metric`. |
| `created_at` | `TIMESTAMPTZ` | 8 byte; mikrodetik | Tidak | — | Waktu pembuatan; default waktu transaksi. |

## 2. vehicle

**Model C#:** `Vehicle`. Kendaraan pribadi milik satu profil.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `user_profile_id` | `UUID` | 16 byte | Tidak | FK | Pemilik kendaraan. |
| `name` | `VARCHAR(100)` | 100 karakter maks. | Tidak | — | Nama tampilan, wajib berisi teks. |
| `vehicle_type` | `VARCHAR(16)` | 16 karakter maks. | Tidak | — | Moda kendaraan pribadi. Nilai: `Motorcycle`, `Car`. |
| `fuel_type` | `VARCHAR(10)` | 10 karakter maks. | Tidak | — | Jenis bahan bakar kendaraan. Nilai: `Petrol`, `Diesel`. |

## 3. vehicle_monthly_input

**Model C#:** `VehicleMonthlyInput`. Pilihan metode mobilitas untuk satu kendaraan pada satu bulan.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `vehicle_id` | `UUID` | 16 byte | Tidak | FK | Kendaraan yang diatur. |
| `month` | `DATE` | 4 byte; tanggal | Tidak | — | Tanggal pertama bulan, contoh 2026-09-01; bagian UK gabungan. |
| `method` | `VARCHAR(16)` | 16 karakter maks. | Tidak | — | Metode tunggal kendaraan pada bulan tersebut. Nilai: `Distance`, `FuelPurchase`. |

Constraint tingkat tabel:

- `UNIQUE (vehicle_id, month)`

## 4. activity_record

**Model C#:** `ActivityRecord`. Atribut bersama semua aktivitas, dengan discriminator activity_type.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `user_profile_id` | `UUID` | 16 byte | Tidak | FK | Pemilik aktivitas. |
| `name` | `VARCHAR(100)` | 100 karakter maks. | Tidak | — | Nama tampilan, wajib berisi teks. |
| `activity_type` | `VARCHAR(16)` | 16 karakter maks. | Tidak | — | Jenis tabel detail yang wajib dimiliki. Nilai: `Mobility`, `FuelPurchase`, `Electricity`. |
| `unit` | `VARCHAR(10)` | 10 karakter maks. | Tidak | — | Satuan nilai aktivitas; harus sesuai jenis. Nilai: `Kilometer`, `Liter`, `KWh`. |
| `activity_date` | `DATE` | 4 byte; tanggal | Tidak | — | Tanggal perjalanan/pembelian; tanggal awal bulan untuk listrik. |
| `created_at` | `TIMESTAMPTZ` | 8 byte; mikrodetik | Tidak | — | Waktu pembuatan; default waktu transaksi. |

Constraint tingkat tabel:

- `CHECK ((activity_type = 'Mobility' AND unit = 'Kilometer') OR (activity_type = 'FuelPurchase' AND unit = 'Liter') OR (activity_type = 'Electricity' AND unit = 'KWh'))`
- `CHECK (activity_type <> 'Electricity' OR EXTRACT(DAY FROM activity_date) = 1)`

## 5. mobility_activity

**Model C#:** `MobilityActivity`. Detail perjalanan berdasarkan jarak dan frekuensi.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK, FK | PK sekaligus FK ke activity_record.id; ID sama dengan induk. |
| `vehicle_id` | `UUID` | 16 byte | Ya | FK | Wajib untuk Motorcycle/Car; NULL untuk Bus. |
| `transport_mode` | `VARCHAR(16)` | 16 karakter maks. | Tidak | — | Moda perjalanan. Nilai: `Motorcycle`, `Car`, `Bus`. |
| `distance_km` | `NUMERIC(12,3)` | 12 digit: 9 bulat + 3 desimal | Tidak | — | Jarak satu arah per perjalanan dalam km; positif, sampai 999999999.999. |
| `frequency` | `INTEGER` | 4 byte | Tidak | — | Jumlah perjalanan pada activity_date; 1–10000. Batas operasional mencegah input tidak masuk akal. |

Constraint tingkat tabel:

- `CHECK ((transport_mode = 'Bus' AND vehicle_id IS NULL) OR (transport_mode IN ('Motorcycle', 'Car') AND vehicle_id IS NOT NULL))`

## 6. fuel_purchase_activity

**Model C#:** `FuelPurchaseActivity`. Detail pembelian bahan bakar; estimasi berdasarkan liter yang dibeli.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK, FK | PK sekaligus FK ke activity_record.id; ID sama dengan induk. |
| `vehicle_id` | `UUID` | 16 byte | Tidak | FK | Kendaraan yang menerima bahan bakar. |
| `fuel_type` | `VARCHAR(10)` | 10 karakter maks. | Tidak | — | Jenis bahan bakar yang dibeli. Nilai: `Petrol`, `Diesel`. |
| `liters_purchased` | `NUMERIC(12,3)` | 12 digit: 9 bulat + 3 desimal | Tidak | — | Volume pembelian dalam liter; positif, sampai 999999999.999. |
| `total_cost` | `NUMERIC(14,2)` | 14 digit: 12 bulat + 2 desimal | Ya | — | Biaya rupiah opsional, 0–999999999999.99; tidak dipakai menghitung emisi. |

## 7. electricity_usage

**Model C#:** `ElectricityUsage`. Satu bulan listrik; tanggal awal disimpan di activity_record.activity_date.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK, FK | PK sekaligus FK ke activity_record.id; ID sama dengan induk. |
| `input_method` | `VARCHAR(16)` | 16 karakter maks. | Tidak | — | Metode pencatatan listrik. Nilai: `Meter`, `Appliances`. |
| `usage_kwh` | `NUMERIC(12,3)` | 12 digit: 9 bulat + 3 desimal | Ya | — | kWh meter (0–999999999.999); wajib pada Meter dan NULL pada Appliances, yang dihitung dari detail. |

Constraint tingkat tabel:

- `CHECK ((input_method = 'Meter' AND usage_kwh IS NOT NULL) OR (input_method = 'Appliances' AND usage_kwh IS NULL))`

## 8. appliance_usage

**Model C#:** `ApplianceUsage`. Detail alat untuk estimasi listrik.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `electricity_usage_id` | `UUID` | 16 byte | Tidak | FK | Induk listrik dengan metode Appliances. |
| `name` | `VARCHAR(100)` | 100 karakter maks. | Tidak | — | Nama tampilan, wajib berisi teks. |
| `power_watt` | `NUMERIC(9,3)` | 9 digit: 6 bulat + 3 desimal | Tidak | — | Daya rata-rata dalam watt; positif, sampai 999999.999. |
| `quantity` | `SMALLINT` | 2 byte | Tidak | — | Jumlah perangkat identik; 1–1000. |
| `hours_per_day` | `NUMERIC(4,2)` | 4 digit: 2 bulat + 2 desimal | Tidak | — | Jam penggunaan per hari; lebih dari 0 hingga 24.00. |
| `days_used` | `SMALLINT` | 2 byte | Tidak | — | Hari pemakaian; 1–31 dan tidak boleh melebihi jumlah hari bulan aktivitas (validasi backend). |

Constraint tingkat tabel:

- `UNIQUE (id, electricity_usage_id)`

## 9. emission_factor

**Model C#:** `EmissionFactor`. Faktor emisi berversi beserta sumber dan masa berlakunya.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `category` | `VARCHAR(16)` | 16 karakter maks. | Tidak | — | Kategori sumber emisi. Nilai: `Mobility`, `Electricity`. |
| `factor_key` | `VARCHAR(16)` | 16 karakter maks. | Tidak | — | Subkategori; pemetaan dari FactorKey pada domain. Nilai: `Motorcycle`, `Car`, `Bus`, `Petrol`, `Diesel`, `Electricity`. |
| `value` | `NUMERIC(18,9)` | 18 digit: 9 bulat + 9 desimal | Tidak | — | kgCO2e per satu unit aktivitas; 0–999999999.999999999. |
| `unit` | `VARCHAR(10)` | 10 karakter maks. | Tidak | — | Penyebut faktor: km, liter, atau kWh. Nilai: `Kilometer`, `Liter`, `KWh`. |
| `source` | `TEXT` | Bervariasi; tanpa batas aplikasi | Tidak | — | Judul dokumen/URL, penerbit, wilayah, dan asumsi sumber; wajib, tanpa batas panjang aplikasi. |
| `version` | `VARCHAR(30)` | 30 karakter maks. | Tidak | — | Label versi faktor, maksimal 30 karakter; bagian UK gabungan. |
| `valid_from` | `DATE` | 4 byte; tanggal | Tidak | — | Tanggal mulai berlaku, inklusif. |
| `valid_until` | `DATE` | 4 byte; tanggal | Ya | — | Tanggal akhir inklusif; NULL berarti belum ditentukan. |
| `is_active` | `BOOLEAN` | 1 byte | Tidak | — | TRUE dapat dipilih untuk perhitungan baru; FALSE tetap disimpan untuk audit. |

Constraint tingkat tabel:

- `UNIQUE (factor_key, unit, version)`
- `CHECK (valid_until IS NULL OR valid_until >= valid_from)`
- `CHECK ((category = 'Mobility' AND factor_key IN ('Motorcycle', 'Car', 'Bus') AND unit = 'Kilometer') OR (category = 'Mobility' AND factor_key IN ('Petrol', 'Diesel') AND unit = 'Liter') OR (category = 'Electricity' AND factor_key = 'Electricity' AND unit = 'KWh'))`

## 10. emission_calculation

**Model C#:** `EmissionCalculation`. Satu hasil aktif per aktivitas beserta faktor yang digunakan.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `activity_record_id` | `UUID` | 16 byte | Tidak | FK, UK | Aktivitas yang dihitung; unik agar dashboard tidak menghitung hasil ganda. |
| `emission_factor_id` | `UUID` | 16 byte | Tidak | FK | Versi faktor yang dipakai; faktor terpakai tidak boleh dihapus. |
| `activity_value` | `NUMERIC(24,9)` | 24 digit: 15 bulat + 9 desimal | Tidak | — | Nilai aktivitas setelah normalisasi; 15 digit bulat + 9 desimal untuk km, liter, atau kWh. |
| `result_kg_co2e` | `NUMERIC(24,6)` | 24 digit: 18 bulat + 6 desimal | Tidak | — | Hasil emisi kgCO2e; 18 digit bulat + 6 desimal. |
| `calculated_at` | `TIMESTAMPTZ` | 8 byte; mikrodetik | Tidak | — | Waktu perhitungan; diisi ulang backend saat hitung ulang. |

## 11. scenario

**Model C#:** `Scenario`. Rancangan pengurangan aktivitas untuk satu bulan baseline.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `user_profile_id` | `UUID` | 16 byte | Tidak | FK | Pemilik skenario. |
| `name` | `VARCHAR(100)` | 100 karakter maks. | Tidak | — | Nama tampilan, wajib berisi teks. |
| `baseline_start` | `DATE` | 4 byte; tanggal | Tidak | — | Tanggal pertama bulan baseline. |
| `baseline_end` | `DATE` | 4 byte; tanggal | Tidak | — | Tanggal terakhir bulan baseline, inklusif. |
| `created_at` | `TIMESTAMPTZ` | 8 byte; mikrodetik | Tidak | — | Waktu pembuatan; default waktu transaksi. |

Constraint tingkat tabel:

- `CHECK (baseline_end = (baseline_start + INTERVAL '1 month' - INTERVAL '1 day')::date)`

## 12. scenario_change

**Model C#:** `ScenarioChange`. Perubahan persentase yang menarget satu aktivitas atau rincian alat.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `scenario_id` | `UUID` | 16 byte | Tidak | FK | Skenario pemilik perubahan. |
| `change_type` | `VARCHAR(32)` | 32 karakter maks. | Tidak | — | Jenis pengurangan. Nilai: `ReduceDistancePercent`, `ReduceFrequencyPercent`, `ReduceFuelPercent`, `ReduceElectricityPercent`, `ReduceApplianceHoursPercent`. |
| `target_activity_record_id` | `UUID` | 16 byte | Tidak | FK | Aktivitas target, harus milik profil dan berada dalam baseline. |
| `target_appliance_usage_id` | `UUID` | 16 byte | Ya | FK | Alat target; NULL kecuali pengurangan jam alat. FK gabungan dengan target_activity_record_id memastikan alat milik aktivitas target. |
| `change_value` | `NUMERIC(5,2)` | 5 digit: 3 bulat + 2 desimal | Tidak | — | Persentase pengurangan, 0.00–100.00, bukan pecahan 0–1. |

Constraint tingkat tabel:

- `FOREIGN KEY (target_appliance_usage_id, target_activity_record_id) REFERENCES appliance_usage(id, electricity_usage_id) ON DELETE RESTRICT`
- `CHECK ((change_type = 'ReduceApplianceHoursPercent' AND target_appliance_usage_id IS NOT NULL) OR (change_type <> 'ReduceApplianceHoursPercent' AND target_appliance_usage_id IS NULL))`

## 13. scenario_result

**Model C#:** `ScenarioResult`. Hasil terbaru satu skenario; hasil dihapus/dihitung ulang jika baseline berubah.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `scenario_id` | `UUID` | 16 byte | Tidak | FK, UK | Skenario yang dihitung; satu hasil per skenario. |
| `baseline_kg_co2e` | `NUMERIC(24,6)` | 24 digit: 18 bulat + 6 desimal | Tidak | — | Total emisi kondisi awal, kgCO2e; 18 digit bulat + 6 desimal. |
| `scenario_kg_co2e` | `NUMERIC(24,6)` | 24 digit: 18 bulat + 6 desimal | Tidak | — | Total emisi setelah perubahan, kgCO2e; tidak melebihi baseline. |
| `reduction_kg_co2e` | `NUMERIC(24,6)` | 24 digit: 18 bulat + 6 desimal | Tidak | — | Kolom otomatis: baseline dikurangi skenario; aplikasi tidak mengisinya. |
| `calculated_at` | `TIMESTAMPTZ` | 8 byte; mikrodetik | Tidak | — | Waktu simulasi; diisi ulang ketika hasil diperbarui. |

Constraint tingkat tabel:

- `CHECK (scenario_kg_co2e <= baseline_kg_co2e)`

## 14. recommendation

**Model C#:** `Recommendation`. Saran dari hasil skenario, diurutkan berdasarkan estimasi dampak.

| Atribut | Tipe PostgreSQL | Ukuran/presisi | NULL? | Kunci | Penjelasan dan batas nilai |
| --- | --- | --- | --- | --- | --- |
| `id` | `UUID` | 16 byte | Tidak | PK | Identitas unik; dibuat otomatis jika tidak dikirim aplikasi. |
| `scenario_result_id` | `UUID` | 16 byte | Tidak | FK | Hasil simulasi yang menghasilkan rekomendasi. |
| `title` | `VARCHAR(150)` | 150 karakter maks. | Tidak | — | Judul singkat rekomendasi, maksimal 150 karakter. |
| `description` | `TEXT` | Bervariasi; tanpa batas aplikasi | Tidak | — | Penjelasan dampak dan asumsi; teks wajib. |
| `target_category` | `VARCHAR(16)` | 16 karakter maks. | Tidak | — | Kategori aktivitas yang direkomendasikan. Nilai: `Mobility`, `Electricity`. |
| `recommended_action` | `TEXT` | Bervariasi; tanpa batas aplikasi | Tidak | — | Tindakan konkret yang disarankan; teks wajib. |
| `estimated_reduction_kg_co2e` | `NUMERIC(24,6)` | 24 digit: 18 bulat + 6 desimal | Tidak | — | Estimasi pengurangan positif pada bulan baseline; kgCO2e. |
| `priority` | `INTEGER` | 4 byte | Tidak | — | Urutan 1 tertinggi, maksimal 2147483647; unik dalam satu hasil. |
| `created_at` | `TIMESTAMPTZ` | 8 byte; mikrodetik | Tidak | — | Waktu pembuatan; default waktu transaksi. |

Constraint tingkat tabel:

- `UNIQUE (scenario_result_id, priority)`

## Atribut domain yang diturunkan dan pemetaan

| Atribut domain | Penyimpanan/pemetaan |
| --- | --- |
| MobilityActivity.ActivityDate | activity_record.activity_date |
| FuelPurchaseActivity.PurchaseDate | activity_record.activity_date |
| ElectricityUsage.PeriodStart | activity_record.activity_date (tanggal pertama bulan) |
| ElectricityUsage.PeriodEnd | Dihitung dari tanggal awal + 1 bulan − 1 hari; tidak disimpan agar tidak bertentangan |
| ElectricityUsage.UsageKWh pada metode Appliances | SUM(power_watt / 1000 × quantity × hours_per_day × days_used), bukan input manual; nilai normalisasi tersimpan di emission_calculation.activity_value |
| ActivityRecord.Unit | activity_record.unit, CHECK sesuai discriminator |
| ActivityRecord.FactorKey / EmissionFactor.Subcategory | Diturunkan dari moda, bahan bakar, atau Electricity; disimpan sebagai emission_factor.factor_key |
| ScenarioResult.ReductionKgCO2e | Kolom generated dari baseline_kg_co2e − scenario_kg_co2e |
| ScenarioResult.GetReductionPercentage() | reduction_kg_co2e / NULLIF(baseline_kg_co2e, 0) × 100; NULL ketika baseline nol |
| Navigasi objek/koleksi | Dibentuk oleh FK/join, tidak disimpan sebagai array atau objek JSON |

Rancangan ini mengikuti kebutuhan produk di PRD dan empat metode input pada README. Prototype belum terhubung ke database ini. Perbedaan yang perlu diadaptasi backend: DateOnly/UTC, kode VARCHAR enum, detail ID bersama (pola table-per-type), listrik alat memakai NULL untuk input meter, serta atribut profil/audit/rekomendasi yang belum seluruhnya ada di prototype.
