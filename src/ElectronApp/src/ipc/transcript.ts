import { ipcMain } from "electron";
import { listTranscripts, appendTranscript, readTranscript, deleteTranscript } from "../transcript-store";

// Backs both transcript-viewer-preload.ts (list/read/delete) and
// player-preload.ts's transcript bridge (append only) — registering every handler
// here doesn't by itself grant either window access to the others; each
// preload only bridges the subset of these channels it exposes via
// contextBridge, and that's the actual security boundary (see both
// preloads' comments).
export function registerTranscriptHandlers(): void {
    ipcMain.handle("transcript:list", async () => listTranscripts());
    ipcMain.handle("transcript:append", async (_event, name: string, data: string) => appendTranscript(name, data));
    ipcMain.handle("transcript:read", async (_event, name: string) => readTranscript(name));
    ipcMain.handle("transcript:delete", async (_event, name: string) => deleteTranscript(name));
}
