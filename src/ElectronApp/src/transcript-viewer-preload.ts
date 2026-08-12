import { contextBridge, ipcRenderer } from "electron";

// Scoped bridge for the TranscriptViewer child window only (see
// ipc/player.ts's did-create-window handler, which points its
// overrideBrowserWindowOptions.webPreferences.preload here) — separate from
// preload.ts's window.electronApp (the editor window's full fs/dialog
// bridge) and from player-preload.ts (the player window's much narrower
// append-only transcript bridge, see that file's comment for why the two must
// stay split). TranscriptViewer is first-party content we ship, not
// game-supplied, so it's fine for it to read and delete transcripts as well
// as list them — mirrors what TranscriptViewer/index.html used to do
// directly against localStorage.
contextBridge.exposeInMainWorld("electronTranscripts", {
    list: (): Promise<string[]> => ipcRenderer.invoke("transcript:list"),
    read: (name: string): Promise<string | null> => ipcRenderer.invoke("transcript:read", name),
    delete: (name: string): Promise<void> => ipcRenderer.invoke("transcript:delete", name),
});
