import { app, ipcMain } from "electron";
import path from "node:path";

// Backs window.electronApp.paths in preload.ts. Matches Quest 5's desktop
// editor, which defaults new games into "Documents/Quest Games/<name>"
// rather than making the user pick a folder for every new game.
export function registerPathsHandlers(): void {
    ipcMain.handle("paths:defaultGamesDir", () => path.join(app.getPath("documents"), "Quest Games"));
}
