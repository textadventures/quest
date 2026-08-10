import { ipcMain } from "electron";
import { promises as fs, watch as fsWatch, type FSWatcher } from "node:fs";
import * as path from "node:path";

// Debounces a burst of fs.watch events (many editors emit several writes per
// save) into a single restat/notify pass.
const DEBOUNCE_MS = 400;

let watchedDir: string | null = null;
let watcher: FSWatcher | null = null;
let baseline = new Map<string, number>(); // filename -> mtimeMs, both relative to watchedDir
let debounceTimer: ReturnType<typeof setTimeout> | null = null;

function stopWatching(): void {
    watcher?.close();
    watcher = null;
    watchedDir = null;
    baseline = new Map();
    if (debounceTimer) {
        clearTimeout(debounceTimer);
        debounceTimer = null;
    }
}

async function statMtime(fullPath: string): Promise<number | null> {
    try {
        return (await fs.stat(fullPath)).mtimeMs;
    } catch {
        // Deleted/renamed mid-edit (e.g. an external editor's atomic-save swap
        // caught between unlink and rename) — best effort, a later event will
        // restat successfully once the file settles.
        return null;
    }
}

async function checkForChanges(dirPath: string, onChanged: (filenames: string[]) => void): Promise<void> {
    if (watchedDir !== dirPath) return; // superseded by a newer watch() call
    const changed: string[] = [];
    for (const [filename, knownMtime] of baseline) {
        const mtime = await statMtime(path.join(dirPath, filename));
        if (mtime !== null && mtime > knownMtime) {
            baseline.set(filename, mtime);
            changed.push(filename);
        }
    }
    if (changed.length > 0) onChanged(changed);
}

// Backs window.electronApp.fileWatch in preload.ts — lets the renderer flag
// when the currently-open game file (or one of its included-library .aslx
// siblings) was modified by something other than this app, e.g. an external
// text editor. Renderer-driven, not adapter-driven: electron-adapter.ts calls
// watch() every time it loads/saves/reloads a file, which both arms the
// watch and re-baselines it so the app's own writes never self-trigger.
// Single active watch at a time, matching ElectronApp's single-editor-window
// model — a new watch() call always supersedes whatever was watched before.
export function registerFileWatchHandlers(onChanged: (filenames: string[]) => void): void {
    ipcMain.handle("fileWatch:watch", async (_event, dirPath: string, filenames: string[]) => {
        stopWatching();
        watchedDir = dirPath;
        for (const filename of filenames) {
            const mtime = await statMtime(path.join(dirPath, filename));
            baseline.set(filename, mtime ?? 0);
        }
        try {
            watcher = fsWatch(dirPath, () => {
                if (debounceTimer) clearTimeout(debounceTimer);
                debounceTimer = setTimeout(() => {
                    debounceTimer = null;
                    void checkForChanges(dirPath, onChanged);
                }, DEBOUNCE_MS);
            });
        } catch {
            // Directory unwatchable (e.g. removable media ejected mid-session)
            // — editing still works, just without external-change detection.
        }
    });

    ipcMain.handle("fileWatch:unwatch", () => {
        stopWatching();
    });
}
