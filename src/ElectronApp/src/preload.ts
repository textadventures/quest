import { contextBridge, ipcRenderer } from "electron";
import type { RecentGame, RecentKind } from "./recent-games";

interface FileFilter {
    name: string;
    extensions: string[];
}

// Plain string join, no Node "path" module — sandboxed preload only gets a
// polyfilled subset of Node built-ins, and this doesn't need the real thing.
// Separator style follows dirPath (an OS-native absolute path from the main
// process, e.g. from a dialog result), not process.platform.
function joinPath(...segments: string[]): string {
    const nonEmpty = segments.filter((s) => s.length > 0);
    if (nonEmpty.length === 0) return "";
    const sep = nonEmpty[0].includes("\\") && !nonEmpty[0].includes("/") ? "\\" : "/";
    return nonEmpty
        .map((s, i) => (i === 0 ? s.replace(/[\\/]+$/, "") : s.replace(/^[\\/]+|[\\/]+$/g, "")))
        .filter((s) => s.length > 0)
        .join(sep);
}

// Sent as analytics metadata alongside webhome_games_list/webhome_game_play
// (see ClientInfo on textadventures.co.uk's ApiController, and
// home-catalog.ts's clientInfoParams()). process.platform is only available
// here in the preload context, not the sandboxed renderer.
function hostPlatform(): "Mac" | "Windows" | "Linux" | undefined {
    switch (process.platform) {
        case "darwin": return "Mac";
        case "win32": return "Windows";
        case "linux": return "Linux";
        default: return undefined;
    }
}

// Matches the window.electronApp shape sketched in docs/electron-desktop-app.md
// and consumed by src/AppShell/src/lib/filesystem/electron-adapter.ts.
contextBridge.exposeInMainWorld("electronApp", {
    platform: hostPlatform(),
    fs: {
        readFile: (path: string): Promise<Uint8Array> => ipcRenderer.invoke("fs:readFile", path),
        writeFile: (path: string, data: Uint8Array | string): Promise<void> => ipcRenderer.invoke("fs:writeFile", path, data),
        readDir: (path: string): Promise<{ name: string; isFile: boolean }[]> => ipcRenderer.invoke("fs:readDir", path),
        exists: (path: string): Promise<boolean> => ipcRenderer.invoke("fs:exists", path),
        mkdir: (path: string): Promise<void> => ipcRenderer.invoke("fs:mkdir", path),
        unlink: (path: string): Promise<void> => ipcRenderer.invoke("fs:unlink", path),
    },
    dialog: {
        openFile: (options?: { filters?: FileFilter[]; defaultPath?: string }): Promise<string | null> =>
            ipcRenderer.invoke("dialog:openFile", options),
        openDirectory: (options?: { defaultPath?: string; title?: string; message?: string; buttonLabel?: string }): Promise<string | null> =>
            ipcRenderer.invoke("dialog:openDirectory", options),
        saveFile: (options?: { defaultPath?: string; filters?: FileFilter[] }): Promise<string | null> =>
            ipcRenderer.invoke("dialog:saveFile", options),
    },
    shell: {
        openExternal: (url: string): Promise<void> => ipcRenderer.invoke("shell:openExternal", url),
    },
    path: {
        join: joinPath,
    },
    paths: {
        defaultGamesDir: (): Promise<string> => ipcRenderer.invoke("paths:defaultGamesDir"),
    },
    recent: {
        // kind distinguishes the editor's Recent (backs File > Open Recent
        // and the /open page) from the Play tab's Recently Played — see
        // recent-games.ts's RecentKind. Two separate on-disk lists, so
        // playing a game never pollutes the list File > Open Recent loads
        // into the editor, and vice versa.
        list: (kind: RecentKind): Promise<RecentGame[]> => ipcRenderer.invoke("recent:list", kind),
        add: (kind: RecentKind, dirPath: string, filename: string): Promise<RecentGame[]> =>
            ipcRenderer.invoke("recent:add", kind, dirPath, filename),
        remove: (kind: RecentKind, dirPath: string, filename: string): Promise<RecentGame[]> =>
            ipcRenderer.invoke("recent:remove", kind, dirPath, filename),
        // Fires whenever a recent list changes from outside the page that's
        // showing it — e.g. the native "Clear Recent" menu item (edit-kind
        // only), which mutates the list entirely in the main process with no
        // renderer round trip otherwise. Callback receives which kind changed
        // so a listener can ignore updates to the other list. Pages that
        // mutate their own list locally (e.g. a Remove button) already update
        // their local state directly and don't need this for that path.
        onChanged: (callback: (kind: RecentKind) => void): (() => void) => {
            const listener = (_event: Electron.IpcRendererEvent, kind: RecentKind) => callback(kind);
            ipcRenderer.on("recent-games-changed", listener);
            return () => ipcRenderer.removeListener("recent-games-changed", listener);
        },
    },
    menu: {
        // Backs the native File menu built in main.ts's buildAppMenu() — action
        // is one of MenuAction there ("new-game" | "open-file" | "save" |
        // "save-as"), kept as a plain string here since preload has no import
        // from main's module graph. Returns an unsubscribe so +layout.svelte's
        // onMount can clean up on HMR/teardown.
        onAction: (callback: (action: string) => void): (() => void) => {
            const listener = (_event: Electron.IpcRendererEvent, action: string) => callback(action);
            ipcRenderer.on("menu-action", listener);
            return () => ipcRenderer.removeListener("menu-action", listener);
        },
        // Fired when the user picks an entry from the native "Open Recent"
        // submenu (main.ts's sendOpenRecentGame) — a separate channel from
        // onAction since it carries a payload rather than a fixed action name.
        onOpenRecent: (callback: (game: { dirPath: string; filename: string }) => void): (() => void) => {
            const listener = (_event: Electron.IpcRendererEvent, game: { dirPath: string; filename: string }) => callback(game);
            ipcRenderer.on("open-recent-game", listener);
            return () => ipcRenderer.removeListener("open-recent-game", listener);
        },
    },
    player: {
        // Opens a dedicated player BrowserWindow (see ipc/player.ts) rather
        // than a renderer-driven window.open() — sidesteps the browser's
        // popup-blocker/user-activation rules entirely (main-process window
        // creation isn't subject to them), which is what lets Play launch
        // straight from a single file-picker click instead of needing a
        // second "Start" click to satisfy a fresh gesture.
        openWindow: (request?: { id?: string }): Promise<boolean> => ipcRenderer.invoke("player:openWindow", request),
        // Fired when the OS launches or relaunches the app via a play-kind
        // file association (.quest/.asl/.cas) while a window already exists
        // — main.ts's routeOpenedFile sends this instead of focusing straight
        // into a player window, since PlayCatalog.svelte needs to load the
        // file's bytes first (see its onOpenPlayFile listener). Cold-start
        // opens don't use this channel at all — see main.ts's initialUrlPath.
        onOpenPlayFile: (callback: (file: { dirPath: string; filename: string }) => void): (() => void) => {
            const listener = (_event: Electron.IpcRendererEvent, file: { dirPath: string; filename: string }) => callback(file);
            ipcRenderer.on("open-play-file", listener);
            return () => ipcRenderer.removeListener("open-play-file", listener);
        },
    },
});
