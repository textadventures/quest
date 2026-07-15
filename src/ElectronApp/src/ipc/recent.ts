import { ipcMain } from "electron";
import { listRecentGames, addRecentGame, removeRecentGame } from "../recent-games";

// Backs window.electronApp.recent in preload.ts. onChange fires after any
// mutation so main.ts can rebuild the native "Open Recent" submenu — recent-games.ts
// itself doesn't know about the Menu, this keeps that wiring in main.ts.
export function registerRecentHandlers(onChange: () => void): void {
    ipcMain.handle("recent:list", async () => listRecentGames());

    ipcMain.handle("recent:add", async (_event, dirPath: string, filename: string) => {
        const games = await addRecentGame(dirPath, filename);
        onChange();
        return games;
    });

    ipcMain.handle("recent:remove", async (_event, dirPath: string, filename: string) => {
        const games = await removeRecentGame(dirPath, filename);
        onChange();
        return games;
    });
}
