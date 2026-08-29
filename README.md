# Amphiteater

A game of Roman spectacle, strategy, and survival. You are a *lanista* in Capua: you own a gladiator school, not (yet) the games.

## Current Status
- **M1 ludus slice is playable**; rules live in `Amphiteater.Sim`
- C# console, .NET 10
- Day loop: familia, drill, forum, rental fights, locked hosting
- Save / load (`amphiteater_save.json` beside the exe)
- Headless: `.\dist\Amphiteater.exe --report 200`

## Quick Start

```powershell
# Build
.\build.ps1

# Run
.\run.ps1
# or directly
.\dist\Amphiteater.exe
```

## What you are
A lanista. Infamis. You lease men to aediles and rich *editores* for a little coin if they walk out, and a lot if they do not. When the ludus has *fama* and a man with a *palma*, the duumviri may let you stage a munus of your own.

See:
- PROJECT_PLAN.md
- docs/design/00_overview_stub.md
- production/task_board.md
- QUICKSTART.md

Vale.
