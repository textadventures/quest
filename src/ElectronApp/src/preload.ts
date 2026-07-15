import { contextBridge, ipcRenderer } from "electron";

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
// and consumed by src/WebEditor/src/lib/filesystem/electron-adapter.ts.
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
        openDirectory: (options?: { defaultPath?: string; title?: string; buttonLabel?: string }): Promise<string | null> =>
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
    menu: {
        // Backs the native File menu built in main.ts's buildAppMenu() — action
        // is one of MenuAction there ("new-game" | "open-folder" | "save" |
        // "save-as"), kept as a plain string here since preload has no import
        // from main's module graph. Returns an unsubscribe so +layout.svelte's
        // onMount can clean up on HMR/teardown.
        onAction: (callback: (action: string) => void): (() => void) => {
            const listener = (_event: Electron.IpcRendererEvent, action: string) => callback(action);
            ipcRenderer.on("menu-action", listener);
            return () => ipcRenderer.removeListener("menu-action", listener);
        },
    },
});
