import { isElectron } from "./runtime";

// "quest" is the plain .quest package (submitted to textadventures.co.uk in server mode,
// downloaded otherwise); "cdn" and "zip" wrap that same package in the WasmPlayer.
export type PublishTarget = "quest" | "cdn" | "zip";

function isPublishTarget(value: unknown): value is PublishTarget {
    return value === "quest" || value === "cdn" || value === "zip";
}

// Per-game last-used Publish dialog target, keyed by the game's <gameid> — where a game
// gets hosted is a per-game decision. localStorage is fine in the browser; Electron
// persists to a userData file via IPC instead (publish-target-store.ts), for the same
// ephemeral-origin reason as tree-state.ts.
const STORAGE_KEY_PREFIX = "questviva-publish-target-";

export async function loadPublishTarget(gameId: string): Promise<PublishTarget | null> {
    if (isElectron()) {
        try {
            const stored = await window.electronApp!.publishTarget.get(gameId);
            return isPublishTarget(stored) ? stored : null;
        } catch {
            // Losing the preference just means the dialog starts on its default, not fatal.
            return null;
        }
    }
    if (typeof localStorage === "undefined") return null;
    try {
        const stored = localStorage.getItem(STORAGE_KEY_PREFIX + gameId);
        return isPublishTarget(stored) ? stored : null;
    } catch {
        return null;
    }
}

export function savePublishTarget(gameId: string, target: PublishTarget): void {
    if (isElectron()) {
        try {
            void window.electronApp!.publishTarget.set(gameId, target).catch(() => {
                // Same degradation as the read path — nothing to recover, not fatal.
            });
        } catch {
            // Same degradation as the read path — nothing to recover, not fatal.
        }
        return;
    }
    if (typeof localStorage === "undefined") return;
    try {
        localStorage.setItem(STORAGE_KEY_PREFIX + gameId, target);
    } catch {
        // Storage can be unavailable (private browsing, quota) — losing the
        // preference just means the dialog starts on its default, not fatal.
    }
}
