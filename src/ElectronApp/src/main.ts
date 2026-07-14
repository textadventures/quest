import { app, BrowserWindow, shell } from "electron";
import path from "node:path";
import { startStaticServer, type StaticServerHandle } from "./static-server";
import { registerFsHandlers } from "./ipc/fs";
import { registerDialogHandlers } from "./ipc/dialog";
import { registerShellHandlers } from "./ipc/shell";

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
