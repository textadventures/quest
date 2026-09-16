import { ipcMain } from "electron";
import { promises as fs } from "node:fs";
import path from "node:path";

// Backs window.electronApp.fs in preload.ts — plain Node fs/promises, no
// abstraction beyond what ElectronFileAdapter (src/AppShell) actually calls.
export function registerFsHandlers(): void {
    ipcMain.handle("fs:readFile", async (_event, filePath: string) => {
        const buf = await fs.readFile(filePath);
        return new Uint8Array(buf);
    });

    ipcMain.handle("fs:writeFile", async (_event, filePath: string, data: Uint8Array | string) => {
        await fs.writeFile(filePath, data instanceof Uint8Array ? Buffer.from(data) : data);
    });

    ipcMain.handle("fs:readDir", async (_event, dirPath: string) => {
        const entries = await fs.readdir(dirPath, { withFileTypes: true });
        // size lets the Publish dialog show what a publish will include without reading every file.
        return Promise.all(entries.map(async (entry) => ({
            name: entry.name,
            isFile: entry.isFile(),
            size: entry.isFile() ? (await fs.stat(path.join(dirPath, entry.name)).catch(() => null))?.size ?? null : null,
        })));
    });

    ipcMain.handle("fs:exists", async (_event, filePath: string) => {
        try {
            await fs.access(filePath);
            return true;
        } catch {
            return false;
        }
    });

    ipcMain.handle("fs:mkdir", async (_event, dirPath: string) => {
        await fs.mkdir(dirPath, { recursive: true });
    });

    ipcMain.handle("fs:unlink", async (_event, filePath: string) => {
        await fs.unlink(filePath);
    });
}
