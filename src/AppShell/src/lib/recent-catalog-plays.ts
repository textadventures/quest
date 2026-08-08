import type { CatalogGame } from "./home-catalog";

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

// Already stored sorted most-recent-first (see recordCatalogPlay), so this is a plain read.
export function listRecentCatalogPlays(): RecentCatalogPlay[] {
    return readAll();
}

// Best-effort, mirrors electron-adapter.ts's trackRecent — a tracking failure must never
// surface as an error on the play it followed. Dedupes by id (a replay bumps lastPlayed
// and moves back to the front rather than creating a second entry).
export function recordCatalogPlay(game: CatalogGame): void {
    try {
        const rest = readAll().filter((entry) => entry.id !== game.id);
        const entries = [{ ...game, lastPlayed: Date.now() }, ...rest].slice(0, MAX_ENTRIES);
        writeAll(entries);
    } catch {
        // ignore
    }
}

export function removeRecentCatalogPlay(id: string): void {
    writeAll(readAll().filter((entry) => entry.id !== id));
}
