import { loadElectronFile, openElectronPlayFile, listRecentGames } from "./electron-adapter";
import type { FileAdapter } from "./types";
import { resolveAndCacheCover } from "../local-cover";

function blobToDataUrl(blob: Blob): Promise<string> {
    return new Promise((resolve) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result as string);
        reader.readAsDataURL(blob);
    });
}

// An unpublished .aslx on disk can contain <include ref="custom.aslx"/> for a custom Included
// Library, whose content lives in a sibling file rather than the game bytes themselves — same
// situation editor-store.ts's preloadAdjacentLibraryAssets resolves for the editor. The player
// window here runs as its own separate WASM module with no filesystem access, so those bytes
// have to be resolved here and handed over alongside the game bytes (see WorldModel.GetLibraryStream).
async function resolveAdjacentLibraryFiles(adapter: FileAdapter): Promise<Record<string, Uint8Array>> {
    const candidates = adapter.listLibraryCandidates
        ? await adapter.listLibraryCandidates()
        : [];
    const files: Record<string, Uint8Array> = {};
    for (const key of candidates) {
        const blob = await adapter.getAsset(key);
        if (!blob) continue;
        files[key] = new Uint8Array(await blob.arrayBuffer());
    }
    return files;
}

// Singleton, not per-caller — only one channel is ever "the current answerer" at a time
// regardless of which of PlayCatalog.svelte's call sites (the manual toolbar click, a
// Recently Played replay, or the file-association effect) most recently launched a game,
// so "close the previous one before opening a new one" only needs one shared slot.
let playChannel: BroadcastChannel | null = null;

export function closeLocalPlayChannel(): void {
    playChannel?.close();
    playChannel = null;
}

// Loads dirPath/filename's bytes, opens a dedicated Electron player window (see
// ipc/player.ts's player:openWindow), and answers that window's game/resource-request
// handoff over BroadcastChannel("quest-play-local") for as long as it stays open. Throws
// on failure — the caller (PlayCatalog.svelte) owns its own busy/error UI around this call.
export async function playElectronFile(dirPath: string, filename: string, onPlayed?: () => void): Promise<void> {
    const { bytes, adapter } = await loadElectronFile(dirPath, filename, "play");
    onPlayed?.();

    void resolveCoverIfNeeded(dirPath, filename);

    closeLocalPlayChannel();
    const bc = new BroadcastChannel("quest-play-local");
    playChannel = bc;
    bc.onmessage = async ({ data }) => {
        if (data.type === "ready") {
            const adjacentFiles = await resolveAdjacentLibraryFiles(adapter);
            bc.postMessage({ type: "game", bytes, filename, adjacentFiles });
        } else if (data.type === "resource-request") {
            const blob = await adapter.getAsset(data.name);
            if (blob) {
                const dataUrl = await blobToDataUrl(blob);
                bc.postMessage({ type: "resource-response", id: data.id, dataUrl });
            }
        }
    };

    const opened = await window.electronApp!.player.openWindow();
    if (!opened) {
        bc.close();
        playChannel = null;
        throw new Error("Couldn't open the player window.");
    }
}

// Fire-and-forget, deliberately not awaited by playElectronFile — cover resolution boots the
// full engine just to read one field (see local-cover.ts), which must never delay the player
// window opening. Runs in this (the main AppShell) renderer, a separate OS process from the
// player window it just opened, so it can't make that window janky either. Skipped when this
// file's cover is already cached — addRecentGame's carry-forward (see recent-games.ts) means
// replaying an already-resolved game leaves its coverDataUrl untouched, so this only ever does
// real work the first time a given local file is played.
async function resolveCoverIfNeeded(dirPath: string, filename: string): Promise<void> {
    const recents = await listRecentGames("play");
    const entry = recents.find((g) => g.dirPath === dirPath && g.filename === filename);
    if (entry && entry.coverDataUrl === undefined) {
        await resolveAndCacheCover(dirPath, filename);
    }
}

// Returns false only when the user cancelled the native file picker — a real failure (e.g.
// playElectronFile's "couldn't open the player window") throws instead, so callers can
// tell "user backed out" apart from "something went wrong".
export async function pickAndPlayElectronFile(onPlayed?: () => void): Promise<boolean> {
    const picked = await openElectronPlayFile();
    if (!picked) return false;
    await playElectronFile(picked.dirPath, picked.filename, onPlayed);
    return true;
}
