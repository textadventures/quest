// Ambient typing for window.electronApp, exposed via contextBridge by
// src/ElectronApp/src/preload.ts. Present only when running inside the
// Electron desktop app (see runtime.ts's isElectron()).

export {};

interface ElectronFileFilter {
    name: string;
    extensions: string[];
}

interface ElectronFsApi {
    readFile(path: string): Promise<Uint8Array>;
    writeFile(path: string, data: Uint8Array | string): Promise<void>;
    readDir(path: string): Promise<{ name: string; isFile: boolean }[]>;
    exists(path: string): Promise<boolean>;
    mkdir(path: string): Promise<void>;
    unlink(path: string): Promise<void>;
}

interface ElectronDialogApi {
    openFile(options?: { filters?: ElectronFileFilter[]; defaultPath?: string }): Promise<string | null>;
    openDirectory(options?: { defaultPath?: string }): Promise<string | null>;
    saveFile(options?: { defaultPath?: string; filters?: ElectronFileFilter[] }): Promise<string | null>;
}

interface ElectronShellApi {
    openExternal(url: string): Promise<void>;
}

interface ElectronPathApi {
    join(...segments: string[]): string;
}

interface ElectronApi {
    fs: ElectronFsApi;
    dialog: ElectronDialogApi;
    shell: ElectronShellApi;
    path: ElectronPathApi;
}

declare global {
    interface Window {
        electronApp?: ElectronApi;
    }
}
