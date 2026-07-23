# Editor progressive disclosure (Simple Mode successor)

## Background

Quest 5 had a global "Simple Mode" toggle (Tools menu on desktop; a per-user settings
cookie in the web editor, default off) aimed at beginners and classrooms. It did four
things:

1. **Tree filtering** — hid the advanced element categories entirely: Functions, Timers,
   Walkthroughs, Templates, Object Types, Included Libraries, Javascript, plus the
   Commands/Verbs nodes under `game`.
2. **Tab/control filtering** — hid any editor tab or control flagged `<advanced/>` in the
   Core `.aslx` editor definitions.
3. **Script picker filtering** — the script command adder only offered categories whose
   commands weren't flagged `advanced`.
4. Desktop-only menu/toolbar simplification.

## Decision

We are **not** porting Simple Mode as a global toggle. Reasons:

- Modes fork the docs/tutorials and double the manual/e2e test surface.
- v5's version could make existing content invisible: open a game that uses functions
  with Simple Mode on and the Functions category simply wasn't there.
- Quest Viva already covers much of the same ground per-game rather than per-user: the
  Features tab (`game.feature_*` + `onlydisplayif`) hides whole tabs until a game opts
  in, and the tree/script-adder filter textboxes address findability.

Instead we reuse the `<advanced/>` metadata (still present throughout
`src/Engine/Core/*.aslx`) for **disclosure, not removal**: advanced things are folded
away by default but always reachable, and nothing is ever uneditable.

## Current state

- **EditorCore** still carries the full v5 plumbing, all dormant: `SimpleMode` /
  `SimpleModeChanged` on `EditorController`, the `m_advancedTypes` tree filtering,
  `IsTabVisibleInSimpleMode` (`EditorTab.cs`), `IsControlVisibleInSimpleMode`
  (`EditorControl.cs`), `IsVisibleInSimpleMode` (`EditableScriptFactory.cs`), and
  `GetCategories(simpleModeOnly, showAll)`. Nothing sets `SimpleMode`, and
  `WasmEditorBridge` never consults any of the flags.
- **TreePanel.svelte** already collapses the `_advanced` group (Included Libraries,
  Templates, Dynamic Templates, Object Types, Javascript) by default on game load.
- **Toolbar.svelte** has a global "+" add menu (Add Room / Function / Timer / …) that
  doesn't depend on tree headers being visible.

## Plan

Three independent workstreams, one PR each. No shared state, no toggle, no persistence.

### 1. Advanced sections in the property editor

Fold `<advanced/>`-flagged controls into a collapsed "Advanced" expander at the bottom of
each tab, instead of rendering them inline.

- **Bridge**: add `bool Advanced` to `ControlInfo` (`WasmEditorBridge.cs`), populated
  from `IEditorControl.IsControlVisibleInSimpleMode` (inverted) in `ToControlInfo`. Apply
  to both tab controls and top-level controls.
- **AppShell**: in `PropertyEditor.svelte`, partition each tab's controls into main +
  advanced. Render the advanced group inside a collapsed `<details>`-style expander
  labelled "Advanced", preserving relative order within each partition. Tabs with no
  advanced controls render unchanged; a tab whose controls are *all* advanced renders
  them normally (an expander containing the entire tab is noise).
- **Tab-level `<advanced/>`** (`IsTabVisibleInSimpleMode`): leave tabs alone in this
  pass. Hiding or demoting whole tabs re-introduces the "where did X go" problem; the
  Features tab already gates the noisiest ones.

### 2. Hide empty advanced tree categories

Empty categories are noise for everyone, not just beginners.

- In `TreePanel.svelte`, hide a category header when it has no children, for the
  advanced set only: `_functions`, `_timers`, `_walkthrough`, and the `_advanced` group's
  children (`_include`, `_template`, `_dynamictemplate`, `_objecttype`); hide `_advanced`
  itself when all its children are hidden. Never hide `_objects`.
- The add path stays the Toolbar "+" menu, which is header-independent. Adding the first
  element of a type makes its header appear (and should select the new element, which it
  already does).
- Do this purely in the Svelte layer from the existing `GetTreeNodes()` payload — don't
  touch EditorCore's tree events, which are shared with node add/remove bookkeeping.
- Interaction with the tree filter textbox: filtering already prunes empty branches, so
  no special handling expected — verify.

### 3. Demote advanced categories in the script adder

Keep every command available, but stop presenting "Output" and "Advanced Scripts" as
peers.

- **Bridge**: add `bool Advanced` to `ScriptCategoryInfo`, computed per category from
  `EditableScriptData.IsVisibleInSimpleMode` (a category is advanced when *all* its
  visible commands are advanced, mirroring v5's `GetCategories` filter in reverse).
- **AppShell**: in `AddScriptModal.svelte`, order non-advanced categories first, then an
  "Advanced" divider, then the rest. The filter textbox searches everything regardless.

### Metadata audit (follow-up, low priority)

The `<advanced/>` flags were curated for v5 circa 2011 and have never been exercised in
Viva. Once workstream 1 ships, review the flags in `src/Engine/Core/CoreEditor*.aslx`
against what the expanders actually fold away — some judgements may want revisiting
(e.g. controls that are advanced-flagged but commonly needed, or vice versa). Cheap to
adjust: it's data, not code.

## Explicitly out of scope

- **A global Simple/Advanced toggle.** If a locked-down classroom deployment is ever
  wanted, the dormant EditorCore plumbing makes it cheap to add later (a bridge property
  plus UI conditionals, defaulted per-deployment via query param rather than per-user).
  Wait for actual demand.
- **Removing the dormant EditorCore plumbing.** It's small, tested by inheritance from
  v5, and is the escape hatch above. Leave it.
- **Gamebook mode changes.** Gamebooks already have a reduced tree (`EditorStyle`
  filtering); the same control/script changes apply there automatically and nothing
  extra is planned.

## Testing

- EditorCoreTests: none needed — no EditorCore behavior changes.
- Playwright (tests/e2e): per workstream — advanced expander opens and its controls
  round-trip edits; empty categories absent on a fresh game and appearing after add via
  the Toolbar "+" menu; script adder shows advanced categories after the divider and
  still creates their commands.
