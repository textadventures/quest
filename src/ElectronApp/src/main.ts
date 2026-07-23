import { app, BrowserWindow, Menu, dialog, shell, type MenuItemConstructorOptions } from "electron";
import path from "node:path";
import { startStaticServer, type StaticServerHandle } from "./static-server";
import { registerFsHandlers } from "./ipc/fs";
import { registerDialogHandlers } from "./ipc/dialog";
import { registerShellHandlers } from "./ipc/shell";
import { registerPathsHandlers } from "./ipc/paths";
import { registerRecentHandlers } from "./ipc/recent";
import { registerPlayerHandlers } from "./ipc/player";
import { listRecentGames, clearRecentGames, type RecentGame, type RecentKind } from "./recent-games";

let editorWindow: BrowserWindow | null = null;
let staticServer: StaticServerHandle | null = null;

// Set by the "open-file" handler below when macOS launches the app fresh via
// a file association (double-click, "Open With…") — that event can fire
// before editorWindow exists *and before staticServer exists* (even before
// 'ready'), so there's nothing to route it to yet. whenReady's
// createEditorWindow call picks this up once the window exists, by folding
// it into the window's initial URL instead of an IPC send — see
// routeOpenedFile's comment for why cold-start and warm-instance opens need
// different delivery mechanisms. Only ever consumed there — once
// staticServer exists, the "open-file" handler below creates windows
// directly instead of going through this.
let pendingOpenPath: string | null = null;

// macOS-only: file-association launches ("Open With…", double-click) arrive
// here instead of as an argv entry (Windows/Linux — see extractGameFilePath).
// Must be registered before app.whenReady() resolves — Electron's docs note
// mac can fire this as early as during startup, before 'ready' — hence it's
// registered immediately after the two vars above it depends on, ahead of
// every other statement in this file.
//
// Three cases: a window is already up (route straight to it); the app is
// warm but every window's been closed — mac keeps the app running with no
// windows (see window-all-closed below), so staticServer is already up and
// a fresh window can be created immediately with the file folded into its
// initial URL; or this is a genuine cold start (staticServer doesn't exist
// yet), which stashes the path for whenReady's createEditorWindow call to
// pick up. Without the middle case, a double-click while windowless would
// silently drop the file — clicking the dock icon first (which creates a
// blank window via "activate") was the only way to recover.
app.on("open-file", (event, filePath) => {
    event.preventDefault();
    if (editorWindow) routeOpenedFile(filePath);
    else if (staticServer) createEditorWindow(staticServer.port, filePath);
    else pendingOpenPath = filePath;
});

// Without this, Electron's default macOS menu ("About "/"Quit ") falls back
// to package.json's "name" ("quest-viva-desktop") in dev — set as early as
// possible so it's correct whether packaged or run via `electron .`.
app.setName("Quest Viva");

// Packaged: resources/app-static ships via electron-builder's extraResources.
// Dev: `npm run build` copies the same three directories here directly (see
// scripts/copy-static.mjs) — one code path for both.
function staticRoot(): string {
    return app.isPackaged
        ? path.join(process.resourcesPath, "app-static")
        : path.join(__dirname, "..", "resources", "app-static");
}

// build/icons/512x512.png ships via extraResources (as icon.png) for the
// packaged app; dev runs against the source file directly since there's no
// resourcesPath yet.
function aboutIconPath(): string {
    return app.isPackaged
        ? path.join(process.resourcesPath, "icon.png")
        : path.join(__dirname, "..", "build", "icons", "512x512.png");
}

if (process.platform === "darwin") {
    // macOS's About panel shows "Version {CFBundleShortVersionString} ({CFBundleVersion})"
    // — the parenthetical is meant for a separate build number, but electron-builder's
    // -c.extraMetadata.version sets both plist fields to the same VERSION string
    // (this project has no separate build-number concept), so it reads as a literal
    // duplicate. Empty version suppresses the parenthetical rather than showing "()".
    // Name/icon come from the app bundle itself on mac, so nothing else to set here.
    app.setAboutPanelOptions({ version: "" });
} else {
    // Unlike mac, Linux/Windows have no defaults for setAboutPanelOptions —
    // any field left unset renders blank rather than falling back to the app's
    // own name/icon/version, so all three have to be provided explicitly.
    app.setAboutPanelOptions({
        applicationName: "Quest Viva",
        applicationVersion: app.getVersion(),
        iconPath: aboutIconPath(),
    });
}

// Mirrors the union AppShell's src/routes/+layout.svelte switches on (see
// window.electronApp.menu.onAction in preload.ts) — the two sides can't share
// a type since they're separate npm projects, so keep them in sync by hand.
type MenuAction = "new-game" | "open-file" | "save" | "save-as";

// Every File-menu editor action is reachable even while a player window (or,
// on mac, no window at all) is frontmost — mac has one shared app-level menu
// bar, and Windows/Linux windows without their own menu fall back to the
// same application menu. Without this, "New Game"/"Open Game" would silently
// operate on an editorWindow the user isn't even looking at. Also un-minimizes:
// show() alone doesn't un-minimize on all platforms.
function focusEditorWindow(): void {
    if (!editorWindow) return;
    if (editorWindow.isMinimized()) editorWindow.restore();
    editorWindow.show();
    editorWindow.focus();
}

// Notifies the renderer that a recent list changed for reasons outside its
// own control-flow (native "Clear Recent", or a mutation made through the
// other window/tab) — an already-mounted /open page (edit-kind) or Play tab
// (play-kind) has no other way to hear about it. Renderer-initiated
// add/remove (Open/Remove buttons) already update local state directly and
// don't depend on this round trip.
function broadcastRecentChanged(kind: RecentKind): void {
    editorWindow?.webContents.send("recent-games-changed", kind);
}

function sendMenuAction(action: MenuAction): void {
    focusEditorWindow();
    editorWindow?.webContents.send("menu-action", action);
}

// Renderer has no fixed "open this specific game" action string (unlike
// MenuAction) since it needs to carry a path — a dedicated channel instead of
// overloading sendMenuAction/onAction with a payload.
function sendOpenRecentGame(game: RecentGame): void {
    focusEditorWindow();
    editorWindow?.webContents.send("open-recent-game", { dirPath: game.dirPath, filename: game.filename });
}

// Same delivery as sendOpenRecentGame, for a file-association open of a
// play-kind file (.quest/.asl/.cas) — PlayCatalog.svelte's onOpenPlayFile
// listener (see preload.ts) launches a player window for it, the same as
// its own file-picker/Recently Played flows.
function sendOpenPlayFile(file: { dirPath: string; filename: string }): void {
    focusEditorWindow();
    editorWindow?.webContents.send("open-play-file", file);
}

// .aslx opens the editor (it's the unpacked source format the editor works
// with); .quest/.asl/.cas launch the player directly — matches the split
// PlayCatalog.svelte/electron-adapter.ts already draw between the editor's
// Open (ASLX_FILTER, .aslx only) and Play's file picker (PLAY_FILTER, all
// four).
const PLAY_EXTENSIONS = new Set([".quest", ".asl", ".cas"]);
const GAME_EXTENSIONS = new Set([".aslx", ...PLAY_EXTENSIONS]);

// Windows/Linux hand the launched file to us as a plain argv entry — used
// both for this process's own process.argv (cold start) and for the argv
// Electron forwards from a second launch attempt (see the "second-instance"
// handler below). The packaged exe path (argv[0]) and dev's literal "."
// (from `electron .`) never carry a recognized extension, so both are
// naturally skipped without special-casing them.
function extractGameFilePath(argv: string[]): string | null {
    for (const arg of argv) {
        if (GAME_EXTENSIONS.has(path.extname(arg).toLowerCase())) return arg;
    }
    return null;
}

// Warm-instance delivery only (editorWindow already exists and has been
// loaded/listening since the app's first launch) — called from
// "second-instance" (Windows/Linux relaunch) and from the "open-file"
// handler above once a window is already up. Cold-start delivery is a
// separate mechanism (see createEditorWindow/initialUrlPath below): sending
// over IPC immediately after constructing a fresh BrowserWindow would race
// the page's own onMount listeners, which aren't wired up until well after
// loadURL resolves, and the message would just be dropped.
function routeOpenedFile(filePath: string): void {
    const ext = path.extname(filePath).toLowerCase();
    const dirPath = path.dirname(filePath);
    const filename = path.basename(filePath);
    if (ext === ".aslx") {
        sendOpenRecentGame({ dirPath, filename, lastOpened: Date.now() });
    } else if (PLAY_EXTENSIONS.has(ext)) {
        sendOpenPlayFile({ dirPath, filename });
    }
}

// Cold-start file-association open, folded into the editor window's initial
// URL rather than delivered over IPC (see routeOpenedFile's comment) — both
// query shapes are already handled by the target page: /open?action=
// open-recent&... by open/+page.svelte (shared with the native "Open Recent"
// menu), /?action=play-file&... by PlayCatalog.svelte (new, mirrors it for
// Play).
function initialUrlPath(filePath: string | null): string {
    if (!filePath) return "/";
    const ext = path.extname(filePath).toLowerCase();
    const dirPath = path.dirname(filePath);
    const filename = path.basename(filePath);
    const t = Date.now();
    if (ext === ".aslx") {
        return `/open?action=open-recent&dir=${encodeURIComponent(dirPath)}&file=${encodeURIComponent(filename)}&t=${t}`;
    }
    if (PLAY_EXTENSIONS.has(ext)) {
        return `/?action=play-file&dir=${encodeURIComponent(dirPath)}&file=${encodeURIComponent(filename)}&t=${t}`;
    }
    return "/";
}

// Everything the editor itself can do (New/Open/Save/Save As) has to round-trip
// to the renderer — that's where WasmEditor and the file adapters live, main
// only owns the native chrome around it. Built once for every platform (rather
// than only patching non-darwin's missing About, as an earlier pass here did)
// so File/Save exist at all: without a menu at all, AppShell's own UI has no
// "switch to a different project" affordance short of a full page reload.
//
// Takes the current recent-games list rather than reading it itself, so it
// stays a pure template builder — refreshMenu() below is what re-derives the
// list and re-applies the menu whenever it changes.
function buildAppMenu(recentGames: RecentGame[]): Menu {
    const isMac = process.platform === "darwin";
    const openRecentSubmenu: MenuItemConstructorOptions[] = recentGames.length > 0
        ? [
            ...recentGames.map((game): MenuItemConstructorOptions => ({
                label: `${game.filename} — ${path.basename(game.dirPath)}`,
                click: () => sendOpenRecentGame(game),
            })),
            { type: "separator" },
            {
                label: "Clear Recent",
                click: () => {
                    void clearRecentGames("edit").then(async () => {
                        await refreshMenu();
                        broadcastRecentChanged("edit");
                    });
                },
            },
        ]
        : [{ label: "No Recent Games", enabled: false }];
    const template: MenuItemConstructorOptions[] = [
        ...(isMac ? [{ role: "appMenu" as const }] : []),
        {
            label: "File",
            submenu: [
                { label: "New Game…", accelerator: "CmdOrCtrl+N", click: () => sendMenuAction("new-game") },
                { label: "Open Game…", accelerator: "CmdOrCtrl+O", click: () => sendMenuAction("open-file") },
                { label: "Open Recent", submenu: openRecentSubmenu },
                { type: "separator" },
                { label: "Save", accelerator: "CmdOrCtrl+S", click: () => sendMenuAction("save") },
                { label: "Save As…", accelerator: "CmdOrCtrl+Shift+S", click: () => sendMenuAction("save-as") },
                ...(isMac ? [] : [{ type: "separator" as const }, { role: "quit" as const }]),
            ],
        },
        { role: "editMenu" },
        { role: "viewMenu" },
        { role: "windowMenu" },
        {
            role: "help",
            // A submenu with at least one real item, on every platform — an
            // *explicit* empty array ([]) here crashes Cocoa's menu code
            // outright on mac (that's why an earlier pass left it unset
            // there), but a non-empty one renders fine and is what makes the
            // Help menu show up at all on mac (0 items = Cocoa hides the
            // whole top-level entry). On mac, About already lives in the
            // appMenu role above, so it's just the docs link there; elsewhere
            // it's the only place About can go, so both are added.
            submenu: [
                { label: "Quest Viva Documentation", click: () => void shell.openExternal("https://questviva.com") },
                ...(isMac ? [] : [{ type: "separator" as const }, { role: "about" as const }]),
            ],
        },
    ];
    return Menu.buildFromTemplate(template);
}

// Re-reads the edit-kind recent-games list from disk and re-applies the app
// menu — called once at startup and again after every edit-kind
// recent:add/remove (via registerRecentHandlers' onChange) and after "Clear
// Recent". The native "Open Recent" submenu only ever reflects the editor's
// list — a game opened to play never belongs there, see recent-games.ts's
// RecentKind — so play-kind changes skip this entirely (see the onChange
// wiring in app.whenReady below).
async function refreshMenu(): Promise<void> {
    Menu.setApplicationMenu(buildAppMenu(await listRecentGames("edit")));
}

function createEditorWindow(port: number, initialPath?: string | null): void {
    editorWindow = new BrowserWindow({
        title: "Quest Viva",
        width: 1280,
        height: 860,
        // BrowserWindow's `icon` option is only implemented on Linux and
        // Windows (Electron docs), but Windows doesn't need it set here:
        // electron-builder auto-picks up build/icon.ico for the Windows
        // build the same way it does icon.icns for mac, so the .exe already
        // has the icon embedded as a resource and the taskbar reads it from
        // there. Linux has no equivalent — there's no single "the
        // executable's icon" for an AppImage — so the window manager
        // depends on this option instead. Without it, GNOME/Ubuntu falls
        // back to a generic icon for the window even though the AppImage
        // itself has the right icon in its packaged metadata.
        // aboutIconPath() already resolves build/icons/512x512.png (dev) vs.
        // the extraResource copy (packaged).
        ...(process.platform === "linux" ? { icon: aboutIconPath() } : {}),
        webPreferences: {
            preload: path.join(__dirname, "preload.js"),
            contextIsolation: true,
            nodeIntegration: false,
            // This window never actually plays untrusted game content itself
            // (catalog play and local-file play both go through
            // ipc/player.ts's dedicated player windows instead, precisely to
            // keep this window's fs/dialog-capable preload away from game
            // content — see setWindowOpenHandler below and player.ts). Set
            // here anyway for belt-and-suspenders consistency with every
            // other player-window surface, rather than relying on Electron's
            // already-permissive default.
            autoplayPolicy: "no-user-gesture-required",
        },
    });

    // AppShell's own <title> ("Quest Viva Editor") is correct for the
    // browser build, where it's the only app; the desktop app covers editing
    // and playing, so it's just "Quest Viva" here — override the page's title
    // instead of changing the shared app.html tag.
    editorWindow.on("page-title-updated", (event) => event.preventDefault());

    // AppShell's edit/+page.svelte calls e.preventDefault() in its own
    // beforeunload handler whenever there are unsaved changes — in a regular
    // browser that triggers the native "Leave site?" confirmation, but
    // Electron doesn't show any UI for that on its own; will-prevent-unload
    // is the hook it gives main for exactly this. Without a handler here the
    // window just silently refuses to close (no dialog, no feedback) and
    // Force Quit becomes the only way out.
    editorWindow.webContents.on("will-prevent-unload", (event) => {
        if (!editorWindow) return;
        const choice = dialog.showMessageBoxSync(editorWindow, {
            type: "question",
            buttons: ["Cancel", "Leave"],
            defaultId: 0,
            cancelId: 0,
            message: "Leave without saving?",
            detail: "You have unsaved changes. If you leave now, they'll be lost.",
        });
        if (choice === 1) event.preventDefault();
    });

    // The editor's Preview button calls window.open('/player/...') unchanged
    // from the browser build (see previewInWasmPlayer() in editor-store.ts) —
    // allow that same-origin popup to become the WasmPlayer window; send
    // anything else (e.g. external links) to the OS browser instead of a
    // second app window.
    editorWindow.webContents.setWindowOpenHandler(({ url }) => {
        if (url.startsWith(`http://127.0.0.1:${port}/player/`)) {
            return {
                action: "allow",
                // No preload for this popup — matches ipc/player.ts's
                // main-process-created player windows: it's untrusted game
                // content (games can eval their own <javascript> resources),
                // so it must never get window.electronApp's fs/dialog bridge.
                // autoplayPolicy explicit for the same reason as
                // createEditorWindow's own webPreferences above.
                overrideBrowserWindowOptions: {
                    // Electron's BrowserWindow default (800x600) is narrower
                    // than playercore.js's 950px gameWidth threshold, so the
                    // side panes would start collapsed behind the
                    // cmdShowPanes toggle button instead of docked. Wide
                    // enough to clear that threshold with margin to spare.
                    width: 1050,
                    height: 850,
                    webPreferences: {
                        contextIsolation: true,
                        nodeIntegration: false,
                        autoplayPolicy: "no-user-gesture-required",
                    },
                },
            };
        }
        void shell.openExternal(url);
        return { action: "deny" };
    });

    void editorWindow.loadURL(`http://127.0.0.1:${port}${initialUrlPath(initialPath ?? null)}`);

    editorWindow.on("closed", () => {
        editorWindow = null;
    });
}

// Windows/Linux only have one way to "double-click a second file while the
// app is already running": the OS launches a whole new process for it. This
// claims the lock for the first instance and quits every later one
// immediately — its argv (carrying the file path) is forwarded to the first
// instance's "second-instance" event below instead. macOS never launches a
// second process for this (Launch Services routes it to "open-file" on the
// existing instance instead), but requesting the lock is harmless there too.
const gotLock = app.requestSingleInstanceLock();
if (!gotLock) {
    app.quit();
} else {
    app.on("second-instance", (_event, argv) => {
        focusEditorWindow();
        const filePath = extractGameFilePath(argv);
        if (filePath) routeOpenedFile(filePath);
    });

    app.whenReady().then(async () => {
        registerFsHandlers();
        registerDialogHandlers();
        registerShellHandlers();
        registerPathsHandlers();
        // Edit-kind changes rebuild the native "Open Recent" submenu; both kinds
        // also get broadcast to the renderer (see broadcastRecentChanged).
        registerRecentHandlers((kind) => {
            if (kind === "edit") void refreshMenu();
            broadcastRecentChanged(kind);
        });
        registerPlayerHandlers(() => (staticServer ? `http://127.0.0.1:${staticServer.port}` : null));
        await refreshMenu();

        const root = staticRoot();
        staticServer = await startStaticServer({
            editor: path.join(root, "editor"),
            appBundle: path.join(root, "AppBundle"),
            player: path.join(root, "player"),
        });

        // Windows/Linux cold start: the launched file is this process's own
        // argv. macOS cold start: the "open-file" handler above already
        // captured it into pendingOpenPath (it fires before this resolves).
        createEditorWindow(staticServer.port, pendingOpenPath ?? extractGameFilePath(process.argv));

        app.on("activate", () => {
            if (BrowserWindow.getAllWindows().length === 0 && staticServer) createEditorWindow(staticServer.port);
        });
    });

    app.on("window-all-closed", () => {
        if (process.platform !== "darwin") app.quit();
    });

    app.on("before-quit", () => {
        void staticServer?.close();
    });
}
