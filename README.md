# VeloCarbon

**VeloCarbon** adalah aplikasi desktop berbasis **C# WPF** dengan arsitektur **MVVM** untuk membantu individu memahami, memantau, dan mengurangi jejak karbon dari mobilitas harian serta konsumsi listrik pribadi atau tempat tinggal (kos).

> VeloCarbon adalah alat edukasi berbasis estimasi; hasilnya bukan inventarisasi emisi resmi, audit karbon, atau dasar klaim sertifikasi.

## Informasi Proyek

| Elemen | Keterangan |
| --- | --- |
| Tema | Climate Action |
| Kategori | Pelacakan jejak karbon pribadi dan rekomendasi perubahan kebiasaan |
| Tipe aplikasi | Aplikasi desktop Windows |
| Teknologi utama | C#, .NET, WPF, MVVM, PostgreSQL, Entity Framework Core |
| Nama repository | `VeloCarbon` |

## Kelompok dan Tanggung Jawab

| Anggota | NIM/NIU | Peran | Tanggung jawab utama |
| --- | --- | --- | --- |
| Rafif Raihan Bahrul Alam | 24/534432/TK/59237 | Frontend Developer| Antarmuka WPF, visualisasi tren dan hasil simulasi, pengalaman pengguna |
| Faaid Sakhaa | 24/539398/TK/59820 | Software Architect | Arsitektur MVVM, model PBO, skema basis data dan ERD, kontrak antarmodul, standar Git, serta integrasi keseluruhan sistem. |
| Hendra Kurnia Maliqi | 24/542344/TK/60216 | Backend Developer | eksekusi ERD, develop API, menyusun method method API, integrasi backend frontend |


## Latar Belakang dan Permasalahan

Aktivitas sehari-hari seperti perjalanan menggunakan kendaraan pribadi dan penggunaan listrik menghasilkan emisi gas rumah kaca. Namun, pengguna sering tidak mengetahui sumber emisi terbesarnya maupun dampak dari perubahan kebiasaan sederhana. Informasi emisi tersebar dalam satuan yang sulit dibandingkan—liter bahan bakar, kilometer perjalanan, dan kWh listrik—sehingga pengguna tidak mempunyai dasar yang jelas untuk memilih aksi pengurangan emisi yang paling berdampak.

Kalkulator emisi sederhana umumnya hanya menampilkan total emisi sekali hitung. Pendekatan ini belum membantu pengguna menyimpan riwayat aktivitas, melihat tren, membandingkan skenario, dan memilih perubahan yang realistis.

## Solusi yang Diusulkan

VeloCarbon menyatukan catatan mobilitas dan konsumsi listrik dalam satu profil emisi pribadi. Pengguna memasukkan perjalanan harian—moda transportasi, jarak, dan frekuensi—serta konsumsi listrik bulanan. Aplikasi menghitung estimasi emisi CO2e menggunakan faktor emisi yang memiliki sumber dan versi, menyimpan riwayat, lalu menjalankan simulasi perubahan kebiasaan.

Contoh simulasi: pengguna membandingkan kondisi saat ini dengan skenario menggunakan transportasi umum dua hari setiap minggu atau mengurangi konsumsi listrik sebesar 10%. Aplikasi menampilkan estimasi penurunan emisi per bulan dan per tahun, serta rekomendasi yang diprioritaskan berdasarkan dampak emisi dan kemudahan penerapannya.

```text
Emisi aktivitas (kgCO2e) = nilai aktivitas x faktor emisi (kgCO2e per satuan)
Total emisi periode = jumlah emisi seluruh aktivitas pada periode tersebut
Pengurangan skenario = emisi kondisi awal - emisi skenario
```

Satuan aktivitas disesuaikan dengan kategorinya, misalnya kilometer untuk perjalanan atau kWh untuk listrik. Faktor emisi disimpan di basis data bersama sumber, satuan, versi, dan tanggal berlaku agar perhitungan dapat ditelusuri dan diperbarui.

## Fitur Utama

1. **Profil emisi pribadi**: menyimpan profil pengguna, kendaraan, dan preferensi satuan.
2. **Pencatatan aktivitas mobilitas**: mencatat moda transportasi, jarak, frekuensi, dan tanggal perjalanan.
3. **Pencatatan konsumsi listrik**: mencatat penggunaan listrik pribadi/kos dalam kWh untuk periode tertentu.
4. **Manajemen faktor emisi**: menyimpan faktor emisi, sumber, satuan, versi, dan masa berlaku.
5. **Perhitungan emisi CO2e**: menghitung emisi per aktivitas, kategori, dan periode.
6. **Dashboard dan tren**: menampilkan total emisi, sumber terbesar, serta grafik tren bulanan.
7. **Simulasi skenario**: membandingkan kondisi dasar dengan perubahan moda perjalanan atau konsumsi listrik.
8. **Rekomendasi aksi**: memberi saran perubahan kebiasaan berdasarkan estimasi pengurangan emisi.
9. **Penyimpanan dan riwayat**: menyimpan aktivitas, faktor emisi, skenario, dan hasil simulasi dalam PostgreSQL.

## Batasan Ruang Lingkup MVP

- Sasaran pengguna adalah **individu**, bukan organisasi atau rumah tangga dengan banyak penghuni.
- Kategori yang dihitung pada versi awal hanya mobilitas darat dan listrik pribadi/kos.
- Masukan perjalanan dilakukan manual; aplikasi tidak melacak GPS otomatis.
- Faktor emisi digunakan sebagai estimasi edukatif dan wajib mencantumkan sumber.
- Integrasi layanan cuaca hanya bersifat opsional sebagai konteks rekomendasi mobilitas; perhitungan inti tidak bergantung padanya.

## Aplikasi Sejenis dan Perbedaan

| Aplikasi sejenis | Fokus umum | Perbedaan VeloCarbon |
| --- | --- | --- |
| Carbon Footprint Calculator | Perhitungan total emisi berbasis kuesioner | VeloCarbon menyimpan aktivitas periodik, menampilkan tren, dan membandingkan skenario perubahan kebiasaan. |
| JouleBug | Pelacakan kebiasaan ramah lingkungan | VeloCarbon berfokus pada kalkulasi CO2e yang dapat ditelusuri melalui faktor emisi berversi. |
| Aplikasi pencatat pengeluaran/transportasi | Pencatatan biaya atau perjalanan | VeloCarbon mengubah aktivitas menjadi dampak emisi dan mengutamakan rekomendasi pengurangan emisi. |

## Arsitektur Ringkas

VeloCarbon menerapkan MVVM agar tampilan, logika aplikasi, dan data tidak saling bergantung secara langsung.

```text
WPF Views <-> ViewModels <-> Application Services <-> Repositories <-> PostgreSQL
                              |                    |
                              |                    +-> Emission Factor Data
                              +-> Scenario Engine
```

Model domain inti: `UserProfile`, `Vehicle`, `ActivityRecord`, `ElectricityUsage`, `EmissionFactor`, `EmissionCalculation`, `Scenario`, dan `ScenarioResult`. Rancangan lengkapnya tersedia di [PRD VeloCarbon](docs/PRD-VeloCarbon.md).

## Referensi Awal

1. Intergovernmental Panel on Climate Change (IPCC), *2006 IPCC Guidelines for National Greenhouse Gas Inventories* dan pembaruannya.
2. Kementerian Lingkungan Hidup dan Kehutanan Republik Indonesia, dokumen faktor emisi dan inventarisasi gas rumah kaca yang berlaku.
3. Kementerian Energi dan Sumber Daya Mineral Republik Indonesia / PT PLN (Persero), publikasi bauran dan faktor emisi kelistrikan.
4. NASA POWER API — https://power.larc.nasa.gov/docs/services/api/ (opsional untuk fitur konteks iklim).
5. BMKG Data Prakiraan Cuaca Terbuka — https://data.bmkg.go.id/prakiraan-cuaca/ (opsional untuk konteks cuaca).

## Menjalankan Demo

Instruksi instalasi dan menjalankan aplikasi akan ditambahkan setelah struktur proyek WPF tersedia. Saat demo, aplikasi harus dapat dijalankan dengan basis data lokal berisi data contoh serta tanpa ketergantungan layanan eksternal untuk perhitungan inti.

## Kontribusi dan Alur Git

1. Setiap anggota bekerja pada branch yang menggunakan NIU/NIM masing-masing.
2. Setiap perubahan dibuat melalui commit dengan pesan yang jelas.
3. Sebelum menggabungkan perubahan ke `main`, anggota membuat Pull Request untuk ditinjau anggota lain.
4. Branch anggota tidak dihapus setelah Pull Request digabungkan, sesuai ketentuan Modul 1.

Contoh branch Software Architect: `539398`.
