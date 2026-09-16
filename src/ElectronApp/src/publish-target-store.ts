import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

// Per-game last-used Publish dialog target ("quest" / "cdn" / "zip"), keyed by
// the game's <gameid>. Persisted to a userData file rather than localStorage —
// same rationale as ui-state-store.ts: static-server.ts's ephemeral port means
// localStorage set in one session is invisible in the next.
export type PublishTargets = Record<string, string>;

function storePath(): string {
    return path.join(app.getPath("userData"), "publish-target.json");
}

// Missing file (first run) or corrupt JSON is an empty map, never an error.
export async function readPublishTargets(): Promise<PublishTargets> {
    try {
        const text = await fs.readFile(storePath(), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        if (typeof parsed !== "object" || parsed === null) return {};
        const result: PublishTargets = {};
        for (const [gameId, target] of Object.entries(parsed)) {
            if (typeof target === "string") result[gameId] = target;
        }
        return result;
    } catch {
        return {};
    }
}

// Serialized read-modify-write, same as ui-state-store.ts's setUiState.
let writeQueue: Promise<void> = Promise.resolve();

export function setPublishTarget(gameId: string, target: string): Promise<void> {
    writeQueue = writeQueue.then(async () => {
        const all = await readPublishTargets();
        all[gameId] = target;
        const finalPath = storePath();
        const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
        await fs.writeFile(tmpPath, JSON.stringify(all));
        await fs.rename(tmpPath, finalPath);
    });
    return writeQueue;
}
