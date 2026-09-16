import { ipcMain } from "electron";
import { readPublishTargets, setPublishTarget } from "../publish-target-store";

// Backs window.electronApp.publishTarget in preload.ts — see
// publish-target-store.ts for why this can't just be localStorage.
export function registerPublishTargetHandlers(): void {
    ipcMain.handle("publishTarget:get", async (_event, gameId: string) => (await readPublishTargets())[gameId] ?? null);
    ipcMain.handle("publishTarget:set", async (_event, gameId: string, target: string) => setPublishTarget(gameId, target));
}
