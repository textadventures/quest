import { ipcMain } from "electron";
import { listCatalogPlays, recordCatalogPlay, removeCatalogPlay, type RecentCatalogPlay } from "../catalog-plays-store";

// Backs window.electronApp.catalogPlays in preload.ts.
export function registerCatalogPlaysHandlers(): void {
    ipcMain.handle("catalogPlays:list", async () => listCatalogPlays());
    ipcMain.handle("catalogPlays:record", async (_event, game: Omit<RecentCatalogPlay, "lastPlayed">) => recordCatalogPlay(game));
    ipcMain.handle("catalogPlays:remove", async (_event, id: string) => removeCatalogPlay(id));
}
