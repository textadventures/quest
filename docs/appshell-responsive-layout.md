# AppShell responsive layout plan

Status: **planned, not yet implemented** (July 2026).

## Context

The editor (`src/AppShell`, SvelteKit + Tailwind 4 + Skeleton v4) works well at desktop widths but is unusable on phones: the toolbar has ~12 always-visible buttons that overflow, the tree + properties panes are hard-coded side-by-side with a pointer-only splitter, hover-revealed controls (tree `⋯` menus, script row copy/paste/delete) are invisible on touch, and several modals have fixed pixel widths wider than a phone screen.

More toolbar functionality is coming (cut/copy/paste/move elements, code view, dark mode toggle, library-elements-in-tree), so this restructure also creates scalable homes for those — without implementing them as part of this work.

**Decisions:**

- **Mobile panes**: master-detail push — tree is the full-screen home; tapping an element pushes a full-screen properties view with a back button. Desktop (≥ `md`, 768px) keeps the current side-by-side + splitter layout.
- **Toolbar**: reorganized at *all* widths — grouped dropdown menus, not just a mobile collapse.
- **Scope**: responsive-only. New features land later in the homes this plan reserves.

## Key files

- `src/AppShell/src/routes/edit/+page.svelte` — editor shell: Toolbar + TreePanel + splitter + PropertyEditor
- `src/AppShell/src/components/Toolbar.svelte` — AppBar with all buttons; has an inline "add-dropdown" pattern
- `src/AppShell/src/components/TreePanel.svelte` — tree with filter box, per-node `⋯` dropdown (`nodeMenuOptions`), fixed `width` px prop
- `src/AppShell/src/components/PropertyEditor.svelte` — "Properties" header, tab strip, control rows; `textProcessorPanel` renders beside richtext textareas
- `src/AppShell/src/components/ScriptEditor.svelte` — script rows already `flex-wrap`; hover-only row buttons at line ~395
- `src/AppShell/src/components/AttributesEditor.svelte` — attributes table + mouse-only splitter + fixed-width (360px default) "Assignment" panel
- `src/AppShell/src/components/VerbsEditor.svelte` — verbs table + same splitter pattern + fixed-width (480px default) "Behaviour" panel
- Modals: `AddElementModal` (`w-80`), `PublishModal` (`w-[28rem]`), `AssetManagerModal` (`w-[32rem]`), `AddScriptModal` (`w-[600px] h-[520px]`)
- `src/AppShell/src/app.css` — Skeleton theme overrides (tree-view data-scope selectors live here)
- `src/AppShell/src/app.html` — viewport meta
- `src/AppShell/src/lib/editor-store.ts` — `selectedKey`, `selectNode`, `treeNodes`, etc. (no changes expected)

Existing conventions to reuse: Lucide icons via `@lucide/svelte/icons/*`; the outside-click dropdown `$effect` pattern (Toolbar add-dropdown, TreePanel tree-dropdown); Tailwind color pairs (`surface-200-800` etc.).

## Implementation

### 1. Layout primitives

- `src/AppShell/src/lib/layout.svelte.ts` (new): export `isNarrow = new MediaQuery("(max-width: 767px)")` from `svelte/reactivity` — one shared reactive source for behavioral (not just cosmetic) breakpoint switches. Cosmetic changes use Tailwind `md:` / `pointer-coarse:` classes directly.
- `app.html`: viewport meta → `width=device-width, initial-scale=1, maximum-scale=1, viewport-fit=cover`. `maximum-scale=1` stops iOS Safari's auto-zoom on focus of the editor's 12px inputs (iOS still allows user pinch-zoom, so it's not an a11y regression); the alternative (16px inputs on mobile) would wreck the editor's density.
- `app.css`: `@media (pointer: coarse)` block bumping `padding-block` on `[data-scope='tree-view'] [data-part='item'|'branch-control']` for finger-sized hit targets.
- `routes/edit/+page.svelte`: root container `h-svh` → `h-dvh` so the layout tracks iOS URL-bar collapse instead of leaving a dead strip.

### 2. Extract a shared `DropdownMenu` component

`src/AppShell/src/components/DropdownMenu.svelte` (new): trigger snippet + item list (`{label, action}`, optional icon/divider/disabled), the existing outside-click `$effect`, `Escape` to close, right-or-left alignment prop. Replaces the hand-rolled add-dropdown in `Toolbar.svelte` and backs the two new menus below. (TreePanel's fixed-position node dropdown stays as-is — its positioning is different.)

### 3. Toolbar reorganization (`Toolbar.svelte`)

Desktop (≥ md):

```
[⌂] filename [save-chip] ......... [+ Add ▾] [🗑 Delete] | [image][undo][redo] | [File ▾] [▶ Preview] [⋯]
```

- **File ▾** menu (new, DropdownMenu): Save As…, Backup…, Publish… — same visibility conditions (`$canSaveAs`, `$canBackup`, `$gameFilename`) now hide individual items instead of buttons.
- **⋯ overflow** menu (new): Discord, GitHub links. *Reserved future home for: dark mode toggle, and anything that doesn't earn a top-level slot.*
- Filename gets `truncate min-w-0` inside a `flex-1 min-w-0` lead so it shrinks first.

Mobile (< md, via Tailwind `hidden md:flex` / `md:hidden` groupings — keep one AppBar, toggle classes rather than duplicating markup):

```
[⌂] filename… [chip] .... [+ ▾] [▶] [⋯]
```

- Add and Preview become icon-only (`title` retained).
- Save chip: icon-only (drop the text + `min-width`) below md.
- **⋯** absorbs everything else: Delete (disabled state as today), Undo, Redo, Assets, Save As, Backup, Publish, Discord, GitHub — with dividers between groups.
- Delete/Undo/Redo need their store-driven disabled states passed into menu items.

### 4. Master-detail on mobile (`routes/edit/+page.svelte`)

- New local state `mobilePane = $state<"tree" | "props">("tree")`.
- TreePanel gets an `onactivate?: () => void` callback prop, called from its existing `onSelectionChange` handler after `selectNode(...)` — the page sets `mobilePane = "props"` there. Programmatic selection (game load auto-select) doesn't fire it, so no spurious pushes.
- Rendering: when `isNarrow.current`, render a single full-width pane per `mobilePane`; splitter div not rendered. Desktop branch unchanged (keeps `treeWidth` splitter logic). Keep both panes mounted where practical (`hidden` class rather than `{#if}`) so tree expansion state and property tab state survive switching.
- PropertyEditor gets a `onback?: () => void` prop: when set (mobile), its "Properties" header row becomes a back header — `← {selected element name}` button (Lucide `chevron-left`), calling back into the page to set `mobilePane = "tree"`. Element name comes from `treeNodes`/`selectedKey` (same derivation as Toolbar's `selectedNode`).
- Toolbar's Delete already acts on `selectedKey`, so it works from either pane.

### 5. TreePanel

- `width` prop becomes optional; mobile renders `w-full` (no inline style), desktop passes px as today.
- Touch visibility: node `⋯` wrapper `opacity-0 group-hover:opacity-100` → add `pointer-coarse:opacity-100`.
- *Reserved future homes: Cut/Copy/Paste/Move → new entries in `nodeMenuOptions` (the per-node `⋯` menu) plus keyboard shortcuts on desktop; "show library elements" toggle → an icon button in the "Game Objects" header row next to the filter box.*

### 6. PropertyEditor + ScriptEditor

- Tab strip already `overflow-x-auto` — fine.
- Richtext + text-processor side panel (`.richtext-wrap`, 3 occurrences): stack vertically on narrow *containers* (the pane can be narrow on desktop too when the splitter is dragged). Make the properties pane a container (`@container`) and switch `.richtext-wrap` to `flex-col @2xl:flex-row` (Tailwind 4 container queries, no plugin needed). The text-processor command list also gets `max-h` + `overflow-y-auto` when stacked so it can't dwarf the textarea.
- ScriptEditor hover-revealed row buttons (copy/paste/delete, ~line 395): add `pointer-coarse:opacity-100`.
- ScriptEditor "if/else" style nested rows already wrap; no structural change.

### 7. AttributesEditor + VerbsEditor split panes

Both have an internal list + draggable splitter + fixed-width right panel (Assignment / Behaviour). Same treatment for each, driven by container width (the properties pane container from step 6), stacking below `@2xl` (672px container):

- Split wrapper: `flex flex-col @2xl:flex-row` — on narrow, the list stacks on top with the Assignment/Behaviour panel below it (still above AttributesEditor's "Add attribute" footer).
- Right panel width: replace `style="width: {panelWidth}px"` with `style="--panel-width: {panelWidth}px"` + classes `w-full @2xl:w-[var(--panel-width)] @2xl:flex-shrink-0` so the inline style can't defeat the stacked layout.
- Splitter: `hidden @2xl:block`; also convert both from `onmousedown`/mousemove to pointer events (same pattern as `routes/edit/+page.svelte`'s `handleSplitterPointerDown`) so iPad-width touch devices can drag it.
- When stacked and a row is selected, `scrollIntoView({ block: "nearest" })` the panel so the editor doesn't open off-screen below a long attributes table.
- AttributesEditor's "Source" column: `hidden @lg:table-cell` (both header and cells) so Name/Value stay readable at phone widths; row `title` tooltips already carry the info.

### 8. Modal sizing pass

All five modal cards get the same treatment: overlay container gains `p-4`; card fixed widths become `w-full max-w-<current>` (e.g. `w-80` → `w-full max-w-80`); `AddScriptModal`'s `w-[600px] h-[520px]` → `w-full max-w-[600px] h-[min(520px,85dvh)]`; `AssetManagerModal` `max-h-[80vh]` → `max-h-[85dvh]`. AddScriptModal's two-column category browser (`w-40` sidebar) is acceptable at 375px width; leave unless testing shows otherwise.

### 9. Out of scope / untouched

- `/open`, PlayCatalog, `/play/[id]`, HomeHeader/HomeTabs — already centered/narrow or partially responsive; not part of this change beyond anything found broken during verification.
- No new features (cut/copy/paste, code view, dark mode, library elements) — only their homes as noted above.

## Verification

1. `cd src/AppShell && npm run check && npm run lint`.
2. Runtime-verify via Playwright: load the editor with a sample game, then
   - Desktop viewport (1280×800): toolbar shows new File ▾ / ⋯ menus; splitter still drags; add/delete/undo flows work from menus.
   - iPhone viewport (375×667, `hasTouch`): toolbar fits with no horizontal overflow; tree full-width; tapping a room pushes properties with back header; back returns with tree state intact; node `⋯` visible without hover; each modal opens fully on-screen; richtext text-processor panel stacked below textarea; Attributes tab and an object's Verbs tab stack list-above-panel and selecting a row scrolls the editor into view.
   - Screenshot both widths for eyeballing.
3. Electron unaffected check: it always runs at desktop size, but confirm `npm run check` covers shared components; no Electron-specific code touched.
