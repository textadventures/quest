import type { AssetInfo, FileAdapter, LoadedFile } from "./types";

export class ServerFileAdapter implements FileAdapter {
    constructor(
        private readonly _gameId: string,
        private readonly _filename: string,
        readonly previewUrl: string | null,
    ) {}

    get filename() { return this._filename; }
    readonly canSaveAs = false;

    async saveFile(data: Uint8Array | string): Promise<void> {
        const body = typeof data === "string" ? data : new Blob([data.slice()]);
        const resp = await fetch(`/api/editor/games/${this._gameId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/xml" },
            body,
        });
        if (!resp.ok) throw new Error(`Save failed: ${resp.status} ${resp.statusText}`);
    }

    // Server mode has no "save as" concept — save in place
    async saveFileAs(data: Uint8Array | string): Promise<string | null> {
        await this.saveFile(data);
        return this._filename;
    }

    async putAsset(key: string, data: Blob): Promise<void> {
        const form = new FormData();
        form.append("file", data, key);
        const resp = await fetch(`/api/editor/games/${this._gameId}/assets`, {
            method: "POST",
            body: form,
        });
        if (!resp.ok) throw new Error(`Asset upload failed: ${resp.status} ${resp.statusText}`);
    }

    async getAsset(key: string): Promise<Blob | null> {
        const resp = await fetch(`/api/editor/games/${this._gameId}/assets/${encodeURIComponent(key)}`);
        if (resp.status === 404) return null;
        if (!resp.ok) throw new Error(`Asset fetch failed: ${resp.status} ${resp.statusText}`);
        return await resp.blob();
    }

    async listAssets(): Promise<AssetInfo[]> {
        const resp = await fetch(`/api/editor/games/${this._gameId}/assets`);
        if (!resp.ok) throw new Error(`Asset list failed: ${resp.status} ${resp.statusText}`);
        const data = await resp.json() as { assets: { filename: string; url: string }[] };
        return data.assets.map(a => ({ key: a.filename, url: a.url }));
    }

    async deleteAsset(key: string): Promise<void> {
        const resp = await fetch(`/api/editor/games/${this._gameId}/assets/${encodeURIComponent(key)}`, {
            method: "DELETE",
        });
        if (!resp.ok) throw new Error(`Asset delete failed: ${resp.status} ${resp.statusText}`);
    }
}

export async function loadFromServer(gameId: string): Promise<LoadedFile> {
    const resp = await fetch(`/api/editor/games/${gameId}`);
    if (!resp.ok) throw new Error(`Failed to load game: ${resp.status} ${resp.statusText}`);
    const disposition = resp.headers.get("Content-Disposition") ?? "";
    const match = /filename[^;=\n]*=['"]?([^'";\n]+)['"]?/.exec(disposition);
    const filename = match?.[1]?.trim() ?? `${gameId}.aslx`;
    const previewUrl = resp.headers.get("X-Preview-Url");
    const bytes = new Uint8Array(await resp.arrayBuffer());
    return { bytes, adapter: new ServerFileAdapter(gameId, filename, previewUrl) };
}
