import { unzipSync } from "fflate";
import type { AssetInfo, FileAdapter } from "./types";
import { resolveOpfsDir, removeOpfsFile, removeOpfsDir, writeOpfsFile } from "./opfs-writer";

// Fixed entry name for the game file inside an exported project zip (see
// editor-store.exportGame) — independent of the draft's own display filename.
export const ZIP_GAME_ENTRY = "game.aslx";

// Local drafts live in OPFS under games/<gameId>/, alongside the game's own asset
// files and a small meta.json used by the drafts list so it doesn't need to load
// each game through WASM just to show a filename.
const ROOT = ["games"];

interface DraftMeta {
    filename: string;
    lastModified: number;
}

function dirPath(gameId: string): string[] {
    return [...ROOT, gameId];
}

async function readMeta(gameId: string): Promise<DraftMeta | null> {
    try {
        const dir = await resolveOpfsDir(dirPath(gameId), false);
        if (!dir) return null;
        const fh = await dir.getFileHandle("meta.json");
        const text = await (await fh.getFile()).text();
        return JSON.parse(text) as DraftMeta;
    } catch {
        return null;
    }
}

async function writeMeta(gameId: string, meta: DraftMeta): Promise<void> {
    await writeOpfsFile(dirPath(gameId), "meta.json", new TextEncoder().encode(JSON.stringify(meta)));
}

export interface LocalDraftSummary {
    gameId: string;
    filename: string;
    lastModified: number;
}

export async function listLocalDrafts(): Promise<LocalDraftSummary[]> {
    const root = await resolveOpfsDir(ROOT, true);
    if (!root) return [];
    const drafts: LocalDraftSummary[] = [];
    for await (const [name, handle] of root as unknown as AsyncIterable<[string, FileSystemHandle]>) {
        if (handle.kind !== "directory") continue;
        const meta = await readMeta(name);
        if (meta) drafts.push({ gameId: name, filename: meta.filename, lastModified: meta.lastModified });
    }
    return drafts;
}

export async function deleteLocalDraft(gameId: string): Promise<void> {
    await removeOpfsDir(dirPath(gameId));
}

export async function loadLocalDraft(gameId: string): Promise<{ bytes: Uint8Array; adapter: LocalDraftAdapter } | null> {
    const meta = await readMeta(gameId);
    if (!meta) return null;
    const dir = await resolveOpfsDir(dirPath(gameId), false);
    if (!dir) return null;
    const fh = await dir.getFileHandle(meta.filename);
    const bytes = new Uint8Array(await (await fh.getFile()).arrayBuffer());
    return { bytes, adapter: new LocalDraftAdapter(gameId, meta.filename) };
}

// Creates (or overwrites) a draft in OPFS from already-loaded content — used for
// both "create new game" and "import a file" flows, which both land here so that
// every local game becomes a persistent draft rather than a one-shot session.
export async function createLocalDraft(gameId: string, filename: string, bytes: Uint8Array | string): Promise<LocalDraftAdapter> {
    const adapter = new LocalDraftAdapter(gameId, filename);
    await adapter.saveFile(bytes);
    void requestPersistentStorage();
    return adapter;
}

// Imports a picked File — either a plain .aslx, or a .zip previously produced by
// editor-store.exportGame() (game.aslx entry + loose asset entries) — into a new
// local draft, keyed by the gameid found inside the game file.
export async function createLocalDraftFromFile(file: File): Promise<{ bytes: Uint8Array; adapter: LocalDraftAdapter }> {
    const raw = new Uint8Array(await file.arrayBuffer());

    if (!file.name.toLowerCase().endsWith(".zip")) {
        const gameId = parseGameIdFromAslx(new TextDecoder().decode(raw));
        if (!gameId) throw new Error("Could not find a gameid in the imported file.");
        const adapter = await createLocalDraft(gameId, file.name, raw);
        return { bytes: raw, adapter };
    }

    const entries = unzipSync(raw);
    const gameBytes = entries[ZIP_GAME_ENTRY];
    if (!gameBytes) throw new Error(`Zip file is missing ${ZIP_GAME_ENTRY} — not a game export.`);
    const gameId = parseGameIdFromAslx(new TextDecoder().decode(gameBytes));
    if (!gameId) throw new Error("Could not find a gameid in the imported file.");

    const filename = file.name.replace(/\.zip$/i, ".aslx");
    const adapter = await createLocalDraft(gameId, filename, gameBytes);
    for (const [name, bytes] of Object.entries(entries)) {
        if (name === ZIP_GAME_ENTRY || name.endsWith("/")) continue;
        await adapter.putAsset(name, new Blob([bytes]));
    }
    return { bytes: gameBytes, adapter };
}

// Pulls the <gameid> value out of raw .aslx XML before the game is loaded through
// WASM — needed up front to know which OPFS directory a game belongs to.
export function parseGameIdFromAslx(xmlText: string): string | null {
    try {
        const doc = new DOMParser().parseFromString(xmlText, "application/xml");
        if (doc.querySelector("parsererror")) return null;
        return doc.getElementsByTagName("gameid")[0]?.textContent?.trim() || null;
    } catch {
        return null;
    }
}

let _persistRequested = false;
async function requestPersistentStorage(): Promise<void> {
    if (_persistRequested) return;
    _persistRequested = true;
    try {
        await navigator.storage?.persist?.();
    } catch {
        // Best-effort only — reduces eviction risk under storage pressure, doesn't
        // help against Safari's separate 7-day inactivity eviction.
    }
}

export class LocalDraftAdapter implements FileAdapter {
    constructor(
        private _gameId: string,
        private _filename: string,
    ) {}

    get filename() { return this._filename; }
    get gameId() { return this._gameId; }

    // No real "save as" for a local draft — Export (see editor-store.exportGame) is
    // the equivalent of taking a copy out to a real file.
    readonly canSaveAs = false;

    async saveFile(data: Uint8Array | string): Promise<void> {
        const bytes = typeof data === "string" ? new TextEncoder().encode(data) : data;
        await writeOpfsFile(dirPath(this._gameId), this._filename, bytes);
        await writeMeta(this._gameId, { filename: this._filename, lastModified: Date.now() });
    }

    async saveFileAs(data: Uint8Array | string): Promise<string | null> {
        await this.saveFile(data);
        return this._filename;
    }

    async putAsset(key: string, data: Blob): Promise<void> {
        const bytes = new Uint8Array(await data.arrayBuffer());
        await writeOpfsFile(dirPath(this._gameId), key, bytes);
    }

    async getAsset(key: string): Promise<Blob | null> {
        const dir = await resolveOpfsDir(dirPath(this._gameId), false);
        if (!dir) return null;
        try {
            return await (await dir.getFileHandle(key)).getFile();
        } catch {
            return null;
        }
    }

    async listAssets(): Promise<AssetInfo[]> {
        const dir = await resolveOpfsDir(dirPath(this._gameId), false);
        if (!dir) return [];
        const assets: AssetInfo[] = [];
        for await (const [name, handle] of dir as unknown as AsyncIterable<[string, FileSystemHandle]>) {
            if (handle.kind !== "file" || name === this._filename || name === "meta.json") continue;
            assets.push({ key: name, url: "" });
        }
        return assets;
    }

    async deleteAsset(key: string): Promise<void> {
        await removeOpfsFile(dirPath(this._gameId), key);
    }

    // Moves this draft to a new OPFS directory keyed by a new GameId — needed when
    // the user regenerates the gameid field mid-edit (PropertyEditor.svelte's
    // "Generate" button), so the next save doesn't silently start a second, empty
    // draft under the new id and orphan the one under the old id.
    async rekey(newGameId: string): Promise<void> {
        if (newGameId === this._gameId) return;
        const oldDir = await resolveOpfsDir(dirPath(this._gameId), false);
        if (oldDir) {
            for await (const [name, handle] of oldDir as unknown as AsyncIterable<[string, FileSystemHandle]>) {
                if (handle.kind !== "file") continue;
                const file = await (handle as FileSystemFileHandle).getFile();
                const bytes = new Uint8Array(await file.arrayBuffer());
                await writeOpfsFile(dirPath(newGameId), name, bytes);
            }
            await removeOpfsDir(dirPath(this._gameId));
        }
        this._gameId = newGameId;
    }
}
