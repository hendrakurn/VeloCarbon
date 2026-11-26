# VeloCarbon Web API

Base URL default: `http://localhost:5080` — dokumentasi interaktif (Swagger UI): `/swagger`.
Format JSON, enum berupa teks. Kesalahan aturan bisnis mengembalikan **HTTP 400** `{ "error": "pesan" }`.
Jika `Api:ApiKey` diisi di `appsettings.json`, semua `/api/*` (kecuali `/api/health`) wajib header `X-Api-Key`.

| Metode | Endpoint | Fungsi |
| --- | --- | --- |
| GET | `/api/health` | Cek layanan |
| POST | `/api/estimate` | **Kalkulator cepat** emisi sekali pakai (tidak disimpan) |
| GET | `/api/emission-factors` | Daftar faktor emisi (sumber, satuan, versi) |
| POST | `/api/emission-factors` | Tambah **versi baru** faktor (versi lama ditutup, tidak ditimpa) |
| GET/POST | `/api/profiles` | Daftar / buat profil |
| GET/PUT | `/api/profiles/{id}` | Detail / ubah nama profil |
| POST | `/api/profiles/{id}/vehicles` | Tambah kendaraan |
| PUT/DELETE | `/api/profiles/{id}/vehicles/{vehicleId}` | Ubah / hapus kendaraan (ditolak bila masih dipakai aktivitas) |
| GET | `/api/profiles/{id}/activities?month=yyyy-MM` | Aktivitas satu bulan beserta emisi |
| POST | `/api/activities/distance` | Catat perjalanan (jarak) |
| POST | `/api/activities/fuel` | Catat pembelian bahan bakar |
| POST | `/api/activities/electricity/meter` | Catat listrik total kWh |
| POST | `/api/activities/electricity/appliances` | Catat listrik estimasi alat |
| DELETE | `/api/activities/{id}` | Hapus aktivitas |
| GET | `/api/profiles/{id}/dashboard?month=yyyy-MM` | Total, kontribusi kategori, sumber terbesar |
| GET | `/api/profiles/{id}/trend?endMonth=yyyy-MM&months=6` | Tren bulanan |
| GET | `/api/profiles/{id}/report?month=yyyy-MM` | Laporan gabungan (ringkasan + tren + rincian) |
| POST | `/api/scenarios` | Jalankan & simpan skenario (+ rekomendasi) |
| GET | `/api/profiles/{id}/scenarios` | Skenario tersimpan |

## Contoh

```bash
# Kalkulator cepat: mobil 10 km x 2 perjalanan
curl -X POST http://localhost:5080/api/estimate -H "Content-Type: application/json" \
  -d '{"kind":"Distance","value":10,"mode":"Car","frequency":2}'
# -> {"activityValue":20,"unit":"Km","factor":0.192,...,"kgCO2e":3.84,...}

# Catat listrik 50 kWh (Oktober 2026)
curl -X POST http://localhost:5080/api/activities/electricity/meter -H "Content-Type: application/json" \
  -d '{"profileId":"<id-profil>","month":"2026-10-01","usageKWh":50}'

# Simulasi: kurangi jarak 10%
curl -X POST http://localhost:5080/api/scenarios -H "Content-Type: application/json" \
  -d '{"profileId":"<id>","name":"Hemat 10%","month":"2026-10-01",
       "changes":[{"changeType":"ReduceDistance","targetActivityRecordId":"<id-aktivitas>","reductionPercent":10}]}'
```

`kind` pada `/api/estimate`: `Distance` (wajib `mode`, opsional `frequency`), `Fuel` (wajib `fuelType`), `Electricity`.
`changeType`: `ReduceDistance`, `ReduceLiters`, `ReduceKWh`, `ReduceApplianceHours` (wajib `targetApplianceUsageId`).
