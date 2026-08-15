import { isElectron } from "./runtime";

// Per-game game-tree expansion state (which node ids are expanded), keyed by
// the game's <gameid> element so each game remembers its own tree layout
// across sessions. localStorage is fine in the browser; Electron persists to a
// userData file via IPC instead (ui-state-store.ts) — static-server.ts's
// ephemeral port makes the page origin different every launch, so localStorage
// never survives a restart there (same rationale as theme-store.ts).
const STORAGE_KEY_PREFIX = "questviva-tree-state-";

export async function loadTreeState(gameId: string): Promise<string[] | null> {
    if (isElectron()) {
        try {
            const all = await window.electronApp!.uiState.get();
            const state = all[gameId];
            return Array.isArray(state) ? state : null;
        } catch {
            // Losing the tree state just means the default (collapsed) is used, not fatal.
            return null;
        }
    }
    if (typeof localStorage === "undefined") return null;
    try {
        const raw = localStorage.getItem(STORAGE_KEY_PREFIX + gameId);
        if (!raw) return null;
        const parsed: unknown = JSON.parse(raw);
        return Array.isArray(parsed) ? (parsed as string[]) : null;
    } catch {
        return null;
    }
}

export function saveTreeState(gameId: string, expandedIds: string[]): void {
    if (isElectron()) {
        try {
            void window.electronApp!.uiState.set(gameId, expandedIds);
        } catch {
            // Same degradation as the read path — nothing to recover, not fatal.
        }
        return;
    }
    if (typeof localStorage === "undefined") return;
    try {
        localStorage.setItem(STORAGE_KEY_PREFIX + gameId, JSON.stringify(expandedIds));
    } catch {
        // Storage can be unavailable (private browsing, quota) — losing the
        // tree state just means the default is used next load, not fatal.
    }
}
