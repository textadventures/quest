import { ipcMain, dialog, BrowserWindow, type FileFilter } from "electron";

interface OpenFileOptions {
    filters?: FileFilter[];
    defaultPath?: string;
}

interface SaveFileOptions {
    defaultPath?: string;
    filters?: FileFilter[];
}

function focusedOrFirstWindow(): BrowserWindow | undefined {
    return BrowserWindow.getFocusedWindow() ?? BrowserWindow.getAllWindows()[0];
}

// Backs window.electronApp.dialog in preload.ts.
export function registerDialogHandlers(): void {
    ipcMain.handle("dialog:openFile", async (_event, options?: OpenFileOptions) => {
        const win = focusedOrFirstWindow();
        const opts = { properties: ["openFile" as const], filters: options?.filters, defaultPath: options?.defaultPath };
        const result = win ? await dialog.showOpenDialog(win, opts) : await dialog.showOpenDialog(opts);
        return result.canceled || result.filePaths.length === 0 ? null : result.filePaths[0];
    });

    ipcMain.handle("dialog:openDirectory", async (_event, options?: { defaultPath?: string }) => {
        const win = focusedOrFirstWindow();
        // createDirectory (macOS) / promptToCreate (Windows) — without these,
        // showOpenDialog's directory picker has no "New Folder" affordance,
        // unlike the browser's showDirectoryPicker(), which always has one.
        const opts = {
            properties: ["openDirectory" as const, "createDirectory" as const, "promptToCreate" as const],
            defaultPath: options?.defaultPath,
        };
        const result = win ? await dialog.showOpenDialog(win, opts) : await dialog.showOpenDialog(opts);
        return result.canceled || result.filePaths.length === 0 ? null : result.filePaths[0];
    });

    ipcMain.handle("dialog:saveFile", async (_event, options?: SaveFileOptions) => {
        const win = focusedOrFirstWindow();
        const opts = { defaultPath: options?.defaultPath, filters: options?.filters };
        const result = win ? await dialog.showSaveDialog(win, opts) : await dialog.showSaveDialog(opts);
        return result.canceled || !result.filePath ? null : result.filePath;
    });
}
