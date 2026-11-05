import { app } from "electron";
import { promises as fs, readFileSync } from "node:fs";
import path from "node:path";

// The player's own preferences, shared by every game: for now just v2's light or dark choice
// ("system" | "light" | "dark", src/WasmPlayer/ui-v2/src/scheme.ts). A browser keeps it in
// localStorage, but here the origin changes every launch (static-server.ts listens on an
// OS-assigned port), so it's a file under userData, as theme-store.ts does for the editor's own
// theme. Only the three known values are ever written or read back.
export const colourSchemes = ["system", "light", "dark"] as const;
export type ColourScheme = (typeof colourSchemes)[number];

function storePath(): string {
    return path.join(app.getPath("userData"), "player-settings.json");
}

function isColourScheme(value: unknown): value is ColourScheme {
    return typeof value === "string" && (colourSchemes as readonly string[]).includes(value);
}

// Synchronous, since the player reads it as it sets up its UI, before its first paint (as
// readThemeSync does for the editor).
export function readColourSchemeSync(): ColourScheme | null {
    try {
        const parsed = JSON.parse(readFileSync(storePath(), "utf-8")) as unknown;
        const value = typeof parsed === "object" && parsed !== null ? (parsed as { colourScheme?: unknown }).colourScheme : null;
        return isColourScheme(value) ? value : null;
    } catch {
        // Missing file (first run) or corrupt JSON: no choice made yet.
        return null;
    }
}

// Writes to a temp file and renames over the real one, as theme-store.ts's writeTheme does.
export async function writeColourScheme(value: unknown): Promise<void> {
    if (!isColourScheme(value)) return;
    const finalPath = storePath();
    const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
    await fs.writeFile(tmpPath, JSON.stringify({ colourScheme: value }));
    await fs.rename(tmpPath, finalPath);
}
