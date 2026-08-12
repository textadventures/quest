import { contextBridge, ipcRenderer } from "electron";

// The one preload for player windows (see ipc/player.ts's player:openWindow
// handler and main.ts's Preview popup override, both of which point their
// webPreferences.preload here) — the only IPC surface ever exposed to a
// window that runs untrusted, game-supplied <javascript> resources via eval
// (see ipc/player.ts's existing comment on why these windows otherwise get
// no preload at all). Electron only allows one preload script per window, so
// both bridges this untrusted context needs — transcripts and game saves —
// live in this single file rather than one each.
//
// electronTranscripts.append: writes a chunk of text to the game's own named
// transcript. transcript-store.ts always escapes `name` into a single path
// segment before touching disk, so a game can't traverse outside
// userData/transcripts or reach another transcript beyond appending to
// whichever one it names.
//
// electronGameSaves: save/list/load/delete, keyed by [gameId, slotIndex]
// exactly like the IndexedDB store this replaces (see save-store.ts) — same
// per-game path escaping, so a game can only ever touch its own save slots.
//
// Neither bridge exposes list/read/delete for transcripts, or any operation
// on another game's saves — those stay on transcript-viewer-preload.ts's
// separate, trusted-content-only bridge (there's no equivalent save-browsing
// UI outside the player window itself, so no second bridge is needed there).
contextBridge.exposeInMainWorld("electronTranscripts", {
    append: (name: string, data: string): Promise<void> => ipcRenderer.invoke("transcript:append", name, data),
});

contextBridge.exposeInMainWorld("electronGameSaves", {
    list: (
        gameId: string,
    ): Promise<{ slotIndex: number; name: string | null; timestamp: number | null; chromeStrings: Record<string, string> | null }[]> =>
        ipcRenderer.invoke("gamesave:list", gameId),
    save: (
        gameId: string,
        slotIndex: number,
        data: Uint8Array,
        name: string | null,
        chromeStrings?: Record<string, string> | null,
    ): Promise<void> => ipcRenderer.invoke("gamesave:save", gameId, slotIndex, data, name, chromeStrings),
    load: (gameId: string, slotIndex: number): Promise<Uint8Array | null> =>
        ipcRenderer.invoke("gamesave:load", gameId, slotIndex),
    delete: (gameId: string, slotIndex: number): Promise<void> => ipcRenderer.invoke("gamesave:delete", gameId, slotIndex),
});
