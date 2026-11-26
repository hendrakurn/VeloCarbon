# Instalasi & Konfigurasi VeloCarbon (MVP)

Target: **Windows 10/11 64-bit**. Aplikasi sudah *self-contained* — **tidak perlu memasang .NET**. Yang dibutuhkan hanya basis data PostgreSQL.

## Isi paket

```
VeloCarbon-MVP/
├─ App/VeloCarbon.exe            aplikasi desktop (WPF)
├─ Api/VeloCarbon.Api.exe        layanan Web API (opsional, nilai tambah)
├─ Database/                     docker-compose.yml + setup-database.sql
├─ Dokumen/                      panduan pengguna, API, dan hasil pengujian
├─ Siapkan-Database.bat          menyiapkan PostgreSQL (Docker atau PostgreSQL terpasang)
├─ Jalankan-VeloCarbon.bat       membuka aplikasi
└─ Jalankan-API.bat              menjalankan API + Swagger
```

## Langkah cepat (3 langkah)

1. **Ekstrak** `VeloCarbon-MVP.zip` ke folder mana saja.
2. **Siapkan database** — klik dua kali `Siapkan-Database.bat`. Skrip memilih otomatis:
   - jika **Docker Desktop** ada → menjalankan PostgreSQL 16 di container, atau
   - jika **PostgreSQL** sudah terpasang → membuat role & database `velocarbon` (minta password user `postgres`).
3. **Jalankan** `Jalankan-VeloCarbon.bat`.

Saat pertama dijalankan aplikasi **membuat tabel sendiri (migrasi), mengisi faktor emisi, dan memuat data contoh** (profil "Mahasiswa Demo"). Tidak ada skrip SQL skema yang perlu dijalankan manual.

## Opsi database

| Cara | Perintah / langkah |
| --- | --- |
| Docker | `docker compose -f Database/docker-compose.yml up -d` |
| PostgreSQL terpasang | `psql -U postgres -f Database/setup-database.sql` |
| Manual (pgAdmin) | buat role `velocarbon` / password `velocarbon` dan database `velocarbon` (owner `velocarbon`) |

## Konfigurasi

Pengaturan ada di `App/appsettings.json` (dan `Api/appsettings.json`):

```json
{
  "ConnectionStrings": {
    "VeloCarbon": "Host=localhost;Port=5432;Database=velocarbon;Username=velocarbon;Password=velocarbon"
  },
  "Seed": { "DemoData": true }
}
```

- Ubah `Host`/`Port`/`Username`/`Password` bila PostgreSQL Anda berbeda.
- `Seed:DemoData` — `true` memuat data contoh pada database kosong; set `false` untuk database bersih.
- Agar pengaturan tidak tertimpa saat memperbarui aplikasi, buat `appsettings.local.json` di samping `VeloCarbon.exe` (hanya berisi bagian yang ingin diganti).
- API: `Urls` (default `http://localhost:5080`) dan `Api:ApiKey` (jika diisi, semua endpoint `/api/*` kecuali `/api/health` wajib header `X-Api-Key`).

## Pemecahan masalah

| Gejala | Penyebab & solusi |
| --- | --- |
| Pesan "tidak dapat terhubung ke basis data" saat membuka aplikasi | PostgreSQL belum berjalan, atau connection string salah. Jalankan `Siapkan-Database.bat` / periksa `appsettings.json`. |
| `password authentication failed` | Username/password di `appsettings.json` tidak sama dengan yang dibuat di PostgreSQL. |
| `database "velocarbon" does not exist` | Jalankan `Siapkan-Database.bat` (atau `Database/setup-database.sql`). |
| Port 5432 sudah dipakai | Hentikan PostgreSQL lain, atau ubah port di `docker-compose.yml` **dan** `Port=` di connection string. |
| Windows SmartScreen memperingatkan | Aplikasi tidak bertanda tangan digital. Pilih *More info → Run anyway*. |
| Port 5080 dipakai (API) | Ubah `Urls` di `Api/appsettings.json`. |

## Untuk pengembang (build dari kode sumber)

Prasyarat: .NET SDK 8, PostgreSQL (atau Docker).

```bash
docker compose -f deploy/docker-compose.yml up -d     # database
dotnet test                                             # 44 unit test
VELOCARBON_TEST_DB="Host=localhost;Username=velocarbon;Password=velocarbon;Database=postgres" dotnet test   # + 4 tes integrasi PostgreSQL
dotnet run --project src/VeloCarbon.Api                 # API  (Swagger: http://localhost:5080/swagger)
dotnet run --project src/VeloCarbon.App                 # WPF  (khusus Windows)
.\deploy\build-release.ps1                              # paket dist\VeloCarbon-MVP.zip (PowerShell Windows)
./deploy/build-release.sh                               # paket dist/VeloCarbon-MVP.zip (Git Bash / Linux)
```

Migrasi baru: `dotnet ef migrations add <Nama> -p src/VeloCarbon.Infrastructure -o Persistence/Migrations`.
