import { PUBLIC_APPSHELL_VERSION } from "$env/static/public";
import { isElectron } from "./runtime";

export interface CatalogGame {
    id: string;
    name: string;
    author: string | null;
    cover: string | null;
    thumbnail: string | null;
    rating: number;
    isGamebook: boolean;
    language: string;
}

export interface CatalogCategory {
    title: string;
    // Tag slug for paging this category via fetchCategory — null for the
    // curated homepage-style sections (Latest Games, Random Picks, etc.),
    // which aren't a single browsable tag (see ApiController.Catalog).
    slug: string | null;
    games: CatalogGame[];
}

export interface CategoryPage {
    title: string;
    games: CatalogGame[];
    totalCount: number;
    page: number;
    pageCount: number;
}

export interface SearchResults {
    games: CatalogGame[];
    totalCount: number;
    page: number;
    pageCount: number;
}

// Present only for Electron clients running an outdated build (see
// ApiController.Catalog on textadventures.co.uk, which compares client.Version
// against the latest tagged GitHub release) — web builds are static-redeployed
// on every release and are always current, so this is always null there.
export interface UpdateInfo {
    latestVersion: string;
    url: string;
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
    isGamebook: boolean;
    language: string;
}

// Friendly display name for a game's ISO language code (e.g. "de" -> "German"),
// falling back to the raw code if the runtime can't resolve it (Intl.DisplayNames
// is broadly supported, but not guaranteed for every code the server sends).
export function languageName(code: string): string {
    try {
        return new Intl.DisplayNames(["en"], { type: "language" }).of(code) ?? code;
    } catch {
        return code;
    }
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
    if (PUBLIC_APPSHELL_VERSION) params.set("version", PUBLIC_APPSHELL_VERSION);
    const platform = window.electronApp?.platform;
    if (platform) params.set("platform", platform);
    const arch = window.electronApp?.arch;
    if (arch) params.set("arch", arch);
    return params;
}

export async function fetchCatalog(): Promise<{ categories: CatalogCategory[]; update: UpdateInfo | null }> {
    const params = clientInfoParams();
    params.set("maxAslVersion", String(MAX_ASL_VERSION));
    const response = await fetch(`${API_ROOT}/Catalog?${params}`);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    const data = await response.json() as { categories: CatalogCategory[]; update: UpdateInfo | null };
    return { categories: data.categories, update: data.update ?? null };
}

export async function fetchGameDetails(id: string): Promise<GameDetails> {
    const response = await fetch(`${API_ROOT}/GameDetails/${encodeURIComponent(id)}?${clientInfoParams()}`);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return await response.json() as GameDetails;
}

// Paginated browsing for a single category (tag) — `slug` is a CatalogCategory's
// `slug` as returned by fetchCatalog. Mirrors ApiController.Category.
export async function fetchCategory(slug: string, page: number): Promise<CategoryPage> {
    const params = clientInfoParams();
    params.set("maxAslVersion", String(MAX_ASL_VERSION));
    params.set("category", slug);
    params.set("page", String(page));
    const response = await fetch(`${API_ROOT}/Category?${params}`);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return await response.json() as CategoryPage;
}

// Free-text search (with category:/platform:/language: operators, same syntax
// as the website's own search) for the Play tab's search box. Mirrors
// ApiController.Search.
export async function searchGames(query: string, page: number): Promise<SearchResults> {
    const params = clientInfoParams();
    params.set("maxAslVersion", String(MAX_ASL_VERSION));
    params.set("q", query);
    params.set("page", String(page));
    const response = await fetch(`${API_ROOT}/Search?${params}`);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return await response.json() as SearchResults;
}
