<#
  run.ps1
  Launch the current Amphiteater build.
#>

Set-Location $PSScriptRoot

$exe = "dist\Amphiteater.exe"

if (-not (Test-Path $exe)) {
    Write-Host "No executable found. Building first..." -ForegroundColor Yellow
    & "$PSScriptRoot\build.ps1"
}

Write-Host "Launching Amphiteater..." -ForegroundColor Cyan
& $exe
