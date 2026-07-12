# WebEditor: EditorCore WASM + SvelteKit

## Status

Core editing is done and in daily use: open/create a game, browse and edit the tree, edit
attributes and scripts (visual + code view), add/delete every element type, undo/redo, save
locally (File System Access API, with an `<input>`/download fallback) or to
textadventures.co.uk, upload/browse/delete image and sound assets, and preview in WasmPlayer.
See [Remaining work](#remaining-work) below for what's left.

## Architecture

```
Browser
├── SvelteKit frontend  (src/WebEditor/)
│   ├── TreePanel        — game object hierarchy (Skeleton TreeView)
│   ├── PropertyEditor   — attribute display with typed controls and tab navigation
│   ├── ScriptEditor      — visual script editor with code view and copy/paste
│   ├── AddScriptModal    — categorised command picker
│   ├── AddElementModal   — new room/object/function/timer/template/etc.
│   └── Toolbar           — save, undo/redo, preview (Skeleton AppBar)
│
└── .NET WASM module  (src/WasmEditor/)
    ├── WasmEditorBridge  — thin JSExport interop layer
    └── EditorCore → Engine / Utility / Common
```

All data crossing the JS/WASM boundary is JSON. The Svelte layer calls `[JSExport]` methods on the bridge; events fire back synchronously during `Initialise` and are collected in C# lists/flags before being returned or polled (`GetTreeNodes()`, `IsDirty()`, etc.) — see [Not yet implemented](#not-yet-implemented).

---

## WasmEditor project

`src/WasmEditor/` targets `net10.0` with `<RuntimeIdentifier>browser-wasm</RuntimeIdentifier>`. It depends on `EditorCore` and `Common`.

### Bridge API (`WasmEditorBridge.cs`)

The bridge now exposes ~65 `[JSExport]` methods and has grown well past what's practical to hand-maintain as a list here — `WasmEditorBridge.cs` is the source of truth. Broad categories:

- **Lifecycle**: `Initialise`, `Save`, `IsDirty`, `CanUndo`/`CanRedo`, `Undo`/`Redo`
- **Tree / attributes**: `GetTreeNodes`, `GetEditorData`, `GetFullAttributeData`, `SetAttribute`, `RemoveAttribute`, `ChangeAttributeType`, `SetMultiType`, `SetObjectReference`, `SetDropdownType`, `SetPatternAttribute`
- **Lists / dictionaries**: `AddListItem`/`RemoveListItem`/`UpdateListItem`, `AddDictionaryItem`/`RemoveDictionaryItem`/`UpdateDictionaryItem`, `MakeScriptEditable`, `MakeScriptDictEditable`, `AddScriptDictionaryItem`/`RemoveScriptDictionaryItem`
- **Scripts**: `GetScriptData`, `SetScriptParameter`, `SetIfExpression`/`SetElseIfExpression`, `AddScript`/`DeleteScript`/`DeleteScripts`/`MoveScript`, `AddElse`/`AddElseIf`/`RemoveElse`/`RemoveElseIf`, `GetScriptCode`/`SetScriptCode`, `CopyScripts`/`CutScripts`/`PasteScripts`/`CanPasteScript`, `GetScriptCommandCategories`, `GetObjectNames`, `GetIfExpressionTemplates`/`GetIfExpressionTemplateData`
- **Element creation/deletion**: `ValidateName`, `GetUniqueName`, `CreateRoom`/`CreateObject`/`CreateFunction`/`CreateTimer`/`CreateExit`/`CreateTurnScript`/`CreateCommand`/`CreateVerb`/`CreateWalkthrough`/`CreateTemplate`/`CreateDynamicTemplate`/`CreateObjectType`/`CreateIncludedLibrary`/`CreateJavascript`, `DeleteElement`, `SwapElements`
- **Types**: `AddInheritedType`/`RemoveInheritedType`, `GetTypeNames`
- **Templates**: `GetGameTemplates`, `CreateGameFromTemplate`

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
          "options": [{ "value": "north", "label": "north" }, ...] },
        { "attribute": "cover", "controlType": "file", "caption": "Cover art",
          "source": "*.jpg;*.jpeg;*.png;*.gif" }
      ]
    }
  ],
  "controls": []
}
```

`SetAttribute` converts `value` (always a string) to the correct .NET type based on `controlType` (`checkbox` → `bool`, `number` → `int`, `numberdouble` → `double`, otherwise `string`), wraps in a transaction for undo support, and returns `"ok"` or an error message.

Both `ControlInfo` and `ScriptControlData` (the latter used for `simpleeditor="file"` script parameters like "Play sound"/"Show picture", not just attribute-level `file` controls) carry a `source` field — the control's `<source>` filter (e.g. `*.jpg;*.jpeg;*.png;*.gif` or `*.wav;*.mp3`), resolved from any `[TemplateName]` reference (e.g. `[EditorImageFormats]`) by `EditorControl.GetString` before it reaches JS. The frontend's `AssetPicker.svelte` parses this into an upload `accept` filter, a dropdown filter, and an image/non-image thumbnail decision — confirmed empirically that the template resolution happens correctly rather than passing through as a literal bracketed string.

Tree nodes are collected during `Initialise` by subscribing to `EditorController.AddedNode`, then returned as a flat list via `GetTreeNodes()`.

### Not yet implemented

- Live JS callbacks (`[JSImport]`) — events are still captured into C# state (flags, lists) and polled/re-fetched from JS rather than pushed as they occur. Fine for single-user editing; would matter for any future collaborative or long-running-background-task scenario.

### Supporting types

- `WasmConfig` (`src/WasmEditor/WasmConfig.cs`) — `IConfig` implementation with `UseNCalc = true`
- `ByteArrayGameDataProvider` (`src/Common/`) — wraps a `byte[]` as a `MemoryStream` so `EditorController` can load a file passed in from the browser File API

### Build

```bash
dotnet build src/WasmEditor --configuration Debug
```

Output lands in `src/WasmEditor/bin/Debug/net10.0/browser-wasm/AppBundle/`. The Vite dev server serves this directory at `/AppBundle/` (see `vite.config.ts`). Set `WASM_CONFIG=Release` to serve the AOT-compiled build instead (e.g. for profiling).

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
├── vite.config.ts          # AppBundle middleware, WasmPlayer proxy for preview
├── src/
│   ├── app.css             # @import tailwindcss + skeleton + cerberus theme
│   ├── app.html             # data-theme="cerberus"
│   ├── lib/
│   │   ├── wasm.ts          # loads dotnet.js, exposes WasmBridge
│   │   ├── editor-store.ts  # Svelte stores + wrappers for all WASM calls
│   │   ├── types.ts         # TreeNode, ControlInfo, TabInfo, EditorDataResponse, ScriptNodeData, …
│   │   └── filesystem/      # FileAdapter interface + browser/server implementations
│   ├── components/
│   │   ├── Toolbar.svelte             # AppBar: title, filename, undo/redo/save/preview
│   │   ├── TreePanel.svelte           # Skeleton TreeView (flat→hierarchy conversion)
│   │   ├── PropertyEditor.svelte      # Typed controls with tab navigation
│   │   ├── AttributesEditor.svelte    # Raw attribute list editor
│   │   ├── ScriptEditor.svelte        # Visual script editor (recursive); code view; copy/paste
│   │   ├── ScriptDictionaryEditor.svelte
│   │   ├── AddScriptModal.svelte      # Categorised command picker modal
│   │   ├── AddElementModal.svelte     # New room/object/function/.../template modal
│   │   ├── ElementsList.svelte        # Objects/Verbs/Commands/Functions/Timers list views
│   │   ├── AssetPicker.svelte         # Image/sound picker for "file" attribute & script controls
│   │   ├── AssetManagerModal.svelte   # Standalone browse/upload/delete assets modal
│   │   ├── ListEditor.svelte
│   │   ├── DictionaryEditor.svelte
│   │   └── Combobox.svelte
│   └── routes/
│       ├── +layout.svelte  # imports app.css
│       ├── +layout.ts      # prerendering config
│       ├── +page.svelte    # Editor layout (Toolbar + TreePanel + PropertyEditor), or redirect to /open if no game loaded
│       └── open/
│           └── +page.svelte  # File picker / create-new-game / server load
```

### Running the dev server

Requires a WasmEditor Debug build first (see above), then:

```bash
cd src/WebEditor
npm run dev     # http://localhost:5174
```

### WASM loading

`wasm.ts` uses `new Function("url", "return import(url)")` to load `dotnet.js` at runtime. This prevents Vite's import-analysis plugin from trying to resolve the URL at build time (the file only exists as a runtime-served AppBundle asset).

---

## File handling

The browser cannot read files from the local filesystem directly.

**Option A (superseded)** — File picker open, download save
- User opens a file via `<input type="file">`
- JS reads it as `ArrayBuffer`, converts to `Uint8Array`, passes to `Initialise`
- Save triggers a download via a temporary `<a>` with an object URL
- Still used as the fallback path for browsers without the File System Access API (Firefox)

**Option B (implemented)** — File System Access API (`showDirectoryPicker`)
- The user opens a **game folder**, not just a single file. `openDirectory()` calls `showDirectoryPicker({ mode: "readwrite" })` and scans for `.aslx` files inside. If exactly one is found it auto-loads; if multiple (split-file games, custom libraries) the open page shows a file picker within the folder.
- `BrowserFileAdapter` holds a `FileSystemDirectoryHandle`; saves write the `.aslx` back to the same folder (no download prompt). `saveFileAs` uses `showSaveFilePicker({ startIn: dir })` so it opens in the right folder by default.
- The directory model is necessary because the FSA API deliberately provides no path from a `FileSystemFileHandle` to its parent — you cannot reach sibling asset files from a file handle alone.
- Non-FSA browsers (Firefox): `loadLocalFile()` falls back to `<input type="file">` + download save. A clear notice directs users to a supported browser for full asset support.

**Option C (implemented)** — Asset storage
- **Directory mode** (`BrowserFileAdapter`): `putAsset` / `getAsset` / `listAssets` / `deleteAsset` are real sibling files on disk inside the game folder. Assets persist across sessions, are portable (the desktop editor and other tools find them), and require no upload to any server.
- **Fallback mode** (`BrowserFileAdapter`): assets go to OPFS under a per-session UUID. They survive the session but cannot be extracted or used outside the browser. Upload works the same as directory mode — there was never an adapter-level restriction here, an earlier version of this doc claimed otherwise.
- **Server mode** (`ServerFileAdapter`): asset operations hit `/api/editor/games/{gameId}/assets` REST endpoints (see `docs/textadventures-api.md`).
- `AssetPicker.svelte` (used by both `file`-type attribute controls and `simpleeditor="file"` script parameters, e.g. "Play sound"/"Show picture") and the standalone `AssetManagerModal.svelte` (opened from the toolbar's "Assets" button) both consume these adapter methods via a shared `assets` store in `editor-store.ts`. Pickers filter the asset list and restrict uploads by the control's `<source>` extension filter (resolved server-side by `WasmEditorBridge` — see the `Source` field below) — an image control won't offer a `.wav` file and vice versa; unknown control kinds fall back to showing every asset with no thumbnail.
- Asset URL resolution inside the WasmPlayer preview does **not** need a Service Worker — an earlier version of this doc said one was required and that assets weren't resolvable in preview. That was already wrong by the time it was written: `previewInWasmPlayer()` in `editor-store.ts` implements a `resource-request`/`resource-response` protocol over a `BroadcastChannel`, and `getResourceUrl()` in `wasm-player.js` (registered as the Engine's resource-lookup callback) already consumes it, for both editor-preview and normal server-hosted play.

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
  saveFile(data: Uint8Array | string): Promise<void>
  saveFileAs(data: Uint8Array | string, suggestedName?: string): Promise<string | null>  // returns new filename or null if cancelled
  putAsset(key: string, data: Blob): Promise<void>
  getAsset(key: string): Promise<Blob | null>
  listAssets(): Promise<AssetInfo[]>
  deleteAsset(key: string): Promise<void>
}
```

`saveFileAs` returns the filename that was saved to (so the toolbar can update), or `null` if the user cancelled.

### Preview

The toolbar's Preview button opens WasmPlayer (`previewInWasmPlayer` in `editor-store.ts`) in a new tab, passing the current in-memory game bytes via `BroadcastChannel` — this works the same way for local and server-backed games, since WasmPlayer itself now handles both local files and server-backed games (see `#1852`). There is no more local/server split for preview.

### Unsaved changes

`UndoLogger` fires `TransactionCommitted` when a transaction records actual changes (not on no-op transactions or undo/redo). `EditorController` surfaces this as a `Dirty` event — matching the v5 desktop editor pattern. The bridge sets `_isDirty = true` on `Dirty` and clears it in `Save()`. The `isDirty` Svelte store drives a `*` indicator in the toolbar filename. A `beforeunload` handler on the editor page triggers the browser's native "unsaved changes" prompt if the tab is closed or navigated away while dirty.

**Known limitation**: undoing all changes back to the last-saved state does not clear dirty — the `*` stays until the next explicit save. Fixing this requires either XML hash comparison on undo or a generation-counter approach; deferred as a future improvement.

### Server-side API (textadventures.co.uk)

`ServerFileAdapter` expects the API described in `docs/textadventures-api.md`. When the editor is opened with `?game={guid}`, it auto-loads via `loadFromServer` and uses `ServerFileAdapter` for all subsequent saves and asset operations. The server API uses the user's existing session cookie — no token exchange needed.

`docs/textadventures-api.md` is slightly behind the real implementation in the sibling `textadventures.co.uk` repo (`TextAdventures.Web/Controllers/EditorApiController.cs`) — that repo also has `POST /api/editor/games` (create a new server-side game from name + `.aslx`) which isn't documented yet. Worth reconciling when picking up the Export/Publish work below, since it touches the same controller.

---

## Export & Publish

Both features hinge on one missing piece: producing a `.quest` package (a zip with `game.aslx` + asset files, the format v5's desktop editor called "Publish to file"). The **read** side already fully supports this — `PackageReader.cs` in Engine loads `.quest` files today using plain `System.IO.Compression.ZipArchive` (pure BCL, works fine under `browser-wasm`). The **write** side, `Packager.CreatePackage`, is a stub (`throw new NotImplementedException()`) with the old Quest 5 implementation commented out, written against a legacy Ionic Zip library and local-disk scanning that don't apply in a browser.

`EditorController.Publish(filename, includeWalkthrough, includeFiles, outputStream)` already has the right shape for this — it accepts an explicit `IEnumerable<PackageIncludeFile>` and bypasses disk scanning entirely when one is supplied. `WorldModel.Save(SaveMode.Package, includeWalkthrough)` already produces a correct, self-contained `game.aslx` (inlines included-library content, strips editor-only elements) — the only thing the zip needs beyond that is the asset bytes the `FileAdapter` already knows about.

### Export (this repo)

1. **Engine**: implement `Packager.CreatePackage` for real using `ZipArchive` in write mode. Delete the old disk-scanning branch (`GetAvailableExternalFiles`/`AddLibraryResources`) rather than porting it — the browser caller will always pass `includeFiles` explicitly, so that code path is dead on arrival.
   - `game.aslx` entry ← `_worldModel.Save(SaveMode.Package, includeWalkthrough)`
   - One zip entry per `PackageIncludeFile` (filename + stream)
2. **`WasmEditorBridge`**: `[JSExport]` doesn't cleanly marshal an array of blobs in one call, so follow the bridge's existing stage-then-consume pattern (same shape as the dirty-flag polling): `AddPackageAsset(string filename, byte[] data)` to stage assets one at a time, then `ExportPackage(bool includeWalkthrough)` → `byte[]` that consumes the staged list, calls `EditorController.Publish(null, includeWalkthrough, staged, memoryStream)`, and returns the zip bytes.
3. **WebEditor UI**: "Export…" toolbar action → small modal with an "Include walkthrough" checkbox (mirrors v5's Publish dialog) → `editor-store.ts` calls `adapter.listAssets()` + `getAsset()` per file, stages them via the bridge, calls `ExportPackage`, then triggers a browser download of `{gameName}.quest`. This is adapter-agnostic — identical code path for local and server-mode games, since it only touches the `FileAdapter` interface.
4. **Known gap, deferred deliberately**: multi-file/library games (a `.aslx` that itself includes sibling library files on disk) are rare in WebEditor today and out of scope for v1 — only assets the `FileAdapter` knows about get bundled.
5. Natural to pair with a round-trip test (export → reload the zip via `Initialise` or through WasmPlayer) — fits alongside the already-planned WASM bridge test suite.

### Publish (cross-repo: quest + textadventures.co.uk)

Unlike Export, this isn't a file-format problem — it's textadventures.co.uk's existing public-listing/submission flow, and most of it is **already built and wired, just waiting for a `.quest` file that nothing currently produces**:

- `TextAdventures.Web/Controllers/CreateController.cs` has `Publish(int id)`, whose comment reads *"Used only by Quest WebEditor, which redirects here after publishing game output to Azure"* — it forwards to `SubmitController.QuestWebEditor(id)`.
- `Views/Submit/Quest.razor` downloads `{editorGameDir}/Output/{gamename}.quest` from blob storage, parses it via the existing `GameQuery` reader (same one used by the classic manual-upload flow), and either shows the `GameDetails` submission form (title/description/category/cover/visibility) for a first publish, or calls `SubmitService.UpdateGameFile` to update an already-published listing (tracked via `EditorGame.PublishId`).
- Category, tags, and visibility (`Game.IsVisible`) all reuse the same infrastructure as the classic upload and old v5-desktop-editor publish paths — no new data model needed.
- **The one missing piece is server-side**: `EditorApiController.cs` has no endpoint to accept an uploaded compiled package. It needs something like `POST /api/editor/games/{gameId}/publish` that stores the uploaded bytes at the blob path `Quest.razor` already reads from (`{editorGameDir}/Output/{gamename}.quest`).
- **quest (this repo)**: add a "Publish" toolbar action, shown only when `ServerFileAdapter` is active (local-mode games have no `gameId` to attach a listing to). It reuses Export's `ExportPackage()` output, POSTs the zip to the new endpoint above, then navigates to `/create/publish/{gameId}` on textadventures.co.uk.
- Local-mode users aren't blocked by any of this: they can already Export a `.quest` file and use the site's existing classic manual "submit a game" upload page.

Since the counterpart repo lives outside this one, this section should be treated as the plan of record for both sides — update it here if the shape changes, since `docs/textadventures-api.md` in this repo is the only place that spec is written down for someone working purely in `quest`.

---

## Remaining work

Core editing, element CRUD, script editing, save/load (local + server), new-game-from-template, preview, and asset management (upload/browse/delete, plus pickers for both attribute-level and script-level file controls) are all done. What's left, roughly in the order it'll likely get tackled:

1. **Export & Publish** — not started. Fully scoped now, see [Export & Publish](#export--publish) above: Export means finishing the stubbed `Packager.CreatePackage` (Engine) plus a small bridge/UI layer; Publish is a small cross-repo addition since textadventures.co.uk's submission flow already exists and is just waiting on the `.quest` file Export produces. Export should land first since Publish depends on it. Export's actual risky logic (`Packager.CreatePackage` — the zip writing, round-tripping through `PackageReader`) lives in `Engine`, a normal `net10.0` project, so it should get ordinary `EditorCoreTests`/`EngineTests`-style unit tests as part of that same PR — no new test infrastructure needed, this isn't gated on item 8 below.
2. **Raw ASLX (whole-file) editor** — a CodeMirror 6 XML view as a top-level toggle, for parity with the v5 desktop editor's raw code view. Round-trips through save/reload; needs a clear "you are editing raw XML, this bypasses validation" warning. CodeMirror 6 has built-in XML highlighting, so no custom grammar is needed; Monaco is the alternative if a more VS Code–like experience is wanted at the cost of bundle size.
3. **Dirty flag doesn't clear on undo to saved state** — see [Unsaved changes](#unsaved-changes) above.
4. **Richer room-exits dialog** — deferred from the add/delete-elements work; exits are currently created anonymously and edited like any other element rather than through a dedicated exits tab.
5. **`EditorController.AddControlType`/`GetControlType`** — dead code now the only consumer is Svelte (the `EditorMode` Desktop/Web split itself was already removed). Cleanup, not urgent.
6. **Live JS push events** (`[JSImport]`) — see [Not yet implemented](#not-yet-implemented). Only matters if/when the editor needs push-based updates (e.g. background operations, collaboration); not needed for anything currently planned.
7. **Electron wrapper** — future; see Option D above and [Open questions](#open-questions).
8. **WebEditor e2e coverage** (Playwright, `tests/e2e`) — this is the real gap, not a "WasmEditorBridge unit test project". `WasmEditor.csproj` targets `<RuntimeIdentifier>browser-wasm</RuntimeIdentifier>` with `OutputType>Exe`, which a normal MSTest project can't reference the way `EditorCoreTests` references `EditorCore.csproj` — there's no in-process host to unit-test the `[JSExport]` boundary itself. Most of the ~65 bridge methods are thin pass-throughs to `EditorController`, which already has decent coverage (`EditableListTests`, `EditableScriptTests`, `EditorControllerTests`, etc.); what's actually untested is the JS↔WASM marshaling boundary and end-to-end UI wiring, which only a real browser driving the built `AppBundle` can exercise — i.e. Playwright, extending the existing pattern in `tests/e2e` (currently WebPlayer/WasmPlayer verification scripts only). Independent of item 1; not a prerequisite for it. (The Asset UI work verified this approach informally — a throwaway Playwright script driving the built AppBundle via the non-FSA `<input type="file">` fallback path worked well for exercising the picker/upload/preview flow end-to-end; formalizing that into `tests/e2e` is what this item is.)
9. **Blockly** — future; see [Blockly (future)](#blockly-future) below.

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

- **Live C# → JS events**: Currently tree/dirty state is snapshot- or poll-based. As editing adds/removes/renames nodes, would the bridge ever need to push updates to JS rather than relying on re-fetch? Not needed for anything currently planned (see Remaining work item 6).
- **Shared filesystem adapter**: `src/lib/filesystem/` has no Svelte/Quest-specific imports by design (see File handling section and [squiffy#215](https://github.com/textadventures/squiffy/issues/215)); extract into a published package when Squiffy becomes a second consumer.
- **Electron wrapper**: Both Quest and Squiffy editors are candidates for an Electron app. The `FileAdapter` interface is the seam; `createElectronAdapter()` bridges to Node.js `fs` via `contextBridge` IPC. Decide whether to build one Electron shell that hosts both, or separate apps.
