import { loadWasm } from "./wasm";
import { ElectronFileAdapter, setRecentGameCover } from "./filesystem/electron-adapter";

interface LocalCoverResult {
    name: string;
    dataUrl: string | null;
}

function blobToDataUrl(blob: Blob): Promise<string> {
    return new Promise((resolve) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result as string);
        reader.readAsDataURL(blob);
    });
}

// Computing a local play file's cover art means booting the full engine just to read one field
// (see WasmEditorBridge.ResolveLocalCover) — too expensive to redo on every app startup. The
// result is cached on the RecentGame record itself (coverDataUrl, persisted to disk — see
// ElectronApp's recent-games.ts), resolved once via resolveAndCacheCover below, either when the
// game is played (local-play.ts) or, for a legacy/never-resolved entry, the first time its card
// is shown (LocalFileRecentCard.svelte). Neither caller needs an in-memory result cache of its
// own on top of that — this module only guards against two callers racing to resolve the same
// entry concurrently (e.g. a Play-tab re-render overlapping a play-time resolution).
const inFlight = new Map<string, Promise<string | null>>();

function cacheKey(dirPath: string, filename: string): string {
    return dirPath + "/" + filename;
}

// Best-effort: a malformed/unsupported local file, or one with no cover attribute set, must
// never break the Recently Played list — any failure here resolves to null, same as "no cover".
function resolveLocalCoverUrl(dirPath: string, filename: string): Promise<string | null> {
    const key = cacheKey(dirPath, filename);
    const existing = inFlight.get(key);
    if (existing) return existing;

    const promise = resolveUncached(dirPath, filename).finally(() => {
        if (inFlight.get(key) === promise) inFlight.delete(key);
    });
    inFlight.set(key, promise);
    return promise;
}

async function resolveUncached(dirPath: string, filename: string): Promise<string | null> {
    try {
        const bytes = await window.electronApp!.fs.readFile(window.electronApp!.path.join(dirPath, filename));
        const bridge = await loadWasm();
        const resultJson = await bridge.ResolveLocalCover(bytes, filename);
        if (!resultJson) return null;
        const { name, dataUrl } = JSON.parse(resultJson) as LocalCoverResult;

        // Embedded (a .quest package, or a self-contained legacy .asl/.cas) — the bridge
        // already extracted and base64-encoded the bytes from the game's own archive, so
        // dataUrl is directly usable as an <img src> and safe to cache to disk as-is.
        // Otherwise (a plain unpacked .aslx) the cover is a sibling file on disk — also
        // converted to a data: URL rather than an object URL, since this result gets
        // persisted on the RecentGame record and object URLs don't survive past the page
        // that created them.
        if (dataUrl) return dataUrl;

        const adapter = new ElectronFileAdapter(dirPath, filename);
        const blob = await adapter.getAsset(name);
        return blob ? await blobToDataUrl(blob) : null;
    } catch {
        return null;
    }
}

// Resolves (if not already in flight) and persists a local play file's cover onto its
// RecentGame record — the one-stop call every caller should use, so resolving never happens
// without also being cached for next time.
export async function resolveAndCacheCover(dirPath: string, filename: string): Promise<string | null> {
    const url = await resolveLocalCoverUrl(dirPath, filename);
    await setRecentGameCover(dirPath, filename, url);
    return url;
}
