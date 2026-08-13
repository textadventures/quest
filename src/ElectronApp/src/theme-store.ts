import { app } from "electron";
import { promises as fs, readFileSync } from "node:fs";
import path from "node:path";

// UI theme preference ("light" | "dark" | "system"), persisted separately
// from AppShell's own localStorage-based store
// (src/AppShell/src/lib/theme-store.ts). localStorage is scoped per-origin,
// but static-server.ts binds to an OS-assigned ephemeral port
// (listen(0, ...)) that changes every launch, so the origin
// (http://127.0.0.1:<port>) is never the same twice — localStorage set in one
// session is invisible in the next. A plain file under userData sidesteps
// that entirely, same rationale as locale-store.ts.
function storePath(): string {
    return path.join(app.getPath("userData"), "theme.json");
}

export async function readTheme(): Promise<string | null> {
    try {
        const text = await fs.readFile(storePath(), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        return typeof parsed === "object" && parsed !== null && typeof (parsed as { theme?: unknown }).theme === "string"
            ? (parsed as { theme: string }).theme
            : null;
    } catch {
        // Missing file (first run) or corrupt JSON — no preference stored yet.
        return null;
    }
}

// Synchronous variant backing app.html's pre-paint inline script — the
// renderer needs the stored theme before the Svelte app boots so there's no
// flash of the wrong theme, and an inline script can't await an async IPC.
// Same parse as readTheme, just with the sync fs API.
export function readThemeSync(): string | null {
    try {
        const parsed = JSON.parse(readFileSync(storePath(), "utf-8")) as unknown;
        return typeof parsed === "object" && parsed !== null && typeof (parsed as { theme?: unknown }).theme === "string"
            ? (parsed as { theme: string }).theme
            : null;
    } catch {
        return null;
    }
}

// Writes to a temp file and renames over the real one — same atomicity
// rationale as locale-store.ts's writeLocale.
export async function writeTheme(theme: string): Promise<void> {
    const finalPath = storePath();
    const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
    await fs.writeFile(tmpPath, JSON.stringify({ theme }));
    await fs.rename(tmpPath, finalPath);
}
