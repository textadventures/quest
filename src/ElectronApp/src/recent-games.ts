import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

export interface RecentGame {
    dirPath: string;
    filename: string;
    lastOpened: number;
    // "play"-kind only — the game's cover art (a data: URL), resolved once when the game is
    // played and cached here so the Play tab's Recently Played list never has to re-derive it
    // (a full engine boot) on every app startup. Three states: undefined = never resolved yet
    // (a legacy entry, or this session's very first add before play-time resolution finishes);
    // null = resolved once, confirmed the game has no cover; string = the resolved data URL.
    // See AppShell's local-cover.ts and local-play.ts.
    coverDataUrl?: string | null;
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

// Writes to a temp file and renames over the real one — rename() is atomic at the OS level, so
// a reader (readAll, possibly from a different Electron process) can never observe a
// partially-written file, even for a large one (a cover art data: URL can be several hundred
// KB — see RecentGame.coverDataUrl). This alone doesn't stop two of *this* process's own writes
// from racing each other (see enqueue below for that), but it does stop a crash or a second
// instance mid-write from corrupting the file others still read.
async function writeAll(kind: RecentKind, games: RecentGame[]): Promise<void> {
    const finalPath = storePath(kind);
    const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
    await fs.writeFile(tmpPath, JSON.stringify(games));
    await fs.rename(tmpPath, finalPath);
}

// Every read-modify-write cycle below (add/setCover/remove/clear) for a given kind is chained
// onto this per-kind queue, so at most one is ever in flight at a time. Without this, two
// overlapping cycles — e.g. two LocalFileRecentCards each self-healing their own cover at
// nearly the same moment — read the same starting snapshot and then both write it back
// separately: not just a lost update, but (before the atomic rename above) their two
// fs.writeFile calls could genuinely interleave at the OS level and corrupt the file outright
// (a short write's bytes followed by a stray tail fragment of a longer concurrent one — this is
// exactly what happened once in testing: valid JSON followed by ~700KB of leaked raw base64,
// which readAll's catch-all then silently turned into "no recent games at all").
const writeQueues = new Map<RecentKind, Promise<unknown>>();

function enqueue<T>(kind: RecentKind, task: () => Promise<T>): Promise<T> {
    const previous = writeQueues.get(kind) ?? Promise.resolve();
    const next = previous.then(task, task);
    // A rejected task must not wedge the queue for later callers — but that rejection still
    // needs to go somewhere, or Node logs an unhandled rejection warning for it. `next` itself
    // (returned below) is that "somewhere": each caller awaits its own `next` and sees the
    // error, so this second, ignored chain off `next` exists only to keep the *queue's* stored
    // promise perpetually resolved for whoever chains after it.
    writeQueues.set(kind, next.catch(() => undefined));
    return next;
}

// Reads are queued too — otherwise one could still land in the middle of another, unrelated
// write's temp-file swap and briefly see a torn/former version. Queuing costs nothing here
// (reads are cheap) and removes that window entirely.
export function listRecentGames(kind: RecentKind): Promise<RecentGame[]> {
    return enqueue(kind, () => readAll(kind));
}

// Upserts by (dirPath, filename) and moves it to the front — entries are always written
// most-recent-first, so callers don't need to re-sort. coverDataUrl carries forward from the
// existing entry (if any) when replaying an already-cached game without re-resolving its cover;
// pass it explicitly to set/refresh it (e.g. local-play.ts's play-time resolution).
export function addRecentGame(
    kind: RecentKind,
    dirPath: string,
    filename: string,
    coverDataUrl?: string | null,
): Promise<RecentGame[]> {
    return enqueue(kind, async () => {
        const games = await readAll(kind);
        const existing = games.find((g) => g.dirPath === dirPath && g.filename === filename);
        const remaining = games.filter((g) => g !== existing);
        remaining.unshift({
            dirPath,
            filename,
            lastOpened: Date.now(),
            coverDataUrl: coverDataUrl !== undefined ? coverDataUrl : existing?.coverDataUrl,
        });
        const capped = remaining.slice(0, MAX_RECENT);
        await writeAll(kind, capped);
        return capped;
    });
}

// Patches just coverDataUrl on an existing entry — unlike addRecentGame, doesn't touch
// lastOpened or reorder the list. Used to self-heal a legacy/unresolved entry (coverDataUrl
// undefined) found sitting anywhere in the list, not just the one just played. No-op if the
// entry's gone (e.g. removed from Recently Played while resolution was still in flight).
export function setRecentGameCover(
    kind: RecentKind,
    dirPath: string,
    filename: string,
    coverDataUrl: string | null,
): Promise<RecentGame[]> {
    return enqueue(kind, async () => {
        const games = await readAll(kind);
        const entry = games.find((g) => g.dirPath === dirPath && g.filename === filename);
        if (entry) {
            entry.coverDataUrl = coverDataUrl;
            await writeAll(kind, games);
        }
        return games;
    });
}

export function removeRecentGame(kind: RecentKind, dirPath: string, filename: string): Promise<RecentGame[]> {
    return enqueue(kind, async () => {
        const games = (await readAll(kind)).filter((g) => !(g.dirPath === dirPath && g.filename === filename));
        await writeAll(kind, games);
        return games;
    });
}

export function clearRecentGames(kind: RecentKind): Promise<void> {
    return enqueue(kind, () => writeAll(kind, []));
}
