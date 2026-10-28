# Design: Rival *cena* (banquets)

**Status:** Locked 2026-10-24 (Victor) — **docs only**; no invite / dusk / cup code in this PR. Quiz closed **all A**.  
**Product:** **Velarium**. Amphiteater is historical flavor and the sim folder name (`src/Amphiteater.Sim`).  
**Audience:** implementers of the console banquet sprint  
**Holds:** no virtus / vigor / palmae retune. Console stays the playable host (`production/decisions/001_console_ludus.md`). Do not fold this into Godot. Do not retune night-ops catch tables this sitting.

Decision: [`../../production/decisions/002_rival_cena.md`](../../production/decisions/002_rival_cena.md).  
Knobs: [`../../production/economy.md`](../../production/economy.md) (Rival *cena*).  
Fantasy frame: [`00_overview_stub.md`](00_overview_stub.md). Night ops / kitchen: [`01_expansion.md`](01_expansion.md). Citations: [`../research/ludus_sources.md`](../research/ludus_sources.md).

---

## Overview

The lanista can **invite the named rival** to eat at the ludus mess. The *cena* is hospitality first: coin, a staffed cook, a table event, a shift in hostility. Poison is an **optional verb at the table**, not the reason the meal exists, and not a random roll that murders the guest.

This is a **new evening verb**, not a night sneak. Wine left at the rival's gate (`NightOrder.Poison`) stays until a later sitting decides to fold it. Street bowls (*thermopolium*) stay street bowls. The *cena* is the same hearth, a different register: you feed a competitor, not the crowd at the window.

Infamia does not come off with the tablecloth. Magistrates and editors do not recline here.

---

## Why this, not the courtyard sprint

Draft PR #27 (`courtyard-morning`) sequences Godot yard sprites. Playtest holds still forbid an engine switch before the console loop is loved. The human ask this sitting is a **social mechanic**. SoT for the queue is this board sprint, not that draft.

---

## Alternatives considered

| Approach | What | Why not (for v1) |
|---|---|---|
| **A. Evening *cena* (recommended)** | Pay, invite the named rival, resolve at dusk. Night sneak forced to rest that evening. Table event always; poison is an opt-in. | — |
| **B. Replace `NightOrder.Poison`** | Banquet is the only poison channel; delete wine-at-the-gate. | Cleaner fiction, but it retunes a shipped night verb this sitting. Hold. |
| **C. Banquet as a night order** | New `NightOrder.Cena` next to spy / poison / sabotage. | A feast is not a sneak. Catch-at-the-gate math does not belong on a guest you invited. |

v1 ships **A**. B is a later fold if the table poison is loved. C is rejected.

---

## Fantasy (truth vs concession)

| Topic | History / existing law | Game | Label |
|---|---|---|---|
| Who dines | Lanistae are *infames*. Fama buys contracts, not dinner with an *augustalis* (already a night-event line). | Guests are **rival lanistae** only. No aedile, no editor, no duumvir. | **True** (infamia lock) |
| Where | Kitchen / mess is barracks dining (Jacobelli). | *Cena* at the *culina*, not a senatorial triclinium. | **True** (register) |
| Street vs table | Thermopolium is a street window. | Stall income unchanged. Banquet does not sell bowls. | **True** |
| Poison | Elite dinner-poison tropes are not in `ludus_sources.md`. Night ops already concede wine at the gate. | Optional cup at your table. Outcome is **sick champion / missed morning**, not a corpse in the mess. | **Gameplay concession** (same family as gate wine) |
| Guest death | Hollywood; also collapses the one-rival loop. | **No** killing the lanista at table in v1. Death stays on the sand. | **Omission** |
| Grit | Slavery, death, infamia stay; no sexual violence; no child fighters. | Unchanged. | **True** |

---

## v1 loop

One named rival (`GameState.Rival`, already `Content.RivalLanistae`). No circuit yet (expansion PR9).

1. **Gate.** Staffed kitchen (`Ludus.Staffed` *culina*). Purse ≥ cost. Cooldown elapsed. Rival not already `MissTomorrow` for a reason that would make the visit absurd (they can still come if they will miss *tomorrow's editor* — they eat tonight).
2. **Invite (day, Domus).** Player sets `PendingCena`. Optional: mark `CenaPoison` (the cup). Cost is reserved or paid immediately — **pay immediately** so dusk cannot double-spend.
3. **Refuse.** If `Rival.Hostility` ≥ `CenaRefuseHostility`, they send a boy: no. Coin is not refunded (you bought the food). Flavor: they remember the gate.
4. **Evening.** `NightOrder` is forced to `Rest`. You cannot spy or leave wine at their gate on a night they sit in your mess. `ResolveNightOps` no-ops. Table event still allows a quieter `NightEvent` roll (same reduced 15% as a busy night).
5. **Resolve at dusk** (after stall profit, before upkeep — same neighborhood as the cook's work).
6. **Hospitality** (if they came, poison or not): hostility −1 (floor 0); small fama chance; then one table event.
7. **Poison** (if opted in and they came): separate catch / success, using **new** knobs, not the night-ops table. Success reuses `NextFoePoisoned` (and may also set `MissTomorrow`). Caught: hostility spike, fama dock, fine; the guest leaves; skip the friendly hospitality tick.

Career AI (`--report`) does not host a *cena* this sprint (it already rests at night).

---

## Model sketch

Code homes (after lock):

| Piece | Home |
|---|---|
| Pending flag, poison flag, cooldown | `GameState` + `Save.cs` (`amphiteater_save.json`) |
| Costs, refuse threshold, catch | `Ludus` constants; sheet `production/economy.md` |
| Invite / refuse / resolve | `Ludus.House.cs` (next to `ResolveNightOps`) |
| Copy | `Content` (*cena* nom; event lines) |
| Console verb | Domus menu in `src/Velarium/Game.cs` — not Forum, not Host munus |

Suggested shape (names may move; the rules may not):

```
GameState.CenaPending : bool
GameState.CenaPoison  : bool
GameState.CenaCooldown: int   // days remaining; ticks at Calendar.Next
```

Old saves: missing fields default false / 0. `EnsureHouse` does not invent a feast.

No new `RoomKind`. No triclinium tile. No second rival object.

---

## Economy (locked sheet)

Cheaper than a hosted munus (220), dearer than a sneak (free except risk). About two *pro sudore* purses.

| Knob | Proposed | Notes |
|---|---|---|
| Cost | 48 + 12 × kitchen level | lv1 60, lv2 72, lv3 84 |
| Cooldown | 4 days after a resolved invite (refuse counts) | Stops farm |
| Refuse | hostility ≥ 6 | After ~3 night-ops catches |
| Hospitality | hostility −1; 40% fama +1 | Never a magistrate dinner |
| Poison catch | 28% + 4 × hostility | Not the gate table |
| Poison miss (not caught) | 35% | Cup unnoticed, rival unharmed |
| Poison success | `NextFoePoisoned = true`; 40% also `MissTomorrow` | Reuse sand flags; poison still outranks spy-weak |
| Poison caught | hostility +3 (cap 12); fama −2; fine 18–36 | No guest corpse |
| Table event | always exactly one if they sat | See table below |

Must not out-pay a locatio palma on the average night. The *cena* is a spend.

---

## Table events (v1)

Always one line if the rival sat. Poison caught skips this table (they left). Weights are equal unless noted. None of these kill the guest.

| Id | Beat | Effect |
|---|---|---|
| `toast` | They drink to a shared editor | hostility −1 extra (floor 0) |
| `brawl` | Household vs their boy in the yard | one worker *or* one gladiator `Vulneratus` if any living |
| `tiro` | They offer a tired tiro | volunteer-for-pay hook like `NightEvent` auctoratus, or flavor-only if beds full |
| `rumor` | They let slip tomorrow's kit | same seed as `SpyIntelKind.TomorrowArmatura` |
| `boast` | They talk too loud | rival fama −1 (floor 0); your fama unchanged |
| `watch` | A watchman smells infames at table | small fine 8–18, same flavor as the unbarred-door night event |
| `quiet` | Oil, barley, little talk | no extra numeric effect |

`rumor` is intel that **pays** (same law as spy: not flavor-only). If the offer is already seeded, pick `toast` instead.

---

## Console

Domus gains a line: **Cena (invite the rival)**. Sub-menu:

1. Hospitality only
2. Hospitality, and the cup (poison)
3. Cancel pending (refund **no** — food is bought; only allowed before dusk if we need an escape: **no cancel after pay**. Back out before confirming.)

Main day HUD already shows night order; add `Cena pending` when set.

Copy is antique and short. Do not write a romance. Do not write a magistrate at the couch.

---

## Not in v1

- Editors, aediles, *augustales* as guests
- Killing the rival at table
- Multiple guests / Campanian circuit (expansion PR9)
- Replacing `NightOrder.Poison`
- Godot triclinium, PixelLab feast frames
- CareerSim hosting *cenae*
- Ingredient market (still none)
- Combat knobs

---

## Quiz (closed 2026-10-24)

Victor: **all A**. Locked picks:

1. **Who sits?** named rival lanista only  
2. **Night poison?** leave wine-at-the-gate; banquet is additive  
3. **The cup?** sick champion (`NextFoePoisoned`) ± missed morning — no guest corpse  
4. **How many houses?** one named rival  
5. **Gate?** staffed kitchen, any level  
6. **The evening?** feast forces night rest (no sneak the same night)  

---

## Sprint (after this lock)

Each claim is independently playable. Console build required. One claim at a time. Do not start a later claim in the same PR.

### `banquet-design-lock` (this PR)
Docs only. This file Locked. Economy knobs are the live sheet (constants named, not yet in `Ludus`). Decision `002_rival_cena.md`. No C#.

### `banquet-model-save`
**Files:** `src/Amphiteater.Sim/Models.cs` (`GameState`), `Save.cs` already serializes the DTO.  
**Accept:** `CenaPending` / `CenaPoison` / `CenaCooldown` round-trip in `amphiteater_save.json`. Old saves load with false / false / 0. `EnsureHouse` does not invent a feast. Tests next to `EquipmentSaveTests`. No Domus verb.

### `banquet-host`
**Files:** `Ludus` / `Ludus.House.cs`, `Content`, `src/Velarium/Game.cs` (Domus).  
**Accept:** staffed-kitchen + purse + cooldown gate; pay on confirm (`CenaCostBase + CenaCostPerKitchenLevel × level`); refuse at hostility ≥ `CenaRefuseHostility` keeps the coin; pending feast forces `NightOrder.Rest`; dusk hospitality hostility −1 and 40% fama +1 if they sat. No event table. No cup (ignore `CenaPoison` or do not offer it yet). `--report` still never invites.

### `banquet-events`
**Accept:** exactly one row from the table if they sat. `rumor` seeds dawn locatio kit (`SpyIntelKind.TomorrowArmatura`); if already seeded, `toast`. None kill the guest. Poison-caught skip is N/A until the cup PR.

### `banquet-poison`
**Accept:** Domus offers the cup. New catch table (`CenaPoisonCatchBase + CenaPoisonCatchPerHostility × hostility`), not `ResolveNightOps`. Success sets `NextFoePoisoned`, 40% also `MissTomorrow`. Caught: hostility +3, fama −2, fine 18–36, skip hospitality. Miss: guest unharmed. Tests: poison still outranks spy-weak; `--report` still never hosts a *cena*.
