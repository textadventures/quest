import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

// Mirrors localStorage's "questtranscript-<name>" keying (see player.js's
// WriteToTranscript / TranscriptViewer/index.html), but as one file per
// transcript under userData rather than a single origin-scoped blob — see
// the port-change data-loss problem this migration exists to fix.
const EXT = ".transcript";

function transcriptsDir(): string {
    return path.join(app.getPath("userData"), "transcripts");
}

// encodeURIComponent never produces "/", "\", or NUL, and the EXT suffix
// means the result is always a single path segment (even a pathological
// name like ".." just becomes "...transcript") — so this can't escape
// transcriptsDir() regardless of what a game names its transcript.
function filenameFor(name: string): string {
    return encodeURIComponent(name) + EXT;
}

function nameFromFilename(filename: string): string | null {
    if (!filename.endsWith(EXT)) return null;
    try {
        return decodeURIComponent(filename.slice(0, -EXT.length));
    } catch {
        return null;
    }
}

function filePathFor(name: string): string {
    return path.join(transcriptsDir(), filenameFor(name));
}

// Keyed by filename (not name) so appendTranscript/readTranscript/deleteTranscript
// against the *same* transcript are always ordered, while different transcripts
// (or a whole-directory list()) never wait on each other. Same shape as
// recent-games.ts's writeQueues, one level more granular.
const writeQueues = new Map<string, Promise<unknown>>();

function enqueue<T>(key: string, task: () => Promise<T>): Promise<T> {
    const previous = writeQueues.get(key) ?? Promise.resolve();
    const next = previous.then(task, task);
    writeQueues.set(key, next.catch(() => undefined));
    return next;
}

export async function listTranscripts(): Promise<string[]> {
    try {
        const entries = await fs.readdir(transcriptsDir());
        return entries
            .map(nameFromFilename)
            .filter((n): n is string => n !== null)
            .sort();
    } catch {
        // Missing directory (nothing saved yet) — no transcripts.
        return [];
    }
}

// Appends in place rather than read-modify-write like recent-games.ts's
// writeAll — WriteToTranscript calls this once per game turn, so re-reading
// and rewriting the whole (potentially long-running) transcript every time
// would get slower as a session goes on. fs.appendFile creates the file if
// it doesn't exist yet.
export function appendTranscript(name: string, data: string): Promise<void> {
    return enqueue(filenameFor(name), async () => {
        await fs.mkdir(transcriptsDir(), { recursive: true });
        await fs.appendFile(filePathFor(name), data, "utf-8");
    });
}

export function readTranscript(name: string): Promise<string | null> {
    return enqueue(filenameFor(name), async () => {
        try {
            return await fs.readFile(filePathFor(name), "utf-8");
        } catch {
            return null;
        }
    });
}

export function deleteTranscript(name: string): Promise<void> {
    return enqueue(filenameFor(name), async () => {
        try {
            await fs.unlink(filePathFor(name));
        } catch {
            // Already gone — fine.
        }
    });
}
