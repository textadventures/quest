import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

export interface RecentGame {
    dirPath: string;
    filename: string;
    lastOpened: number;
}

// Most-recently-used list of opened/created/saved-as game folders, shown in
// the native File > Open Recent submenu and on the /open page. Small enough
// (10 entries max) that a plain JSON file under userData is simpler than
// pulling in a dependency for it.
const MAX_RECENT = 10;

function storePath(): string {
    return path.join(app.getPath("userData"), "recent-games.json");
}

async function readAll(): Promise<RecentGame[]> {
    try {
        const text = await fs.readFile(storePath(), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        return Array.isArray(parsed) ? (parsed as RecentGame[]) : [];
    } catch {
        // Missing file (first run) or corrupt JSON — either way, no recent list yet.
        return [];
    }
}

async function writeAll(games: RecentGame[]): Promise<void> {
    await fs.writeFile(storePath(), JSON.stringify(games));
}

export async function listRecentGames(): Promise<RecentGame[]> {
    return readAll();
}

// Upserts by (dirPath, filename) and moves it to the front — entries are
// always written most-recent-first, so callers don't need to re-sort.
export async function addRecentGame(dirPath: string, filename: string): Promise<RecentGame[]> {
    const games = (await readAll()).filter((g) => !(g.dirPath === dirPath && g.filename === filename));
    games.unshift({ dirPath, filename, lastOpened: Date.now() });
    const capped = games.slice(0, MAX_RECENT);
    await writeAll(capped);
    return capped;
}

export async function removeRecentGame(dirPath: string, filename: string): Promise<RecentGame[]> {
    const games = (await readAll()).filter((g) => !(g.dirPath === dirPath && g.filename === filename));
    await writeAll(games);
    return games;
}

export async function clearRecentGames(): Promise<void> {
    await writeAll([]);
}
