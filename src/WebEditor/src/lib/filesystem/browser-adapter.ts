import type { AssetInfo, FileAdapter, LoadedFile } from "./types";

// FSA picker globals — not in the TS DOM lib at this target version
declare function showOpenFilePicker(options?: {
    multiple?: boolean;
    excludeAcceptAllOption?: boolean;
    types?: { description?: string; accept?: Record<string, string[]> }[];
}): Promise<FileSystemFileHandle[]>;
declare function showSaveFilePicker(options?: {
    excludeAcceptAllOption?: boolean;
    suggestedName?: string;
    types?: { description?: string; accept?: Record<string, string[]> }[];
}): Promise<FileSystemFileHandle>;

const ASLX_FILE_TYPE = {
    description: "Quest game files",
    accept: { "application/xml": [".aslx"] as `.${string}`[] },
};

export async function loadLocalFile(): Promise<LoadedFile | null> {
    if ("showOpenFilePicker" in window) {
        try {
            const [handle] = await showOpenFilePicker({
                types: [ASLX_FILE_TYPE],
                excludeAcceptAllOption: true,
                multiple: false,
            });
            const file = await handle.getFile();
            const bytes = new Uint8Array(await file.arrayBuffer());
            return { bytes, adapter: new BrowserFileAdapter(file.name, handle) };
        } catch (err: unknown) {
            if (err instanceof Error && err.name === "AbortError") return null;
            throw err;
        }
    }
    // <input> fallback — called synchronously from a user gesture, no prior await
    return promptFileInput();
}

function promptFileInput(): Promise<LoadedFile | null> {
    return new Promise((resolve) => {
        const input = document.createElement("input");
        input.type = "file";
        input.accept = ".aslx";
        input.addEventListener("change", async () => {
            const file = input.files?.[0];
            if (!file) { resolve(null); return; }
            const bytes = new Uint8Array(await file.arrayBuffer());
            resolve({ bytes, adapter: new BrowserFileAdapter(file.name, null) });
        }, { once: true });
        input.addEventListener("cancel", () => resolve(null), { once: true });
        input.click();
    });
}

export class BrowserFileAdapter implements FileAdapter {
    // Stable key for OPFS: reuses across saves within the same session.
    // A future "recent files" feature can persist this key in IndexedDB alongside
    // the FSA handle so assets survive across sessions.
    private readonly _opfsKey: string;

    constructor(
        private _filename: string,
        private _handle: FileSystemFileHandle | null,
    ) {
        this._opfsKey = crypto.randomUUID();
    }

    get filename() { return this._filename; }
    readonly canSaveAs = true;

    async saveFile(data: Uint8Array | string): Promise<void> {
        if (this._handle) {
            await writeToHandle(this._handle, data);
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
                });
                await writeToHandle(handle, data);
                this._handle = handle;
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
        const dir = await getOpfsGameDir(this._opfsKey);
        const fh = await dir.getFileHandle(key, { create: true });
        const writable = await fh.createWritable();
        await writable.write(data);
        await writable.close();
    }

    async getAsset(key: string): Promise<Blob | null> {
        try {
            const dir = await getOpfsGameDir(this._opfsKey);
            const fh = await dir.getFileHandle(key);
            return await fh.getFile();
        } catch {
            return null;
        }
    }

    async listAssets(): Promise<AssetInfo[]> {
        try {
            const dir = await getOpfsGameDir(this._opfsKey);
            const assets: AssetInfo[] = [];
            for await (const [name, handle] of dir) {
                if (handle.kind === "file") {
                    assets.push({ key: name, url: "" });
                }
            }
            return assets;
        } catch {
            return [];
        }
    }

    async deleteAsset(key: string): Promise<void> {
        const dir = await getOpfsGameDir(this._opfsKey);
        await dir.removeEntry(key);
    }
}

function triggerDownload(data: Uint8Array | string, filename: string) {
    const blob = toBlob(data);
    const url = URL.createObjectURL(blob);
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
