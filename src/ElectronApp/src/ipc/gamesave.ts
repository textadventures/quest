import { ipcMain } from "electron";
import { listSaves, saveGame, loadGame, deleteSaveSlot } from "../save-store";

// Backs player-preload.ts's window.electronGameSaves — the player window's
// own bridge (like window.electronTranscripts, registering here doesn't by
// itself expose these to any window; only what a preload bridges via
// contextBridge does — see player-preload.ts's comment).
export function registerGameSaveHandlers(): void {
    ipcMain.handle("gamesave:list", async (_event, gameId: string) => listSaves(gameId));

    ipcMain.handle(
        "gamesave:save",
        async (
            _event,
            gameId: string,
            slotIndex: number,
            data: Uint8Array,
            name: string | null,
            chromeStrings?: Record<string, string> | null,
        ) => saveGame(gameId, slotIndex, data, name, chromeStrings),
    );

    ipcMain.handle("gamesave:load", async (_event, gameId: string, slotIndex: number) => loadGame(gameId, slotIndex));

    ipcMain.handle("gamesave:delete", async (_event, gameId: string, slotIndex: number) => deleteSaveSlot(gameId, slotIndex));
}
