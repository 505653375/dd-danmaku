# Build script for Jellyfin.DD.Danmaku plugin
# Usage: .\build.ps1

param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Configuration=Release"
)

$ErrorActionPreference = "Stop"
$projectDir = $PSScriptRoot
$pluginProject = Join-Path $projectDir "Jellyfin.Plugin.DD.Danmaku"
$outputDir = Join-Path $projectDir "output"
$dllName = "Jellyfin.Plugin.DD.Danmaku.dll"

Write-Host "=== Building Jellyfin.DD.Danmaku v$Version ===" -ForegroundColor Cyan

# Clean output
if (Test-Path $outputDir) {
    Remove-Item $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

# Build
Write-Host "Building plugin..." -ForegroundColor Yellow
dotnet build $pluginProject -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Copy DLL
$dllSource = Join-Path $pluginProject "bin\Release\net8.0\$dllName"
$dllDest = Join-Path $outputDir $dllName
Copy-Item $dllSource $dllDest -Force

# Calculate MD5 checksum
$hash = Get-FileHash $dllDest -Algorithm MD5
$checksum = $hash.Hash.ToLower()
Write-Host "Checksum: $checksum" -ForegroundColor Green

# Update meta.json
$metaPath = Join-Path $projectDir "meta.json"
$meta = Get-Content $metaPath -Raw | ConvertFrom-Json
$meta.versions[0].version = $Version
$meta.versions[0].checksum = $checksum
$meta.versions[0].timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$meta.versions[0].sourceUrl = "https://github.com/505653375/dd-danmaku/releases/download/v$Version/$dllName"
$meta | ConvertTo-Json -Depth 10 | Set-Content $metaPath -Encoding UTF8

Write-Host "=== Build complete ===" -ForegroundColor Cyan
Write-Host "Output: $dllDest" -ForegroundColor White
Write-Host "Checksum: $checksum" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  git add ." -ForegroundColor White
Write-Host "  git commit -m 'Release v$Version'" -ForegroundColor White
Write-Host "  git tag v$Version" -ForegroundColor White
Write-Host "  git push && git push --tags" -ForegroundColor White
