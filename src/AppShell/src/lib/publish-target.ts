import { isElectron } from "./runtime";
import { localStorageOrNull } from "./web-storage";

// "online" uploads the .quest package to textadventures.co.uk (see publish-online.ts); "quest"
// is that same package, downloaded; "cdn" and "zip" wrap it in the WasmPlayer.
export type PublishTarget = "online" | "quest" | "cdn" | "zip";

// What the dialog starts on for a game that hasn't been published from here before
export const DEFAULT_PUBLISH_TARGET: PublishTarget = "online";

function isPublishTarget(value: unknown): value is PublishTarget {
    return value === "online" || value === "quest" || value === "cdn" || value === "zip";
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
    const storage = localStorageOrNull();
    if (!storage) return null;
    try {
        const stored = storage.getItem(STORAGE_KEY_PREFIX + gameId);
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
    const storage = localStorageOrNull();
    if (!storage) return;
    try {
        storage.setItem(STORAGE_KEY_PREFIX + gameId, target);
    } catch {
        // Storage can be unavailable (private browsing, quota) — losing the
        // preference just means the dialog starts on its default, not fatal.
    }
}
