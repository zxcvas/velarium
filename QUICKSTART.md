# Amphiteater Quickstart

## First Time
1. Make sure the .NET SDK is installed (`dotnet --version`)
2. Build:
   ```powershell
   .\build.ps1
   ```
3. Run:
   ```powershell
   .\run.ps1
   ```

## Manual
```powershell
dotnet build src/Velarium -c Release -o dist
dist\Amphiteater.exe
```

## What you will see
- Title, then **Novum ludum**
- Pick a Roman name (or compose *praenomen + nomen + cognomen*)
- A ludus in Capua, Kalends of May, a.u.c. DCCLXXXII
- Three *tiros*: murmillo, thraex, retiarius
- A day menu: familia, exercitia, forum, locatio, edere munus (locked), end day

Day one an editor will usually be at the gate. Send the type he asked for.

Saves write to `dist/amphiteater_save.json` (ignored by git). Continue from the title screen.

## Keys
Menus are numbers. `0` goes back. `s` / `n` confirm. Enter pages.
