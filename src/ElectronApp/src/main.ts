import { app, BrowserWindow, Menu, shell, type MenuItemConstructorOptions } from "electron";
import path from "node:path";
import { startStaticServer, type StaticServerHandle } from "./static-server";
import { registerFsHandlers } from "./ipc/fs";
import { registerDialogHandlers } from "./ipc/dialog";
import { registerShellHandlers } from "./ipc/shell";
import { registerRecentHandlers } from "./ipc/recent";
import { listRecentGames, clearRecentGames, type RecentGame } from "./recent-games";

// Without this, Electron's default macOS menu ("About "/"Quit ") falls back
// to package.json's "name" ("quest-viva-desktop") in dev — set as early as
// possible so it's correct whether packaged or run via `electron .`.
app.setName("Quest Viva");

// macOS's About panel shows "Version {CFBundleShortVersionString} ({CFBundleVersion})"
// — the parenthetical is meant for a separate build number, but electron-builder's
// -c.extraMetadata.version sets both plist fields to the same VERSION string
// (this project has no separate build-number concept), so it reads as a literal
// duplicate. Empty version suppresses the parenthetical rather than showing "()"
app.setAboutPanelOptions({ version: "" });

// Packaged: resources/app-static ships via electron-builder's extraResources.
// Dev: `npm run build` copies the same three directories here directly (see
// scripts/copy-static.mjs) — one code path for both.
function staticRoot(): string {
    return app.isPackaged
        ? path.join(process.resourcesPath, "app-static")
        : path.join(__dirname, "..", "resources", "app-static");
}

let editorWindow: BrowserWindow | null = null;
let staticServer: StaticServerHandle | null = null;

// Mirrors the union WebEditor's src/routes/+layout.svelte switches on (see
// window.electronApp.menu.onAction in preload.ts) — the two sides can't share
// a type since they're separate npm projects, so keep them in sync by hand.
type MenuAction = "new-game" | "open-folder" | "save" | "save-as";

function sendMenuAction(action: MenuAction): void {
    editorWindow?.webContents.send("menu-action", action);
}

// Renderer has no fixed "open this specific game" action string (unlike
// MenuAction) since it needs to carry a path — a dedicated channel instead of
// overloading sendMenuAction/onAction with a payload.
function sendOpenRecentGame(game: RecentGame): void {
    editorWindow?.webContents.send("open-recent-game", { dirPath: game.dirPath, filename: game.filename });
}

// Everything the editor itself can do (New/Open/Save/Save As) has to round-trip
// to the renderer — that's where WasmEditor and the file adapters live, main
// only owns the native chrome around it. Built once for every platform (rather
// than only patching non-darwin's missing About, as an earlier pass here did)
// so File/Save exist at all: without a menu at all, WebEditor's own UI has no
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
            { label: "Clear Recent", click: () => { void clearRecentGames().then(refreshMenu); } },
        ]
        : [{ label: "No Recent Games", enabled: false }];
    const template: MenuItemConstructorOptions[] = [
        ...(isMac ? [{ role: "appMenu" as const }] : []),
        {
            label: "File",
            submenu: [
                { label: "New Game…", accelerator: "CmdOrCtrl+N", click: () => sendMenuAction("new-game") },
                { label: "Open Game Folder…", accelerator: "CmdOrCtrl+O", click: () => sendMenuAction("open-folder") },
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

// Re-reads the recent-games list from disk and re-applies the app menu —
// called once at startup and again after every recent:add/remove (via
// registerRecentHandlers' onChange) and after "Clear Recent". Also notifies
// the renderer: the /open page's own Remove button already updates its local
// state directly, but a change that originates in the main process (Clear
// Recent) has no other way to reach an already-mounted /open page.
async function refreshMenu(): Promise<void> {
    Menu.setApplicationMenu(buildAppMenu(await listRecentGames()));
    editorWindow?.webContents.send("recent-games-changed");
}

function createEditorWindow(port: number): void {
    editorWindow = new BrowserWindow({
        title: "Quest Viva",
        width: 1280,
        height: 860,
        webPreferences: {
            preload: path.join(__dirname, "preload.js"),
            contextIsolation: true,
            nodeIntegration: false,
        },
    });

    // WebEditor's own <title> ("Quest Viva Editor") is correct for the
    // browser build, where it's the only app; the desktop app covers editing
    // and playing, so it's just "Quest Viva" here — override the page's title
    // instead of changing the shared app.html tag.
    editorWindow.on("page-title-updated", (event) => event.preventDefault());

    // The editor's Preview button calls window.open('/player/...') unchanged
    // from the browser build (see previewInWasmPlayer() in editor-store.ts) —
    // allow that same-origin popup to become the WasmPlayer window; send
    // anything else (e.g. external links) to the OS browser instead of a
    // second app window.
    editorWindow.webContents.setWindowOpenHandler(({ url }) => {
        if (url.startsWith(`http://127.0.0.1:${port}/player/`)) {
            return { action: "allow" };
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
    registerRecentHandlers(() => void refreshMenu());
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
