export type Locale = string;

export const DEFAULT_LOCALE: Locale = "en";

// "xx" (pseudo-locale, static/locales/xx.json) is deliberately absent — it's a
// manual verification aid (see tests/e2e/appshell/i18n-pseudoloc.spec.mjs),
// never a real user-facing option.
export const SUPPORTED_LOCALES: readonly Locale[] = ["en", "de", "es"];
