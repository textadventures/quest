import { ipcMain, BrowserWindow, dialog, shell } from "electron";
import path from "node:path";

export interface PlayerWindowRequest {
    // Catalog game (textadventures.co.uk id) vs. a locally-picked file whose
    // bytes/resource-request handling PlayCatalog.svelte hands over the
    // BroadcastChannel once this window signals 'ready' — see wasm-player.js's
    // `source=local` boot branch.
    id?: string;
}

// Backs window.electronApp.player in preload.ts. Takes a request *shape*, not
// a URL — the renderer never gets to hand main.ts an arbitrary string to
// navigate a new window to, only the two known player boot modes.
//
// Deliberately no preload here (unlike editorWindow): the player window runs
// arbitrary game content, including game-supplied <javascript> resources
// executed via eval (see wasm-player.js's WebPlayer.runJs) — an untrusted
// trust boundary that must never get window.electronApp's fs/dialog bridge.
// A locally-picked game's sibling resources are instead resolved by
// PlayCatalog.svelte (which *does* have fs access) answering
// 'resource-request' messages over the same BroadcastChannel used to hand
// over the initial game bytes, exactly like the existing editor-preview path.
export function registerPlayerHandlers(getOrigin: () => string | null): void {
    ipcMain.handle("player:openWindow", (_event, request?: PlayerWindowRequest) => {
        const origin = getOrigin();
        if (!origin) return false;

        const url = request?.id
            ? `${origin}/player/?id=${encodeURIComponent(request.id)}`
            : `${origin}/player/?source=local`;

        const win = new BrowserWindow({
            width: 1000,
            height: 800,
            webPreferences: {
                contextIsolation: true,
                nodeIntegration: false,
                // Electron's own default is already no-user-gesture-required
                // (unlike stock Chrome) — set explicitly so this doesn't
                // silently change if that default ever does. Paired with
                // wasm-player.js's Electron-UA check, which skips its
                // "click to begin" sound-activation gate entirely here.
                autoplayPolicy: "no-user-gesture-required",
                // The one narrow exception to "player windows get no
                // preload" — see player-preload.ts's comment for why
                // writeToTranscript()/GameSaver (running inside this
                // untrusted window) need this and why it's scoped so tightly.
                preload: path.join(__dirname, "..", "player-preload.js"),
            },
        });
        // Games can't open further windows of their own — same-origin
        // player navigation isn't a thing a game needs, and anything else
        // (a game script's window.open to some external URL) goes to the OS
        // browser instead of spawning another in-app window. The one
        // exception is the "view transcript" link JS.showTranscript() prints
        // (playercore.js's transcriptUrl, a same-origin relative link to
        // TranscriptViewer/index.html): that page reads the transcript data
        // straight out of this session's localStorage, so it has to open as
        // an in-app window on this same origin — sending it to the OS
        // browser would land on a *different* browser's storage jar (or, if
        // that browser happens to be Chromium-based and somehow shared,
        // still the wrong origin, since static-server.ts binds a fresh
        // random port every launch) and always show "no transcripts".
        win.webContents.setWindowOpenHandler(({ url: targetUrl }) => {
            if (targetUrl.startsWith(`${origin}/player/TranscriptViewer/`)) {
                return {
                    action: "allow",
                    overrideBrowserWindowOptions: {
                        width: 640,
                        height: 480,
                        webPreferences: {
                            contextIsolation: true,
                            nodeIntegration: false,
                            // First-party content (not game-supplied) — gets
                            // the fuller list/read/delete transcript bridge,
                            // not the player window's append-only one. See
                            // transcript-viewer-preload.ts.
                            preload: path.join(__dirname, "..", "transcript-viewer-preload.js"),
                        },
                    },
                };
            }
            void shell.openExternal(targetUrl);
            return { action: "deny" };
        });
        // TranscriptViewer/index.html's own OPEN button does
        // window.open("about:blank", ...) + document.write(...) to display a
        // transcript's contents — that's a *second* window.open, from the
        // TranscriptViewer window's webContents rather than this one, so it
        // needs its own handler. Trusted first-party content (shipped by us,
        // not game- or web-supplied), so allow it unconditionally rather
        // than pattern-matching about:blank.
        win.webContents.on("did-create-window", (childWindow, details) => {
            if (details.url.startsWith(`${origin}/player/TranscriptViewer/`)) {
                childWindow.webContents.setWindowOpenHandler(() => ({ action: "allow" }));
            }
        });
        // playercore.js's beforeunload handler calls e.preventDefault() once
        // hasUnsavedProgress is set (i.e. after a turn) — a real browser
        // would show its native "Leave site?" dialog for that, but Electron
        // gives no UI of its own; will-prevent-unload is the hook it exposes
        // for main to provide one. Without this, the window silently refuses
        // to close (same gap as createEditorWindow's handler in main.ts).
        win.webContents.on("will-prevent-unload", (event) => {
            const choice = dialog.showMessageBoxSync(win, {
                type: "question",
                buttons: ["Cancel", "Leave"],
                defaultId: 0,
                cancelId: 0,
                message: "Leave without saving?",
                detail: "You've made progress in this game that hasn't been saved. If you leave now, it'll be lost.",
            });
            if (choice === 1) event.preventDefault();
        });
        void win.loadURL(url);
        return true;
    });
}
