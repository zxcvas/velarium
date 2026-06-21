# Velarium Quickstart

## First Time
1. Make sure .NET SDK is installed (dotnet --version)
2. Run the build:
   ```powershell
   .\build.ps1
   ```
3. Run the game:
   ```powershell
   .\run.ps1
   ```

## Manual
```powershell
dotnet build src/Velarium -c Release -o dist
dist\Velarium.exe
```

## What you will see
- Roman-themed ASCII banner
- Simple 4-choice menu (only stubs right now)
- Exit cleanly

This is intentionally minimal so we can practice the full agent-organized development cycle on real slices.
