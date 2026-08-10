import { DEFAULT_LOCALE, SUPPORTED_LOCALES, type Locale } from "./locales";

// Picks the closest supported locale for the browser's language preference
// (e.g. "en-GB" -> "en"), falling back to DEFAULT_LOCALE. Only used as the
// pre-any-preference default, before a stored questviva-ui-language choice
// exists — see initI18n() in ./index.ts.
export function detectDefaultLocale(): Locale {
    const candidates = typeof navigator !== "undefined" ? (navigator.languages ?? [navigator.language]) : [];
    for (const raw of candidates) {
        if (!raw) continue;
        const lower = raw.toLowerCase();
        const exact = SUPPORTED_LOCALES.find(l => l.toLowerCase() === lower);
        if (exact) return exact;
        const base = lower.split("-")[0];
        const baseMatch = SUPPORTED_LOCALES.find(l => l.toLowerCase().split("-")[0] === base);
        if (baseMatch) return baseMatch;
    }
    return DEFAULT_LOCALE;
}
