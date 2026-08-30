# Amphiteater — PixelLab style lock (NES / SNES)

**Look:** late-80s / early-90s console pixel art. Think *Zelda: A Link to the Past*, *Secret of Mana*, *River City Ransom* — not painterly 16-bit concept art, not isometric dioramas.

**Why:** the harena is ugly in prose; on screen it should read as a **cartoon brawl**. Red pixels and a prone sprite, not anatomy. It is a game.

**Camera:** SNES 3/4 **low top-down** for the courtyard, rooms, and the sand. Same sprites for ludus and harena. No separate fighter-game camera in wave 1.

**Native sizes (PixelLab → Godot integer scale 3× or 4×):**
- Tiles: **16×16** (PixelLab tileset 16) or **32×32** if 16 fails; never 128+
- Characters: **32×32** or **48×48** 4-direction (PixelLab floor). Display chunky.
- FX: **32×32**

**Palette:** one master strip, ~16 colours, NES-hard. Ochre, soot, iron, dirty linen, Pompeii red, charcoal, one sea-green. Forced on every call via `color_image`. No gradients that aren't dither. No gold leaf, no neon.

**Line / shade:** 1px dark outline or cluster outline; flat + one shadow step. High-contrast silhouettes at 32px.

**Kit (still Pompeii, drawn like SNES):**
- Murmillo: fish-crest *galea*, big scutum, short blade, manica
- Thraex: griffin helm, small parmula, sica, long greaves
- Retiarius: bare head, *galerus*, net + trident, tunic
- Secutor: smooth helm, two eye-holes, scutum
- Household: undyed tunic, no helm, no weapon

**Gore language:** 2–4 red pixels, a puff, stars, X-eyes, prone. *Streets of Rage*, not autopsy. *Iugula* is a cartoon KO.

**Do not**
- Hollywood cuirass / comic abs / photoreal blood
- 8 directions (NES/SNES walk is 4)
- Child fighters, sexual content
- Runtime PixelLab from the game
- Commit tokens

**Pipeline:** `python tools/pixellab_gen.py` once `pixellab.env` returns 200 on `GET /balance`. Imagine comps in the session folder are **mood only**, not production.
