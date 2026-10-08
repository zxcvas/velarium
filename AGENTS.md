# Agents

This file is a pointer, not a second process. Follow the in-repo cycle.

## Start
1. Read [`production/task_board.md`](production/task_board.md) first — **Now / Next / Blocked** is the source of truth. Do not treat [`TASK_SCHEDULE.md`](TASK_SCHEDULE.md) as the queue.
2. Follow [`DEVELOPMENT_CYCLE.md`](DEVELOPMENT_CYCLE.md) and the role contracts: [`agents/ORGANIZER.md`](agents/ORGANIZER.md), [`agents/subagents/README.md`](agents/subagents/README.md).

## Claim before coding
During **Task Dispatch**, claim **one** board bullet plus a milestone/PR id (`production/milestones/`, open PRs). Write the claim on the board. No second agent on the same claim.

## Source of truth
| Kind | Authority |
|---|---|
| Game rules | `Amphiteater.Sim` (`src/Amphiteater.Sim/`); knobs in [`production/economy.md`](production/economy.md) |
| Fantasy | [`docs/design/00_*`](docs/design/00_overview_stub.md) |
| Presentation roadmap | [`docs/design/01_expansion.md`](docs/design/01_expansion.md) |
| Process | [`DEVELOPMENT_CYCLE.md`](DEVELOPMENT_CYCLE.md) + [`agents/`](agents/) |

## Dispatch
Subagents work **only** when explicitly dispatched (Organizer or human). Hard rule: [`agents/subagents/README.md`](agents/subagents/README.md).

## Done
Build/run (`.\build.ps1` / `.\run.ps1`) or headless `--report`. Update the board. Small commits. See Review & Commit in the cycle.

## Holds — do not reopen without a human ask
Playtest **holds** ([`production/playtest_m2.md`](production/playtest_m2.md)):
- Combat virtus / vigor / palmae retune this sitting
- Engine switch before the console loop is loved ([`production/decisions/001_console_ludus.md`](production/decisions/001_console_ludus.md))

**Godot editor zip** is still a local install (`tools/godot-editor/`, gitignored). C# restore / .NET 8 targeting shipped in PR #12 — see [`godot/README.md`](godot/README.md). Do not switch the playable host off the console.

## Cursor Cloud specific instructions

Image (`.cursor/Dockerfile`) has .NET SDK 8 and 10 in `/usr/share/dotnet` (`dotnet` on `PATH`) plus Python 3 and Pillow. There is no boot daemon: the playable host is the console build, not a server.

```bash
dotnet restore Amphiteater.slnx
dotnet restore godot/Amphiteater.Godot.csproj
dotnet build src/Velarium -c Release -o dist
dotnet test src/Amphiteater.Sim.Tests --nologo
./dist/Amphiteater --report 200
dotnet build godot/Amphiteater.Godot.csproj -f net8.0
```

On Linux the host is `dist/Amphiteater` (the `.exe` name in `build.ps1` / `run.ps1` is the Windows output). Godot editor unzip stays local; restore and `-f net8.0` build do not need it.

PixelLab art batches: `python3 tools/pixellab_gen.py --check` then a wave only when the board allows it. Token is `PIXELLAB_API_TOKEN` or gitignored `pixellab.env` — never commit it. Palette writes need Pillow (`python3-pil`).
