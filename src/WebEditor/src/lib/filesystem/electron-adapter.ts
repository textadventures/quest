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

// Exposed so callers (open/+page.svelte's new-game location preview) never
// need to reach into window.electronApp directly — every other Electron
// access in this codebase goes through this file.
export function joinGamePath(...segments: string[]): string {
    return electronApp().path.join(...segments);
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

// Documents/Quest Games — matches Quest 5's desktop editor, which proposes
// this as the default new-game location rather than always prompting.
export async function getDefaultGamesDir(): Promise<string> {
    return electronApp().paths.defaultGamesDir();
}

// Escape hatch from the default games dir, for a new game that should live
// somewhere else (a synced folder, a different drive, ...). Returns null if
// the user cancels — caller keeps whatever location it already had.
export async function pickGameLocation(defaultPath?: string): Promise<string | null> {
    return electronApp().dialog.openDirectory({
        defaultPath,
        title: "Choose a location for your new game",
        // macOS's NSOpenPanel doesn't render a custom title at all — message
        // (darwin-only) is what actually shows there; see dialog.ts.
        message: "Choose a location for your new game",
        buttonLabel: "Select Folder",
    });
}

// Same sanitization as safeFilename() in open/+page.svelte, minus the .aslx
// suffix — used both to name the new subfolder and to preview it to the user
// before they commit (see open/+page.svelte's previewFolderName).
export function sanitizeFolderName(name: string): string {
    return name.replace(/[^a-zA-Z0-9 _-]/g, "").trim() || "game";
}

/**
 * Creates a new game file with the given content, inside a new subfolder
 * named after the game under parentDir — IDE-"new project" style — instead
 * of writing directly into parentDir the way browser-adapter.ts's
 * createLocalGame() (FSA) has to (there's no way to create a folder via the
 * FSA picker itself — the picker's target *is* the game folder).
 *
 * Throws if a folder with that name already exists under parentDir (caller
 * surfaces this as an error — see open/+page.svelte's handleCreateLocal —
 * rather than silently reusing or renaming it, since either could put new
 * content in an existing, unrelated folder).
 */
export async function createElectronGame(
    parentDir: string,
    filename: string,
    content: string,
): Promise<{ bytes: Uint8Array; adapter: ElectronFileAdapter }> {
    // filename is already sanitized (safeFilename() in open/+page.svelte),
    // so this is just stripping the extension, not re-sanitizing.
    const folderName = filename.replace(/\.aslx$/i, "");
    const dirPath = electronApp().path.join(parentDir, folderName);
    if (await electronApp().fs.exists(dirPath)) {
        throw new Error(`A folder named "${folderName}" already exists there — choose a different game name or a different location.`);
    }
    await electronApp().fs.mkdir(dirPath);
    const filePath = electronApp().path.join(dirPath, filename);
    await electronApp().fs.writeFile(filePath, content);
    return { bytes: new TextEncoder().encode(content), adapter: new ElectronFileAdapter(dirPath, filename) };
}
