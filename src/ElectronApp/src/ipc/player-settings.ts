import { ipcMain } from "electron";
import { readColourSchemeSync, writeColourScheme } from "../player-settings-store";

// Backs window.electronPlayerSettings in player-preload.ts: the player's light or dark choice,
// and nothing else. Player windows run untrusted game code, so the store only ever accepts or
// returns one of its three known values (player-settings-store.ts).
export function registerPlayerSettingsHandlers(): void {
    ipcMain.on("playersettings:getColourSchemeSync", (event) => {
        event.returnValue = readColourSchemeSync();
    });
    ipcMain.handle("playersettings:setColourScheme", async (_event, value: unknown) => writeColourScheme(value));
}
