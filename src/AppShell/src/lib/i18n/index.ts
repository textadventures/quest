import { writable, get } from "svelte/store";
import { base } from "$app/paths";
import { DEFAULT_LOCALE, SUPPORTED_LOCALES, type Locale } from "./locales";
import { detectDefaultLocale } from "./detect";

export { DEFAULT_LOCALE, SUPPORTED_LOCALES };
export type { Locale };

const STORAGE_KEY = "questviva-ui-language";

export const locale = writable<Locale>(DEFAULT_LOCALE);

// Gates first render (see +layout.svelte) so nothing shows raw dotted keys
// before the initial fetchMessages() resolves.
export const localeReady = writable(false);

type Messages = Record<string, string>;
let messages: Messages = {};

function readStoredLocale(): Locale | null {
    if (typeof localStorage === "undefined") return null;
    try {
        return localStorage.getItem(STORAGE_KEY);
    } catch {
        return null;
    }
}

function writeStoredLocale(loc: Locale): void {
    if (typeof localStorage === "undefined") return;
    try {
        localStorage.setItem(STORAGE_KEY, loc);
    } catch {
        // Storage can be unavailable (private browsing, quota) — losing the
        // preference just means it's re-detected next load, not fatal.
    }
}

async function fetchMessages(loc: Locale): Promise<Messages> {
    try {
        const res = await fetch(`${base}/locales/${loc}.json`);
        return res.ok ? await res.json() : {};
    } catch {
        return {};
    }
}

// Called once from +layout.svelte's onMount, before route content renders.
export async function initI18n(): Promise<void> {
    const stored = readStoredLocale();
    await setLocale(stored ?? detectDefaultLocale(), { persist: false });
}

// Called directly by SettingsModal on selection — caller reloads the page
// afterwards (t() reads `messages` synchronously, it isn't reactive; see
// the plan's "reload, not live re-render" rationale).
export async function setLocale(loc: Locale, opts: { persist?: boolean } = {}): Promise<void> {
    const target = await fetchMessages(loc);
    // English backfills any key missing from a non-English (or not-yet-translated)
    // locale, so a partial locale file degrades gracefully per-key instead of
    // showing raw dotted keys for anything untranslated.
    const en = loc === DEFAULT_LOCALE ? target : await fetchMessages(DEFAULT_LOCALE);
    messages = loc === DEFAULT_LOCALE ? target : { ...en, ...target };
    locale.set(loc);
    localeReady.set(true);
    if (opts.persist !== false) writeStoredLocale(loc);
}

export function t(key: string, params?: Record<string, string | number>): string {
    const template = messages[key] ?? key; // missing key shows the raw key — visible, not silently swallowed
    if (!params) return template;
    return template.replace(/\{(\w+)\}/g, (_, name: string) => String(params[name] ?? `{${name}}`));
}

export function tPlural(key: string, count: number, params?: Record<string, string | number>): string {
    const category = new Intl.PluralRules(get(locale)).select(count);
    const template = messages[`${key}.${category}`] ?? messages[`${key}.other`] ?? `${key}.${category}`;
    return t(template, { count, ...params });
}
