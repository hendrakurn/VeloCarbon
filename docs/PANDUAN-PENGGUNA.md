# Panduan Pengguna VeloCarbon

VeloCarbon membantu Anda memahami jejak karbon dari **perjalanan** dan **listrik**, lalu mencoba perubahan kebiasaan lewat **simulasi**. Semua angka adalah **estimasi edukatif**, bukan inventarisasi emisi resmi.

> Instalasi: lihat `INSTALASI.md`. Setelah database siap, buka `Jalankan-VeloCarbon.bat`.

## Mengenal layar

Bagian atas selalu menampilkan **Profil** dan **Bulan** yang sedang dilihat. Semua tab mengikuti dua pilihan ini. Bagian bawah menampilkan pesan status (hijau = berhasil, merah = ada yang perlu diperbaiki).

| Tab | Fungsi |
| --- | --- |
| Dashboard | Total emisi bulan terpilih, sumber terbesar, kontribusi per kategori, tren 6 bulan |
| Input Aktivitas | Empat cara mencatat: perjalanan (jarak), bahan bakar (liter), listrik (kWh meter), listrik (estimasi alat) |
| Riwayat | Daftar catatan bulan terpilih beserta emisi dan faktor yang dipakai; hapus catatan |
| Simulasi | Bandingkan kondisi awal dengan skenario pengurangan; lihat rekomendasi |
| Faktor Emisi | Faktor yang dipakai: nilai, satuan, sumber, versi, masa berlaku |
| Profil & Kendaraan | Buat profil dan tambah kendaraan pribadi |

## Mulai cepat (5 menit)

1. **Profil & Kendaraan** → isi nama, *Buat profil*. Tambah kendaraan (mis. "Motor Harian", tipe Motorcycle, bahan bakar Petrol).
2. Pilih **Bulan** di bagian atas.
3. **Input Aktivitas** → *Perjalanan (jarak)*: pilih moda dan kendaraan, isi tanggal, jarak satu arah (km), dan frekuensi → *Simpan*. Emisi langsung dihitung.
4. **Input Aktivitas** → *Listrik (total kWh)* atau *Listrik (estimasi alat)*.
5. Buka **Dashboard** untuk melihat hasil, lalu **Simulasi** untuk mencoba pengurangan.

> Data contoh ("Mahasiswa Demo") sudah tersedia agar Anda bisa langsung menjelajah.

## Empat cara input

| Cara | Isi | Rumus |
| --- | --- | --- |
| Perjalanan (jarak) | Moda, kendaraan (untuk Car/Motorcycle), tanggal, km, frekuensi | km × frekuensi × faktor (kgCO2e/km) |
| Bahan bakar | Kendaraan, tanggal beli, liter, biaya (opsional) | liter × faktor (kgCO2e/liter) |
| Listrik (kWh) | Total kWh meter satu bulan | kWh × faktor (kgCO2e/kWh) |
| Listrik (alat) | Nama, watt, jumlah, jam/hari, hari pakai (satu baris per alat) | Σ(watt/1000 × jumlah × jam × hari) × faktor |

Angka boleh memakai koma atau titik desimal (`12,5` atau `12.5`).

## Aturan agar tidak terhitung ganda

- **Satu kendaraan, satu metode per bulan**: pakai *jarak* **atau** *bahan bakar*, tidak keduanya. Kendaraan atau bulan lain boleh memakai metode berbeda.
- **Listrik hanya satu catatan per bulan** (meter **atau** estimasi alat).
- Untuk mengganti metode atau memperbaiki data: **Riwayat → pilih baris → Hapus terpilih**, lalu input ulang. Aplikasi akan menolak input yang bentrok dan menjelaskan alasannya.
- Jam per hari harus 0–24, hari pakai tidak boleh melebihi jumlah hari dalam bulan; jarak, liter, watt, dan kWh harus lebih dari 0.

## Simulasi

1. Pilih bulan yang sudah berisi aktivitas.
2. Di tab **Simulasi**, pilih **target** (mis. "Mobilitas - Motorcycle: jarak/frekuensi" atau "Listrik: jam pakai Kipas Angin"), isi **persentase pengurangan** (mis. 10), klik **+ Tambah perubahan**. Ulangi untuk beberapa target.
3. Beri nama skenario → **Jalankan simulasi**.
4. Hasil: emisi kondisi awal, emisi dengan skenario, pengurangan (kgCO2e dan %), serta **rekomendasi** yang diurutkan dari dampak terbesar. Skenario tersimpan di daftar bawah.

Simulasi **tidak mengubah** data aktivitas Anda. Jika emisi awal 0, persentase ditampilkan "tidak tersedia".

## Membaca faktor emisi

Tab **Faktor Emisi** menampilkan setiap faktor beserta **sumber**, **satuan**, **versi**, dan **masa berlaku**. Riwayat menampilkan faktor mana yang dipakai pada tiap catatan, sehingga hasil dapat ditelusuri. Faktor lama tidak pernah ditimpa; pembaruan membuat versi baru (lewat API `POST /api/emission-factors`).

## Layanan Web API (opsional)

Jalankan `Jalankan-API.bat`; dokumentasi interaktif terbuka di `http://localhost:5080/swagger`. Berguna untuk kalkulator cepat (`POST /api/estimate`) dan integrasi aplikasi lain. Lihat `API.md`.
