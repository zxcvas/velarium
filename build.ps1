<#
  build.ps1
  Build the Velarium placeholder (and later the full game).
#>

param(
    [string]$Configuration = "Release",
    [string]$Output = "dist"
)

Set-Location $PSScriptRoot

Write-Host "=== Building Velarium ($Configuration) ===" -ForegroundColor Cyan

dotnet build src/Velarium -c $Configuration -o $Output

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build succeeded. Executable should be at $Output\Velarium.exe" -ForegroundColor Green
    Get-ChildItem $Output -Filter Velarium* | Format-Table Name, Length
} else {
    Write-Error "Build failed"
    exit 1
}
