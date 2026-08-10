export interface AssetInfo {
    key: string;
    url: string;
}

export interface FileAdapter {
    readonly filename: string;
    readonly canSaveAs: boolean;
    saveFile(data: Uint8Array | string): Promise<void>;
    // Returns the filename that was saved to, or null if the user cancelled.
    saveFileAs(data: Uint8Array | string, suggestedName?: string): Promise<string | null>;
    putAsset(key: string, data: Blob): Promise<void>;
    getAsset(key: string): Promise<Blob | null>;
    listAssets(): Promise<AssetInfo[]>;
    deleteAsset(key: string): Promise<void>;
    // Only implemented by adapters keyed by GameId (LocalDraftAdapter) — moves
    // storage to a new key when the game's gameid field changes mid-edit.
    rekey?(newGameId: string): Promise<void>;
    // Re-reads this adapter's own current file from its backing store, for an
    // in-place reload (e.g. after adding an Included Library — see
    // LibraryReloadBanner.svelte) that re-runs WASM Initialise() without a full
    // browser navigation. Every adapter already holds what it needs to do this
    // with no user interaction (no picker/dialog), so it's always implemented.
    reload(): Promise<Uint8Array>;
}

export interface LoadedFile {
    bytes: Uint8Array;
    adapter: FileAdapter;
}

// Real on-disk folders (BrowserFileAdapter, ElectronFileAdapter) can pick up
// Finder-generated sidecar files that were never game assets — filter these
// out of listAssets() wherever a real directory is being scanned, same as
// local-adapter.ts already does for zip imports (isMacZipJunk).
export function isJunkAssetName(name: string): boolean {
    return name === ".DS_Store" || name.startsWith("._");
}
