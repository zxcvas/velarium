# Amphiteater — Godot courtyard (not runnable on this SDK yet)

Playtest **holds**. The courtyard should be Godot 4 C# on `Amphiteater.Sim` (`docs/design/01_expansion.md`).

This machine’s `dotnet` is **10.0 only** (`dotnet new -f net8.0` is rejected). Godot 4.6 C# still targets **net8.0**. Do not fake a Godot project that cannot restore.

## When you sit down for the courtyard

1. Install .NET 8 targeting pack / SDK 8 (`winget install Microsoft.DotNet.SDK.8`).
2. Unzip `Godot_v4.6.3-stable_mono_win64.zip` (already in Downloads) to `tools/godot-editor/` (gitignored).
3. Multi-target `Amphiteater.Sim` to `net8.0;net10.0`. Console stays net10.
4. `godot/` project: nearest-neighbor, integer scale 4×, `sample_dirt` fill, wave-1 props, murmillo south idle. Same JSON save. `.\build.ps1` must still pass.

Until then the playable host is `.\run.ps1` (console). Arrow keys now work in menus.
