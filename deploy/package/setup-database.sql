-- Menyiapkan role dan database VeloCarbon pada PostgreSQL yang sudah terpasang.
-- Jalankan sebagai superuser:   psql -U postgres -f setup-database.sql
-- Tabel TIDAK perlu dibuat manual: aplikasi menerapkan migrasi & mengisi faktor emisi saat pertama dijalankan.
DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'velocarbon') THEN
    CREATE ROLE velocarbon LOGIN PASSWORD 'velocarbon';
  END IF;
END $$;

SELECT 'CREATE DATABASE velocarbon OWNER velocarbon'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'velocarbon')\gexec
