import { ipcMain } from "electron";
import { readDefaultCodeView, writeDefaultCodeView } from "../default-code-view-store";

// Backs window.electronApp.defaultCodeView in preload.ts — see
// default-code-view-store.ts for why this can't just be AppShell's own
// localStorage-based persistence.
export function registerDefaultCodeViewHandlers(): void {
    ipcMain.handle("defaultCodeView:get", async () => readDefaultCodeView());
    ipcMain.handle("defaultCodeView:set", async (_event, enabled: boolean) => writeDefaultCodeView(enabled));
}
