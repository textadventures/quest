import { ipcMain } from "electron";
import { readTheme, readThemeSync, writeTheme } from "../theme-store";

// Backs window.electronApp.theme in preload.ts — see theme-store.ts for why
// this can't just be AppShell's own localStorage-based persistence. The sync
// channel exists for app.html's pre-paint inline script, which can't await.
export function registerThemeHandlers(): void {
    ipcMain.handle("theme:get", async () => readTheme());
    ipcMain.handle("theme:set", async (_event, theme: string) => writeTheme(theme));
    ipcMain.on("theme:getSync", (event) => {
        event.returnValue = readThemeSync();
    });
}
