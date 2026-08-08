import { loadElectronFile, openElectronPlayFile } from "./electron-adapter";

function blobToDataUrl(blob: Blob): Promise<string> {
    return new Promise((resolve) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result as string);
        reader.readAsDataURL(blob);
    });
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

    closeLocalPlayChannel();
    const bc = new BroadcastChannel("quest-play-local");
    playChannel = bc;
    bc.onmessage = async ({ data }) => {
        if (data.type === "ready") {
            bc.postMessage({ type: "game", bytes, filename });
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

// Returns false only when the user cancelled the native file picker — a real failure (e.g.
// playElectronFile's "couldn't open the player window") throws instead, so callers can
// tell "user backed out" apart from "something went wrong".
export async function pickAndPlayElectronFile(onPlayed?: () => void): Promise<boolean> {
    const picked = await openElectronPlayFile();
    if (!picked) return false;
    await playElectronFile(picked.dirPath, picked.filename, onPlayed);
    return true;
}
