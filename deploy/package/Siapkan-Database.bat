@echo off
chcp 65001 >nul
echo === VeloCarbon: siapkan database PostgreSQL ===
echo.
where docker >nul 2>nul
if %errorlevel%==0 (
  echo Docker ditemukan. Menjalankan PostgreSQL lewat Docker...
  docker compose -f "%~dp0Database\docker-compose.yml" up -d
  echo.
  echo Selesai. Database: localhost:5432, nama velocarbon, user velocarbon, password velocarbon.
  goto :end
)
where psql >nul 2>nul
if %errorlevel%==0 (
  echo PostgreSQL ditemukan. Membuat role dan database velocarbon...
  echo Masukkan password user "postgres" bila diminta.
  psql -U postgres -f "%~dp0Database\setup-database.sql"
  goto :end
)
echo Docker maupun PostgreSQL belum terpasang.
echo Pasang salah satunya, lalu jalankan ulang file ini ^(lihat INSTALASI.md^):
echo   - PostgreSQL : https://www.postgresql.org/download/windows/
echo   - Docker     : https://www.docker.com/products/docker-desktop/
:end
echo.
pause
