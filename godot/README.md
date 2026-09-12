# Amphiteater — Godot courtyard

Playtest **holds**. The courtyard is Godot 4.6 C# on `Amphiteater.Sim` (`docs/design/01_expansion.md`). The playable host stays `.\run.ps1` (console). This folder is presentation: same JSON save, no new sim verbs.

## What this claim unblocks

| Was blocked | Now |
|---|---|
| Sim was `net10.0` only | `Amphiteater.Sim` multi-targets `net8.0;net10.0`. Console (`src/Velarium`) stays `net10.0`. |
| Agent / cloud SDK was net10-only (`dotnet new -f net8.0` rejected) | `.cursor/Dockerfile` installs **SDK 8 and SDK 10** side by side. |
| `godot/` was a README only | Restorable C# project: `Amphiteater.Godot.csproj` (`Godot.NET.Sdk/4.6.3`, `net8.0`) + `project.godot` + courtyard scene. |

`dotnet restore godot/Amphiteater.Godot.csproj` does **not** need the editor. Running the courtyard does.

## Restore (no editor)

From the repo root, after .NET 8 + 10 SDKs are on `PATH`:

```powershell
dotnet restore Amphiteater.slnx
dotnet restore godot/Amphiteater.Godot.csproj
dotnet build src/Velarium -c Release -o dist
dotnet test src/Amphiteater.Sim.Tests --nologo
```

Or `.\build.ps1` for the console host + tests. That path must stay green.

## Run the courtyard (editor)

The mono zip is **not** committed (large; gitignored). CI / cloud agents do not unzip it.

1. SDKs: `winget install Microsoft.DotNet.SDK.8` and `Microsoft.DotNet.SDK.10` (or the Dockerfile). Confirm `dotnet new console -f net8.0` works.
2. Unzip `Godot_v4.6.3-stable_mono_win64.zip` (Windows Downloads) — or the 4.6.3 mono build for your OS — to `tools/godot-editor/` (gitignored).
3. Open `godot/project.godot` with that editor. First open imports C#; build from the editor if asked.

## Courtyard contract (do not invent systems)

Locked in `project.godot` / `Courtyard.cs`:

- Nearest-neighbor (`default_texture_filter=0`)
- Integer scale **4×** (`stretch/scale=4`, `scale_mode=integer`, 320×180 viewport)
- Yard fill: `assets/art/tiles/sample_dirt.png` when present; STYLE packed-dirt swatch otherwise
- Wave-1 props from `assets/art/props/` when present (`prop_palus`, `prop_cellae`, `prop_hearth`, `prop_porta`, `prop_medicus`). Unbuilt rooms (medicus starts at 0) stay off the yard.
- Murmillo south idle when `characters/murmillo_s_idle_00.png` or `sample_murmillo_s.png` exists **and** the loaded familia has a living murmillo
- Same save: `amphiteater_save.json` (`Save.FileName`). Prefers `dist/` next to the console exe, then Godot `user://`, then `Save.DefaultPath()`.
- **End Day** calls `Ludus.EndDay` and `Save.Write`. No new rooms, combat, or economy.

PixelLab PNGs live under `assets/art/` (see `ASSET_MAP.md`). They are not required to restore. To let the editor import them as `res://art/...`, junction or copy `assets/art` → `godot/art`. Runtime also loads `../assets/art` from disk.

## Layout

```
godot/Amphiteater.Godot.csproj   # net8.0, Godot.NET.Sdk/4.6.3, refs Sim
godot/Amphiteater.Godot.slnx     # Sim + this project
godot/project.godot
godot/Courtyard.cs
godot/PixelArt.cs
godot/scenes/courtyard.tscn
```

Root `Amphiteater.slnx` stays the console solution (Sim, Velarium, tests) so `dotnet build Amphiteater.slnx` does not require the Godot SDK.
