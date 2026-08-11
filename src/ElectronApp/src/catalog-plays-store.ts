import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

// Persisted to a userData file rather than localStorage — same rationale as
// locale-store.ts: static-server.ts's origin isn't guaranteed to stay the
// same across restarts, so AppShell's own localStorage-based tracking
// (recent-catalog-plays.ts) would silently lose this list. Mirrors
// recent-games.ts's shape (atomic write, capped, dedupe-and-move-to-front)
// but keyed by catalog game id instead of a local file path.
export interface RecentCatalogPlay {
    id: string;
    name: string;
    author: string | null;
    cover: string | null;
    thumbnail: string | null;
    rating: number;
    isGamebook: boolean;
    language: string;
    lastPlayed: number;
}

const MAX_ENTRIES = 12;

function storePath(): string {
    return path.join(app.getPath("userData"), "catalog-plays.json");
}

async function readAll(): Promise<RecentCatalogPlay[]> {
    try {
        const text = await fs.readFile(storePath(), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        return Array.isArray(parsed) ? (parsed as RecentCatalogPlay[]) : [];
    } catch {
        return [];
    }
}

async function writeAll(entries: RecentCatalogPlay[]): Promise<void> {
    const finalPath = storePath();
    const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
    await fs.writeFile(tmpPath, JSON.stringify(entries));
    await fs.rename(tmpPath, finalPath);
}

// Single list, so a single queue is enough (unlike recent-games.ts's
// per-kind Map) — same ordering rationale as there: every read/write below
// chains onto this so two overlapping mutations never race each other.
let writeQueue: Promise<unknown> = Promise.resolve();

function enqueue<T>(task: () => Promise<T>): Promise<T> {
    const next = writeQueue.then(task, task);
    writeQueue = next.catch(() => undefined);
    return next;
}

export function listCatalogPlays(): Promise<RecentCatalogPlay[]> {
    return enqueue(() => readAll());
}

export function recordCatalogPlay(game: Omit<RecentCatalogPlay, "lastPlayed">): Promise<RecentCatalogPlay[]> {
    return enqueue(async () => {
        const rest = (await readAll()).filter((entry) => entry.id !== game.id);
        const entries = [{ ...game, lastPlayed: Date.now() }, ...rest].slice(0, MAX_ENTRIES);
        await writeAll(entries);
        return entries;
    });
}

export function removeCatalogPlay(id: string): Promise<RecentCatalogPlay[]> {
    return enqueue(async () => {
        const entries = (await readAll()).filter((entry) => entry.id !== id);
        await writeAll(entries);
        return entries;
    });
}
