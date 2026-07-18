import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

export interface RecentGame {
    dirPath: string;
    filename: string;
    lastOpened: number;
}

// "edit" = opened/created/saved-as through the editor (/open) — backs the
// native File > Open Recent submenu and the /open page's Recent section.
// "play" = opened through the Play tab's file picker — backs that tab's own
// Recently Played list. Two separate stores (not one list with a kind field)
// so picking a game to play never surfaces it as something File > Open
// Recent would load into the editor, and vice versa — see the ElectronApp
// File-menu-vs-Play split this was introduced for.
export type RecentKind = "edit" | "play";

// Small enough (10 entries max, per kind) that a plain JSON file under
// userData is simpler than pulling in a dependency for it.
const MAX_RECENT = 10;

function storePath(kind: RecentKind): string {
    const filename = kind === "edit" ? "recent-games.json" : "recent-played.json";
    return path.join(app.getPath("userData"), filename);
}

async function readAll(kind: RecentKind): Promise<RecentGame[]> {
    try {
        const text = await fs.readFile(storePath(kind), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        return Array.isArray(parsed) ? (parsed as RecentGame[]) : [];
    } catch {
        // Missing file (first run) or corrupt JSON — either way, no recent list yet.
        return [];
    }
}

async function writeAll(kind: RecentKind, games: RecentGame[]): Promise<void> {
    await fs.writeFile(storePath(kind), JSON.stringify(games));
}

export async function listRecentGames(kind: RecentKind): Promise<RecentGame[]> {
    return readAll(kind);
}

// Upserts by (dirPath, filename) and moves it to the front — entries are
// always written most-recent-first, so callers don't need to re-sort.
export async function addRecentGame(kind: RecentKind, dirPath: string, filename: string): Promise<RecentGame[]> {
    const games = (await readAll(kind)).filter((g) => !(g.dirPath === dirPath && g.filename === filename));
    games.unshift({ dirPath, filename, lastOpened: Date.now() });
    const capped = games.slice(0, MAX_RECENT);
    await writeAll(kind, capped);
    return capped;
}

export async function removeRecentGame(kind: RecentKind, dirPath: string, filename: string): Promise<RecentGame[]> {
    const games = (await readAll(kind)).filter((g) => !(g.dirPath === dirPath && g.filename === filename));
    await writeAll(kind, games);
    return games;
}

export async function clearRecentGames(kind: RecentKind): Promise<void> {
    await writeAll(kind, []);
}
