<#
  run.ps1
  Launch the current Velarium build.
#>

Set-Location $PSScriptRoot

$exe = "dist\Velarium.exe"

if (-not (Test-Path $exe)) {
    Write-Host "No executable found. Building first..." -ForegroundColor Yellow
    & "$PSScriptRoot\build.ps1"
}

Write-Host "Launching Velarium..." -ForegroundColor Cyan
& $exe
