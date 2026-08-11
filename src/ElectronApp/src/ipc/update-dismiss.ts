import { ipcMain } from "electron";
import { readDismissedVersion, writeDismissedVersion } from "../update-dismiss-store";

// Backs window.electronApp.updateDismiss in preload.ts — see
// update-dismiss-store.ts for why this can't just be UpdateBanner.svelte's
// own localStorage-based persistence.
export function registerUpdateDismissHandlers(): void {
    ipcMain.handle("updateDismiss:get", async () => readDismissedVersion());
    ipcMain.handle("updateDismiss:set", async (_event, version: string) => writeDismissedVersion(version));
}
