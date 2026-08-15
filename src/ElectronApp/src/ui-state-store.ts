import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

// Per-game editor UI state (currently the game tree's expanded-node ids),
// keyed by the game's <gameid> element so each game remembers its own tree
// layout across sessions. Persisted to a userData file rather than
// localStorage — same rationale as theme-store.ts: static-server.ts binds to
// an OS-assigned ephemeral port (listen(0, ...)) that changes every launch,
// so the page origin is never the same twice and localStorage never survives
// a restart in Electron.
export type UiState = Record<string, string[]>;

function storePath(): string {
    return path.join(app.getPath("userData"), "ui-state.json");
}

// Whole-map read: the renderer loads it once per game open. Missing file
// (first run) or corrupt JSON is an empty map, never an error.
export async function readUiState(): Promise<UiState> {
    try {
        const text = await fs.readFile(storePath(), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        if (typeof parsed !== "object" || parsed === null) return {};
        const result: UiState = {};
        for (const [gameId, state] of Object.entries(parsed)) {
            if (Array.isArray(state)) result[gameId] = state as string[];
        }
        return result;
    } catch {
        return {};
    }
}

// Serializes writes — each set is a read-modify-write of the whole map, and
// two overlapping ones (a debounced save landing while a flush-on-unmount is in
// flight) could otherwise interleave and lose a game's state.
let writeQueue: Promise<void> = Promise.resolve();

// Writes to a temp file and renames over the real one — same atomicity
// rationale as theme-store.ts's writeTheme. Debounced on the renderer side,
// so this is called at most a couple of times a second.
export function setUiState(gameId: string, state: string[]): Promise<void> {
    writeQueue = writeQueue.then(async () => {
        const all = await readUiState();
        all[gameId] = state;
        const finalPath = storePath();
        const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
        await fs.writeFile(tmpPath, JSON.stringify(all));
        await fs.rename(tmpPath, finalPath);
    });
    return writeQueue;
}
