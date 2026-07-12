import type { AssetInfo, FileAdapter, LoadedFile } from "./types";
import { toBlob, triggerDownload } from "./download";

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

// ── Loaders / creators ───────────────────────────────────────────────────────

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
    return { bytes, adapter: new BrowserFileAdapter(filename, dir) };
}

/**
 * Creates a new game file with the given content.
 *
 * FSA path: opens a directory picker, writes the file, and returns a LoadedFile
 * so the editor can open it immediately. Returns null if the user cancels the picker.
 */
export async function createLocalGame(
    filename: string,
    content: string,
): Promise<{ kind: "opened"; loaded: LoadedFile } | null> {
    try {
        const dir = await showDirectoryPicker({ mode: "readwrite" });
        const fh = await dir.getFileHandle(filename, { create: true });
        await writeToHandle(fh, content);
        const bytes = new TextEncoder().encode(content);
        return {
            kind: "opened",
            loaded: { bytes, adapter: new BrowserFileAdapter(filename, dir) },
        };
    } catch (err: unknown) {
        if (err instanceof Error && err.name === "AbortError") return null;
        throw err;
    }
}

// ── Adapter ───────────────────────────────────────────────────────────────────

export class BrowserFileAdapter implements FileAdapter {
    constructor(
        private _filename: string,
        private _dir: FileSystemDirectoryHandle,
    ) {}

    get filename() { return this._filename; }
    readonly canSaveAs = true;

    async saveFile(data: Uint8Array | string): Promise<void> {
        const fh = await this._dir.getFileHandle(this._filename, { create: true });
        await writeToHandle(fh, data);
    }

    async saveFileAs(data: Uint8Array | string, suggestedName?: string): Promise<string | null> {
        if ("showSaveFilePicker" in window) {
            try {
                const handle = await showSaveFilePicker({
                    suggestedName: suggestedName ?? this._filename,
                    types: [ASLX_FILE_TYPE],
                    // Start in the game folder so the user doesn't have to navigate there
                    startIn: this._dir,
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
        const fh = await this._dir.getFileHandle(key, { create: true });
        const writable = await fh.createWritable();
        await writable.write(data);
        await writable.close();
    }

    async getAsset(key: string): Promise<Blob | null> {
        try {
            return await (await this._dir.getFileHandle(key)).getFile();
        } catch {
            return null;
        }
    }

    async listAssets(): Promise<AssetInfo[]> {
        try {
            const assets: AssetInfo[] = [];
            for await (const [name, handle] of this._dir) {
                if (handle.kind === "file" && !name.toLowerCase().endsWith(".aslx")) {
                    assets.push({ key: name, url: "" });
                }
            }
            return assets;
        } catch {
            return [];
        }
    }

    async deleteAsset(key: string): Promise<void> {
        await this._dir.removeEntry(key);
    }
}

// ── Helpers ───────────────────────────────────────────────────────────────────

async function writeToHandle(handle: FileSystemFileHandle, data: Uint8Array | string): Promise<void> {
    const writable = await handle.createWritable();
    await writable.write(toBlob(data));
    await writable.close();
}
