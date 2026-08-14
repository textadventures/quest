# Editor progressive disclosure (Simple Mode successor)

Decision record. The work described here shipped in PRs #1934 (property-editor
Advanced expanders), #1935 (hide empty advanced tree categories), #1936 (script-adder
Advanced demotion), and a follow-up metadata audit of the `<advanced/>` flags.

## Background

Quest 5 had a global "Simple Mode" toggle aimed at beginners and classrooms: it hid
advanced tree categories entirely, hid any editor tab or control flagged `<advanced/>`
in the Core `.aslx` editor definitions, and filtered the script command adder to
non-advanced categories.

## Decision: disclosure, not a mode

We did **not** port Simple Mode as a global toggle:

- Modes fork the docs/tutorials and double the manual/e2e test surface.
- v5's version could make existing content invisible: open a game that uses functions
  with Simple Mode on and the Functions category simply wasn't there.
- Quest Viva already covers much of the same ground per-game rather than per-user: the
  Features tab (`game.feature_*` + `onlydisplayif`) hides whole tabs until a game opts
  in, and the tree/script-adder filter textboxes address findability.

Instead the `<advanced/>` metadata (throughout `src/Engine/Core/*.aslx`) drives
**disclosure**: advanced controls fold into a collapsed "Advanced" expander at the
bottom of each property-editor tab, and the script adder orders non-advanced categories
first with the rest below an "Advanced" divider (and, within mixed categories, sorts
advanced commands into an "Advanced" tail). Everything stays reachable and editable.

Two conventions for the flags themselves, established by the audit:

- **Feature-gated content is not also advanced-flagged.** If a command or control is
  already behind a `game.feature_*` opt-in (`onlydisplayif`), the feature toggle is the
  disclosure — flagging it advanced too would demote it for exactly the games that
  opted in. (The Player score/health/money commands are the model; darkness and frame
  picture commands were fixed to match.) Exception: the Drawing category stays
  advanced — most grid-map users never touch the custom drawing layer.
- **Tab-level `<advanced/>` is ignored.** Hiding or demoting whole tabs re-introduces
  the "where did X go" problem; the Features tab already gates the noisiest ones.

## Dormant EditorCore plumbing — keep it

EditorCore still carries the full v5 Simple Mode plumbing, all deliberately dormant:
`SimpleMode` / `SimpleModeChanged` on `EditorController`, the `m_advancedTypes` tree
filtering, `IsTabVisibleInSimpleMode` (`EditorTab.cs`), `IsControlVisibleInSimpleMode`
(`EditorControl.cs` — this one *is* consulted, inverted, by `WasmEditorBridge` to
populate the `Advanced` flags above), `IsVisibleInSimpleMode`
(`EditableScriptFactory.cs`), and `GetCategories(simpleModeOnly, showAll)`.

Don't remove it. If a locked-down classroom deployment is ever wanted, this makes a
global toggle cheap to add later (a bridge property plus UI conditionals, defaulted
per-deployment via query param rather than per-user). Wait for actual demand.
