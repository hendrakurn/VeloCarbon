# VeloCarbon

**VeloCarbon** adalah aplikasi desktop berbasis **C# WPF** dengan arsitektur **MVVM** untuk membantu individu memahami, memantau, dan mengurangi jejak karbon dari mobilitas harian serta konsumsi listrik pribadi atau tempat tinggal (kos).

> VeloCarbon adalah alat edukasi berbasis estimasi; hasilnya bukan inventarisasi emisi resmi, audit karbon, atau dasar klaim sertifikasi.

## Informasi Proyek

| Elemen | Keterangan |
| --- | --- |
| Tema | Climate Action |
| Kategori | Pelacakan jejak karbon pribadi dan rekomendasi perubahan kebiasaan |
| Tipe aplikasi | Aplikasi desktop Windows |
| Teknologi utama | C#, .NET 8, WPF, MVVM, PostgreSQL, Entity Framework Core |
| Nama repository | `VeloCarbon` |

## Kelompok dan Tanggung Jawab

| Anggota | NIM/NIU | Peran | Tanggung jawab utama |
| --- | --- | --- | --- |
| Rafif Raihan Bahrul Alam | 24/534432/TK/59237 | Ketua Kelompok dan Frontend Developer | Antarmuka WPF, ViewModel, visualisasi tren dan hasil simulasi, pengalaman pengguna, serta dokumentasi penggunaan. |
| Faaid Sakhaa | 24/539398/TK/59820 | Software Architect | Arsitektur MVVM, model PBO, skema basis data dan ERD, kontrak antarmodul, standar Git, serta integrasi keseluruhan sistem. |
| Hendra Kurnia Maliqi | 24/542344/TK/60216 | Backend Developer | eksekusi ERD, develop API, menyusun method method API, integrasi backend frontend |

## Latar Belakang dan Permasalahan

Aktivitas sehari-hari seperti perjalanan menggunakan kendaraan pribadi dan penggunaan listrik menghasilkan emisi gas rumah kaca. Namun, pengguna sering tidak mengetahui sumber emisi terbesarnya maupun dampak dari perubahan kebiasaan sederhana. Informasi emisi tersebar dalam satuan yang sulit dibandingkan—liter bahan bakar, kilometer perjalanan, dan kWh listrik—sehingga pengguna tidak mempunyai dasar yang jelas untuk memilih aksi pengurangan emisi yang paling berdampak.

Kalkulator emisi sederhana umumnya hanya menampilkan total emisi sekali hitung. Pendekatan ini belum membantu pengguna menyimpan riwayat aktivitas, melihat tren, membandingkan skenario, dan memilih perubahan yang realistis.

## Solusi yang Diusulkan

VeloCarbon menyatukan catatan mobilitas dan konsumsi listrik dalam satu profil emisi pribadi. MVP mendukung empat metode input: perjalanan berdasarkan moda, jarak, dan frekuensi; pembelian bahan bakar dalam liter; total kWh meter; serta estimasi listrik berdasarkan daya, jumlah alat, dan lama pemakaian. Aplikasi menghitung estimasi emisi CO2e menggunakan faktor emisi yang memiliki sumber dan versi, menyimpan riwayat, lalu menjalankan simulasi perubahan kebiasaan.

Contoh simulasi: pengguna membandingkan kondisi saat ini dengan pengurangan jarak perjalanan, liter pembelian bahan bakar, total kWh, atau jam pemakaian alat sebesar 10%. Aplikasi menampilkan emisi kondisi awal, emisi skenario, nilai dan persentase pengurangan pada bulan baseline, serta rekomendasi yang diprioritaskan berdasarkan estimasi dampaknya.

```text
Emisi aktivitas (kgCO2e) = nilai aktivitas x faktor emisi (kgCO2e per satuan)
Total emisi periode = jumlah emisi seluruh aktivitas pada periode tersebut
Pengurangan skenario = emisi kondisi awal - emisi skenario
```

Satuan aktivitas disesuaikan dengan kategorinya, misalnya kilometer untuk perjalanan atau kWh untuk listrik. Faktor emisi disimpan di basis data bersama sumber, satuan, versi, dan tanggal berlaku agar perhitungan dapat ditelusuri dan diperbarui.

## Fitur Utama

1. **Profil emisi pribadi**: menyimpan profil pengguna, kendaraan, dan preferensi satuan.
2. **Pencatatan aktivitas mobilitas**: mencatat perjalanan berdasarkan jarak atau pembelian bahan bakar berdasarkan kendaraan, tanggal, jenis bahan bakar, dan liter; biaya bersifat opsional.
3. **Pencatatan konsumsi listrik**: memasukkan total kWh meter atau menghitung estimasi dari nama alat, watt, jumlah perangkat, jam per hari, serta hari pemakaian.
4. **Manajemen faktor emisi**: menyimpan faktor emisi, sumber, satuan, versi, dan masa berlaku.
5. **Perhitungan emisi CO2e**: menghitung emisi per aktivitas, kategori, dan periode.
6. **Dashboard dan tren**: menampilkan total emisi, sumber terbesar, serta grafik tren bulanan.
7. **Simulasi skenario**: membandingkan kondisi dasar dengan pengurangan jarak/frekuensi, liter bahan bakar, total kWh, atau jam pemakaian alat.
8. **Rekomendasi aksi**: memberi saran perubahan kebiasaan berdasarkan estimasi pengurangan emisi.
9. **Penyimpanan dan riwayat**: menyimpan aktivitas, faktor emisi, skenario, dan hasil simulasi dalam PostgreSQL.

## Batasan Ruang Lingkup MVP

- Sasaran pengguna adalah **individu**, bukan organisasi atau rumah tangga dengan banyak penghuni.
- Kategori yang dihitung pada versi awal hanya mobilitas darat dan listrik pribadi/kos.
- Masukan perjalanan dilakukan manual; aplikasi tidak melacak GPS otomatis.
- Faktor emisi digunakan sebagai estimasi edukatif dan wajib mencantumkan sumber.
- Pembelian bahan bakar merupakan pendekatan estimasi, bukan pengukuran bahan bakar yang terbakar pada bulan tersebut. Estimasi alat mengasumsikan daya rata-rata selama jam pemakaian.
- Laporan dan baseline memakai bulan kalender. Satu kendaraan memakai satu metode mobilitas per bulan; satu profil memakai satu metode listrik per bulan agar sumber yang sama tidak dihitung dua kali.
- Perpindahan moda otomatis, proyeksi tahunan, dan integrasi cuaca belum termasuk MVP.

## Aplikasi Sejenis dan Perbedaan

| Aplikasi sejenis | Fokus umum | Perbedaan VeloCarbon |
| --- | --- | --- |
| [CarbonFootprint.com Calculator](https://www.carbonfootprint.com/calculator.aspx) | Perhitungan emisi rumah, perjalanan, dan gaya hidup berbasis kuesioner | VeloCarbon menyimpan aktivitas periodik, menampilkan tren, dan membandingkan skenario perubahan kebiasaan. |
| [JouleBug](https://joulebug.com/) | Pelacakan aksi dan tantangan kebiasaan berkelanjutan | VeloCarbon berfokus pada kalkulasi CO2e pribadi yang dapat ditelusuri melalui faktor emisi berversi. |
| [Giki Zero](https://giki.earth/giki-zero-your-step-by-step-guide-to-a-sustainable-life/) | Estimasi jejak karbon pribadi dan saran langkah pengurangan | VeloCarbon membatasi MVP pada mobilitas darat dan listrik, menyimpan riwayat aktivitas, serta menyediakan simulasi skenario lokal. |

## Arsitektur Ringkas

VeloCarbon menerapkan MVVM agar tampilan, logika aplikasi, dan data tidak saling bergantung secara langsung.

```text
WPF Views <-> ViewModels <-> Application Services <-> Repositories <-> PostgreSQL
                              |                    |
                              |                    +-> Emission Factor Data
                              +-> Scenario Engine
```

Model domain inti: `UserProfile`, `Vehicle`, `VehicleMonthlyInput`, `ActivityRecord`, `MobilityActivity`, `FuelPurchaseActivity`, `ElectricityUsage`, `ApplianceUsage`, `EmissionFactor`, `EmissionCalculation`, `Scenario`, `ScenarioChange`, `ScenarioResult`, dan `Recommendation`.

## Referensi Awal

1. Intergovernmental Panel on Climate Change (IPCC), [*2006 IPCC Guidelines for National Greenhouse Gas Inventories*](https://www.ipcc-nggip.iges.or.jp/public/2006gl/) dan pembaruannya.
2. Kementerian Lingkungan Hidup dan Kehutanan Republik Indonesia, dokumen faktor emisi dan inventarisasi gas rumah kaca yang berlaku.
3. Kementerian Energi dan Sumber Daya Mineral Republik Indonesia / PT PLN (Persero), publikasi bauran dan faktor emisi kelistrikan.

## Menjalankan Demo

Instruksi instalasi dan menjalankan aplikasi akan ditambahkan setelah struktur proyek WPF tersedia. Saat demo, aplikasi harus dapat dijalankan dengan basis data lokal berisi data contoh serta tanpa ketergantungan layanan eksternal untuk perhitungan inti.

## Kontribusi dan Alur Git

1. Setiap anggota bekerja pada branch yang menggunakan NIU/NIM masing-masing.
2. Setiap perubahan dibuat melalui commit dengan pesan yang jelas.
3. Sebelum menggabungkan perubahan ke `main`, anggota membuat Pull Request untuk ditinjau anggota lain.
4. Branch anggota tidak dihapus setelah Pull Request digabungkan, sesuai ketentuan Modul 1.

Contoh branch Software Architect: `539398`.
