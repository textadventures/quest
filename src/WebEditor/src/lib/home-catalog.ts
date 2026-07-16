import { PUBLIC_WEBEDITOR_VERSION } from "$env/static/public";
import { isElectron } from "./runtime";

export interface CatalogGame {
    id: string;
    name: string;
    author: string | null;
    cover: string | null;
    thumbnail: string | null;
    rating: number;
}

export interface CatalogCategory {
    title: string;
    games: CatalogGame[];
}

export interface GameDetails {
    id: string;
    name: string;
    description: string | null;
    tags: string[];
    author: string | null;
    rating: number;
    reviewCount: number;
    cover: string | null;
    thumbnail: string | null;
    publishedDate: string;
    url: string;
}

const API_ROOT = "https://textadventures.co.uk/api";

// The highest ASL version this build's WasmPlayer can load — must track the
// `Versions` dictionary in src/Engine/GameLoader/GameLoader.cs (currently maxing
// out at 580); bump this alongside any Engine change that adds a new version.
const MAX_ASL_VERSION = 580;

// Attached to catalog/details requests as analytics metadata (see ClientInfo
// on textadventures.co.uk's ApiController). source/platform are free strings,
// not enums, so future iOS/Android clients can self-report without a server
// change — platform isn't populated yet (see electron-types.d.ts).
function clientInfoParams(): URLSearchParams {
    const params = new URLSearchParams();
    params.set("source", isElectron() ? "electron" : "web");
    if (PUBLIC_WEBEDITOR_VERSION) params.set("version", PUBLIC_WEBEDITOR_VERSION);
    const platform = window.electronApp?.platform;
    if (platform) params.set("platform", platform);
    return params;
}

export async function fetchCatalog(): Promise<CatalogCategory[]> {
    const params = clientInfoParams();
    params.set("maxAslVersion", String(MAX_ASL_VERSION));
    const response = await fetch(`${API_ROOT}/Catalog?${params}`);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    const data = await response.json() as { categories: CatalogCategory[] };
    return data.categories;
}

export async function fetchGameDetails(id: string): Promise<GameDetails> {
    const response = await fetch(`${API_ROOT}/GameDetails/${encodeURIComponent(id)}?${clientInfoParams()}`);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return await response.json() as GameDetails;
}
