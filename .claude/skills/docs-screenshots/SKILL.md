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

**In scope today:**
- **Editor-chrome screenshots** — pages showing the editor's own UI (tree,
  tabs, forms, dialogs). Root `site/public/images/` and
  `site/public/images/other_guides/`, roughly 150 images.
- **In-game player screenshots** (~60 images, showing gameplay output) — via
  `openPreview`/`sendCommand` (see below), which drives the *same* AppShell
  draft a capture script already built, through the real toolbar Preview
  button into a live WasmPlayer tab. No separate `.aslx` fixtures needed — one
  script can build the draft and capture both the editor states and the
  resulting gameplay in one run.

**Out of scope — do not extend this harness to these without re-scoping first:**
- **`site/public/images/helpsheets/`** (~152 images) — a large, distinct
  beginner-oriented track. Worth its own decision on approach before touching.
- **External/non-Quest images** — Trizbort app screenshots, YouTube embed UI,
  hand-drawn diagrams like `architecture.png`. Not Quest UI; nothing to regenerate.

## How it works

`tests/e2e/docs-screenshots/` (part of the `quest-e2e` Playwright package —
see `.claude/skills/verify/SKILL.md` for the base convention this extends):

- **`lib.mjs`** — shared helpers: `createLocalDraft`, `selectTreeNode`,
  `addElement`, `openTab`, `fieldByLabel`/`setLabeledField`/`selectLabeledField`,
  `capture`, `openPreview`/`sendCommand` (player screenshots, see below), and
  the `runCapture` try/finally wrapper. Reuse these rather than re-deriving
  selectors — they're built on DOM patterns already proven out across
  `tests/e2e/verify-appshell-*.mjs`.
- **One capture script per doc page**, e.g.
  `capture-tutorial-creating-a-simple-game.mjs`. Each script walks through
  *that page's own narrative* — the same room/object names and steps the
  prose already tells the reader to perform — and calls `capture()` at every
  point the doc currently embeds a screenshot, saving over the **existing
  filename** in `site/public/images/`. Same filename in, same filename out —
  no markdown edits needed.

Viewport is fixed at 960×800 (`lib.mjs`'s `VIEWPORT`), light theme, for a
consistent look across captures. Narrower than AppShell's usual 1280 dev-preview
width on purpose — Starlight downscales images to its ~700-800px content column,
so a narrower source image means less downscaling and more legible on-screen text.
`capture()` crops to the last relevant element's bottom edge (`untilLocator`)
rather than the full viewport height, since most states don't fill it.

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

## Capturing player (gameplay) screenshots

`openPreview(page)` clicks the toolbar's Preview button and returns the
WasmPlayer tab it opens (a second Playwright `page` in the same browser
context — Preview always opens via `window.open`, proxied to the same origin
so the editor tab and player tab can talk over `BroadcastChannel`; see
`vite.config.ts`'s `/player` proxy comment). **The original editor page must
stay open and untouched for the whole capture** — it's the thing answering
WasmPlayer's game-bytes request, including on every reload.

```js
const playerPage = await openPreview(page);
await sendCommand(playerPage, 'open fridge');
await capture(playerPage, out('Containerfridge.png'), { untilLocator: playerPage.locator('#txtCommand') });
```

`sendCommand` waits for the previous turn to finish (`window.canSendCommand`)
both before and after submitting, so the transcript is fully settled by the
time you call `capture()` — no arbitrary `waitForTimeout`. `capture()` itself
is unchanged and works against either page; `#txtCommand` is always the last
element in the transcript, so it's the natural `untilLocator` for player shots.

**Gotcha:** several container/object fields the tutorial text references
(e.g. "List children when object is looked at or opened") are flagged
`<advanced/>` in the `.aslx` control definition and live behind a collapsed
"Advanced" `<summary>` at the bottom of their tab (see
`tests/e2e/verify-appshell-advanced-controls.mjs`) — invisible, and
`.check()`/`.fill()` will hang waiting for visibility, until you click
`page.locator('summary', { hasText: 'Advanced' })` open first.

## Pointing at a specific control

`capture()` takes an optional `cursorAt` to draw a synthetic cursor (a real
screenshot never captures the actual OS pointer) at a locator's position —
either the locator itself (tip centered on it) or `{ locator, at }` with
`at` one of `'center' | 'left' | 'right' | 'top' | 'bottom'`, e.g. pointing at
the left edge of a dropdown you're about to describe opening. Note this only
*points at* a control — it can't show a native `<select>` actually open
(that's an OS-level popup outside anything a page screenshot can capture);
for "here's what to pick" framing on a native select, show it closed with the
cursor on it, then capture the *result* state after selecting.

## Re-running when the editor UI changes

AppShell is under active development. When its UI visibly changes in a way
that makes existing captures stale, re-run the affected `capture-*.mjs`
scripts rather than manually redoing screenshots by hand — that's the entire
point of this harness existing.
