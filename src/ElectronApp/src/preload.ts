import { contextBridge, ipcRenderer } from "electron";
import type { RecentGame } from "./recent-games";

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

// Matches the window.electronApp shape sketched in docs/electron-desktop-app.md
// and consumed by src/AppShell/src/lib/filesystem/electron-adapter.ts.
contextBridge.exposeInMainWorld("electronApp", {
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
        list: (): Promise<RecentGame[]> => ipcRenderer.invoke("recent:list"),
        add: (dirPath: string, filename: string): Promise<RecentGame[]> => ipcRenderer.invoke("recent:add", dirPath, filename),
        remove: (dirPath: string, filename: string): Promise<RecentGame[]> => ipcRenderer.invoke("recent:remove", dirPath, filename),
        // Fires whenever the recent list changes from outside the /open page
        // itself — e.g. the native "Clear Recent" menu item, which mutates
        // the list entirely in the main process with no renderer round trip
        // otherwise. The /open page's own Remove button already updates its
        // local state directly and doesn't need this.
        onChanged: (callback: () => void): (() => void) => {
            const listener = () => callback();
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
});
