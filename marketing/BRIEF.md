# Amphiteater / Velarium — marketing brief

**Owner:** Velarium Director (site + copy only)  
**Date:** 2026-09-12  
**Status:** DRAFT. No Steam publish. No new PixelLab spend.  
**Tone sources:** `docs/design/00_overview_stub.md`, `docs/design/01_expansion.md`, `assets/art/STYLE.md`, Notion [Marketing & monetization plan](https://app.notion.com/p/3d904b1313e781eea260cef19db26548).

This file is planning. It is **not** served by GitHub Pages. Public files live under `marketing/site/`.

---

## Locks (already decided — do not reopen on the landing)

| Lock | Line |
|---|---|
| Role | You are a *lanista* in **Capua**. Not the emperor’s procurator. Not a Hollywood Colosseum start. |
| Economy verbs | *Infamia*, *locatio*, *ludus*; *munus* when the house has *fama* and a *palma*. |
| Tone | Honest grit + cartoon KO pixel presentation (NES/SNES). Red pixels and a prone sprite, not anatomy. |
| Forbidden promises | No sexual content. No child fighters. No “become consul.” Infamia closes the curia. |

The product name on the page is **Amphiteater**. The repo is **Velarium** (the awning). That split is a store-name question, not a rewrite of the fantasy.

---

## Open questions (need Victor)

These are why this brief exists. The public site must not invent answers.

### 1. Store name — Amphiteater vs Velarium vs other

- **Amphiteater** is the game name in `PROJECT_PLAN.md` and the design stubs.
- **Velarium** is the GitHub repo and the awning over the cavea — shade, not the curia.
- Steam / itch listing cannot stay “working title” forever. Pick one wordmark before the wishlist goes live.

### 2. Price band

Internal hypothesis from the marketing plan (not locked, not on the landing):

- Early Access ~$12–15
- 1.0 ~$18–22
- Post-1.0 DLC ~$5–8

Confirm against sibling titles when a Steam stub exists. Do not print a price on Pages.

### 3. Demo scope

Options on the table:

- Headless-feeling short Capua week (console)
- Itch free console build
- Godot courtyard demo (only after editor unzip + playtest holds lift enough that empty-purse / *locatio* feel fair)

Do not ship a public demo until that call is made. The landing must not claim a download.

### 4. Publisher / Steam org

Who lists the page: personal Steam org, a house name, or a third-party publisher?  
Until that exists, the wishlist control stays a **disabled stub** (`#` / `disabled`), with a note that there is no store listing yet.

---

## Public IA (this shell)

| Surface | Job |
|---|---|
| `marketing/site/index.html` | Pitch, Capua one-liner, loop, courtyard sprites, current status, Steam stub, GitHub + Notion |
| `marketing/site/press.html` | Press-ish facts — setting, role, what it is not |
| `marketing/site/img/` | Curated copy of existing PixelLab stills (south idles, yard props, `sample_dirt`). No generate. |
| `marketing/BRIEF.md` | This file (not in the Pages artifact) |

Single-scroll landing plus a thin press page. No blog, no analytics, no live Steam embed.

---

## Hosting

Sibling pattern (FOON `web/site/`, Infected `site/`, Latifundium `marketing/site/`):

- Workflow: `.github/workflows/pages.yml` uploads `marketing/site/` as the Pages artifact.
- Deploy on push to `master` (or `workflow_dispatch`). PRs package only.
- Human step after merge: repo **Settings → Pages → Source = GitHub Actions**.
- Do not create a repo-root `docs/` tree just to publish.

---

## Must not appear on the public site

- Live Steam URL, price, launch date, “buy now”
- Imperial procurator / Ludus Magnus as the start
- Consul / senate career fantasy
- Sexual content, child fighters, photoreal gore
- New PixelLab jobs or invented sprites
- Claiming the Godot editor is unzipped in CI (it is still local)

---

## Next (after this shell)

1. Victor: store name + publisher + whether a demo is console or courtyard.
2. Art Director: composed 16:9 hero / wordmark from **existing** assets if the CSS courtyard is not enough. No new generate.
3. Wishlist campaign only once a courtyard GIF exists (south idle + End Day) **and** a Steam URL exists.
