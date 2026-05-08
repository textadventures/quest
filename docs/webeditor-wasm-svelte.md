# WebEditor: EditorCore WASM + Svelte

## Architecture

```
Browser
├── Svelte frontend (UI)
│   ├── Tree panel (game object hierarchy)
│   ├── Property editor panels
│   ├── Script editor (Monaco/CodeMirror)
│   └── Toolbar (save, undo/redo, publish)
│
└── .NET WASM module
    ├── WasmEditorBridge  ← new project, thin interop layer
    │   └── JSExport/JSImport attributes
    └── EditorCore (redesigned API)
        └── Engine / Utility / Common
```

The Svelte layer calls JS-exported functions on the WASM module and receives events back via registered callbacks. All data crossing the boundary is JSON.

---

## WASM feasibility for EditorCore

**Good news:** `EditorController.Initialise` and all edit-path code is fully `async/await` — no `new Thread()`. The thread-based blocking in `WorldModel.DoInNewThreadAndWait` is used only during *game execution*, which the editor never triggers. So the threading blocker that breaks `WasmPlayer` does not affect the editor path.

**Issues to fix before EditorCore compiles to WASM:**

### 1. File I/O in `EditorController.Initialise`

Currently hardcoded to `FileGameDataProvider` which uses `System.IO.File`:

```csharp
// EditorController.cs:335
var gameDataProvider = new FileGameDataProvider(filename);
```

Fix: Change the signature to accept an `IGameDataProvider` directly:

```csharp
public async Task<bool> Initialise(IConfig config, IGameDataProvider gameDataProvider, bool partialInit = false)
```

The WASM interop layer will pass a provider backed by a byte array (the file contents passed in from JS via the browser File API).

### 2. `GetAvailableExternalFiles` uses the file system

```csharp
// EditorController.cs:1753
string baseFolder = System.IO.Path.GetDirectoryName(m_worldModel.Filename);
return m_worldModel.GetAvailableExternalFiles(searchPattern)
    .Select(f => System.IO.Path.Combine(baseFolder, f));
```

This will need a different approach in WASM — probably the browser-side handles file listings and passes them to the bridge, or this feature is deferred.

### 3. `System.Data.DataSetExtensions` NuGet package

Referenced in `EditorCore.csproj` but not actually used anywhere in the source. Remove it.

### 4. `EditorController.Publish` uses `System.IO.Stream`

The method signature accepts an optional `outputStream`. For WASM, the output should be returned as a string (the XML is already produced as a string internally). The method can be adapted or a separate WASM-friendly overload added.

### 5. `IConfig` dependency

`WorldModel` takes an `IConfig` — need to verify `IConfig`'s implementation doesn't use file I/O. If it does, a WASM-compatible implementation will be needed.

---

## New project: `WasmEditor`

A new project alongside `WasmPlayer`, targeting `net10.0` with `<RuntimeIdentifier>browser-wasm</RuntimeIdentifier>`.

Responsibilities:
- Expose `[JSExport]` methods for every operation Svelte needs
- Register callbacks so C# events fire into JS
- Handle serialisation (EditorCore types → JSON → Svelte)
- Manage the `EditorController` singleton lifecycle

Example shape:

```csharp
[JSExport]
public static Task<bool> Initialise(byte[] gameFileBytes, string filename) { ... }

[JSExport]
public static string GetTreeNodes() { ... }        // returns JSON

[JSExport]
public static string GetEditorData(string key) { ... }  // returns JSON

[JSExport]
public static void SetAttribute(string element, string attribute, string jsonValue) { ... }

[JSExport]
public static string Save() { ... }                // returns XML string

[JSExport]
public static void Undo() { ... }

[JSExport]
public static void Redo() { ... }
```

Events fired back into JS:

```csharp
// Registered once at init, called as EditorController events fire
[JSImport("questEditor.onNodeAdded", "quest-editor.js")]
static partial void OnNodeAdded(string key, string text, string parent);

[JSImport("questEditor.onElementUpdated", "quest-editor.js")]
static partial void OnElementUpdated(string element, string attribute, string jsonValue);
```

---

## EditorCore API redesign

Since the new editor is the only consumer, the existing event-heavy API can be simplified. Proposed direction:

- **Remove `EditorMode`** — `Desktop` vs `Web` distinction is no longer needed; everything is web.
- **Remove `AddControlType`/`GetControlType`** — these registered .NET UI control types; Svelte handles its own rendering.
- **Replace C# events with coarse-grained notifications** — instead of many fine-grained events firing into the WASM bridge, batch tree/state updates and let Svelte poll or receive a single "state changed" signal.
- **Expose `Initialise` as stream/bytes-based** — remove the `filename` string path (see above).
- **Keep undo/redo, validation, and element manipulation APIs** — these are stable and useful.

---

## File handling in the browser

The browser cannot open files from the local filesystem directly. Two options:

**Option A — File picker on open, OPFS on save**
- User opens a file via `<input type="file">` or File System Access API `showOpenFilePicker()`
- JS reads it as `ArrayBuffer`, passes bytes to WASM at init
- Saves write back via `showSaveFilePicker()` or download

**Option B — OPFS (Origin Private File System)**
- Game files live in the browser's private storage
- Allows read/write without prompts after initial import
- Enables auto-save

Option A is simpler to implement first. Option B is the better long-term UX.

---

## Svelte frontend structure

```
src/
├── lib/
│   ├── wasm.ts          # WASM module loader + typed wrapper
│   ├── editor-store.ts  # Svelte stores for tree state, selection, undo list
│   └── types.ts         # TypeScript types mirroring EditorCore data shapes
├── components/
│   ├── TreePanel.svelte
│   ├── PropertyEditor.svelte
│   ├── ScriptEditor.svelte   # Monaco or CodeMirror instance
│   ├── Toolbar.svelte
│   └── StatusBar.svelte
└── routes/
    ├── +page.svelte      # Main editor layout
    └── welcome/+page.svelte  # New game / open game
```

Use **SvelteKit** for routing (welcome → editor) and dev tooling. Monaco Editor for the script editor (first-class WASM compatibility, excellent syntax highlighting). Or CodeMirror 6 for a lighter build.

---

## Phased delivery

### Phase 1 — WASM proof of concept ✅
- ~~Remove `System.Data.DataSetExtensions` from `EditorCore.csproj`~~
- ~~Change `EditorController.Initialise` to accept `IGameDataProvider` instead of `filename`~~
- ~~Create `WasmEditor` project, get it compiling to WASM~~
- ~~Export a minimal API (init, get tree, get element data, set attribute, save)~~
- Verify EditorCore loads a `.aslx` file and round-trips saves (covered by Phase 2 integration)

Notes:
- `ByteArrayGameDataProvider` added to `Common` — wraps `byte[]` from JS as a `MemoryStream`
- `WasmConfig` uses `UseNCalc = true`
- `WasmEditorBridge` uses source-generated JSON (`JsonSerializerContext`) and `[assembly: SupportedOSPlatform("browser")]` — builds with zero warnings

### Phase 2 — Svelte skeleton
- Set up Vite + SvelteKit project
- Load WASM module, wire up init (file picker → bytes → WASM)
- Render the tree from `GetTreeNodes()` JSON
- Click a tree node → show raw attribute list in a side panel
- Toolbar: save (download XML), undo, redo

### Phase 3 — Property editors
- Map each EditorCore control type (`dropdown`, `textbox`, `script`, etc.) to a Svelte component
- Property editor panel renders the right controls for the selected element
- Live two-way binding: editing a control → `SetAttribute` → C# event → store update

### Phase 4 — Script editor
- Integrate Monaco (or CodeMirror) for script editing
- Wire up the script editor data API from EditorCore

### Phase 5 — Full feature parity
- New game (from templates)
- Publish/export
- OPFS persistence
- Test suite for the WASM bridge

---

## Open questions

- **WasmPlayer**: The Blazor WasmPlayer prototype has been deleted. A future native-WASM player could follow the same JSExport/JSImport pattern once Engine's threading is resolved.
- **Build integration**: Should `WasmEditor` output be served by `WebPlayer` (as an embedded SPA) or deployed separately?
- **Auth / server-side storage**: Is cloud save (user accounts, server-side game storage) in scope? Affects whether the editor is purely client-side or needs a backend.
