import type { AssetInfo, FileAdapter, LoadedFile } from "./types";

export interface LanguageOption {
    id: string;
    label: string;
}

export const LANGUAGES: LanguageOption[] = [
    { id: "English", label: "English" },
    { id: "Dansk", label: "Dansk (Danish)" },
    { id: "Deutsch", label: "Deutsch (German)" },
    { id: "Espanol", label: "Español (Spanish)" },
    { id: "Esperanto", label: "Esperanto" },
    { id: "Francais", label: "Français (French)" },
    { id: "Greek", label: "Greek" },
    { id: "Icelandic", label: "Icelandic" },
    { id: "Italiano", label: "Italiano (Italian)" },
    { id: "Nederlands", label: "Nederlands (Dutch)" },
    { id: "Norsk", label: "Norsk (Norwegian)" },
    { id: "Portugues", label: "Português (Portuguese)" },
    { id: "Portugues-Portugal", label: "Português - Portugal" },
    { id: "Romana", label: "Română (Romanian)" },
    { id: "Russian", label: "Russian" },
];

function escapeXml(s: string): string {
    return s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;");
}

function generateGameTemplate(name: string, type: "textadventure" | "gamebook", language: string): string {
    const escapedName = escapeXml(name);
    const id = crypto.randomUUID();
    const year = new Date().getFullYear();

    if (type === "gamebook") {
        return `<asl version="580" templatetype="gamebook">

  <include ref="GamebookCore.aslx"/>

  <game name="${escapedName}">
    <gameid>${id}</gameid>
    <version>1.0</version>
    <firstpublished>${year}</firstpublished>
  </game>

  <object name="Page1">
    <object name="player">
      <inherit name="defaultplayer"/>
    </object>
    <description>This is page 1. Type a description here, and then create links to other pages below.</description>
    <options type="simplestringdictionary">Page2 = This link goes to page 2;Page3 = And this link goes to page 3</options>
  </object>

  <object name="Page2">
    <description>This is page 2. Type a description here, and then create links to other pages below.</description>
  </object>

  <object name="Page3">
    <description>This is page 3. Type a description here, and then create links to other pages below.</description>
  </object>

</asl>`;
    }

    return `<asl version="580">

  <include ref="${language}.aslx"/>
  <include ref="Core.aslx"/>

  <game name="${escapedName}">
    <gameid>${id}</gameid>
    <version>1.0</version>
    <firstpublished>${year}</firstpublished>
  </game>

  <object name="room">
    <inherit name="editor_room" />

    <object name="player">
      <inherit name="editor_object" />
      <inherit name="editor_player" />
    </object>
  </object>

</asl>`;
}

export async function createNewGame(name: string, type: "textadventure" | "gamebook", language: string): Promise<string> {
    const content = generateGameTemplate(name, type, language);
    const filename = name.replace(/[^a-zA-Z0-9 _-]/g, "").trim() || "game";
    const file = new File([content], `${filename}.aslx`, { type: "application/xml" });
    const form = new FormData();
    form.append("name", name);
    form.append("file", file);
    const resp = await fetch("/api/editor/games", { method: "POST", body: form });
    if (!resp.ok) throw new Error(`Failed to create game: ${resp.status} ${resp.statusText}`);
    const data = await resp.json() as { gameId: string };
    return data.gameId;
}

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
