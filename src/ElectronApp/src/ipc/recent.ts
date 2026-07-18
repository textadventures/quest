import { ipcMain } from "electron";
import { listRecentGames, addRecentGame, removeRecentGame, type RecentKind } from "../recent-games";

// Backs window.electronApp.recent in preload.ts. Every call carries a
// RecentKind ("edit" vs "play" — see recent-games.ts) since the two lists are
// stored and surfaced separately. onChange fires after any mutation, with the
// kind that changed, so main.ts can rebuild the native "Open Recent" submenu
// (edit-kind only) and/or notify the renderer — recent-games.ts itself
// doesn't know about the Menu or any window, that wiring stays in main.ts.
export function registerRecentHandlers(onChange: (kind: RecentKind) => void): void {
    ipcMain.handle("recent:list", async (_event, kind: RecentKind) => listRecentGames(kind));

    ipcMain.handle("recent:add", async (_event, kind: RecentKind, dirPath: string, filename: string) => {
        const games = await addRecentGame(kind, dirPath, filename);
        onChange(kind);
        return games;
    });

    ipcMain.handle("recent:remove", async (_event, kind: RecentKind, dirPath: string, filename: string) => {
        const games = await removeRecentGame(kind, dirPath, filename);
        onChange(kind);
        return games;
    });
}
