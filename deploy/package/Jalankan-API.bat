@echo off
chcp 65001 >nul
cd /d "%~dp0Api"
echo VeloCarbon API berjalan di http://localhost:5080  (dokumentasi interaktif: http://localhost:5080/swagger)
echo Tutup jendela ini untuk menghentikan API.
start "" http://localhost:5080/swagger
VeloCarbon.Api.exe
pause
