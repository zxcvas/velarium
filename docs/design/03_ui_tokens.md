# Design: UI tokens

**Status:** v1, 2026-10-01  
**Code:** `marketing/site/styles.css` (`:root`) · `godot/ui/UiTokens.cs` + `godot/ui/VelariumTheme.cs`  
**Palette authority:** [`assets/art/STYLE.md`](../../assets/art/STYLE.md). Tokens never add a colour.

Two layers. **Primitives** are the 16 sprite colours, named for what they look like. **Roles** are named for their job; UI asks for roles. Change a role in both files in the same PR.

## Roles

| Role | Primitive | CSS | Godot |
|---|---|---|---|
| Page / screen ground | soot | `--surface-ground` | `SurfaceGround` |
| Panel, card | charcoal_deep | `--surface-raised` | `SurfaceRaised` |
| Masthead, halo | dark_pompeii | `--surface-banner` | `SurfaceBanner` |
| Yard fill | packed_dirt | `--surface-sand` | `SurfaceSand` |
| Chrome border | outline | `--border-chrome` | `BorderChrome` |
| Text | pale_linen | `--text-primary` | `TextPrimary` |
| Secondary text | dirty_linen | `--text-secondary` | `TextSecondary` |
| Link / selected | bronze | `--text-link` | `TextLink`, `Selected` |
| Focus ring, link hover | ochre | `--focus-ring` | `FocusRing` |
| Primary action | pompeii_red | `--action-primary` | `ActionPrimary` |
| Primary hover | dark_pompeii | `--action-primary-hover` | `ActionPrimaryHover` |
| Secondary action | charcoal_deep | `--action-secondary` | `ActionSecondary` |
| Disabled | charcoal | `--action-disabled` | `ActionDisabled` |

## Gladiator status

| `GladiatorStatus` | Fill | Text | Rim |
|---|---|---|---|
| *Validus* | sea_green | outline | outline |
| *Fessus* | ochre | outline | outline |
| *Vulneratus* | dark_pompeii | pale_linen | pompeii_red |
| *Aeger* | cold_iron | outline | outline |
| *Mortuus* | soot | dirty_linen | charcoal |

The tag always shows the word as well; colour only confirms it.

## Contrast (WCAG 2)

| Pair | Ratio | Use |
|---|---|---|
| pale_linen on soot | 12.0 | body |
| dirty_linen on soot / charcoal_deep | 6.5 / 5.3 | secondary text |
| iron_rust on soot | 3.4 | **not text** (was eyebrow/notes until v1) |
| pale_linen on pompeii_red | 4.3 | large text only: primary label ≥ 19px bold |
| outline on sea_green / ochre / cold_iron | 5.0 / 6.3 / 6.7 | status tags |
| pale_linen on dark_pompeii | 8.3 | *Vulneratus* tag, masthead |

## Space and chrome

One art pixel = 4 screen px (sprites and the Godot viewport are integer 4×). The site counts in screen px, Godot in viewport px.

| Token | Site | Godot |
|---|---|---|
| `space-1 … space-16` | 4 × n px (`--space-n`) | n px (`SpaceN`) |
| Border | 3px `--border-chrome` | 1px `Border` |
| Halo | 6px `--surface-banner` box-shadow | not drawn yet (StyleBoxFlat has one border colour) |
| Radius | 0 | 0 |

## Type

Site: Cinzel (display, caps, tracked) + Georgia (body). Latin in italics, lining tabular numerals for money and stats.  
Godot HUD: **Silkscreen** (OFL, `godot/fonts/`) — Regular for labels, Bold for buttons, at 8 / 16 viewport px only (32 / 64 on screen, so all HUD text counts as large). Antialiasing and hinting off (`godot/ui/PixelFont.cs`).

| Candidate (8px) | Longest date line* | Verdict |
|---|---|---|
| Press Start 2P | 424px | NES-true but overflows the 304px status label |
| Pixelify Sans | 209px | Mushy at 8px; built for display sizes |
| **Silkscreen** | 276px | Crisp, fits; all caps echoes Cinzel's inscription voice |

\* `ante diem XVIII Kalendas Septembres, a.u.c. DCCLXXXII` — `Calendar.Format`'s longest form.

Silkscreen is caps-only, so Latin italics do not exist in the HUD; the site keeps them.
