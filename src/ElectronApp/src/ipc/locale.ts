import { ipcMain } from "electron";
import { readLocale, writeLocale } from "../locale-store";

// Backs window.electronApp.locale in preload.ts — see locale-store.ts for why
// this can't just be AppShell's own localStorage-based persistence.
export function registerLocaleHandlers(): void {
    ipcMain.handle("locale:get", async () => readLocale());
    ipcMain.handle("locale:set", async (_event, locale: string) => writeLocale(locale));
}
