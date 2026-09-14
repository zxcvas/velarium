# Design: Forum equipment (v1 lock)

**Status:** Locked 2026-09-14 (Victor) — **docs only**; no buy / assign / combat code in this PR  
**Product:** **Velarium**. Amphiteater is historical flavor and the sim folder name (`src/Amphiteater.Sim`).  
**Audience:** implementers of the console forum-equipment sprint  
**Holds:** additive combat knobs only. Do not retune virtus / vigor / palmae. Console stays the playable host (`production/decisions/001_console_ludus.md`).

Knobs: [`../../production/economy.md`](../../production/economy.md) (Forum equipment).  
Fantasy frame: [`00_overview_stub.md`](00_overview_stub.md). Courtyard / Godot: [`01_expansion.md`](01_expansion.md) (Armamentarium as a **room** is later; v1 is a ludus pool).

---

## Overview

The forum sells **kit**, not just men. The lanista buys helmets, armour, shields, and weapons into a **Ludus Armory**, then assigns pieces to living gladiators. Gear is property of the house: on death it returns to the Armory. Starters arrive **unequipped** — no free kit.

Combat stays the M1 `Combat.Score` formula. Equipment **adds** (or subtracts) a few points. It does not replace virtus, vigor, or palmae.

---

## Model sketch

Code homes:

| Piece | Home |
|---|---|
| Slot, culture, tier, item identity | `src/Amphiteater.Sim/Models.cs` |
| Armory list on `GameState`; equipped refs on `Gladiator` | `Models.cs` + `Save.cs` (`amphiteater_save.json`) |
| Catalog prices, resale | `EquipmentCatalog` (`BuyPrice` / `ResalePrice` / `CreateInstance`; sheet: `production/economy.md`) |
| Display names | `Content.EquipmentNom` (Roman Weapon = rete+fuscina) |
| Affinity | later `Combat.cs` / `Ludus.cs` |
| Buy into Armory | `Ludus.cs` + forum menu in `src/Velarium/Game.cs` |
| Assign / unassign / death return | `Ludus.cs` |
| Fight-score additives + locatio culture dock | `Combat.cs` (`Score`) and `Ludus.cs` (`LocatioWrongSudoreDock` / `LocatioWrongOccisusDock`) |

Suggested shapes (names may move; the rules may not):

- **Slot:** Helmet, Armor, Shield, Weapon.
- **Culture:** Punic, Greek, Roman.
- **Tier:** T1 / T2 / T3.
- **Item:** `{ id, slot, culture, tier }`. Catalog rows are templates; each purchase is a distinct instance in the Armory.
- **Armory:** unordered pool on the ludus (`GameState`), not a `RoomKind`. v1 does not require a built Armamentarium room or a household slave.
- **Loadout:** at most one item per slot on a living `Gladiator`. Assign moves Armory → slot; unassign and death move slot → Armory.

Forum today sells gladiators, household, medicus, rumors (`Game.ForumScreen`). Equipment is a **new** forum line, not a change to *in catasta*.

---

## Cultures (v1)

Only **Punic | Greek | Roman**. No other origins on the stall this sprint.

Culture is a property of the **item**, not of the man. A Thracian *thraex* may wear Greek kit as natural and Punic as exotic; that is affinity (below), not a fourth catalog.

---

## Slots and who can wear them

Market buy list is exactly four slots:

| Slot | Murmillo | Thraex | Secutor | Retiarius |
|---|---|---|---|---|
| Helmet | yes | yes | yes | yes |
| Armor | yes | yes | yes | yes |
| Shield | yes | yes | yes | **N/A** — cannot buy-for / assign |
| Weapon | yes | yes | yes | **Roman net+trident set only** |

**Retiarius weapon.** The Weapon row for a retiarius **is** the Roman net + trident. Greek and Punic weapons stay on the stall for the heavy *armaturae*; they do not fill a retiarius’s natural weapon. Until a **Roman Weapon** is assigned, the retiarius takes the missing-weapon penalty (combat knobs). Shield is not a slot for him — no *scutum* on a net-man.

---

## Armory, assign, death

1. Buy at the forum → instance enters the **Ludus Armory** (not auto-equipped).
2. Assign only to a **living** gladiator who may wear that slot. Occupied slot: previous piece returns to the Armory first.
3. On **death**, every equipped piece returns to the Armory before the man leaves the roster / goes *ad Libitinam*. The house keeps the bronze.
4. Discharge (*rudis*) is out of band for this lock: treat like death for kit (return to Armory) unless a later claim says he walks with his arms.
5. Starters (`Ludus.Start` three tiros: murmillo, thraex, retiarius) and forum-bought men arrive **with empty slots**. Buy kit at the forum.

Resale (forum, from Armory, unequipped): **≈ 50%** of catalog buy price, integer denarii, round down. No resale of a piece still on a man.

---

## Combat knobs (additive only)

Do **not** change tiro virtus, vigor bands, palmae, round count, or Ville target. Today’s score is virtus + vigor/4 + 2d6 plus existing status/tiro/pair/*auctoratus* mods (`Combat.Score`). Equipment **adds after that**, then clamps.

| Knob | Value |
|---|---|
| Per equipped item, by tier | T1 **+0**, T2 **+1**, T3 **+2** to fight score |
| Cap from gear bonuses | **+4** total (sum of per-item tier bonuses, then clamp) |
| Retiarius, no Roman net+trident assigned | **−2** until that Weapon is on him |
| Culture mismatch vs *armatura* affinity | **−1** to fight score |

Empty slots on murmillo / thraex / secutor are **not** an extra naked penalty. T1 is cheap bronze: it does not raise the score; it only dresses the man (and, for the retiarius, a T1 Roman Weapon is what clears the −2).

Mismatch is **one −1** for the loadout if any equipped piece is off-affinity, not −1 per piece. (Keeps the additive band small next to the +4 cap.)

**Locatio.** Reuse the existing wrong-*armatura* sweat / corpse docks when the man is sent out in culture-mismatched kit: `LocatioWrongSudoreDock` (8, floor 12) and `LocatioWrongOccisusDock` (40, floor 80) in `Ludus.cs`. Same constants; do not invent a second table. If the offer is already the wrong *armatura*, do **not** stack a second dock — one dock, same numbers.

---

## *Armatura* affinity

Natural culture is the *armatura*, not the man’s `Origin` string.

| *Armatura* | Natural | Notes |
|---|---|---|
| Murmillo | **Roman** | Greek / Punic = mismatch |
| Secutor | **Roman** | Greek / Punic = mismatch |
| Thraex | **Greek** | **Punic exotic ok** (no mismatch). Roman = mismatch |
| Retiarius | **Roman** (light) | Shield N/A. Greek / Punic helm or armour = mismatch. Only Roman Weapon clears the net+trident −2 |

“Punic exotic ok” on the *thraex* means Punic pieces are treated as matching for both the combat −1 and the locatio culture dock.

---

## Economy (pointer)

Catalog buy prices (denarii) and the combat table live in `production/economy.md` under **Forum equipment**. That sheet is the tweak surface; this doc is the rule lock.

Starter purse stays 620. A full T1 Roman kit for a murmillo (helm 45 + armour 60 + shield 35 + weapon 40) is 180 denarii — a real morning at the stalls, not a free loadout.

---

## Out of scope (this sprint and this lock)

- **Sprites / PixelLab.** No new gear art; *armatura* bodies stay as they are. Wave 3a south (PR #16) is unrelated.
- **Godot.** No courtyard Armamentarium room, no drag-and-drop racks. Console forum + ludus menus only when code starts. Host stays `src/Velarium`.
- **CareerSim auto-buy.** `--report` must not silently purchase or assign kit until a later, explicit claim. Headless runs keep today’s policy.
- **Other cultures.** Iberian, Gallic, Eastern, etc. stay off the stall.
- **Other slots.** Greaves, *manica*, *galerus*-as-separate-item, *balteus* — not v1. Retiarius shoulder is flavour on the body, not a fifth slot.
- **Virtus / vigor / palmae retune.** Playtest hold. Equipment is additive.
- **Hoplomachus**, gladiatrices, imperial armouries, SC 177 price tables.
- **Foe loadouts** on the sand: later; v1 may treat the editor’s man as unknobbed (no extra gear score) unless a follow-up claim says otherwise.

---

## Implementation order (board Next)

Docs (this file) → model + save → catalog constants (`EquipmentCatalog` + `Content.EquipmentNom`) → forum buy → assign / death return → `Combat.Score` + locatio dock.

Claims: `equip-design-lock` → `equip-model-save` → `equip-catalog` → `equip-forum-buy` → `equip-assign` → `equip-combat`.

---

## Key decisions

1. Four buy slots, three cultures, three tiers — closed catalog.
2. House Armory pool; men borrow kit; death returns it.
3. No free starter kit.
4. Additive score only; +4 cap; retiarius −2 without the Roman net+trident; mismatch −1.
5. Thraex: Greek natural, Punic exotic allowed.
6. Retiarius: Roman light, shield omitted, weapon is the net+trident set.
7. Locatio culture dock reuses wrong-*armatura* numbers, no stack.
