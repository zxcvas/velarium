# Amphiteater marketing site

Static landing for **Amphiteater** (product) / **Velarium** (repo). HTML + CSS. No framework.

Sprites in `img/` are a curated copy of existing PixelLab stills from `assets/art/`. Do not call PixelLab from here. Do not deep-link `../assets` (breaks when this folder is the web root).

Planning questions live in [`../BRIEF.md`](../BRIEF.md) and are **not** part of this folder.

## Local

From this folder:

```bash
python3 -m http.server 8080
```

Open http://127.0.0.1:8080/

`index.html` also works over `file://` — image paths are relative.

| Path | What |
|---|---|
| `index.html` | Pitch, loop, courtyard, status, Steam stub |
| `press.html` | Press-ish fact sheet |
| `img/` | South idles, yard props, `sample_dirt`, NES palette strip |

## GitHub Pages

`.github/workflows/pages.yml` uploads this folder as the Pages artifact.

1. Merge to `master` (or run **workflow_dispatch**).
2. Human: repo **Settings → Pages → Source = GitHub Actions**.

Do not publish a game download from Pages. There is no public build.

## Do not

- Promise Steam, a demo zip, sexual content, child fighters, or becoming consul
- Start the player as the emperor’s procurator or in the Hollywood Colosseum
- Invent new sprites or regenerate PixelLab art
