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
    openDirectory(options?: { defaultPath?: string; title?: string; message?: string; buttonLabel?: string }): Promise<string | null>;
    saveFile(options?: { defaultPath?: string; filters?: ElectronFileFilter[] }): Promise<string | null>;
}

interface ElectronShellApi {
    openExternal(url: string): Promise<void>;
}

interface ElectronPathApi {
    join(...segments: string[]): string;
}

interface ElectronPathsApi {
    // Documents/Quest Games — matches Quest 5's desktop editor default.
    defaultGamesDir(): Promise<string>;
}

// action is one of MenuAction in ElectronApp's src/main.ts ("new-game" |
// "open-folder" | "save" | "save-as") — kept as a plain string since these
// are separate npm projects with no shared type.
interface ElectronMenuApi {
    onAction(callback: (action: string) => void): () => void;
}

interface ElectronApi {
    fs: ElectronFsApi;
    dialog: ElectronDialogApi;
    shell: ElectronShellApi;
    path: ElectronPathApi;
    paths: ElectronPathsApi;
    menu: ElectronMenuApi;
}

declare global {
    interface Window {
        electronApp?: ElectronApi;
    }
}
