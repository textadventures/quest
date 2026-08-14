---
name: docs-screenshots
description: Regenerate stale editor screenshots embedded in site/src/content/docs/ against the current AppShell editor, using a Playwright capture harness
---

# Regenerating docs editor screenshots

## Why this exists

The migrated Quest 5 docs (`site/src/content/docs/`) embed ~299 screenshots.
Most show the **old Quest 5 desktop/web editor** — a Windows Forms app with a
ribbon toolbar — which no longer exists. Quest Viva ships one unified editor
now (`src/AppShell/`, a SvelteKit SPA, same UI in a browser tab or the
Electron app). This skill captures fresh screenshots against that real,
running editor instead of by hand.

## Current scope

**In scope today: editor-chrome screenshots** — pages showing the editor's own
UI (tree, tabs, forms, dialogs). Root `site/public/images/` and
`site/public/images/other_guides/`, roughly 150 images.

**Out of scope — do not extend this harness to these without re-scoping first:**
- **In-game player screenshots** (~60 images, showing gameplay output, not the
  editor) — needs a different harness driving WasmPlayer/WebPlayer with an
  actual built `.aslx` game reaching a specific play state. A separate problem.
- **`site/public/images/helpsheets/`** (~152 images) — a large, distinct
  beginner-oriented track. Worth its own decision on approach before touching.
- **External/non-Quest images** — Trizbort app screenshots, YouTube embed UI,
  hand-drawn diagrams like `architecture.png`. Not Quest UI; nothing to regenerate.

## How it works

`tests/e2e/docs-screenshots/` (part of the `quest-e2e` Playwright package —
see `.claude/skills/verify/SKILL.md` for the base convention this extends):

- **`lib.mjs`** — shared helpers: `createLocalDraft`, `selectTreeNode`,
  `addElement`, `openTab`, `fieldByLabel`/`setLabeledField`/`selectLabeledField`,
  `capture`, and the `runCapture` try/finally wrapper. Reuse these rather than
  re-deriving selectors — they're built on DOM patterns already proven out
  across `tests/e2e/verify-appshell-*.mjs`.
- **One capture script per doc page**, e.g.
  `capture-tutorial-creating-a-simple-game.mjs`. Each script walks through
  *that page's own narrative* — the same room/object names and steps the
  prose already tells the reader to perform — and calls `capture()` at every
  point the doc currently embeds a screenshot, saving over the **existing
  filename** in `site/public/images/`. Same filename in, same filename out —
  no markdown edits needed.

Viewport is fixed at 1280×800 (`lib.mjs`'s `VIEWPORT`), light theme, for a
consistent look across captures.

## Adding a new capture script

1. Read the target doc page and note, in order, what state each embedded
   image is meant to show — usually obvious from the surrounding prose (it's
   literally instructing the reader what to click/type right before each image).
2. Start the AppShell dev server (`.claude/launch.json`'s `AppShell` config,
   `npm run dev` in `src/AppShell`, port 5174).
3. Walk the same steps interactively first (browser tooling, or Playwright's
   inspector) to confirm selectors/labels — **don't guess DOM structure**.
   Property-panel fields have no id/name/aria-label; they're `<span>label</span>`
   immediately followed by the input, which is what `fieldByLabel` encodes.
   Watch for real ellipsis characters (`…`, not `...`) in placeholders, and
   remember tab bars can have near-duplicate labels (e.g. an object has both
   an "Object" tab and an "Objects" tab) — `openTab`/`addElement` use exact
   role matching for this reason, not substring `:has-text()`.
4. Write `capture-<page-slug>.mjs` in `tests/e2e/docs-screenshots/`, importing
   from `./lib.mjs`, following `capture-tutorial-creating-a-simple-game.mjs`
   as the reference example.
5. Run it: `cd tests/e2e && node docs-screenshots/capture-<page-slug>.mjs http://localhost:5174`
6. `Read` each output image and check it actually matches what the doc prose
   describes before considering it done (right object names, right tab, right
   content — not just "a screenshot got saved").

## Re-running when the editor UI changes

AppShell is under active development. When its UI visibly changes in a way
that makes existing captures stale, re-run the affected `capture-*.mjs`
scripts rather than manually redoing screenshots by hand — that's the entire
point of this harness existing.
