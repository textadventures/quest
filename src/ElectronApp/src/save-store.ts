import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

// Mirrors the [gameId, slotIndex] keying GameSaver's IndexedDB store used
// (playercore.js) — one directory per game under userData/saves, holding a
// small JSON manifest (name/timestamp per slot, cheap to read-modify-write —
// see recent-games.ts, which this queuing/atomic-write approach copies) plus
// one binary file per slot (never round-tripped through JSON/base64 — see
// the WasmPlayer save-perf work this migration inherits that concern from).
export interface SaveSlot {
    slotIndex: number;
    name: string | null;
    timestamp: number | null;
    // Snapshot of the game's own translated chrome strings at the moment it
    // was saved (see ChromeStrings.Resolve in PlayerCore) — lets the boot-time
    // "Continue your game?" prompt show the right language next session,
    // before anything is parsed into a WorldModel. Absent on saves made
    // before this field existed, and on non-Electron (IndexedDB) saves — both
    // fall back to English, no version/schema handling needed.
    chromeStrings?: Record<string, string> | null;
}

const MANIFEST_FILE = "manifest.json";

// Same single-path-segment escaping as transcript-store.ts's filenameFor —
// a game's id (its filename or textadventures.co.uk ifid) is untrusted
// input, and this keeps it from ever landing outside userData/saves.
function gameDirName(gameId: string): string {
    return encodeURIComponent(gameId);
}

function gameDir(gameId: string): string {
    return path.join(app.getPath("userData"), "saves", gameDirName(gameId));
}

function manifestPath(gameId: string): string {
    return path.join(gameDir(gameId), MANIFEST_FILE);
}

// slotIndex always comes from nextSlotIndex()/existing manifest entries — a
// plain non-negative integer, never attacker-controlled text — so no escaping
// is needed here the way gameId's is above.
function blobPath(gameId: string, slotIndex: number): string {
    return path.join(gameDir(gameId), `${slotIndex}.bin`);
}

async function readManifest(gameId: string): Promise<SaveSlot[]> {
    try {
        const text = await fs.readFile(manifestPath(gameId), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        return Array.isArray(parsed) ? (parsed as SaveSlot[]) : [];
    } catch {
        return [];
    }
}

// Same atomic tmp-file + rename as recent-games.ts's writeAll, for the same
// reason (no reader ever observes a torn manifest write).
async function writeManifest(gameId: string, slots: SaveSlot[]): Promise<void> {
    const finalPath = manifestPath(gameId);
    const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
    await fs.writeFile(tmpPath, JSON.stringify(slots));
    await fs.rename(tmpPath, finalPath);
}

// Keyed by gameId (encoded) — every read/write below for a given game is
// chained onto this so a save/delete never races another for the same game,
// while different games never wait on each other. Same shape as
// recent-games.ts's per-kind writeQueues / transcript-store.ts's per-file one.
const writeQueues = new Map<string, Promise<unknown>>();

function enqueue<T>(key: string, task: () => Promise<T>): Promise<T> {
    const previous = writeQueues.get(key) ?? Promise.resolve();
    const next = previous.then(task, task);
    writeQueues.set(key, next.catch(() => undefined));
    return next;
}

export function listSaves(gameId: string): Promise<SaveSlot[]> {
    return enqueue(gameDirName(gameId), () => readManifest(gameId));
}

export function saveGame(
    gameId: string,
    slotIndex: number,
    data: Uint8Array,
    name: string | null,
    chromeStrings?: Record<string, string> | null,
): Promise<void> {
    return enqueue(gameDirName(gameId), async () => {
        await fs.mkdir(gameDir(gameId), { recursive: true });
        await fs.writeFile(blobPath(gameId, slotIndex), data);
        const slots = await readManifest(gameId);
        const remaining = slots.filter((s) => s.slotIndex !== slotIndex);
        remaining.push({ slotIndex, name, timestamp: Date.now(), chromeStrings: chromeStrings ?? null });
        await writeManifest(gameId, remaining);
    });
}

export function loadGame(gameId: string, slotIndex: number): Promise<Uint8Array | null> {
    return enqueue(gameDirName(gameId), async () => {
        try {
            return await fs.readFile(blobPath(gameId, slotIndex));
        } catch {
            return null;
        }
    });
}

export function deleteSaveSlot(gameId: string, slotIndex: number): Promise<void> {
    return enqueue(gameDirName(gameId), async () => {
        try {
            await fs.unlink(blobPath(gameId, slotIndex));
        } catch {
            // Already gone — fine.
        }
        const slots = (await readManifest(gameId)).filter((s) => s.slotIndex !== slotIndex);
        await writeManifest(gameId, slots);
    });
}
