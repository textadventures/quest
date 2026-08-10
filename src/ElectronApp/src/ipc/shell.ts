import { ipcMain, shell } from "electron";

// Backs window.electronApp.shell in preload.ts.
export function registerShellHandlers(): void {
    ipcMain.handle("shell:openExternal", async (_event, url: string) => {
        await shell.openExternal(url);
    });

    // Reveals a file in Finder/Explorer/the platform file manager — used by
    // the /open page's Recent list. showItemInFolder is synchronous in
    // Electron's API; wrapped in handle() only for a consistent invoke-based
    // call shape with the rest of this namespace.
    ipcMain.handle("shell:showItemInFolder", (_event, fullPath: string) => {
        shell.showItemInFolder(fullPath);
    });
}
