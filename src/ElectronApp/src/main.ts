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

let editorWindow: BrowserWindow | null = null;
let staticServer: StaticServerHandle | null = null;

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

function createEditorWindow(port: number): void {
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

    void editorWindow.loadURL(`http://127.0.0.1:${port}/`);

    editorWindow.on("closed", () => {
        editorWindow = null;
    });
}

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

    createEditorWindow(staticServer.port);

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
