#!/usr/bin/env bash
# Membuat paket rilis Windows x64 (self-contained: pengguna TIDAK perlu memasang .NET).
# Pemakaian:  ./deploy/build-release.sh        (butuh .NET SDK 8)
set -euo pipefail
cd "$(dirname "$0")/.."
OUT=dist/VeloCarbon-MVP
rm -rf "$OUT"
for p in App Api; do
  dotnet publish "src/VeloCarbon.$p" -c Release -r win-x64 --self-contained true \
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None \
    -o "$OUT/$p"
done
rm -f "$OUT/Api/web.config"
mkdir -p "$OUT/Database" "$OUT/Dokumen"
cp deploy/package/*.bat "$OUT/"
cp deploy/package/setup-database.sql deploy/docker-compose.yml "$OUT/Database/"
cp docs/INSTALASI.md docs/PANDUAN-PENGGUNA.md docs/API.md docs/PENGUJIAN-BlackBox.md "$OUT/Dokumen/" 2>/dev/null || true
cp docs/PENGUJIAN-BlackBox.docx "$OUT/Dokumen/" 2>/dev/null || true
cp docs/INSTALASI.md "$OUT/INSTALASI.md"
(cd dist && rm -f VeloCarbon-MVP.zip && python3 -c "import shutil;shutil.make_archive('VeloCarbon-MVP','zip','.','VeloCarbon-MVP')")
echo "Paket: dist/VeloCarbon-MVP.zip"
