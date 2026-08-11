import type { CatalogGame } from "./home-catalog";
import { isElectron } from "./runtime";

export interface RecentCatalogPlay extends CatalogGame {
    lastPlayed: number;
}

const STORAGE_KEY = "questviva-recent-catalog-plays";
const MAX_ENTRIES = 12;

function readAll(): RecentCatalogPlay[] {
    if (typeof localStorage === "undefined") return [];
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return [];
        const parsed = JSON.parse(raw);
        return Array.isArray(parsed) ? parsed : [];
    } catch {
        return [];
    }
}

function writeAll(entries: RecentCatalogPlay[]): void {
    if (typeof localStorage === "undefined") return;
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(entries));
    } catch {
        // ignore — recent-plays tracking is a convenience, never worth failing the actual play over
    }
}

// Electron: static-server.ts's origin isn't guaranteed to stay the same across
// restarts, so localStorage (origin-scoped) can silently lose this list — persisted
// to a userData file instead via IPC, same rationale as i18n/index.ts's locale
// persistence. See ElectronApp's catalog-plays-store.ts.
export async function listRecentCatalogPlays(): Promise<RecentCatalogPlay[]> {
    if (isElectron()) {
        try {
            return await window.electronApp!.catalogPlays.list();
        } catch {
            return [];
        }
    }
    // Already stored sorted most-recent-first (see recordCatalogPlay), so this is a plain read.
    return readAll();
}

// Best-effort, mirrors electron-adapter.ts's trackRecent — a tracking failure must never
// surface as an error on the play it followed. Dedupes by id (a replay bumps lastPlayed
// and moves back to the front rather than creating a second entry).
export async function recordCatalogPlay(game: CatalogGame): Promise<void> {
    if (isElectron()) {
        try {
            await window.electronApp!.catalogPlays.record(game);
        } catch {
            // ignore
        }
        return;
    }
    try {
        const rest = readAll().filter((entry) => entry.id !== game.id);
        const entries = [{ ...game, lastPlayed: Date.now() }, ...rest].slice(0, MAX_ENTRIES);
        writeAll(entries);
    } catch {
        // ignore
    }
}

export async function removeRecentCatalogPlay(id: string): Promise<void> {
    if (isElectron()) {
        try {
            await window.electronApp!.catalogPlays.remove(id);
        } catch {
            // ignore
        }
        return;
    }
    writeAll(readAll().filter((entry) => entry.id !== id));
}
