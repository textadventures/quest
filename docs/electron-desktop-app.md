# Quest Viva Desktop App (Electron)

## Motivation

WasmPlayer runs the game engine inside the browser's WebAssembly sandbox. The WASM memory model and GC differ from native .NET, and the engine's workload — script interpretation, expression evaluation, heavy string operations — is compute-intensive enough that the overhead is perceptible.

Running a native .NET process locally and connecting to it from Electron's Chromium renderer gives full engine performance, while the UI (editor + player frontend) remains unchanged web technology.

---

## Target architecture

```
Electron app
├── Main process (Node.js)
│   ├── Spawns LocalPlayer .NET process on a random port
│   ├── contextBridge IPC handlers (fs, shell, port, lifecycle)
│   └── BrowserWindow management (editor window, player windows)
│
├── Renderer: Editor (WebEditor SvelteKit SPA)
│   ├── ElectronFileAdapter — Node.js fs via contextBridge IPC
│   └── EditorCore — either WasmEditor (WASM, current) or via LocalBackend HTTP/IPC (native, see Open questions)
│
└── Renderer: Player (one BrowserWindow per game session)
    └── Navigates to http://localhost:{port}/  (served by LocalPlayer)
```

The **player** is the primary motivation for a native backend. Whether the **editor** should also move off WasmEditor (to a native EditorCore via the same backend process) is an open question — see below.

---

## Components

### LocalPlayer (.NET backend)

A new lightweight project `src/LocalPlayer/` — a self-hosted Kestrel HTTP/WebSocket server wrapping PlayerCore, designed to run as a local background process launched by Electron.

Compared to WebPlayer (Blazor Server), LocalPlayer:
- Has no Blazor/SignalR dependency — plain WebSocket or SSE for player events
- Serves the static player frontend from embedded resources
- Accepts `--port` and `--allowed-origin` flags so Electron can bind to a random free port and enforce same-origin

The player frontend (HTML/CSS/JS currently embedded in PlayerCore) is served by LocalPlayer at `/`. The Electron renderer navigates to `http://localhost:{port}/?game={path}` to start a game.

**Compilation options (see Packaging section):**
- Self-contained dotnet publish: ~80–100 MB with runtime bundled
- Native AOT publish: ~20–30 MB, no runtime required, faster cold start

### Electron main process

`src/ElectronApp/` (or a root-level `electron/` directory):

```
electron/
├── main.ts          — app lifecycle, window creation
├── backend.ts       — LocalPlayer process spawn/lifecycle
├── preload.ts       — contextBridge API surface
└── package.json     — electron-builder config
```

**Backend lifecycle (`backend.ts`):**
- On app ready: find a free port, spawn the LocalPlayer binary, wait for it to respond on `/health`
- Crash recovery: restart up to N times, show error UI after repeated failure
- On app quit: terminate the process (SIGTERM, escalate to SIGKILL after 3 s)
- Surface the chosen port to renderers via `ipcMain` / `contextBridge`

**Window management:**
- Editor window: loads the WebEditor SPA over a local loopback HTTP server (`src/ElectronApp/src/static-server.ts`) — not `file://` (fetch of `.wasm`/`.dll` assets over `file://` hits CORS/mime-type issues in Chromium) and not a custom protocol. The server binds `127.0.0.1:0` (random free port, same trick as LocalPlayer's own port selection below) and serves the same three-directory layout `deploy-play.yml` already produces for play.questviva.com — `editor/` (WebEditor build) at `/`, `AppBundle/` (WasmEditor) at `/AppBundle/`, `player/` (WasmPlayer, Phase 1) or LocalPlayer (Phase 2) at `/player/`.
- Player windows: Phase 1 navigates a second `BrowserWindow` to `http://127.0.0.1:{port}/player/` (bundled WasmPlayer); Phase 2 switches this to LocalPlayer's `http://localhost:{port}/?game={encodedPath}`.
- Phase 1's Preview button needs no IPC round-trip at all: `previewInWasmPlayer()` (`editor-store.ts`) already does a plain `window.open('/player/?source=editor', ...)` and talks to it over `BroadcastChannel('quest-preview')` — both work unchanged against the loopback origin. `editorWindow.webContents.setWindowOpenHandler` in `main.ts` just needs to `{ action: "allow" }` same-origin `/player/` popups (and send anything else to `shell.openExternal`) for that `window.open` call to become the player `BrowserWindow`.

### contextBridge API (`preload.ts`)

```typescript
window.electronApp = {
  // File system
  fs: {
    readFile(path: string): Promise<Uint8Array>
    writeFile(path: string, data: Uint8Array | string): Promise<void>
    readDir(path: string): Promise<{ name: string; isFile: boolean }[]>
    exists(path: string): Promise<boolean>
    mkdir(path: string): Promise<void>
    unlink(path: string): Promise<void>
  },

  // Dialogs
  dialog: {
    openFile(options: { filters?, defaultPath? }): Promise<string | null>        // returns path
    openDirectory(options: { defaultPath? }): Promise<string | null>             // returns path
    saveFile(options: { defaultPath?, filters? }): Promise<string | null>        // returns path
  },

  // Shell
  shell: {
    openExternal(url: string): Promise<void>
  },

  // Path join, resolved in preload.ts itself (no Node "path" module — sandboxed
  // preload only gets a polyfilled subset of Node built-ins), separator style
  // detected from dirPath rather than process.platform.
  path: {
    join(...segments: string[]): string
  }
}
```

No `player.*` API yet — Phase 1's Preview button needs no IPC at all (see Window management above); a `player.openGame(gamePath)` entry will be added in Phase 2 once LocalPlayer replaces WasmPlayer as the preview target and a specific game path becomes meaningful to pass across.

### ElectronFileAdapter

Implemented in `src/WebEditor/src/lib/filesystem/electron-adapter.ts`, matching the `FileAdapter` interface (`src/WebEditor/src/lib/filesystem/types.ts`) — same flat single-directory shape as `BrowserFileAdapter` (the FSA adapter), just backed by `window.electronApp.fs`/`.dialog` instead of FSA handles:

```typescript
export class ElectronFileAdapter implements FileAdapter {
  constructor(private readonly dirPath: string, private _filename: string) {}

  get filename() { return this._filename }
  readonly canSaveAs = true

  async saveFile(data: Uint8Array | string): Promise<void> {
    await electronApp().fs.writeFile(this.resolve(this._filename), data)
  }

  async saveFileAs(data: Uint8Array | string, suggestedName?: string): Promise<string | null> {
    const path = await electronApp().dialog.saveFile({
      defaultPath: this.resolve(suggestedName ?? this._filename),
      filters: ASLX_FILTER,
    })
    if (!path) return null
    await electronApp().fs.writeFile(path, data)
    this._filename = basename(path)
    return this._filename
  }

  // putAsset/getAsset/listAssets/deleteAsset: same resolve(key) pattern,
  // reading/writing sibling files in dirPath via window.electronApp.fs
}
```

A new `isElectron()` helper (`src/WebEditor/src/lib/runtime.ts`) is checked in `open/+page.svelte` **before** `hasFSA()` — Electron's Chromium does support `showDirectoryPicker`, but per the note below it must not be used there, so Electron always takes the `ElectronFileAdapter` branch regardless of FSA availability.

There is no adapter-level "preview URL" concept — see Window management above for how Preview actually works (unchanged `window.open` + `BroadcastChannel`, just allowed through `setWindowOpenHandler`).

**Note:** Do not use the File System Access API inside Electron — it has known parity bugs (missing persistent permissions, broken directory iteration in some Electron versions). The contextBridge + Node.js `fs` approach is more reliable.

---

## LocalPlayer project detail

### API surface

LocalPlayer does not need to replicate the full Blazor Server SignalR protocol. A simpler WebSocket-based protocol is sufficient:

```
GET  /health               → 200 OK (used by Electron to detect readiness)
GET  /                     → serves player HTML shell
GET  /assets/*             → serves PlayerCore embedded resources (CSS, JS, jPlayer, …)
GET  /game?path={path}     → loads game, returns session ID
WS   /session/{id}         → bidirectional game event stream
```

The player HTML shell is identical to what PlayerCore already generates; the WebSocket replaces the Blazor SignalR hub.

### Threading

Unlike the browser WASM environment, a native .NET process has real threads. The engine's `Task.Run` usage works as expected — no `SynchronizationContext` constraints.

### Port selection

```csharp
// Pick a random free port at startup
var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
int port = ((IPEndPoint)listener.LocalEndpoint).Port;
listener.Stop();
```

The port is written to stdout on startup (`PORT:{port}`) so the Electron main process can read it without a file or registry entry.

---

## Packaging

### Build pipeline

```bash
# 1. Build WebEditor SPA
cd src/WebEditor && npm run build          # output: dist/

# 2. Build WasmEditor (for in-editor EditorCore)
dotnet build --configuration Debug src/WasmEditor

# 3. Publish LocalPlayer (choose one):

#    Option A — self-contained (runtime bundled, ~100 MB)
dotnet publish src/LocalPlayer -c Release -r osx-arm64 --self-contained

#    Option B — Native AOT (no runtime, ~25 MB, slower build)
dotnet publish src/LocalPlayer -c Release -r osx-arm64 /p:PublishAot=true

# 4. Package Electron app
cd electron && npm run build              # electron-builder → dist/Quest Viva.app
```

### Estimated package sizes

| Component | Self-contained | Native AOT |
|---|---|---|
| Electron framework | ~90 MB | ~90 MB |
| LocalPlayer + .NET runtime | ~100 MB | ~25 MB |
| WebEditor SPA | ~5 MB | ~5 MB |
| WasmEditor WASM bundle (if retained) | ~15 MB | ~15 MB |
| **Total** | **~210 MB** | **~135 MB** |

Native AOT is the better long-term target; it also eliminates the cold-start JIT delay. The trade-off is a longer CI build (~60–90 s per platform).

### Platform targets

- macOS (arm64 + x64 universal binary)
- Windows (x64) — adds NSIS or Squirrel installer; slightly more packaging complexity

---

## Phased delivery

### Phase 1 — Editor wrapper (no player backend) — implemented

Electron shell with WebEditor + ElectronFileAdapter. No LocalPlayer process.

- Open a game folder via `dialog.openDirectory` → `ElectronFileAdapter`
- Full edit/save/undo/redo via WasmEditor (unchanged)
- **Preview is functional**, not "coming soon" — reuses the existing WasmPlayer + `BroadcastChannel` preview mechanism unchanged (see Window management above); LocalPlayer replaces WasmPlayer as the preview target in Phase 2, with no changes needed on the WebEditor side
- All three static bundles (WebEditor build, WasmEditor AppBundle, WasmPlayer AppBundle) served from one loopback HTTP server (`src/ElectronApp/src/static-server.ts`) so they share an origin — same three-directory layout as `deploy-play.yml`
- Proves the Electron packaging pipeline and file adapter before tackling the native backend

`src/ElectronApp/` — `main.ts` (window/lifecycle), `static-server.ts`, `preload.ts`, `ipc/{fs,dialog,shell}.ts`, `scripts/copy-static.mjs` (assembles `resources/app-static/{editor,AppBundle,player}` from the WebEditor/WasmEditor/WasmPlayer build outputs, mirroring `deploy-play.yml`'s `cp -r` steps). CI: `build_electron` job in `.github/workflows/build-and-test.yml`, modeled on `build_webeditor`.

### Phase 2 — Native player backend

LocalPlayer project + backend lifecycle in Electron main process.

- LocalPlayer spawns on a random port, Electron opens a player BrowserWindow
- "Preview" in the editor saves the game then triggers `player.openGame(path)` via IPC
- Player window navigates to `http://localhost:{port}/?game={path}`

### Phase 3 — Standalone game launcher

Home screen (separate from the editor) that lets users open and play `.aslx` files without editing. Potentially a curated library of downloaded/installed games.

### Phase 4 — Native AOT + auto-update

Switch LocalPlayer publish to Native AOT. Integrate `electron-updater` for silent background updates (delta updates against the previous release).

---

## Resolved decisions

- **One app or two?** Resolved: single combined app. The editor opens a player window for preview; a home screen (Phase 3) also lets you play games without editing. Simpler packaging/installer story, matches the Quest 5 desktop precedent. This only actually constrains Phase 3 scaffolding; Phase 1/2 work is unaffected either way.
- **WebPlayer vs. LocalPlayer**: Resolved: build LocalPlayer, do not bundle WebPlayer/Blazor Server. Reusing WebPlayer would pull in the Blazor/SignalR circuit + reconnection machinery — built for unreliable remote connections, not localhost — and would rule out Native AOT (Phase 4), locking in the larger self-contained runtime and slower cold start. It would also mean growing, rather than sunsetting, the Blazor/SignalR dependency the rest of the project is moving away from (see WasmPlayer as the long-term WebPlayer replacement). The Electron-specific surface (contextBridge, file adapter, dialogs) is identical under either backend, so bundling WebPlayer would not actually reduce the number of paths to test.
- **Signing/notarization**: macOS — get an Apple Developer account and notarize as part of the CI pipeline; low admin overhead. Windows — defer EV code-signing; ship unsigned betas for now (SmartScreen warning), consistent with the existing unsigned Windows desktop build. Revisit if it becomes a real adoption blocker.

## Open questions

- **WasmEditor vs. LocalEditor**: The editor currently uses WasmEditor (EditorCore in WASM) for EditorCore operations. While editing is user-paced, WASM overhead could still affect responsiveness for large games (loading, tree navigation, undo/redo chains). A native alternative would be to extend the LocalPlayer backend (or a shared `LocalBackend` process) to also expose an EditorCore API, removing WASM from the editor entirely. This would mean one backend process serving both editor and player, but adds complexity to the protocol and the backend project.
- **Offline asset resolution in player**: Assets (images, audio) referenced by game files need to be resolvable as URLs inside the player BrowserWindow. Since the LocalPlayer serves files from the local filesystem on its port, this is straightforward — but the path rewriting logic may differ from the WebPlayer CDN-based asset URLs.
