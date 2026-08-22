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
    showItemInFolder(path: string): Promise<void>;
}

interface ElectronFileWatchApi {
    watch(dirPath: string, filenames: string[]): Promise<void>;
    unwatch(): Promise<void>;
    onChanged(callback: (filenames: string[]) => void): () => void;
}

interface ElectronPathApi {
    join(...segments: string[]): string;
}

interface ElectronPathsApi {
    // Documents/Quest Games — matches Quest 5's desktop editor default.
    defaultGamesDir(): Promise<string>;
}

// Persisted to a userData file (ElectronApp's locale-store.ts), not
// localStorage — see i18n/index.ts for why Electron can't use the same
// storage as the browser build.
interface ElectronLocaleApi {
    get(): Promise<string | null>;
    set(locale: string): Promise<void>;
}

// Persisted to a userData file (ElectronApp's theme-store.ts), not
// localStorage — same reason as locale above (see theme-store.ts for why).
// getSync is the synchronous variant app.html's pre-paint inline script uses
// (see theme-store.ts's readThemeSync / ipc/theme.ts's "theme:getSync").
interface ElectronThemeApi {
    get(): Promise<string | null>;
    set(theme: string): Promise<void>;
    getSync(): string | null;
}

interface ElectronRecentGame {
    dirPath: string;
    filename: string;
    lastOpened: number;
    // "play"-kind only — see ElectronApp's recent-games.ts. undefined = never resolved;
    // null = resolved, no cover; string = the resolved cover as a data: URL.
    coverDataUrl?: string | null;
}

// "edit" = opened/created/saved-as through the editor (File > Open Recent,
// the /open page's Recent section); "play" = opened through the Play tab's
// file picker (that tab's own Recently Played list) — two separate lists,
// see ElectronApp's recent-games.ts.
type ElectronRecentKind = "edit" | "play";

interface ElectronRecentApi {
    list(kind: ElectronRecentKind): Promise<ElectronRecentGame[]>;
    add(kind: ElectronRecentKind, dirPath: string, filename: string, coverDataUrl?: string | null): Promise<ElectronRecentGame[]>;
    remove(kind: ElectronRecentKind, dirPath: string, filename: string): Promise<ElectronRecentGame[]>;
    // Patches an existing entry's coverDataUrl in place — unlike add(), doesn't bump
    // lastOpened or reorder. Used to self-heal a legacy/unresolved entry in the background.
    setCover(kind: ElectronRecentKind, dirPath: string, filename: string, coverDataUrl: string | null): Promise<ElectronRecentGame[]>;
    onChanged(callback: (kind: ElectronRecentKind) => void): () => void;
}

// action is one of MenuAction in ElectronApp's src/main.ts ("new-game" |
// "open-file" | "save" | "save-as") — kept as a plain string since these
// are separate npm projects with no shared type.
interface ElectronMenuApi {
    onAction(callback: (action: string) => void): () => void;
    onOpenRecent(callback: (game: { dirPath: string; filename: string }) => void): () => void;
}

// Mirrors CatalogGame in home-catalog.ts — kept as its own shape since
// preload.ts's module graph is separate from AppShell's.
interface ElectronCatalogGame {
    id: string;
    name: string;
    author: string | null;
    cover: string | null;
    thumbnail: string | null;
    rating: number;
    isGamebook: boolean;
    language: string;
}

interface ElectronRecentCatalogPlay extends ElectronCatalogGame {
    lastPlayed: number;
}

// Persisted to a userData file (ElectronApp's catalog-plays-store.ts), not
// localStorage — see recent-catalog-plays.ts for why.
interface ElectronCatalogPlaysApi {
    list(): Promise<ElectronRecentCatalogPlay[]>;
    record(game: ElectronCatalogGame): Promise<ElectronRecentCatalogPlay[]>;
    remove(id: string): Promise<ElectronRecentCatalogPlay[]>;
}

// Persisted to a userData file (ElectronApp's update-dismiss-store.ts), not
// localStorage — see UpdateBanner.svelte for why.
interface ElectronUpdateDismissApi {
    get(): Promise<string | null>;
    set(version: string): Promise<void>;
}

// Per-game editor UI state (currently the tree's expanded-node ids), keyed by
// the game's <gameid> element — persisted to a userData file (ElectronApp's
// ui-state-store.ts), not localStorage — see tree-state.ts for why.
interface ElectronUiStateApi {
    get(): Promise<Record<string, string[]>>;
    set(gameId: string, state: string[]): Promise<void>;
}

// Whether ScriptEditor instances default to raw code view — persisted to a
// userData file (ElectronApp's default-code-view-store.ts), not localStorage
// — see code-view-store.ts for why.
interface ElectronDefaultCodeViewApi {
    get(): Promise<boolean | null>;
    set(enabled: boolean): Promise<void>;
}

// id: catalog game (textadventures.co.uk id); omitted: a locally-picked
// file, whose bytes/resources the caller hands over separately via the
// 'quest-play-local' BroadcastChannel — see ipc/player.ts and
// components/PlayCatalog.svelte.
interface ElectronPlayerApi {
    openWindow(request?: { id?: string }): Promise<boolean>;
    // Fired by a file-association open of a play-kind file (.quest/.asl/.cas)
    // while a window already exists — see ElectronApp's main.ts
    // (routeOpenedFile) and preload.ts. PlayCatalog.svelte's listener
    // loads the file and launches a player window for it via
    // playElectronFile, same as its own file-picker/Recently Played flows.
    onOpenPlayFile(callback: (file: { dirPath: string; filename: string }) => void): () => void;
}

interface ElectronApi {
    fs: ElectronFsApi;
    dialog: ElectronDialogApi;
    shell: ElectronShellApi;
    path: ElectronPathApi;
    paths: ElectronPathsApi;
    locale: ElectronLocaleApi;
    theme: ElectronThemeApi;
    recent: ElectronRecentApi;
    catalogPlays: ElectronCatalogPlaysApi;
    updateDismiss: ElectronUpdateDismissApi;
    uiState: ElectronUiStateApi;
    defaultCodeView: ElectronDefaultCodeViewApi;
    fileWatch: ElectronFileWatchApi;
    menu: ElectronMenuApi;
    player: ElectronPlayerApi;
    // Populated by preload.ts from process.platform (main/preload only —
    // unavailable in the sandboxed renderer). Sent as analytics metadata
    // alongside webhome_games_list/webhome_game_play (see ClientInfo on
    // textadventures.co.uk's ApiController). Optional since process.platform
    // could in principle be something outside this union (e.g. freebsd).
    platform?: "Mac" | "Windows" | "Linux";
    // Populated by preload.ts from process.arch. Only used server-side to
    // disambiguate Linux downloads (GetDownloadUrlAsync) — Windows/Mac
    // electron-builder targets are each single-arch.
    arch?: "x64" | "arm64";
}

declare global {
    interface Window {
        electronApp?: ElectronApi;
    }
}
