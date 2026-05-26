import type { AssetInfo, FileAdapter, LoadedFile } from "./types";

// FSA globals — not in the TS DOM lib at this target version
declare function showDirectoryPicker(options?: {
    id?: string;
    mode?: "read" | "readwrite";
    startIn?: FileSystemHandle | "desktop" | "documents" | "downloads" | "music" | "pictures" | "videos";
}): Promise<FileSystemDirectoryHandle>;
declare function showSaveFilePicker(options?: {
    excludeAcceptAllOption?: boolean;
    suggestedName?: string;
    startIn?: FileSystemHandle | "desktop" | "documents" | "downloads" | "music" | "pictures" | "videos";
    types?: { description?: string; accept?: Record<string, string[]> }[];
}): Promise<FileSystemFileHandle>;

const ASLX_FILE_TYPE = {
    description: "Quest game files",
    accept: { "application/xml": [".aslx"] as `.${string}`[] },
};

// ── Loaders ──────────────────────────────────────────────────────────────────

export function hasFSA(): boolean {
    return "showDirectoryPicker" in window;
}

/**
 * FSA path: opens a directory picker and returns the handle plus all .aslx
 * filenames found inside. The caller is responsible for picking which file to
 * load if there are multiple (e.g. a multi-file game or split libraries).
 */
export async function openDirectory(): Promise<{ dir: FileSystemDirectoryHandle; files: string[] } | null> {
    try {
        const dir = await showDirectoryPicker({ mode: "readwrite" });
        const files: string[] = [];
        for await (const [name, handle] of dir) {
            if (handle.kind === "file" && name.toLowerCase().endsWith(".aslx")) {
                files.push(name);
            }
        }
        return { dir, files };
    } catch (err: unknown) {
        if (err instanceof Error && err.name === "AbortError") return null;
        throw err;
    }
}

/**
 * FSA path: loads a specific .aslx from an already-opened directory handle.
 * Assets are siblings on disk in the same folder.
 */
export async function loadFileFromDirectory(dir: FileSystemDirectoryHandle, filename: string): Promise<LoadedFile> {
    const fh = await dir.getFileHandle(filename);
    const file = await fh.getFile();
    const bytes = new Uint8Array(await file.arrayBuffer());
    return { bytes, adapter: new BrowserFileAdapter(filename, { kind: "directory", dir }) };
}

/**
 * Fallback path (<input type="file">): no directory access, so assets go to
 * OPFS for the session only. Called directly from a user-gesture handler —
 * no prior await in the call chain so the programmatic click is permitted.
 */
export function loadLocalFile(): Promise<LoadedFile | null> {
    return new Promise((resolve) => {
        const input = document.createElement("input");
        input.type = "file";
        input.accept = ".aslx";
        input.addEventListener("change", async () => {
            const file = input.files?.[0];
            if (!file) { resolve(null); return; }
            const bytes = new Uint8Array(await file.arrayBuffer());
            resolve({ bytes, adapter: new BrowserFileAdapter(file.name, { kind: "fallback", opfsKey: crypto.randomUUID() }) });
        }, { once: true });
        input.addEventListener("cancel", () => resolve(null), { once: true });
        input.click();
    });
}

// ── Adapter ───────────────────────────────────────────────────────────────────

type DirectoryMode = { kind: "directory"; dir: FileSystemDirectoryHandle };
type FallbackMode  = { kind: "fallback";  opfsKey: string };
type Mode = DirectoryMode | FallbackMode;

export class BrowserFileAdapter implements FileAdapter {
    constructor(
        private _filename: string,
        private _mode: Mode,
    ) {}

    get filename() { return this._filename; }
    readonly previewUrl: string | null = null;

    // Save As is only meaningful in directory mode — in fallback mode both save paths trigger a download.
    get canSaveAs(): boolean { return this._mode.kind === "directory"; }

    // Whether assets are on the real filesystem (vs OPFS-only)
    get assetsOnDisk(): boolean { return this._mode.kind === "directory"; }

    async saveFile(data: Uint8Array | string): Promise<void> {
        if (this._mode.kind === "directory") {
            const fh = await this._mode.dir.getFileHandle(this._filename, { create: true });
            await writeToHandle(fh, data);
            return;
        }
        triggerDownload(data, this._filename);
    }

    async saveFileAs(data: Uint8Array | string, suggestedName?: string): Promise<string | null> {
        if ("showSaveFilePicker" in window) {
            try {
                const handle = await showSaveFilePicker({
                    suggestedName: suggestedName ?? this._filename,
                    types: [ASLX_FILE_TYPE],
                    // Start in the game folder so the user doesn't have to navigate there
                    ...(this._mode.kind === "directory" ? { startIn: this._mode.dir } : {}),
                });
                await writeToHandle(handle, data);
                this._filename = handle.name;
                return handle.name;
            } catch (err: unknown) {
                if (err instanceof Error && err.name === "AbortError") return null;
                throw err;
            }
        }
        const name = suggestedName ?? this._filename;
        triggerDownload(data, name);
        return name;
    }

    async putAsset(key: string, data: Blob): Promise<void> {
        if (this._mode.kind === "directory") {
            const fh = await this._mode.dir.getFileHandle(key, { create: true });
            const writable = await fh.createWritable();
            await writable.write(data);
            await writable.close();
            return;
        }
        const dir = await getOpfsGameDir(this._mode.opfsKey);
        const fh = await dir.getFileHandle(key, { create: true });
        const writable = await fh.createWritable();
        await writable.write(data);
        await writable.close();
    }

    async getAsset(key: string): Promise<Blob | null> {
        try {
            if (this._mode.kind === "directory") {
                return await (await this._mode.dir.getFileHandle(key)).getFile();
            }
            const dir = await getOpfsGameDir(this._mode.opfsKey);
            return await (await dir.getFileHandle(key)).getFile();
        } catch {
            return null;
        }
    }

    async listAssets(): Promise<AssetInfo[]> {
        try {
            const assets: AssetInfo[] = [];
            if (this._mode.kind === "directory") {
                for await (const [name, handle] of this._mode.dir) {
                    if (handle.kind === "file" && !name.toLowerCase().endsWith(".aslx")) {
                        assets.push({ key: name, url: "" });
                    }
                }
            } else {
                const dir = await getOpfsGameDir(this._mode.opfsKey);
                for await (const [name, handle] of dir) {
                    if (handle.kind === "file") assets.push({ key: name, url: "" });
                }
            }
            return assets;
        } catch {
            return [];
        }
    }

    async deleteAsset(key: string): Promise<void> {
        if (this._mode.kind === "directory") {
            await this._mode.dir.removeEntry(key);
            return;
        }
        const dir = await getOpfsGameDir(this._mode.opfsKey);
        await dir.removeEntry(key);
    }
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function triggerDownload(data: Uint8Array | string, filename: string) {
    const url = URL.createObjectURL(toBlob(data));
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
}

function toBlob(data: Uint8Array | string): Blob {
    return typeof data === "string"
        ? new Blob([data], { type: "application/xml" })
        : new Blob([data.slice()]);
}

async function writeToHandle(handle: FileSystemFileHandle, data: Uint8Array | string): Promise<void> {
    const writable = await handle.createWritable();
    await writable.write(toBlob(data));
    await writable.close();
}

async function getOpfsGameDir(opfsKey: string): Promise<FileSystemDirectoryHandle> {
    const root = await navigator.storage.getDirectory();
    return root.getDirectoryHandle(opfsKey, { create: true });
}
