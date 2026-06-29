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
- Editor window: loads the WebEditor SPA (served as a local file or via a local static server)
- Player windows: navigate to `http://localhost:{port}/?game={encodedPath}` — one window per game session, or a single reusable player window
- Open player window from editor Toolbar "Preview" button via IPC

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

  // Player backend
  player: {
    getPort(): Promise<number>
    openGame(gamePath: string): Promise<void>   // opens a new player window
  },

  // Shell
  shell: {
    openExternal(url: string): Promise<void>
  }
}
```

### ElectronFileAdapter

Implements the `FileAdapter` interface defined in `src/WebEditor/src/lib/filesystem/` (see `webeditor-wasm-svelte.md`). Registered at startup when `window.electronApp` is present.

```typescript
// src/WebEditor/src/lib/filesystem/electron-adapter.ts

export class ElectronFileAdapter implements FileAdapter {
  constructor(private readonly dirPath: string, private readonly _filename: string) {}

  readonly canSaveAs = true
  readonly previewUrl = null   // player opened via IPC, not a URL

  async saveFile(data: Uint8Array | string): Promise<void> {
    await window.electronApp.fs.writeFile(this.filePath, data)
  }

  async saveFileAs(data: Uint8Array | string, suggestedName?: string): Promise<string | null> {
    const path = await window.electronApp.dialog.saveFile({
      defaultPath: join(this.dirPath, suggestedName ?? this._filename)
    })
    if (!path) return null
    await window.electronApp.fs.writeFile(path, data)
    return basename(path)
  }

  // Asset operations: read/write sibling files in dirPath
  async putAsset(key: string, data: Blob): Promise<void> {
    await window.electronApp.fs.writeFile(join(this.dirPath, key), new Uint8Array(await data.arrayBuffer()))
  }
  async getAsset(key: string): Promise<Blob | null> {
    try {
      const bytes = await window.electronApp.fs.readFile(join(this.dirPath, key))
      return new Blob([bytes])
    } catch { return null }
  }
  async listAssets(): Promise<AssetInfo[]> { /* readDir, filter non-.aslx files */ }
  async deleteAsset(key: string): Promise<void> {
    await window.electronApp.fs.unlink(join(this.dirPath, key))
  }
}
```

`previewUrl` is `null` in Electron mode. The Toolbar "Preview" button calls `window.electronApp.player.openGame(filePath)` instead of opening a URL tab. This sends an IPC message to the main process, which either navigates the existing player window or opens a new one.

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

### Phase 1 — Editor wrapper (no player backend)

Electron shell with WebEditor + ElectronFileAdapter. No LocalPlayer process.

- Open a game folder via `dialog.openDirectory` → `ElectronFileAdapter`
- Full edit/save/undo/redo via WasmEditor (unchanged)
- Preview button disabled or shows "coming soon"
- Proves the Electron packaging pipeline and file adapter before tackling the backend

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

## Open questions

- **One app or two?** Should the Quest editor and Quest player be a single combined app (the editor opens a player window for preview; a home screen also lets you play games without editing) or separate apps? A combined app is the simpler packaging story and matches the Quest 5 desktop precedent; separate apps give a cleaner UX for players who have no interest in editing.
- **WebPlayer vs. LocalPlayer**: Rather than building LocalPlayer from scratch, could we adapt WebPlayer (Blazor Server) to bind to localhost and be spawned by Electron? This reuses all existing WebPlayer code at the cost of pulling in the Blazor/SignalR stack, and rules out Native AOT (Blazor Server is not AOT-compatible).
- **WasmEditor vs. LocalEditor**: The editor currently uses WasmEditor (EditorCore in WASM) for EditorCore operations. While editing is user-paced, WASM overhead could still affect responsiveness for large games (loading, tree navigation, undo/redo chains). A native alternative would be to extend the LocalPlayer backend (or a shared `LocalBackend` process) to also expose an EditorCore API, removing WASM from the editor entirely. This would mean one backend process serving both editor and player, but adds complexity to the protocol and the backend project.
- **Offline asset resolution in player**: Assets (images, audio) referenced by game files need to be resolvable as URLs inside the player BrowserWindow. Since the LocalPlayer serves files from the local filesystem on its port, this is straightforward — but the path rewriting logic may differ from the WebPlayer CDN-based asset URLs.
- **macOS notarization**: Apple requires apps distributed outside the App Store to be notarized. The `.NET` native binary must be signed and notarized as part of the build pipeline. This adds an Apple Developer account requirement and a CI notarize step.
- **Windows code signing**: Similarly, Windows SmartScreen warnings require an EV code-signing certificate for a clean install experience.
