import { ipcMain, shell } from "electron";

// Backs window.electronApp.shell in preload.ts.
export function registerShellHandlers(): void {
    ipcMain.handle("shell:openExternal", async (_event, url: string) => {
        await shell.openExternal(url);
    });
}
