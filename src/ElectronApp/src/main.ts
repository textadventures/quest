import { app, BrowserWindow, Menu, MenuItem, dialog, shell, type MenuItemConstructorOptions } from "electron";
import path from "node:path";
import { startStaticServer, type StaticServerHandle } from "./static-server";
import { registerFsHandlers } from "./ipc/fs";
import { registerDialogHandlers } from "./ipc/dialog";
import { registerShellHandlers } from "./ipc/shell";
import { registerPathsHandlers } from "./ipc/paths";
import { registerRecentHandlers } from "./ipc/recent";
import { registerPlayerHandlers, attachTranscriptWindowHandling } from "./ipc/player";
import { registerTranscriptHandlers } from "./ipc/transcript";
import { registerGameSaveHandlers } from "./ipc/gamesave";
import { registerCatalogPlaysHandlers } from "./ipc/catalog-plays";
import { registerUpdateDismissHandlers } from "./ipc/update-dismiss";
import { registerDefaultCodeViewHandlers } from "./ipc/default-code-view";
import { registerFileWatchHandlers } from "./ipc/file-watch";
import { registerLocaleHandlers } from "./ipc/locale";
import { registerThemeHandlers } from "./ipc/theme";
import { registerUiStateHandlers } from "./ipc/ui-state";
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
type MenuAction = "new-game" | "open-file" | "save" | "save-as" | "undo" | "redo";

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

// New Game/Open Game must work even with the editor window fully closed
// (macOS: app stays running, menu bar stays up, no window at all) — same
// requirement the comment on focusEditorWindow above already describes, but
// that function (and the plain IPC send below) both silently no-op once
// editorWindow is null, so this was never actually reachable. Save/Save
// As/Undo/Redo have nothing to act on with no game loaded, so those stay
// plain no-ops in that state, same as before.
function sendMenuAction(action: MenuAction): void {
    if (!editorWindow) {
        if (staticServer && (action === "new-game" || action === "open-file")) {
            const query = action === "open-file" ? `?action=open&t=${Date.now()}` : "";
            createEditorWindow(staticServer.port, null, `/open${query}`);
        }
        return;
    }
    focusEditorWindow();
    editorWindow.webContents.send("menu-action", action);
}

// Undo/Redo can't just forward blindly like sendMenuAction does for File
// actions: the editor has its own app-level undo stack (EditorCore's
// UndoLogger, exposed via editor-store's canUndo/canRedo) with no native
// equivalent, but Cmd+Z/Cmd+Shift+Z is a single app-wide accelerator on mac
// shared by every window. Forcing focus to the editor here (the way
// sendMenuAction does) would mean reflexively hitting Cmd+Z while playing a
// game in a player window yanks that window's focus away and undoes an
// unrelated editor edit. Instead: forward to the editor only when it's
// already the focused window; otherwise fall back to the focused window's
// own native text-field undo/redo (e.g. text typed into the player's command
// box), exactly like Electron's default 'undo'/'redo' roles would.
//
// Within the editor window itself, the same ambiguity exists one level down:
// the accelerator fires before the DOM ever sees the keystroke, so it can't
// tell "focus is on the tree/canvas" (app-level undo should apply) apart from
// "focus is in a <textarea>/<input>/contenteditable script or richtext field"
// (the browser's own native undo should apply instead, or app-level undo
// would clobber in-progress text edits the native undo stack still has).
// Ask the renderer which kind of element is currently focused before
// deciding which undo to invoke.
async function isTextEditingFocused(window: BrowserWindow): Promise<boolean> {
    try {
        return await window.webContents.executeJavaScript(
            "(() => { const el = document.activeElement; if (!el) return false; " +
            "return el.tagName === 'TEXTAREA' || el.tagName === 'INPUT' || el.isContentEditable; })()",
        );
    } catch {
        return false;
    }
}

function editorUndoRedo(action: "undo" | "redo") {
    return async (_item: MenuItem, focusedWindow: Electron.BaseWindow | undefined): Promise<void> => {
        if (focusedWindow === editorWindow && editorWindow) {
            if (await isTextEditingFocused(editorWindow)) {
                if (action === "undo") editorWindow.webContents.undo();
                else editorWindow.webContents.redo();
            } else {
                editorWindow.webContents.send("menu-action", action);
            }
        } else if (focusedWindow instanceof BrowserWindow) {
            if (action === "undo") focusedWindow.webContents.undo();
            else focusedWindow.webContents.redo();
        }
    };
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
// menu), /?action=play-file&... by PlayCatalog.svelte (mirrors it for Play —
// root is always the Play tab in this app, PUBLIC_SHOW_HOME is always true here).
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
        {
            label: "Edit",
            submenu: [
                // Undo/Redo replace the role-based defaults (which only ever
                // undo native text-field edits) with the editor's real
                // command-stack undo — see editorUndoRedo's comment. Cut/
                // Copy/Paste/Select All stay as plain roles: there's no
                // app-level clipboard or selection concept for tree/canvas
                // content, only ordinary text fields, which the native roles
                // already handle correctly for whichever window/field is
                // focused. Select All's role must be listed explicitly like
                // this — Electron only auto-adds Cmd+A (and Cut/Copy/Paste)
                // to a *default* Edit menu; once this app builds its own
                // Edit submenu, nothing claims that accelerator unless a
                // role item for it is present, so without one Cmd+A silently
                // does nothing in every text field.
                { label: "Undo", accelerator: "CmdOrCtrl+Z", click: editorUndoRedo("undo") },
                { label: "Redo", accelerator: isMac ? "Shift+Cmd+Z" : "Ctrl+Y", click: editorUndoRedo("redo") },
                { type: "separator" },
                { role: "cut" },
                { role: "copy" },
                { role: "paste" },
                { role: "selectAll" },
            ],
        },
        {
            // Hand-rolled rather than role: "viewMenu" — the default template
            // also includes Reload/Force Reload/Toggle Developer Tools, which
            // don't make sense for the editor window (there's nothing to
            // "reload" from a user's perspective, and DevTools is reserved
            // for Player windows — see createEditorWindow's devTools: false
            // and player.ts, which leaves it at Electron's default instead).
            label: "View",
            submenu: [
                { role: "resetZoom" },
                { role: "zoomIn" },
                { role: "zoomOut" },
                // mac only: omitted here rather than included unconditionally
                // because of an Electron regression (39.1+) where AppKit's
                // own automatic "Enter Full Screen" injection into any menu
                // titled "View" no longer detects our item and skips adding
                // its own — it used to, so before this regression an
                // explicit item here was the only one; now it'd double up.
                // electron/electron#49048 tracked the original report and
                // was closed as fixed (electron/electron#49074, backported
                // to 38.x–41.x), but electron/electron#50531 reproduced the
                // same duplicate afterwards on 41.1.0 and is still open/
                // unresolved as of this writing — so this workaround stays
                // until someone confirms on a newer Electron that the
                // official fix actually holds. Letting AppKit's automatic
                // item be the only one sidesteps the mechanism entirely
                // rather than depending on it. Windows/Linux have no such
                // auto-injection, so they still need an explicit item.
                ...(isMac ? [] : [{ type: "separator" as const }, { role: "togglefullscreen" as const }]),
                // Hidden (visible: false) rather than left off entirely:
                // accelerators in Electron are only live when a matching
                // menu item exists (there's no unconditional Chromium-level
                // F12/DevTools binding independent of the Menu), so dropping
                // role: "toggleDevTools" outright — as an earlier pass here
                // did — silently killed the shortcut everywhere, including
                // in Player/Preview windows where it's supposed to work.
                // Both common bindings are listed so either works. Safe
                // no-op on the editor window regardless, since
                // createEditorWindow sets devTools: false there.
                { role: "toggleDevTools", visible: false, accelerator: "F12" },
                { role: "toggleDevTools", visible: false, accelerator: isMac ? "Alt+Cmd+I" : "Ctrl+Shift+I" },
            ],
        },
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

function createEditorWindow(port: number, initialPath?: string | null, initialRoute?: string): void {
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
            // DevTools is reserved for Player windows (see player.ts, which
            // leaves this at Electron's default true) — this blocks it
            // outright at the webContents level, not just the menu item/F12
            // accelerator, since `devTools: false` makes openDevTools()/
            // toggleDevTools() no-ops too.
            devTools: false,
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
    //
    // Calling event.preventDefault() here is what forces the close through
    // (its semantics: "ignore the page's beforeunload and unload anyway"),
    // and per Electron's contract that has to happen synchronously, before
    // this listener returns — checked immediately once the event finishes
    // firing, with no way to come back to it later. That rules out an async
    // confirmation dialog directly in this handler: it would return before
    // the user answers, preventDefault() would never be called in time, and
    // the close would just stay silently blocked (same as no handler at
    // all), no matter what the user later picks.
    //
    // So instead: leave the first attempt blocked (this handler returns
    // without calling preventDefault(), same as "Cancel" today) while an
    // async dialog is up, and only if the user confirms, close() the window
    // again ourselves — that re-runs the page's beforeunload and re-fires
    // this same event, and this time forceCloseConfirmed lets it through
    // immediately without asking again. Using the async dialog (rather than
    // *Sync) matters beyond just this handler: the sync version blocks the
    // entire main process — no IPC, no other window's menu actions, nothing
    // — for as long as it's on screen; this way only this window sits idle
    // pending an answer while the rest of the app stays responsive.
    let forceCloseConfirmed = false;
    editorWindow.webContents.on("will-prevent-unload", (event) => {
        if (!editorWindow) return;
        if (forceCloseConfirmed) {
            event.preventDefault();
            return;
        }
        void dialog.showMessageBox(editorWindow, {
            type: "question",
            buttons: ["Cancel", "Leave"],
            defaultId: 0,
            cancelId: 0,
            message: "Leave without saving?",
            detail: "You have unsaved changes. If you leave now, they'll be lost.",
        }).then(({ response }) => {
            if (response !== 1 || !editorWindow) return;
            forceCloseConfirmed = true;
            editorWindow.close();
        });
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
                        // Same player-window bridge as ipc/player.ts's player
                        // windows (transcripts + game saves) — a previewed
                        // game can use either. See player-preload.ts's comment.
                        preload: path.join(__dirname, "player-preload.js"),
                    },
                },
            };
        }
        void shell.openExternal(url);
        return { action: "deny" };
    });
    // The preview player window above is itself an opener: its game content
    // can print the same "view transcript" link ipc/player.ts's Play-flow
    // windows do (playercore.js's transcriptUrl), which needs the
    // TranscriptViewer child window wired up the same way — see
    // attachTranscriptWindowHandling's comment. Without this, that popup
    // opens with no preload at all (Electron's default for an
    // unhandled/unconfigured window.open) and window.electronTranscripts is
    // left undefined.
    editorWindow.webContents.on("did-create-window", (childWindow, details) => {
        if (details.url.startsWith(`http://127.0.0.1:${port}/player/`)) {
            attachTranscriptWindowHandling(childWindow, `http://127.0.0.1:${port}`);
        }
    });

    // initialRoute is a pre-built route (used by sendMenuAction below to land
    // straight on /open, optionally with its own action query param) — it
    // bypasses initialUrlPath, which only ever derives a route from a file
    // path (file-association opens), not from an arbitrary in-app action.
    void editorWindow.loadURL(`http://127.0.0.1:${port}${initialRoute ?? initialUrlPath(initialPath ?? null)}`);

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
        registerLocaleHandlers();
        registerThemeHandlers();
        registerUiStateHandlers();
        // Edit-kind changes rebuild the native "Open Recent" submenu; both kinds
        // also get broadcast to the renderer (see broadcastRecentChanged).
        registerRecentHandlers((kind) => {
            if (kind === "edit") void refreshMenu();
            broadcastRecentChanged(kind);
        });
        registerPlayerHandlers(() => (staticServer ? `http://127.0.0.1:${staticServer.port}` : null));
        registerTranscriptHandlers();
        registerGameSaveHandlers();
        registerCatalogPlaysHandlers();
        registerUpdateDismissHandlers();
        registerDefaultCodeViewHandlers();
        // Renderer-armed (see electron-adapter.ts's arm calls) — this only ever
        // notifies about whatever file(s) the renderer last asked to watch.
        registerFileWatchHandlers((filenames) => {
            editorWindow?.webContents.send("file-changed-externally", filenames);
        });
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
