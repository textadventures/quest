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

function dirname(p: string): string {
    const idx = Math.max(p.lastIndexOf("/"), p.lastIndexOf("\\"));
    return idx >= 0 ? p.slice(0, idx) : "";
}

function electronApp() {
    // Only ever constructed when isElectron() is true (see open/+page.svelte),
    // so window.electronApp is always present here.
    return window.electronApp!;
}

// Mirrors ElectronRecentGame in electron-types.d.ts — not imported by name
// since that type lives inside a `declare global` block, not a module export.
export interface RecentGame {
    dirPath: string;
    filename: string;
    lastOpened: number;
}

export async function listRecentGames(): Promise<RecentGame[]> {
    return electronApp().recent.list();
}

export async function removeRecentGame(dirPath: string, filename: string): Promise<void> {
    await electronApp().recent.remove(dirPath, filename);
}

// Best-effort: the recent list is a convenience, so a tracking failure should
// never surface as an error on the actual open/create/save-as it followed.
async function trackRecent(dirPath: string, filename: string): Promise<void> {
    try {
        await electronApp().recent.add(dirPath, filename);
    } catch {
        // ignore
    }
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

    // Resource names reaching getAsset() can be attacker-controlled — a
    // downloaded game's own <javascript>/image/sound references, forwarded
    // here verbatim from the WasmPlayer 'resource-request' handoff (see
    // PlayCatalog.svelte and editor-store.ts's previewInWasmPlayer). window
    // .electronApp.path.join is a plain string join with no traversal
    // protection (see preload.ts), so without this a name like
    // "../../../../.ssh/id_rsa" would resolve outside dirPath and getAsset
    // would happily read it. Collapses "." / ".." segments ourselves and
    // rejects anything that would climb above dirPath, rather than trusting
    // the joined path.
    private resolve(name: string): string {
        const sep = this.dirPath.includes("\\") && !this.dirPath.includes("/") ? "\\" : "/";
        const segments: string[] = [];
        for (const part of name.replace(/\\/g, "/").split("/")) {
            if (part === "" || part === ".") continue;
            if (part === "..") {
                if (segments.length === 0) throw new Error(`Resource name escapes its game folder: ${name}`);
                segments.pop();
                continue;
            }
            segments.push(part);
        }
        return electronApp().path.join(this.dirPath, segments.join(sep));
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
        await trackRecent(dirname(path), this._filename);
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
 * Opens a specific .aslx file via the native file picker. Unlike FSA (which
 * grants permission at the folder level, so browser-adapter.ts's
 * openDirectory() has to pick a whole folder and then disambiguate if there's
 * more than one .aslx inside it), Electron's real filesystem access has no
 * such constraint — the user can pick the exact game file directly, the same
 * as File > Open in any other desktop app.
 */
export async function openElectronFile(): Promise<{ dirPath: string; filename: string } | null> {
    const filePath = await electronApp().dialog.openFile({ filters: ASLX_FILTER });
    if (!filePath) return null;
    return { dirPath: dirname(filePath), filename: basename(filePath) };
}

// Broader than ASLX_FILTER above (which is Save/SaveAs-scoped, and the
// editor only ever opens unpacked .aslx) — Play accepts every format
// WasmPlayer itself can boot, matching the browser build's pickFile(".quest,
// .aslx,.asl,.cas") in PlayCatalog.svelte.
const PLAY_FILTER = [{ name: "Quest game files", extensions: ["quest", "aslx", "asl", "cas"] }];

export async function openElectronPlayFile(): Promise<{ dirPath: string; filename: string } | null> {
    const filePath = await electronApp().dialog.openFile({ filters: PLAY_FILTER });
    if (!filePath) return null;
    return { dirPath: dirname(filePath), filename: basename(filePath) };
}

export async function loadElectronFile(dirPath: string, filename: string): Promise<{ bytes: Uint8Array; adapter: ElectronFileAdapter }> {
    const bytes = await electronApp().fs.readFile(electronApp().path.join(dirPath, filename));
    await trackRecent(dirPath, filename);
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
    await trackRecent(dirPath, filename);
    return { bytes: new TextEncoder().encode(content), adapter: new ElectronFileAdapter(dirPath, filename) };
}
