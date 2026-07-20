# Quest Viva Desktop App (Electron)

## Motivation

WasmPlayer runs the game engine inside the browser's WebAssembly sandbox. The WASM memory model and GC differ from native .NET, and the engine's workload — script interpretation, expression evaluation, heavy string operations — is compute-intensive enough that the overhead is perceptible.

Running a native .NET process locally and connecting to it from Electron's Chromium renderer gives full engine performance, while the UI (editor + player frontend) remains unchanged web technology.

A second, independent motivation: an "upgrade path" for existing Quest 5 desktop users. That audience is used to a native Windows app and shouldn't be told "you now have to use a website" — a Quest Viva desktop app gives them a comparable thing to move to, regardless of whether it's WASM- or native-backed under the hood.

---

## Target architecture

```
Electron app
├── Main process (Node.js)
│   ├── Spawns LocalPlayer .NET process on a random port
│   ├── contextBridge IPC handlers (fs, shell, port, lifecycle)
│   └── BrowserWindow management (editor window, player windows)
│
├── Renderer: Editor (AppShell SvelteKit SPA)
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
- Editor window: loads the AppShell SPA over a local loopback HTTP server (`src/ElectronApp/src/static-server.ts`) — not `file://` (fetch of `.wasm`/`.dll` assets over `file://` hits CORS/mime-type issues in Chromium) and not a custom protocol. The server binds `127.0.0.1:0` (random free port, same trick as LocalPlayer's own port selection below) and serves the same three-directory layout `deploy-play.yml` already produces for play.questviva.com — `editor/` (AppShell build) at `/`, `AppBundle/` (WasmEditor) at `/AppBundle/`, `player/` (WasmPlayer, Phase 1) or LocalPlayer (Phase 2) at `/player/`.
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

Implemented in `src/AppShell/src/lib/filesystem/electron-adapter.ts`, matching the `FileAdapter` interface (`src/AppShell/src/lib/filesystem/types.ts`) — same flat single-directory shape as `BrowserFileAdapter` (the FSA adapter), just backed by `window.electronApp.fs`/`.dialog` instead of FSA handles:

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

A new `isElectron()` helper (`src/AppShell/src/lib/runtime.ts`) is checked in `open/+page.svelte` **before** `hasFSA()` — Electron's Chromium does support `showDirectoryPicker`, but per the note below it must not be used there, so Electron always takes the `ElectronFileAdapter` branch regardless of FSA availability.

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
# 1. Build AppShell SPA
cd src/AppShell && npm run build          # output: dist/

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
| AppShell SPA | ~5 MB | ~5 MB |
| WasmEditor WASM bundle (if retained) | ~15 MB | ~15 MB |
| **Total** | **~210 MB** | **~135 MB** |

Native AOT is the better long-term target; it also eliminates the cold-start JIT delay. The trade-off is a longer CI build (~60–90 s per platform).

### Platform targets

All three built and published by `electron-publish.yml` (see Releasing below) — unsigned everywhere for now.

- macOS (arm64 only) — DMG. Dropped x64: Apple's own Rosetta-deprecation warning (shown when launching an Intel build on Apple Silicon) means new Intel binaries would be investing in a platform Apple is already sunsetting, and there's no existing Intel user base for this app to preserve.
- Windows (x64) — NSIS installer
- Linux (x64 + arm64) — AppImage *and* `.deb`. No "universal" option here unlike macOS — ELF has no fat-binary equivalent to Mach-O's, and none of the common Linux packaging formats (AppImage, deb, rpm, Snap, Flatpak) support single-file multi-arch execution; publishing separate per-arch artifacts is the standard approach, not a workaround. `.deb` was added alongside AppImage (not instead of it) because a plain AppImage never integrates with the desktop on its own — nothing installs its embedded `.desktop`/icon into `~/.local/share/applications` or the icon theme, so GNOME/Ubuntu shows a generic "unknown executable" icon for it indefinitely unless the user separately installs an integration tool (AppImageLauncher is dead; Gear Lever via Flatpak is the current replacement, neither of which ships with Ubuntu). `dpkg -i` installs the `.desktop` file into `/usr/share/applications` and the icon set into `/usr/share/icons/hicolor/*/apps` as a normal part of installing the package — the same "download, double-click, Install button" flow Chrome/Slack/Discord/VS Code use on Debian/Ubuntu, with no extra tooling required. AppImage stays for distro-agnostic, no-root use.

`src/ElectronApp/scripts/dist.mjs` invokes electron-builder's Node API (`build({ config: { extraMetadata: { version } } })`) directly rather than its CLI, so injecting the repo's real `VERSION` at package time doesn't rely on `$(cat ...)` shell command substitution — that's bash-only and breaks under the `pwsh` shell `windows-latest` runners default to. `WASM_CONFIG=Release` (set via the GitHub Actions step's `env:`, not inline shell syntax, for the same cross-platform reason) points `copy-static.mjs` at the Release AppBundles instead of its Debug default.

---

## Phased delivery

### Phase 1 — Editor wrapper (no player backend) — implemented

Electron shell with AppShell + ElectronFileAdapter. No LocalPlayer process.

- Open a game folder via `dialog.openDirectory` → `ElectronFileAdapter`
- Full edit/save/undo/redo via WasmEditor (unchanged)
- **Preview is functional**, not "coming soon" — reuses the existing WasmPlayer + `BroadcastChannel` preview mechanism unchanged (see Window management above); LocalPlayer replaces WasmPlayer as the preview target in Phase 2, with no changes needed on the AppShell side
- All three static bundles (AppShell build, WasmEditor AppBundle, WasmPlayer AppBundle) served from one loopback HTTP server (`src/ElectronApp/src/static-server.ts`) so they share an origin — same three-directory layout as `deploy-play.yml`
- Proves the Electron packaging pipeline and file adapter before tackling the native backend

`src/ElectronApp/` — `main.ts` (window/lifecycle), `static-server.ts`, `preload.ts`, `ipc/{fs,dialog,shell}.ts`, `scripts/copy-static.mjs` (assembles `resources/app-static/{editor,AppBundle,player}` from the AppShell/WasmEditor/WasmPlayer build outputs, mirroring `deploy-play.yml`'s `cp -r` steps). CI: `build_electron` job in `.github/workflows/build-and-test.yml`, modeled on `build_appshell`.

### Phase 2 — Native player backend — deferred

LocalPlayer project + backend lifecycle in Electron main process.

- LocalPlayer spawns on a random port, Electron opens a player BrowserWindow
- "Preview" in the editor saves the game then triggers `player.openGame(path)` via IPC
- Player window navigates to `http://localhost:{port}/?game={path}`

**Deferred 2026-07-15.** Nobody is complaining about WasmPlayer performance yet — unsurprising, since the app has no real usage yet, so there's no signal that the Release/AOT build's interpretation overhead is actually a felt problem rather than a theoretical one. Convenience/offline access and the Quest 5 upgrade path (see Motivation) are the load-bearing reasons for the app right now, and both are already delivered by Phase 1. Building LocalPlayer now would mean maintaining and testing a second player runtime (in addition to WasmPlayer, which the [long-term architecture direction](../CLAUDE.md) already commits to as the web player) against no concrete complaint. Revisit if a specific game or script pattern surfaces a real, felt performance problem — at that point Phase 2 can be picked up as originally scoped, nothing here blocks it.

### Phase 3 — Standalone game launcher

Planned 2026-07-16, partly landed 2026-07-16 (see [deployment-domains.md](./deployment-domains.md#follow-ups-not-in-this-pass)). Home screen that lets users open and play `.aslx` files without editing, plus browse and play games from textadventures.co.uk — folded into AppShell's own routes rather than a separate app, so this phase turned out smaller than scoped: Electron already builds AppShell with no `BASE_PATH` (root) and `static-server.ts` already serves it at bare root, which is exactly the shape the merged Play/Create/editor app needs. Adding `PUBLIC_SHOW_HOME=true` to `build-and-test.yml`'s `build_electron` job and `electron-publish.yml` (done) was the entire integration — no `static-server.ts`/`copy-static.mjs`/`main.ts` changes, resolving the routing question this section used to raise.

Still open:

- **Recently played**: Electron-only addition (the web version has no persistent storage to hang this off). A JSON file under `app.getPath('userData')`, read/written through the existing `contextBridge` fs API (`window.electronApp.fs`) — no new IPC surface needed, `preload.ts` already exposes generic `readFile`/`writeFile`. Would append on every game launch (both "Play" from the catalog and "Open" of a local `.aslx` file). Not built yet.
- **Play** targets WasmPlayer for now, consistent with Phase 2 remaining deferred — a plain navigation to `/player/?id={id}` for catalog games, unchanged. Local `.aslx` files are the harder case: WasmPlayer's `fetchGameBytes` uses `fetch()` against a URL, and there's no existing route for it to read an arbitrary local path, so this needs either a small local-file-serving addition to `static-server.ts` or a `postMessage`/`BroadcastChannel`-based hand-off (similar to the existing `source=editor` preview mechanism). Not yet resolved.
- `window.electronApp.platform` — declared optional on the type (`electron-types.d.ts`) and already read by `home-catalog.ts` for analytics, but `preload.ts` doesn't populate it yet.
- The native File menu's "New"/"Open" actions (`+layout.svelte`'s `onAction` handler) still `goto` straight to `/open` — could offer a "back to Play" affordance too, not done.
- Downloading a catalog game for offline play (rather than just streaming it via WasmPlayer each time) is a later increment, not needed for the first cut.

### Phase 4 — Native AOT + auto-update

Switch LocalPlayer publish to Native AOT. Integrate `electron-updater` for silent background updates (delta updates against the previous release).

---

## Resolved decisions

- **One app or two?** Resolved: single combined app. The editor opens a player window for preview; a home screen (Phase 3) also lets you play games without editing. Simpler packaging/installer story, matches the Quest 5 desktop precedent. This only actually constrains Phase 3 scaffolding; Phase 1/2 work is unaffected either way.
- **WebPlayer vs. LocalPlayer**: Resolved: build LocalPlayer, do not bundle WebPlayer/Blazor Server. Reusing WebPlayer would pull in the Blazor/SignalR circuit + reconnection machinery — built for unreliable remote connections, not localhost — and would rule out Native AOT (Phase 4), locking in the larger self-contained runtime and slower cold start. It would also mean growing, rather than sunsetting, the Blazor/SignalR dependency the rest of the project is moving away from (see WasmPlayer as the long-term WebPlayer replacement). The Electron-specific surface (contextBridge, file adapter, dialogs) is identical under either backend, so bundling WebPlayer would not actually reduce the number of paths to test.
- **Signing/notarization**: macOS — Apple Developer account renewed 2026-07-20; `electron-publish.yml`'s macOS leg now signs with a Developer ID Application cert (`MAC_CSC_LINK`/`MAC_CSC_KEY_PASSWORD` secrets, base64 `.p12`) and notarizes via an App Store Connect API key (`APPLE_API_KEY_P8`/`APPLE_API_KEY_ID`/`APPLE_API_ISSUER` secrets) — both electron-builder built-ins, no custom notarization code needed. `scripts/adhoc-sign.js` steps aside automatically once `CSC_LINK` is set, so it stays in place as the fallback for unsigned local/dev builds. Not yet exercised end-to-end (needs the GitHub secrets populated, then a `workflow_dispatch` run to verify `spctl`/`xcrun stapler validate` pass on the resulting DMG). Windows — defer EV code-signing; ship unsigned betas for now (SmartScreen warning), consistent with the existing unsigned Windows desktop build. Revisit if it becomes a real adoption blocker.
- **Linux sandbox**: `node_modules/electron`'s `chrome-sandbox` SUID helper needs root ownership + mode 4755, which a plain `npm install` never sets up — an unrepaired checkout aborts with a FATAL `setuid_sandbox_host.cc` error before any window opens. AppImage has no root-run install step to fix this either (no `.deb`/`.rpm`-style postinst), and FUSE mounts are commonly `nosuid` regardless, so the packaged app can't rely on the helper working even if permissions were baked in at build time. `electron.sh` passes `--no-sandbox` as a literal argv flag on Linux (must be argv, not `app.commandLine.appendSwitch()` from `main.ts` — the zygote/sandbox-host check runs during native Chromium startup, before Electron loads any of the app's JS). The packaged AppImage sets `linux.executableArgs: ["--no-sandbox"]` in `package.json` — confirmed working (2026-07-14) against a real arm64 AppImage launched directly (not desktop-integrated), on Ubuntu.

## Known Linux runtime prerequisites

Discovered testing the arm64 AppImage on a fresh Ubuntu VM (2026-07-14) — neither is anything our build controls, both are pre-existing host gaps a real end user could equally hit:

- **`libfuse2`**: AppImage's type-2 runtime requires FUSE2 to self-mount; Ubuntu 22.04+ ships FUSE3 only by default. Without it, launching the AppImage fails immediately with `dlopen(): error loading libfuse.so.2`. Fix: `sudo apt-get install libfuse2` (or `libfuse2t64` on newer releases post the 64-bit time_t transition). `--appimage-extract-and-run` works around it without installing anything, at the cost of extracting to a temp dir on every launch.
- **Unversioned `libz.so`**: the electron-builder-provided AppImage runtime stub itself (not our app's Electron binary — that one only needs the normal versioned `libz.so.1`, confirmed via `ldd`) dynamically links against the bare `libz.so` symlink, which only `zlib1g-dev` provides (not `zlib1g`, which most systems do have). Without it: `error while loading shared libraries: libz.so: cannot open shared object file`. Fix: `sudo apt-get install zlib1g-dev`. Since real end users won't have a `-dev` package installed and won't know to install one, this is a real distribution gap, not just a VM quirk — worth investigating whether a newer electron-builder/AppImageKit runtime avoids it before this ships broadly.

Both of the above are specific to AppImage's type-2 FUSE-mounting runtime stub — `.deb` installs the unpacked app straight to `/opt` and runs the real binary directly, so neither applies to it.

- **WasmEditor vs. LocalEditor**: The editor currently uses WasmEditor (EditorCore in WASM) for EditorCore operations. While editing is user-paced, WASM overhead could still affect responsiveness for large games (loading, tree navigation, undo/redo chains). A native alternative would be to extend the LocalPlayer backend (or a shared `LocalBackend` process) to also expose an EditorCore API, removing WASM from the editor entirely. This would mean one backend process serving both editor and player, but adds complexity to the protocol and the backend project.
- **Offline asset resolution in player**: Assets (images, audio) referenced by game files need to be resolvable as URLs inside the player BrowserWindow. Since the LocalPlayer serves files from the local filesystem on its port, this is straightforward — but the path rewriting logic may differ from the WebPlayer CDN-based asset URLs.
- **Home screen routing at root (Phase 3)**: resolved 2026-07-16 by folding Home into AppShell's own routes rather than building a separate static app — `static-server.ts` already served the editor at bare root, which turned out to be exactly the shape the merged app needs, so no `static-server.ts`/`copy-static.mjs`/`main.ts` changes were needed at all. See Phase 3 above.
