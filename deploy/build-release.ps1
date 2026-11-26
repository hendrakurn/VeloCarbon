# Membuat paket rilis Windows x64 (self-contained: pengguna TIDAK perlu memasang .NET).
# Pemakaian (PowerShell, butuh .NET SDK 8):  .\deploy\build-release.ps1
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

$out = 'dist\VeloCarbon-MVP'
$zip = 'dist\VeloCarbon-MVP.zip'
$pub = @(
    '-c', 'Release',
    '-r', 'win-x64',
    '--self-contained', 'true',
    '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true',
    '-p:EnableCompressionInSingleFile=true',
    '-p:DebugType=None',
    '-p:DebugSymbols=false'
)

if (Test-Path $out) { Remove-Item $out -Recurse -Force }
if (Test-Path $zip) { Remove-Item $zip -Force }

foreach ($project in @('App', 'Api')) {
    Write-Host "==> Publish VeloCarbon.$project (win-x64 self-contained)"
    dotnet publish "src\VeloCarbon.$project" @pub -o "$out\$project"
    if ($LASTEXITCODE -ne 0) {
        throw "Publish VeloCarbon.$project gagal dengan kode keluar $LASTEXITCODE."
    }
}

Remove-Item "$out\Api\web.config" -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path "$out\Database", "$out\Dokumen" -Force | Out-Null

Copy-Item 'deploy\package\*.bat' "$out\"
Copy-Item 'deploy\package\setup-database.sql', 'deploy\docker-compose.yml' "$out\Database\"

foreach ($doc in @('docs\INSTALASI.md', 'docs\PANDUAN-PENGGUNA.md', 'docs\API.md', 'docs\PENGUJIAN-BlackBox.md', 'docs\PENGUJIAN-BlackBox.docx', 'docs\KEPUTUSAN-BASELINE.md')) {
    if (Test-Path $doc) {
        Copy-Item $doc "$out\Dokumen\"
    }
}
Copy-Item 'docs\INSTALASI.md' "$out\INSTALASI.md"

Compress-Archive -Path $out -DestinationPath $zip -Force
$hash = (Get-FileHash -Path $zip -Algorithm SHA256).Hash
Set-Content -Path "$zip.sha256" -Value "$hash  VeloCarbon-MVP.zip" -Encoding Ascii

Write-Host "==> Paket selesai: $zip"
Write-Host "==> SHA256: $hash"
