import { isJunkAssetName, type AssetInfo, type FileAdapter } from "./types";

const ASLX_FILTER = [{ name: "Quest game files", extensions: ["aslx"] }];

// Mirrors PlayerHelper.cs's s_mimeTypes. window.electronApp.fs.readFile returns
// raw bytes with no type info (unlike FSA's FileSystemFileHandle.getFile(),
// which browsers auto-type from the OS) — without this, getAsset()'s Blob has
// an empty type, WasmPlayer's data-URL resource protocol produces an untyped
// data: URL, and <audio> (unlike <img>) rejects that with NotSupportedError.
const MIME_TYPES: Record<string, string> = {
    ".jpg": "image/jpeg",
    ".jpeg": "image/jpeg",
    ".gif": "image/gif",
    ".bmp": "image/bmp",
    ".png": "image/png",
    ".wav": "audio/wav",
    ".mp3": "audio/mpeg",
    ".ogg": "audio/ogg",
    ".js": "application/javascript",
    ".ttf": "application/font-woff",
    ".svg": "image/svg+xml",
};

function guessMimeType(name: string): string {
    const dot = name.lastIndexOf(".");
    if (dot < 0) return "";
    return MIME_TYPES[name.slice(dot).toLowerCase()] ?? "";
}

function basename(p: string): string {
    return p.split(/[\\/]/).pop() ?? p;
}

function electronApp() {
    // Only ever constructed when isElectron() is true (see open/+page.svelte),
    // so window.electronApp is always present here.
    return window.electronApp!;
}

/**
 * FileAdapter backed by Electron's contextBridge (Node fs via IPC), for the
 * desktop app. Same flat single-directory model as BrowserFileAdapter — no
 * nested asset subfolders — just Node fs/dialog instead of the File System
 * Access API, which docs/electron-desktop-app.md rules out inside Electron.
 */
export class ElectronFileAdapter implements FileAdapter {
    constructor(
        private readonly dirPath: string,
        private _filename: string,
    ) {}

    get filename() { return this._filename; }
    readonly canSaveAs = true;

    private resolve(name: string): string {
        return electronApp().path.join(this.dirPath, name);
    }

    async saveFile(data: Uint8Array | string): Promise<void> {
        await electronApp().fs.writeFile(this.resolve(this._filename), data);
    }

    async saveFileAs(data: Uint8Array | string, suggestedName?: string): Promise<string | null> {
        const path = await electronApp().dialog.saveFile({
            defaultPath: this.resolve(suggestedName ?? this._filename),
            filters: ASLX_FILTER,
        });
        if (!path) return null;
        await electronApp().fs.writeFile(path, data);
        this._filename = basename(path);
        return this._filename;
    }

    async putAsset(key: string, data: Blob): Promise<void> {
        await electronApp().fs.writeFile(this.resolve(key), new Uint8Array(await data.arrayBuffer()));
    }

    async getAsset(key: string): Promise<Blob | null> {
        try {
            const bytes = await electronApp().fs.readFile(this.resolve(key));
            return new Blob([bytes.slice()], { type: guessMimeType(key) });
        } catch {
            return null;
        }
    }

    async listAssets(): Promise<AssetInfo[]> {
        const entries = await electronApp().fs.readDir(this.dirPath);
        return entries
            .filter((e) => e.isFile && !e.name.toLowerCase().endsWith(".aslx") && !isJunkAssetName(e.name))
            .map((e) => ({ key: e.name, url: "" }));
    }

    async deleteAsset(key: string): Promise<void> {
        await electronApp().fs.unlink(this.resolve(key));
    }
}

/**
 * Opens a game folder via the native directory dialog, scans it for .aslx
 * files, and returns the folder plus the filenames found — mirrors
 * browser-adapter.ts's openDirectory() (FSA) so open/+page.svelte can reuse
 * the same single/multiple-file branching for either backend.
 */
export async function openElectronDirectory(): Promise<{ dirPath: string; files: string[] } | null> {
    const dirPath = await electronApp().dialog.openDirectory();
    if (!dirPath) return null;
    const entries = await electronApp().fs.readDir(dirPath);
    const files = entries
        .filter((e) => e.isFile && e.name.toLowerCase().endsWith(".aslx"))
        .map((e) => e.name)
        .sort();
    return { dirPath, files };
}

export async function loadElectronFile(dirPath: string, filename: string): Promise<{ bytes: Uint8Array; adapter: ElectronFileAdapter }> {
    const bytes = await electronApp().fs.readFile(electronApp().path.join(dirPath, filename));
    return { bytes, adapter: new ElectronFileAdapter(dirPath, filename) };
}

/**
 * Creates a new game file with the given content — mirrors
 * browser-adapter.ts's createLocalGame() (FSA). Returns null if the user
 * cancels the directory picker.
 */
export async function createElectronGame(
    filename: string,
    content: string,
): Promise<{ bytes: Uint8Array; adapter: ElectronFileAdapter } | null> {
    const dirPath = await electronApp().dialog.openDirectory();
    if (!dirPath) return null;
    const filePath = electronApp().path.join(dirPath, filename);
    await electronApp().fs.writeFile(filePath, content);
    return { bytes: new TextEncoder().encode(content), adapter: new ElectronFileAdapter(dirPath, filename) };
}
