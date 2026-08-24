# Product Requirements Document — VeloCarbon

| Informasi | Nilai |
| --- | --- |
| Produk | VeloCarbon |
| Versi dokumen | 0.1 |
| Tanggal | 24 Agustus 2026 |
| Status | Draft untuk persetujuan kelompok |
| Pemilik dokumen | Faaid Sakhaa — Software Architect |
| Platform | C# .NET WPF, Windows desktop |

## 1. Ringkasan Produk

VeloCarbon membantu individu mengubah data aktivitas sehari-hari menjadi estimasi jejak karbon yang dapat dipahami dan ditindaklanjuti. Produk berfokus pada dua sumber emisi yang mudah dicatat: perjalanan darat dan konsumsi listrik pribadi/kos. Pengguna dapat melihat emisi historis, mengetahui sumber terbesar, serta membandingkan dampak beberapa perubahan kebiasaan.

Keberhasilan produk ditentukan oleh tiga hal: perhitungan dapat ditelusuri ke faktor emisi yang digunakan, simulasi menghasilkan perbandingan yang jelas, dan aplikasi dapat berjalan lokal saat demonstrasi.

## 2. Masalah, Pengguna, dan Tujuan

### Masalah

Pengguna tidak memiliki cara praktis untuk menyatukan data perjalanan dan listrik, memahami sumber emisi dominan, lalu mengukur dampak perubahan kebiasaan sebelum menerapkannya.

### Pengguna utama

Mahasiswa atau individu penghuni kos yang menggunakan kendaraan pribadi/transportasi darat dan membayar atau mengestimasi konsumsi listrik sendiri.

### Tujuan produk

1. Menghasilkan estimasi emisi CO2e per aktivitas dan periode.
2. Menunjukkan tren dan sumber emisi terbesar secara visual.
3. Membandingkan kondisi dasar dengan skenario pengurangan emisi.
4. Menyimpan data dan hasil perhitungan secara konsisten di PostgreSQL.

### Di luar ruang lingkup MVP

- Perhitungan emisi industri atau organisasi.
- Pelacakan lokasi otomatis/GPS.
- Pembelian kredit karbon, perdagangan karbon, atau klaim net-zero.
- Perhitungan emisi makanan, penerbangan, dan limbah.
- Akun daring dan sinkronisasi lintas perangkat.

## 3. Kebutuhan Fungsional

| ID | Kebutuhan | Kriteria penerimaan |
| --- | --- | --- |
| FR-01 | Pengguna dapat membuat dan memperbarui profil. | Profil memiliki nama tampilan dan preferensi satuan; perubahan tersimpan. |
| FR-02 | Pengguna dapat mengelola kendaraan. | Pengguna dapat menambah, mengubah, dan menghapus kendaraan beserta jenis bahan bakar/moda. |
| FR-03 | Pengguna dapat mencatat aktivitas perjalanan. | Rekaman minimal memuat tanggal, moda/kendaraan, jarak, dan frekuensi/jumlah perjalanan. |
| FR-04 | Pengguna dapat mencatat konsumsi listrik. | Rekaman memuat periode, kWh, dan catatan opsional. |
| FR-05 | Sistem menghitung emisi berdasarkan faktor emisi aktif. | Hasil menyimpan nilai aktivitas, faktor, satuan, sumber faktor, dan emisi CO2e. |
| FR-06 | Pengguna dapat melihat ringkasan dan tren. | Dashboard menampilkan total periode, kontribusi per kategori, dan grafik bulanan. |
| FR-07 | Pengguna dapat membuat skenario. | Skenario mendukung perubahan jarak/frekuensi perjalanan atau persentase konsumsi listrik. |
| FR-08 | Sistem membandingkan skenario dengan kondisi dasar. | Hasil memuat emisi dasar, emisi skenario, pengurangan kgCO2e, dan persen. |
| FR-09 | Sistem menyajikan rekomendasi. | Rekomendasi menaut pada sumber emisi dominan dan menyebut estimasi dampaknya. |
| FR-10 | Faktor emisi dapat dikelola administrator lokal. | Faktor memiliki kategori, nilai, satuan, sumber, versi, tanggal berlaku, dan status aktif. |

## 4. Kebutuhan Nonfungsional

| ID | Kebutuhan |
| --- | --- |
| NFR-01 | Aplikasi berjalan pada Windows dengan .NET dan PostgreSQL lokal yang didokumentasikan. |
| NFR-02 | Perhitungan inti dapat berjalan tanpa koneksi internet. |
| NFR-03 | Masukan numerik ditolak bila kosong, negatif, atau tidak sesuai satuan. |
| NFR-04 | Kesalahan database/API ditampilkan dalam pesan yang jelas tanpa membuat aplikasi berhenti mendadak. |
| NFR-05 | Pengujian unit mencakup rumus emisi dan mesin skenario. |
| NFR-06 | Semua hasil memiliki satuan dan pembulatan yang konsisten. |

## 5. Aturan Bisnis dan Rumus

### Emisi aktivitas

```text
emisiKgCO2e = nilaiAktivitas x faktorEmisi
```

- Mobilitas menggunakan kilometer perjalanan atau liter bahan bakar, sesuai faktor yang dipilih.
- Listrik menggunakan kWh.
- Faktor emisi harus memiliki satuan yang kompatibel dengan nilai aktivitas.

### Skenario

```text
emisiSkenario = total emisi aktivitas setelah perubahan skenario
penguranganKgCO2e = emisiDasar - emisiSkenario
penguranganPersen = (penguranganKgCO2e / emisiDasar) x 100
```

Jika emisi dasar nol, persentase pengurangan ditampilkan sebagai tidak tersedia untuk menghindari pembagian dengan nol.

## 6. Arsitektur MVP

```text
Presentation (WPF Views)
        <-> Presentation Logic (ViewModels)
        <-> Application Layer (Services / Use Cases)
        <-> Domain Layer (Models, Rules, Scenario Engine)
        <-> Infrastructure (EF Core Repositories, PostgreSQL, optional API client)
```

| Lapisan | Tanggung jawab |
| --- | --- |
| Views | Mengikat data dan menerima interaksi pengguna; tidak memuat rumus bisnis. |
| ViewModels | Mengelola state tampilan, perintah, dan validasi ringan. |
| Services | Menjalankan use case pencatatan, perhitungan, dashboard, dan simulasi. |
| Domain | Menyimpan aturan bisnis dan model PBO yang tidak bergantung pada WPF/EF Core. |
| Infrastructure | Mengakses PostgreSQL melalui EF Core dan mengisolasi layanan eksternal opsional. |

### Batas modul

- `EmissionCalculationService` tidak mengetahui View atau database; ia menerima aktivitas dan faktor melalui antarmuka.
- `ScenarioSimulationService` menghasilkan hasil yang dapat diuji menggunakan data tiruan.
- `EmissionFactorRepository` menjadi jalur akses faktor emisi dari basis data.
- Klien BMKG/NASA, bila dipakai, tidak boleh dipanggil langsung dari ViewModel dan tidak menjadi prasyarat perhitungan inti.

## 7. Model Domain dan Basis Data

| Entitas | Atribut inti | Relasi utama |
| --- | --- | --- |
| UserProfile | Id, DisplayName, UnitPreference | Memiliki kendaraan, aktivitas, dan skenario. |
| Vehicle | Id, UserProfileId, Name, TransportMode, FuelType | Digunakan oleh ActivityRecord. |
| ActivityRecord | Id, UserProfileId, VehicleId?, Date, Category, DistanceKm, TripCount | Menghasilkan perhitungan emisi. |
| ElectricityUsage | Id, UserProfileId, PeriodStart, PeriodEnd, Kwh | Menghasilkan perhitungan emisi. |
| EmissionFactor | Id, Category, Subcategory, Value, Unit, Source, Version, EffectiveFrom, IsActive | Dipakai oleh perhitungan. |
| EmissionCalculation | Id, ActivityRecordId?/ElectricityUsageId?, EmissionFactorId, ActivityValue, ResultKgCO2e, CalculatedAt | Menyimpan jejak audit perhitungan. |
| Scenario | Id, UserProfileId, Name, BaselinePeriod, CreatedAt | Memiliki perubahan dan hasil. |
| ScenarioChange | Id, ScenarioId, ChangeType, TargetCategory, PercentageOrValue | Menentukan perubahan yang disimulasikan. |
| ScenarioResult | Id, ScenarioId, BaselineKgCO2e, ScenarioKgCO2e, ReductionKgCO2e, ReductionPercent | Menyimpan hasil akhir. |

## 8. Alur Data

1. Pengguna menyimpan kendaraan, perjalanan, atau konsumsi listrik.
2. Service memvalidasi satuan dan mengambil faktor emisi aktif yang sesuai.
3. Mesin perhitungan menghasilkan emisi CO2e dan menyimpan hasil beserta faktor yang digunakan.
4. Dashboard mengambil hasil agregat berdasarkan periode dan kategori.
5. Pengguna membuat skenario; mesin simulasi menerapkan perubahan pada kondisi dasar lalu menghitung selisihnya.
6. Recommendation service memilih area dengan emisi terbesar dan menyajikan aksi relevan beserta estimasi dampaknya.

## 9. Rencana Rilis

| Tahap | Luaran |
| --- | --- |
| MVP-1 | Struktur solusi WPF MVVM, PostgreSQL/EF Core, serta CRUD profil, kendaraan, dan aktivitas. |
| MVP-2 | Faktor emisi berversi, perhitungan emisi, dan dashboard ringkas. |
| MVP-3 | Mesin skenario, rekomendasi, grafik tren, dan data contoh untuk demo. |
| Opsional | Klien BMKG untuk konteks cuaca dan perluasan kategori emisi. |

## 10. Risiko dan Mitigasi

| Risiko | Dampak | Mitigasi |
| --- | --- | --- |
| Faktor emisi tidak konsisten satuan/sumber | Hasil keliru atau tidak dapat dijelaskan | Simpan unit, sumber, versi, dan tanggal berlaku; validasi kompatibilitas satuan. |
| Integrasi API gagal saat demo | Aplikasi tidak dapat didemokan | Perhitungan inti memakai data lokal; API hanya fitur tambahan. |
| Scope terlalu besar | MVP tidak selesai | Batasi kategori pada perjalanan darat dan listrik pribadi/kos. |
| Data pengguna tidak lengkap | Simulasi tidak bermakna | Gunakan validasi dan sediakan data contoh. |
| Perhitungan sulit diuji | Regresi logic | Pisahkan domain service dari WPF dan tulis unit test untuk rumus serta skenario. |

## 11. Tanggung Jawab Software Architect

1. Menetapkan struktur solution dan dependensi antarproyek/lapisan.
2. Menetapkan convention MVVM, dependency injection, penamaan, dan error handling.
3. Mendesain ERD dan migration awal PostgreSQL.
4. Menetapkan model domain, interface repository/service, dan kontrak DTO.
5. Meninjau Pull Request untuk menjaga batas arsitektur dan konsistensi kode.
6. Menyiapkan data contoh serta kontrak integrasi agar backend dan frontend dapat dikembangkan paralel.
7. Memelihara dokumentasi arsitektur, keputusan desain, dan alur Git.

## 12. Definisi Selesai untuk Demo

- Aplikasi WPF dapat dibuka dan dipakai dengan data lokal contoh.
- Pengguna dapat menyimpan setidaknya satu perjalanan dan satu catatan listrik.
- Dashboard menampilkan total emisi dan kontribusi kategori untuk periode yang dipilih.
- Skenario dapat dibandingkan dengan kondisi dasar dan menghasilkan pengurangan CO2e.
- Setiap faktor emisi yang digunakan memiliki sumber serta satuan.
- README memuat instruksi demo dan penggunaan saat implementasi tersedia.
