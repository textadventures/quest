import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

// UI language preference, persisted separately from AppShell's own
// localStorage-based store (src/AppShell/src/lib/i18n/index.ts). localStorage
// is scoped per-origin, but static-server.ts binds to an OS-assigned
// ephemeral port (listen(0, ...)) that changes every launch, so the origin
// (http://127.0.0.1:<port>) is never the same twice — localStorage set in one
// session is invisible in the next. A plain file under userData sidesteps
// that entirely, same rationale as recent-games.ts.
function storePath(): string {
    return path.join(app.getPath("userData"), "locale.json");
}

export async function readLocale(): Promise<string | null> {
    try {
        const text = await fs.readFile(storePath(), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        return typeof parsed === "object" && parsed !== null && typeof (parsed as { locale?: unknown }).locale === "string"
            ? (parsed as { locale: string }).locale
            : null;
    } catch {
        // Missing file (first run) or corrupt JSON — no preference stored yet.
        return null;
    }
}

// Writes to a temp file and renames over the real one — same atomicity
// rationale as recent-games.ts's writeAll.
export async function writeLocale(locale: string): Promise<void> {
    const finalPath = storePath();
    const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
    await fs.writeFile(tmpPath, JSON.stringify({ locale }));
    await fs.rename(tmpPath, finalPath);
}
