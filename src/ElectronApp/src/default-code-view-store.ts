import { app } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

// Whether ScriptEditor instances default to raw code view, persisted
// separately from AppShell's own localStorage-based store
// (src/AppShell/src/lib/code-view-store.ts) — same rationale as
// theme-store.ts: static-server.ts's ephemeral port means localStorage set
// in one session is invisible in the next.
function storePath(): string {
    return path.join(app.getPath("userData"), "default-code-view.json");
}

export async function readDefaultCodeView(): Promise<boolean | null> {
    try {
        const text = await fs.readFile(storePath(), "utf-8");
        const parsed = JSON.parse(text) as unknown;
        return typeof parsed === "object" && parsed !== null && typeof (parsed as { enabled?: unknown }).enabled === "boolean"
            ? (parsed as { enabled: boolean }).enabled
            : null;
    } catch {
        return null;
    }
}

// Same atomic tmp-file + rename as theme-store.ts's writeTheme.
export async function writeDefaultCodeView(enabled: boolean): Promise<void> {
    const finalPath = storePath();
    const tmpPath = `${finalPath}.tmp-${process.pid}-${Date.now()}`;
    await fs.writeFile(tmpPath, JSON.stringify({ enabled }));
    await fs.rename(tmpPath, finalPath);
}
