import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

// The version string the user last dismissed UpdateBanner.svelte for.
// Persisted separately from AppShell's own localStorage-based store, same
// rationale as locale-store.ts: static-server.ts's origin isn't guaranteed
// to stay the same across restarts, so localStorage set in one session can
// be invisible in the next.
function storePath(): string {
    return path.join(app.getPath("userData"), "update-dismissed.json");
}

export async function readDismissedVersion(): Promise<string | null> {
    try {
        const text = await fs.readFile(storePath(), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        return typeof parsed === "object" && parsed !== null && typeof (parsed as { version?: unknown }).version === "string"
            ? (parsed as { version: string }).version
            : null;
    } catch {
        return null;
    }
}

// Same atomic tmp-file + rename as recent-games.ts's writeAll.
export async function writeDismissedVersion(version: string): Promise<void> {
    const finalPath = storePath();
    const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
    await fs.writeFile(tmpPath, JSON.stringify({ version }));
    await fs.rename(tmpPath, finalPath);
}
