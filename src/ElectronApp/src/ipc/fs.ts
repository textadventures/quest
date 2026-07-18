import { ipcMain } from "electron";
import { promises as fs } from "node:fs";

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
        return entries.map((entry) => ({ name: entry.name, isFile: entry.isFile() }));
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
