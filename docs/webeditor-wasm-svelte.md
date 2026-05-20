# WebEditor: EditorCore WASM + SvelteKit

## Architecture

```
Browser
├── SvelteKit frontend  (src/WebEditor/)
│   ├── TreePanel       — game object hierarchy (Skeleton TreeView)
│   ├── PropertyEditor  — attribute display with typed controls and tab navigation
│   ├── ScriptEditor    — visual script editor with code view and copy/paste
│   ├── AddScriptModal  — categorised command picker
│   └── Toolbar         — save, undo/redo (Skeleton AppBar)
│
└── .NET WASM module  (src/WasmEditor/)
    ├── WasmEditorBridge  — thin JSExport interop layer
    └── EditorCore → Engine / Utility / Common
```

All data crossing the JS/WASM boundary is JSON. The Svelte layer calls `[JSExport]` methods on the bridge; events fire back synchronously during `Initialise` and are collected in C# lists before being returned via `GetTreeNodes()`.

---

## WasmEditor project

`src/WasmEditor/` targets `net10.0` with `<RuntimeIdentifier>browser-wasm</RuntimeIdentifier>`. It depends on `EditorCore` and `Common`.

### Implemented bridge API (`WasmEditorBridge.cs`)

```csharp
[JSExport] Task<bool> Initialise(byte[] gameFileBytes, string filename)
[JSExport] string     GetTreeNodes()           // JSON: List<{key, text, parent}>
[JSExport] string?    GetEditorData(string key) // JSON: EditorDataResponse (see below)
[JSExport] string     SetAttribute(string elementKey, string attribute, string controlType, string value)
[JSExport] string     Save()                   // returns XML
[JSExport] void       Undo()
[JSExport] void       Redo()
```

`GetEditorData` returns an `EditorDataResponse`:
```json
{
  "attributes": { "name": "My Room", "alias": null, ... },
  "tabs": [
    {
      "caption": "Setup",
      "controls": [
        { "attribute": "name", "controlType": "textbox", "caption": "Name", "options": null },
        { "attribute": "enabled", "controlType": "checkbox", "caption": "Enabled", "options": null },
        { "attribute": "direction", "controlType": "dropdown", "caption": "Direction",
          "options": [{ "value": "north", "label": "north" }, ...] }
      ]
    }
  ],
  "controls": []
}
```

`SetAttribute` converts `value` (always a string) to the correct .NET type based on `controlType` (`checkbox` → `bool`, `number` → `int`, `numberdouble` → `double`, otherwise `string`), wraps in a transaction for undo support, and returns `"ok"` or an error message.

Tree nodes are collected during `Initialise` by subscribing to `EditorController.AddedNode`, then returned as a flat list via `GetTreeNodes()`.

### Not yet implemented

- Live JS callbacks (JSImport) — events currently captured into C# state at init time rather than pushed to JS as they occur

### Supporting types

- `WasmConfig` (`src/WasmEditor/WasmConfig.cs`) — `IConfig` implementation with `UseNCalc = true`
- `ByteArrayGameDataProvider` (`src/Common/`) — wraps a `byte[]` as a `MemoryStream` so `EditorController` can load a file passed in from the browser File API

### Build

```bash
dotnet build src/WasmEditor --configuration Debug
```

Output lands in `src/WasmEditor/bin/Debug/net10.0/browser-wasm/AppBundle/`. The Vite dev server serves this directory at `/AppBundle/` (see `vite.config.ts`).

---

## SvelteKit frontend

`src/WebEditor/` is a SvelteKit SPA (adapter-static, `fallback: 'index.html'`).

### Stack

- **Svelte 5** + **SvelteKit 2**
- **Tailwind CSS 4** via `@tailwindcss/vite`
- **Skeleton UI v4** (`@skeletonlabs/skeleton` + `@skeletonlabs/skeleton-svelte`) — cerberus theme
- **ESLint** with `typescript-eslint` + `eslint-plugin-svelte` (double quotes, semicolons, 4-space indent)

### File structure

```
src/WebEditor/
├── eslint.config.mjs
├── svelte.config.js        # adapter-static, $components alias
├── vite.config.ts          # AppBundle middleware, COOP/COEP headers
├── src/
│   ├── app.css             # @import tailwindcss + skeleton + cerberus theme
│   ├── app.html            # data-theme="cerberus"
│   ├── lib/
│   │   ├── wasm.ts         # loads dotnet.js, exposes WasmBridge
│   │   ├── editor-store.ts # Svelte stores + wrappers for all WASM calls
│   │   └── types.ts        # TreeNode, ControlInfo, TabInfo, EditorDataResponse, ScriptNodeData, …
│   ├── components/
│   │   ├── Toolbar.svelte         # AppBar: title, filename, undo/redo/save
│   │   ├── TreePanel.svelte       # Skeleton TreeView (flat→hierarchy conversion)
│   │   ├── PropertyEditor.svelte  # Typed controls with tab navigation
│   │   ├── ScriptEditor.svelte    # Visual script editor (recursive); code view; copy/paste
│   │   └── AddScriptModal.svelte  # Categorised command picker modal
│   └── routes/
│       ├── +layout.svelte  # imports app.css
│       ├── +layout.ts      # prerendering config
│       ├── +page.svelte    # Welcome: file picker → openGame() → /editor
│       └── editor/
│           └── +page.svelte  # Editor layout: Toolbar + TreePanel + PropertyEditor
```

### Running the dev server

Requires a WasmEditor Debug build first (see above), then:

```bash
cd src/WebEditor
npm run dev     # http://localhost:5174
```

The Vite dev server sets `Cross-Origin-Opener-Policy: same-origin` and `Cross-Origin-Embedder-Policy: require-corp` headers, which are required for the .NET WASM runtime's use of `SharedArrayBuffer`.

### WASM loading

`wasm.ts` uses `new Function("url", "return import(url)")` to load `dotnet.js` at runtime. This prevents Vite's import-analysis plugin from trying to resolve the URL at build time (the file only exists as a runtime-served AppBundle asset).

---

## File handling

The browser cannot read files from the local filesystem directly. Currently implemented: **Option A**.

**Option A (implemented)** — File picker open, download save
- User opens a file via `<input type="file">`
- JS reads it as `ArrayBuffer`, converts to `Uint8Array`, passes to `Initialise`
- Save triggers a download via a temporary `<a>` with an object URL

**Option B (future)** — File System Access API + OPFS
- Chrome/Edge: use `showOpenFilePicker` / `showSaveFilePicker` for direct read/write to the user's real files without a download prompt; store recent file handles in IndexedDB for "recent files"
- Safari/Firefox: fall back to `<input type="file">` open + download save (no persistent handle)
- Consider using [`browser-fs-access`](https://github.com/GoogleChromeLabs/browser-fs-access) (Google Chrome Labs, ~5KB) rather than hand-rolling the feature detection — the Squiffy editor has already done this (see [squiffy#215](https://github.com/textadventures/squiffy/issues/215))
- OPFS (Origin Private File System) for auto-save: supported on Chrome, Safari 15.2+, Firefox 111+; game files live in browser private storage with no prompts after initial import

**Option C (future)** — Asset storage for images
- Images referenced in game files need to survive across sessions and be resolvable during preview
- Store blobs in OPFS (preferred over raw IndexedDB for binary assets)
- A Service Worker intercepts requests for asset URLs during preview and serves from OPFS
- This avoids needing a server round-trip for user-uploaded images

**Option D (future)** — Electron wrapper
- Electron exposes Node.js `fs` to the renderer via `contextBridge`
- A thin adapter (~20 lines) behind a common `FileAdapter` interface would allow the same Svelte code to work in both browser PWA and Electron
- See shared adapter design below

**Shared filesystem adapter** (future, coordinate with Squiffy)

Both Quest WebEditor and Squiffy face the same three-tier file system problem (File System Access API → OPFS → Electron `fs`). The intent is to extract a shared TypeScript package rather than duplicate the pattern. Proposed interface:

```typescript
interface FileAdapter {
  openFile(opts?: OpenOptions): Promise<FileContent | null>
  saveFile(data: Uint8Array | string): Promise<boolean>
  saveFileAs(data: Uint8Array | string, suggestedName?: string): Promise<boolean>
  putAsset(key: string, data: Blob): Promise<void>
  getAsset(key: string): Promise<Blob | null>
}
```

Factory functions: `createBrowserAdapter()`, `createOPFSAdapter()`, `createElectronAdapter()`.

---

## Phased delivery

### Phase 1 — WASM proof of concept ✅
- Removed `System.Data.DataSetExtensions` from `EditorCore.csproj`
- Changed `EditorController.Initialise` to accept `IGameDataProvider` instead of a filename string
- Created `WasmEditor` project, compiles to WASM with zero warnings
- Exported minimal API: init, get tree, get element data, save, undo, redo
- `ByteArrayGameDataProvider` in `Common` wraps `byte[]` from JS
- Source-generated JSON serialisation (`JsonSerializerContext`) throughout

### Phase 2 — SvelteKit skeleton ✅
- SvelteKit SPA at `src/WebEditor/`; Vite AppBundle middleware for WASM serving
- File picker → bytes → `Initialise` → tree rendered with Skeleton `TreeView`
- Click a tree node → attribute list in `PropertyEditor`
- Toolbar: save (file download), undo, redo
- Skeleton UI v4 + Tailwind CSS 4 + ESLint configured

### Phase 3 — Property editors ✅

`PropertyEditor` replaced with typed controls and tab navigation:

1. `SetAttribute(elementKey, attribute, controlType, value)` added to `WasmEditorBridge` — type-converts `value` based on `controlType`, wraps in undo transaction
2. `GetEditorData` extended to return `EditorDataResponse` with `attributes`, `tabs`, and `controls` — each control carries `controlType`, `caption`, and `options` (for dropdowns)
3. `PropertyEditor.svelte` maps `controlType` to inputs: `textbox`/`richtext` → text input, `checkbox` → checkbox, `number`/`numberdouble` → number input, `dropdown` → select with options, `script` → Phase 4 placeholder, others → read-only display
4. Tab navigation: tabs from the EditorDefinition are rendered as a tab bar; clicking a tab switches the visible controls
5. `editor-store.ts` now exports `selectedData` (replacing `selectedAttributes`) and `setAttribute`

Known limitations for this phase: live C#→JS events not yet implemented.

### Phase 4 — Script editor ✅

Visual script editor, code view, and copy/paste — full feature parity with the v5 web editor for script editing.

**Visual editor** (`ScriptEditor.svelte`)
- Script commands render as inline rows: labels + textboxes/dropdowns/expression inputs interleaved in a sentence-like layout (`Print [textbox]`, `Set [dropdown] of [dropdown] to [expression]`)
- Nested scripts (if/then/else, for, foreach) recurse into child `ScriptEditor` instances with their own Add/Delete buttons
- `if` blocks special-cased: condition + Then block, with Add Else If / Add Else buttons
- Expression parameters offer a simple/expression toggle: friendly widget (dropdown, number, object picker) when the value matches a simple form; raw text input for arbitrary expressions
- `if` condition additionally offers a template picker (36 predefined patterns from `usetemplates="if"`) for the most common conditions
- Move up/down and delete via hover action buttons on each row

**Add script** (`AddScriptModal.svelte`)
- Categorised command picker with quick-add shortcuts for common commands
- Keyboard: Escape cancels, Enter confirms, double-click adds immediately

**Code view**
- Toggle button switches the block between the visual editor and a monospace textarea showing the raw Quest script text
- Round-trips through `EditableScripts.Code` (`Save()` / `LoadCode()`); works correctly on empty attributes by auto-creating the container

**Copy / cut / paste**
- Checkbox on each root-level row; checking any shows a selection toolbar with Cut, Copy, Delete, and (single selection) Move Up / Move Down
- Cut/Copy use `IEditableScripts.Cut/Copy` — clipboard lives in `EditorController.m_clipboardScripts`
- Paste appends clipboard scripts at the end of the container; handles empty (unset) attributes by creating the container first
- `scriptClipboardHasContent` Svelte store controls Paste button visibility

**Bridge additions** (all `[JSExport]` on `WasmEditorBridge`):
`GetScriptData`, `SetScriptParameter`, `SetIfExpression`, `SetElseIfExpression`, `AddScript`, `DeleteScript`, `DeleteScripts`, `MoveScript`, `AddElse`, `AddElseIf`, `RemoveElse`, `RemoveElseIf`, `GetScriptCommandCategories`, `GetObjectNames`, `GetIfExpressionTemplates`, `GetIfExpressionTemplateData`, `GetScriptCode`, `SetScriptCode`, `CopyScripts`, `CutScripts`, `PasteScripts`, `CanPasteScript`

### Phase 5 — Full feature parity
- New game (from templates)
- Publish/export
- File System Access API + OPFS persistence (see File handling section)
- Asset storage for images via OPFS + Service Worker
- Test suite for the WASM bridge

---

## Script editor design

### Editor modes (planned)

Three rendering modes over the same `IEditableScript` model, toggleable per-script:

| Mode | Description |
|---|---|
| **Visual (v5-style)** | Inline form rows with typed controls per parameter ✅ |
| **Code view** | Raw Quest script text — round-trips through `EditableScripts.Code` / `Save()` ✅ |
| **Blockly** | Block-based drag-and-drop — future, see below |

All three modes read/write the same underlying script tree. Switching mode is a pure UI concern.

### Expression parameters

Each script command's parameters are defined in `CoreEditorScripts.aslx` with a control type (`expression`, `textbox`, `dropdown`, etc.) and an optional `simpleeditor` hint (`boolean`, `number`, `objects`, `dropdown`, `file`).

For `expression`-type parameters the editor shows:
- A **simple widget** (dropdown, object picker, number input) when the value matches a known simple form
- A **raw text input** fallback for arbitrary Quest expressions

This simple/expression toggle is the correct design boundary — Quest's expression language is a full programming language (NCalc/Flee evaluator, 80+ built-in functions, arbitrary nesting) and is not fully representable as structured UI.

The `usetemplates` mechanism (e.g. `usetemplates="if"`) provides 36 predefined expression patterns — common if-conditions, set targets, foreach lists — each with its own structured controls. These cover the majority of expressions authors write.

### Blockly (future)

Blockly was evaluated as a future alternative to the v5-style visual editor. Key findings:

**What maps well**: script command *structure* (sequence, if/else/elseif, for, foreach) maps naturally to Blockly statement blocks. The existing 36 expression templates map to Blockly value blocks with typed inputs.

**The expression challenge**: Quest expressions are typed, dynamically-evaluated, and can be arbitrarily nested. Full blockification would require ~50+ value blocks for the common built-in functions/operators (Tier 1–2) plus a permanent raw-text fallback block for anything outside that set. Completely eliminating the text fallback is impractical given the 80+ built-in functions and free-form expression syntax.

**Recommended future approach**:
- Blockify script *structure* (commands, control flow) — clean 1:1 mapping
- Keep text input fields *inside* blocks for expression parameters, using the template system's 36 patterns as structured block inputs where applicable
- Raw-text fallback block for expressions outside the template set
- Auto-generate block definitions from `CoreEditorScripts.aslx` XML rather than hand-authoring 60+ blocks

Blockly is ~800KB and requires a TypeScript bridge layer to synchronise the block workspace with the WASM `IEditableScript` tree. Worth revisiting after the v5-style editor is stable.

---

## Open questions

- **EditorCore API cleanup**: `EditorMode` (Desktop/Web distinction) and `AddControlType`/`GetControlType` (.NET UI control registration) are dead weight now the only consumer is Svelte. Remove in a future cleanup pass.
- **Live C# → JS events**: Currently tree state is snapshot-based (collected at init). As editing adds/removes/renames nodes, the bridge will need to push updates to JS rather than relying on a full re-fetch.
- **Build integration**: Should `WasmEditor` output be served by `WebPlayer` (embedded SPA) or deployed separately?
- **Auth / server-side storage**: Is cloud save in scope? Affects whether the editor stays purely client-side.
- **Shared filesystem adapter**: Extract a common `FileAdapter` TypeScript package shared with Squiffy (see File handling section and [squiffy#215](https://github.com/textadventures/squiffy/issues/215)).
- **Electron wrapper**: Both Quest and Squiffy editors are candidates for an Electron app. The `FileAdapter` interface is the seam; `createElectronAdapter()` would be a thin IPC shim. Decide whether to build one Electron shell that hosts both, or separate apps.
