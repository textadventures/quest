# Quest 5 Docs Migration Plan

Migrate Quest 5 documentation from GitHub Pages (Jekyll, v5 branch `/docs/`, hosted at docs.textadventures.co.uk/quest) into the Astro/Starlight site (main branch `/site/`, hosted at questviva.com).

**Why:** Docs for both versions will live in one place, gaining built-in search, consistent UI, and easier maintenance. Most Quest 5 content applies equally to Viva, so there's no need for a separate namespace.

## Decided

- **No `/quest5/` URL prefix** — Quest 5 docs merge into the main namespace alongside Viva docs. Where something is v5-specific or not yet in Viva, use inline Starlight admonition callouts rather than structural separation.
- **Redirects** — a single Cloudflare Pages wildcard rule in `public/_redirects`: `docs.textadventures.co.uk/quest/*` → `questviva.com/:splat`
- **Source of truth** — Quest 5 docs move permanently to the main branch. The `docs/` folder in v5 branch will be removed after migration. Edit links will work correctly as everything will be in main.
- **GitHub Pages** — `docs.textadventures.co.uk` stays live and redirects to Cloudflare. DNS is controlled and can be configured as needed.
- **Jekyll syntax audit** — Required as part of Phase 1 before scripting the migration.

## Migration steps

### Phase 1 — Prep

- Audit for Liquid/Jekyll syntax (`grep -r '{%\|{{' docs/` excluding `_site/`)
- Audit link patterns (`grep -r '\.html' docs/` excluding `_site/`) to understand consistency

### Phase 2 — Content migration script

Copy from v5 branch `docs/` → main branch `site/src/content/docs/`, skipping `_site/`, `_layouts/`, `_config.yml`, `Gemfile*`, `*.sh`, `*.css`:

1. Strip `layout: index` from all frontmatter
2. Rewrite internal `.html` links → remove extension (e.g. `foo.html` → `foo`)
3. Move images to `site/public/images/` and update image paths in markdown
4. Move audio/other assets to `site/public/`

### Phase 3 — Astro config

- Add Quest 5 sections to sidebar in `site/astro.config.mjs` (autogenerate or manually structure: Tutorial, Guides, Functions, Attributes, etc.)
- Update site title if needed

### Phase 4 — QA

- `astro build` — Starlight's broken link checker will surface issues
- Spot-check image rendering
- Check sidebar navigation

### Phase 5 — Redirects

- Add `public/_redirects` with wildcard rule
- Verify old URLs redirect correctly

### Phase 6 — Cleanup

- Remove Jekyll docs from v5 branch (or leave as archive)
- Remove `_site/` pre-built output from v5 branch (42MB, no longer needed)

## Scale

- 739 markdown source files
- ~800 images (PNG/JPG)
- 3 audio files
- Key subdirectories: `functions/` (304 files), `helpsheets/` (185), `attributes/` (151), `guides/` (53), `scripts/` (41), `tutorial/` (16)
