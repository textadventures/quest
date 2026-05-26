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
// Element creation — returns new element key on success, "error:..." on failure
[JSExport] string     ValidateName(string name)
[JSExport] string     GetUniqueName(string baseName)
[JSExport] string     CreateRoom(string name, string? parent)    // parent="" means top-level
[JSExport] string     CreateObject(string name, string? parent)
[JSExport] string     CreateFunction(string name)
[JSExport] string     CreateTimer(string name)
[JSExport] string     CreateExit(string parent)       // anonymous; navigate to it to edit destination
[JSExport] string     CreateTurnScript(string parent) // anonymous
[JSExport] string     CreateCommand(string? parent)   // anonymous; parent="" means global
[JSExport] string     CreateVerb(string? parent)      // anonymous; parent="" means global
[JSExport] void       DeleteElement(string key)
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
│   │   ├── Toolbar.svelte         # AppBar: title, filename, undo/redo/save/preview
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

The browser cannot read files from the local filesystem directly.

**Option A (superseded)** — File picker open, download save
- User opens a file via `<input type="file">`
- JS reads it as `ArrayBuffer`, converts to `Uint8Array`, passes to `Initialise`
- Save triggers a download via a temporary `<a>` with an object URL

**Option B (implemented)** — File System Access API (`showDirectoryPicker`)
- The user opens a **game folder**, not just a single file. `openDirectory()` calls `showDirectoryPicker({ mode: "readwrite" })` and scans for `.aslx` files inside. If exactly one is found it auto-loads; if multiple (split-file games, custom libraries) the open page shows a file picker within the folder.
- `BrowserFileAdapter` holds a `FileSystemDirectoryHandle`; saves write the `.aslx` back to the same folder (no download prompt). `saveFileAs` uses `showSaveFilePicker({ startIn: dir })` so it opens in the right folder by default.
- The directory model is necessary because the FSA API deliberately provides no path from a `FileSystemFileHandle` to its parent — you cannot reach sibling asset files from a file handle alone.
- Non-FSA browsers (Firefox): `loadLocalFile()` falls back to `<input type="file">` + download save. A clear notice directs users to a supported browser for full asset support.

**Option C (partially implemented)** — Asset storage
- **Directory mode** (`BrowserFileAdapter`): `putAsset` / `getAsset` / `listAssets` / `deleteAsset` are real sibling files on disk inside the game folder. Assets persist across sessions, are portable (the desktop editor and other tools find them), and require no upload to any server.
- **Fallback mode** (`BrowserFileAdapter`): assets go to OPFS under a per-session UUID. They survive the session but cannot be extracted or used outside the browser. Asset upload is disabled in this mode — there is nowhere persistent to put them that the user can retrieve later.
- **Server mode** (`ServerFileAdapter`): asset operations hit `/api/editor/games/{gameId}/assets` REST endpoints (see `docs/textadventures-api.md`). This is a transitional path for games already hosted on textadventures.co.uk; the intent is that local directory mode becomes the primary workflow.
- Service Worker to intercept asset URL requests during game preview is **not yet implemented** — assets are stored correctly but are not yet resolvable as URLs inside the player preview.

**Option D (future)** — Electron wrapper
- Electron exposes Node.js `fs` to the renderer via `contextBridge` + IPC handlers in the main process — do **not** attempt to use the File System Access API inside Electron, which has known parity bugs (missing persistent permissions, broken directory iteration)
- Implement `ElectronFileAdapter` in `src/lib/filesystem/electron-adapter.ts`; the renderer calls `window.electronFileAdapter` via `contextBridge`; the main process handles `fs.readFile` / `fs.writeFile` via `ipcRenderer.invoke`
- No Svelte code changes needed — the adapter is swapped at load time

### FileAdapter interface

Implemented in `src/WebEditor/src/lib/filesystem/`. The module has no Svelte or Quest-specific imports — plain TypeScript only, so it can be extracted into a shared package when Squiffy becomes a second consumer.

**Loaders** produce a `{ bytes, adapter }` pair. `openFile` is not part of the adapter — it is a separate concern (only local-file loading needs a picker):

```typescript
loadLocalFile(): Promise<LoadedFile | null>      // FSA API or <input> fallback
loadFromServer(gameId: string): Promise<LoadedFile>  // fetches from /api/editor/games/{gameId}
```

**Adapter interface:**

```typescript
interface FileAdapter {
  readonly filename: string
  readonly canSaveAs: boolean
  readonly previewUrl: string | null   // null for local mode; server-provided URL for online mode
  saveFile(data: Uint8Array | string): Promise<void>
  saveFileAs(data: Uint8Array | string, suggestedName?: string): Promise<string | null>  // returns new filename or null if cancelled
  putAsset(key: string, data: Blob): Promise<void>
  getAsset(key: string): Promise<Blob | null>
  listAssets(): Promise<AssetInfo[]>
  deleteAsset(key: string): Promise<void>
}
```

`saveFileAs` returns the filename that was saved to (so the toolbar can update), or `null` if the user cancelled.

`previewUrl` is exposed as a Svelte store and drives the Preview button in the toolbar:

- **Server mode**: the server returns the player URL in the `X-Preview-Url` response header when loading the game. Clicking Preview saves the game then opens the URL in a new tab (the existing WebPlayer handles rendering).
- **Local mode**: `previewUrl` is `null`. Clicking Preview shows an inline banner explaining that local games must be tested in the Quest desktop app. The WebEditor cannot run a local game preview in the browser because the WASM player was removed — the Engine uses OS threads (via `Task.Run`) that are incompatible with the browser's no-shared-memory WASM model. Fixing this requires migrating the expression evaluator from Ciloci.Flee to NCalc (async-safe), which is still in progress.

### Unsaved changes

`UndoLogger` fires `TransactionCommitted` when a transaction records actual changes (not on no-op transactions or undo/redo). `EditorController` surfaces this as a `Dirty` event — matching the v5 desktop editor pattern. The bridge sets `_isDirty = true` on `Dirty` and clears it in `Save()`. The `isDirty` Svelte store drives a `*` indicator in the toolbar filename. A `beforeunload` handler on the editor page triggers the browser's native "unsaved changes" prompt if the tab is closed or navigated away while dirty.

**Known limitation**: undoing all changes back to the last-saved state does not clear dirty — the `*` stays until the next explicit save. Fixing this requires either XML hash comparison on undo or a generation-counter approach; deferred as a future improvement.

### Server-side API (textadventures.co.uk)

`ServerFileAdapter` expects the API described in `docs/textadventures-api.md`. When the editor is opened with `?game={guid}`, it auto-loads via `loadFromServer` and uses `ServerFileAdapter` for all subsequent saves and asset operations. The server API uses the user's existing session cookie — no token exchange needed.

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

### Phase 5 — Add / delete elements ✅

Tree panel and WASM bridge support creating and deleting all element types:

- **Rooms and Objects** — named, via `AddElementModal` (live name validation, unique-name suggestion)
- **Functions and Timers** — named, same modal flow
- **Exits, Commands, Verbs, Turn Scripts** — anonymous (auto-named), created immediately and selected for editing
- **Delete** — any non-header element; confirm dialog; tree and undo/redo update immediately
- Tree nodes now carry `nodeIcon` so the UI knows element type without a round-trip
- `RemovedNode`, `RenamedNode`, `RetitledNode` events now kept live in C# so `GetTreeNodes()` always returns current state

UX: hovering a tree node reveals "+" (add child) and "×" (delete) action buttons. Header nodes get a single-click "+". The Rooms exits tab with a richer exit dialog is a separate future phase.

### Phase 6 — Remaining full feature parity
- New game (from templates)
- Publish/export
- File System Access API + OPFS persistence (see File handling section)
- Asset storage for images via OPFS + Service Worker
- Test suite for the WASM bridge

### Phase 6 — Raw ASLX editor

The desktop editor (to be replaced by this WebEditor wrapped in Electron) includes a full raw-file code view. Required for parity.

- A CodeMirror 6 XML editor surfaced as a top-level toggle (whole-file view, not per-script)
- XML syntax highlighting is built into CM6 — no custom grammar needed
- Round-trips through save/reload; needs a clear "you are editing raw XML" warning since changes bypass all validation
- Natural fit for CodeMirror 6 given bundle size and the availability of the XML language package; Monaco is an alternative if a more VS Code–like experience is wanted at the cost of a much larger bundle

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
- **Shared filesystem adapter**: Implement `src/lib/filesystem/` here first (see File handling section and [squiffy#215](https://github.com/textadventures/squiffy/issues/215)); extract into a published package when Squiffy becomes the second consumer.
- **Electron wrapper**: Both Quest and Squiffy editors are candidates for an Electron app. The `FileAdapter` interface is the seam; `createElectronAdapter()` bridges to Node.js `fs` via `contextBridge` IPC. Decide whether to build one Electron shell that hosts both, or separate apps.
