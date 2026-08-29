<#
  build.ps1
  Build Amphiteater.
#>

param(
    [string]$Configuration = "Release",
    [string]$Output = "dist"
)

Set-Location $PSScriptRoot

Write-Host "=== Building Amphiteater ($Configuration) ===" -ForegroundColor Cyan

dotnet build src/Velarium -c $Configuration -o $Output
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

Write-Host "=== Tests ===" -ForegroundColor Cyan
dotnet test src/Amphiteater.Sim.Tests --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Error "Tests failed"
    exit 1
}

Write-Host "Build succeeded. Executable should be at $Output\Amphiteater.exe" -ForegroundColor Green
Get-ChildItem $Output -Filter Amphiteater* | Format-Table Name, Length
