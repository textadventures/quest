import { ipcMain } from "electron";
import { readUiState, setUiState } from "../ui-state-store";

// Backs window.electronApp.uiState in preload.ts — per-game editor UI state
// (currently the game tree's expanded-node ids) persisted to a userData file,
// since static-server.ts's ephemeral port makes localStorage unreliable in
// Electron. See ui-state-store.ts.
export function registerUiStateHandlers(): void {
    ipcMain.handle("uiState:get", async () => readUiState());
    ipcMain.handle("uiState:set", async (_event, gameId: string, state: string[]) => setUiState(gameId, state));
}
