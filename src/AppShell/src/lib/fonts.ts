// Fonts for a game. Three kinds can be chosen for its text and its interface:
//
// - The game's own fonts, added from Fontsource (Google Fonts and other open source fonts) by the
//   Fonts dialog (FontsModal.svelte). Each is copied into the game's folder as one file,
//   <id>.font.css: its @font-face rules with the font's files embedded as data: URLs, and its
//   licence in a comment at the top. A game's files are flat and the player turns each into a URL
//   of its own, so a stylesheet couldn't point at font files beside it. The game lists its font
//   files in game.fontfiles, and the player loads them (CoreOutput.aslx's AddFontFiles). From then
//   on the font is an ordinary part of the game: the preview, publishing and qv work offline, and
//   a published game never asks Google for anything. Only adding a font needs the internet.
// - The fonts built into the player (BUILT_IN_FONTS, the themes' fonts), which any game can name.
// - Fonts most computers have (SYSTEM_FONTS).
//
// A font setting is a CSS font-family list, such as 'Literata', Georgia, serif. The player writes
// it into style="" attributes, so family names are quoted with single quotes, never double.

import { get, writable } from "svelte/store";
import { assets, getAssetText, putAssetText, deleteAssetByKey, addListItem, removeListItem } from "./editor-store";

const API = "https://api.fontsource.org/v1/fonts";
const LICENCE_URL = (id: string, version: string) => `https://cdn.jsdelivr.net/npm/@fontsource/${id}@${version}/LICENSE`;

export const FONT_FILE_EXTENSION = ".font.css";
export const FONT_FILES_ATTRIBUTE = "fontfiles";

export type FontCategory = "serif" | "sans-serif" | "display" | "handwriting" | "monospace" | string;

// A font in Fontsource's list
export interface FontSummary {
    id: string;
    family: string;
    category: FontCategory;
    subsets: string[];
    weights: number[];
    styles: string[];
    license?: string;
}

// A font's files: variants[weight][style][subset].url.woff2. The subsets of a font with a lot of
// characters, such as a Japanese one, are numbered ("0", "1", ...), with unicodeRange keys "[0]" etc.
export interface FontDetails extends FontSummary {
    npmVersion?: string;
    unicodeRange: Record<string, string>;
    variants: Record<string, Record<string, Record<string, { url: Record<string, string> }>>>;
}

// A font to choose for the game's text or interface
export interface FontChoice {
    family: string;
    // The font-family list to set
    stack: string;
}

// Built into the player: WasmPlayer's wwwroot/fonts, fetched by its scripts/fetch-builtin-fonts.mjs.
// The same lists as EditorCore's FontsManager, and the themes' (CoreThemes.aslx).
export const BUILT_IN_FONTS: FontChoice[] = [
    { family: "Literata", stack: "Literata, Georgia, serif" },
    { family: "Nunito", stack: "Nunito, 'Trebuchet MS', sans-serif" },
    { family: "Courier Prime", stack: "'Courier Prime', 'Courier New', Courier, monospace" },
    { family: "Special Elite", stack: "'Special Elite', 'Courier New', Courier, monospace" },
    { family: "VT323", stack: "VT323, 'Lucida Console', Monaco, monospace" },
];

// Fonts most computers have, as EditorCore's FontsManager lists them
export const SYSTEM_FONTS: FontChoice[] = [
    "Georgia, serif",
    "'Palatino Linotype', 'Book Antiqua', Palatino, serif",
    "'Times New Roman', Times, serif",
    "Arial, Helvetica, sans-serif",
    "'Arial Black', Gadget, sans-serif",
    "'Comic Sans MS', cursive, sans-serif",
    "Impact, Charcoal, sans-serif",
    "'Lucida Sans Unicode', 'Lucida Grande', sans-serif",
    "Tahoma, Geneva, sans-serif",
    "'Trebuchet MS', Helvetica, sans-serif",
    "Verdana, Geneva, sans-serif",
    "'Courier New', Courier, monospace",
    "'Lucida Console', Monaco, monospace",
].map(stack => ({ family: firstFamily(stack), stack }));

// Good fonts for reading a game in, offered before the whole list is searched. The fonts built
// into the player aren't here: they can be chosen without adding them.
export const RECOMMENDED_FONTS: { id: string; family: string; category: FontCategory }[] = [
    { id: "source-serif-4", family: "Source Serif 4", category: "serif" },
    { id: "lora", family: "Lora", category: "serif" },
    { id: "eb-garamond", family: "EB Garamond", category: "serif" },
    { id: "crimson-pro", family: "Crimson Pro", category: "serif" },
    { id: "libre-baskerville", family: "Libre Baskerville", category: "serif" },
    { id: "alegreya", family: "Alegreya", category: "serif" },
    { id: "newsreader", family: "Newsreader", category: "serif" },
    { id: "im-fell-english", family: "IM Fell English", category: "serif" },
    { id: "atkinson-hyperlegible", family: "Atkinson Hyperlegible", category: "sans-serif" },
    { id: "source-sans-3", family: "Source Sans 3", category: "sans-serif" },
    { id: "lexend", family: "Lexend", category: "sans-serif" },
    { id: "inter", family: "Inter", category: "sans-serif" },
    { id: "jetbrains-mono", family: "JetBrains Mono", category: "monospace" },
    { id: "press-start-2p", family: "Press Start 2P", category: "display" },
    { id: "cinzel", family: "Cinzel", category: "serif" },
    { id: "uncial-antiqua", family: "Uncial Antiqua", category: "display" },
];

// The subsets a new font has to start with, if it has them: enough for English and most
// European languages
export const DEFAULT_SUBSETS = ["latin", "latin-ext"];

// The fonts to fall back on, for characters the font doesn't have, or while it loads
export function fallbackFor(category: FontCategory): string {
    switch (category) {
    case "sans-serif": return "Arial, Helvetica, sans-serif";
    case "monospace": return "'Courier New', Courier, monospace";
    default: return "Georgia, serif";
    }
}

// A family name quoted for a font-family list. Single quotes: see the top of this file.
export const quoteFamily = (family: string) => `'${family.replace(/['\\]/g, "\\$&")}'`;

// The font-family list for a font, with fonts to fall back on
export const fontStack = (family: string, category: FontCategory) => `${quoteFamily(family)}, ${fallbackFor(category)}`;

// The first family in a font-family list, unquoted: the font it's for
export function firstFamily(stack: string): string {
    const match = /^\s*(?:"((?:[^"\\]|\\.)*)"|'((?:[^'\\]|\\.)*)'|([^,]*))/.exec(stack);
    if (!match) return "";
    return (match[1] ?? match[2] ?? match[3] ?? "").replace(/\\(.)/g, "$1").trim();
}

// A subset's name to show, e.g. "latin-ext" -> "Latin Extended"
export function subsetLabel(subset: string): string {
    return subset.split("-").map(word => word === "ext" ? "Extended" : word.charAt(0).toUpperCase() + word.slice(1)).join(" ");
}

let catalogue: Promise<FontSummary[]> | undefined;

// Fontsource's whole list, fetched once. Fails offline.
export function fetchCatalogue(): Promise<FontSummary[]> {
    catalogue ??= fetchJson<FontSummary[]>(API).catch((error: unknown) => {
        catalogue = undefined;
        throw error;
    });
    return catalogue;
}

export const fetchFontDetails = (id: string) => fetchJson<FontDetails>(`${API}/${encodeURIComponent(id)}`);

async function fetchJson<T>(url: string): Promise<T> {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`${url} answered ${response.status}`);
    return await response.json() as T;
}

// The URL of a font's normal, regular weight Latin file, to show its name in the font
export function previewUrl(id: string): string {
    return `https://cdn.jsdelivr.net/fontsource/fonts/${encodeURIComponent(id)}@latest/latin-400-normal.woff2`;
}

// Fontsource's id for a family, such as "Special Elite" -> "special-elite", to find a web font by
// the name a game gave it
export const fontsourceId = (family: string) => family.trim().toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");

export interface FontFace {
    weight: number;
    style: "normal" | "italic";
    // The subset's key in the font's variants, and its unicode-range, if it has one
    subset: string;
    unicodeRange?: string;
    url: string;
}

// The weights a game needs: the regular one (400, or the closest there is) for its text, and a
// bold one (700, or the heaviest heavier one) for <b> and headings, if the font has one
export function chooseWeights(weights: number[]): number[] {
    if (!weights.length) return [];
    const regular = [...weights].sort((a, b) => Math.abs(a - 400) - Math.abs(b - 400) || a - b)[0];
    const heavier = weights.filter(w => w >= regular + 200);
    const bold = heavier.includes(700) ? 700 : heavier.length ? Math.max(...heavier) : undefined;
    return bold === undefined ? [regular] : [regular, bold];
}

// The files to download for a font: its regular and bold weights, normal and italic, in the
// chosen subsets. A subset that isn't among the font's files by name (such as "japanese") is the
// font's numbered subsets.
export function chooseFaces(font: FontDetails, subsets: string[]): FontFace[] {
    const faces: FontFace[] = [];
    for (const weight of chooseWeights(font.weights)) {
        for (const style of ["normal", "italic"] as const) {
            const files = font.variants[String(weight)]?.[style];
            if (!files) continue;
            const keys = Object.keys(files);
            const numbered = keys.filter(key => /^\d+$/.test(key));
            const wanted = new Set(subsets.flatMap(subset => keys.includes(subset) ? [subset] : numbered));
            for (const subset of keys.filter(key => wanted.has(key))) {
                const url = files[subset].url.woff2 ?? files[subset].url.woff;
                if (!url) continue;
                faces.push({
                    weight, style, subset, url,
                    unicodeRange: font.unicodeRange[subset] ?? font.unicodeRange[`[${subset}]`],
                });
            }
        }
    }
    return faces;
}

function base64(bytes: Uint8Array): string {
    let binary = "";
    for (let i = 0; i < bytes.length; i += 0x8000) binary += String.fromCharCode(...bytes.subarray(i, i + 0x8000));
    return btoa(binary);
}

// A comment can't contain the end of a comment
const commentText = (text: string) => text.replace(/\*\//g, "* /").trimEnd();

// A font's file: a note of what it is, its licence, and an @font-face rule for each face, with
// the face's file embedded
export function fontFileCss(font: FontSummary, faces: FontFace[], files: Uint8Array[], licence: string | undefined): string {
    const article = /^[aeiou]/.test(font.category) ? "an" : "a";
    const lines = [
        `/* ${font.family}, ${article} ${font.category} font from Fontsource: https://fontsource.org/fonts/${font.id}`,
        "   Added by Quest Viva's Fonts dialog, which makes this file: there's no need to edit it.",
        ...(licence
            ? [`   Its licence${font.license ? ` (${font.license})` : ""}, which has to stay with the font:`, "", commentText(licence)]
            : [`   Its licence${font.license ? ` is ${font.license}` : " is on that page"}.`]),
        "*/",
        "",
    ];
    for (const [i, face] of faces.entries()) {
        const format = face.url.endsWith(".woff") ? "woff" : "woff2";
        lines.push(
            "@font-face {",
            `    font-family: ${quoteFamily(font.family)};`,
            `    font-style: ${face.style};`,
            `    font-weight: ${face.weight};`,
            "    font-display: swap;",
            `    src: url(data:font/${format};base64,${base64(files[i])}) format("${format}");`,
            ...(face.unicodeRange ? [`    unicode-range: ${face.unicodeRange};`] : []),
            "}",
            "",
        );
    }
    return lines.join("\n");
}

// The family a font file's @font-face rules are for, if it has any
export function fontFamilyIn(css: string): string | undefined {
    const match = /@font-face\s*\{[^}]*?font-family\s*:\s*(?:"((?:[^"\\]|\\.)*)"|'((?:[^'\\]|\\.)*)'|([^;}]+))/i.exec(css);
    if (!match) return undefined;
    return (match[1] ?? match[2] ?? match[3]).replace(/\\(.)/g, "$1").trim();
}

// The kind of font a file this file made is for, from its first line, so it can fall back on a
// font of the same kind
export function categoryIn(css: string): FontCategory | undefined {
    return /^\/\* .+?, an? ([\w-]+) font from Fontsource/.exec(css)?.[1];
}

// ── The game's own fonts ─────────────────────────────────────────────────────

export interface GameFont extends FontChoice {
    // Its file in the game's folder
    file: string;
    category: FontCategory;
    // Whether game.fontfiles lists it, so the player loads it
    listed: boolean;
}

// The game's font files and the fonts in them, kept up to date with the game's files. Each one is
// also loaded into the editor's own page, for the font pickers and previews to show it in.
export const gameFonts = writable<GameFont[]>([]);

// The font files the game lists, from game.fontfiles as the editor gives it (a list's JSON)
export function listedFontFiles(value: string | null): { key: string; file: string }[] {
    if (!value) return [];
    try {
        const items = JSON.parse(value) as { key: string; value: string }[];
        return Array.isArray(items) ? items.map(i => ({ key: i.key, file: i.value })) : [];
    } catch {
        return [];
    }
}

const loadedCss = new Map<string, string>();

function loadIntoPage(file: string, css: string) {
    if (loadedCss.get(file) === css) return;
    loadedCss.set(file, css);
    let style = document.head.querySelector<HTMLStyleElement>(`style[data-game-font="${CSS.escape(file)}"]`);
    if (!style) {
        style = document.createElement("style");
        style.dataset.gameFont = file;
        document.head.append(style);
    }
    style.textContent = css;
}

function unloadFromPage(file: string) {
    loadedCss.delete(file);
    document.head.querySelector(`style[data-game-font="${CSS.escape(file)}"]`)?.remove();
}

let refreshing: Promise<void> = Promise.resolve();

// Reads the game's font files again. listed is game.fontfiles' value.
export function refreshGameFonts(listed: string | null): Promise<void> {
    refreshing = refreshing.then(async () => {
        const files = get(assets).map(a => a.key).filter(key => key.toLowerCase().endsWith(FONT_FILE_EXTENSION)).sort();
        const listedFiles = new Set(listedFontFiles(listed).map(f => f.file));
        const fonts: GameFont[] = [];
        for (const file of files) {
            const css = await getAssetText(file);
            const family = css ? fontFamilyIn(css) : undefined;
            if (!css || !family) continue;
            loadIntoPage(file, css);
            const category = categoryIn(css) ?? "serif";
            fonts.push({ file, family, category, stack: fontStack(family, category), listed: listedFiles.has(file) });
        }
        for (const file of [...loadedCss.keys()]) {
            if (!files.includes(file)) unloadFromPage(file);
        }
        gameFonts.set(fonts);
    });
    return refreshing;
}

// Downloads a font into the game, as one file, and lists it in game.fontfiles. Returns the file.
export async function addFont(gameKey: string, listed: string | null, font: FontDetails, subsets: string[],
    onprogress?: (done: number, total: number) => void): Promise<string> {
    const faces = chooseFaces(font, subsets);
    if (!faces.length) throw new Error(`${font.family} has no files to download.`);
    let done = 0;
    const download = async (url: string) => {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`Couldn't download ${font.family}: ${url} answered ${response.status}.`);
        const bytes = new Uint8Array(await response.arrayBuffer());
        onprogress?.(++done, faces.length);
        return bytes;
    };
    const [files, licence] = await Promise.all([
        Promise.all(faces.map(face => download(face.url))),
        font.npmVersion
            ? fetch(LICENCE_URL(font.id, font.npmVersion)).then(r => r.ok ? r.text() : undefined).catch(() => undefined)
            : Promise.resolve(undefined),
    ]);

    // Everything's downloaded, so nothing changes in the game unless the whole font can be added
    const file = `${font.id}${FONT_FILE_EXTENSION}`;
    await putAssetText(file, fontFileCss(font, faces, files, licence), "text/css");
    if (!listedFontFiles(listed).some(f => f.file === file)) {
        const result = addListItem(gameKey, FONT_FILES_ATTRIBUTE, file);
        if (result !== "ok") throw new Error(result);
    }
    return file;
}

// Lists a font file in game.fontfiles, so the player loads it
export function listFont(gameKey: string, font: GameFont): string {
    return addListItem(gameKey, FONT_FILES_ATTRIBUTE, font.file);
}

// Takes a font out of the game: its file, and from game.fontfiles
export async function removeFont(gameKey: string, listed: string | null, font: GameFont) {
    const item = listedFontFiles(listed).find(f => f.file === font.file);
    if (item) {
        const result = removeListItem(gameKey, FONT_FILES_ATTRIBUTE, item.key);
        if (result !== "ok") throw new Error(result);
    }
    await deleteAssetByKey(font.file);
}

// The font a font setting is for, among the ones the editor knows of
export function findFont(stack: string, fonts: GameFont[]): { kind: "game" | "builtin" | "system"; font: FontChoice } | null {
    const family = firstFamily(stack).toLowerCase();
    if (!family) return null;
    const game = fonts.find(f => f.family.toLowerCase() === family);
    if (game) return { kind: "game", font: game };
    const builtIn = BUILT_IN_FONTS.find(f => f.family.toLowerCase() === family);
    if (builtIn) return { kind: "builtin", font: builtIn };
    const system = SYSTEM_FONTS.find(f => f.stack === stack.trim());
    return system ? { kind: "system", font: system } : null;
}
