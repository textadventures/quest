import { writable, get } from "svelte/store";
import { isElectron } from "./runtime";

export type ThemePreference = "light" | "dark" | "system";

// Defaults to "system" (follow the OS, the pre-toggle behaviour) — the
// effective light/dark is derived from prefers-color-scheme until the user
// picks something explicit in SettingsModal.
export const theme = writable<ThemePreference>("system");

const STORAGE_KEY = "questviva-ui-theme";

// Dark mode is driven by a `.dark` class on <html> (see app.css's
// @custom-variant dark) rather than Tailwind's default prefers-color-scheme
// media query, so an explicit Light/Dark choice can override the OS. The
// class is what flips both the dark: utilities and the light-dark() paired
// tokens (which resolve against :root's color-scheme, set alongside the
// class). applyTheme is synchronous and idempotent — it's also the logic
// behind app.html's pre-paint inline script, which runs before this module
// can (it reads the same storage and toggles the same class/color-scheme).
function systemIsDark(): boolean {
    if (typeof matchMedia === "undefined") return false;
    return matchMedia("(prefers-color-scheme: dark)").matches;
}

function applyTheme(pref: ThemePreference): void {
    if (typeof document === "undefined") return;
    const dark = pref === "dark" || (pref === "system" && systemIsDark());
    document.documentElement.classList.toggle("dark", dark);
    document.documentElement.style.colorScheme = dark ? "dark" : "light";
}

let mediaQuery: MediaQueryList | null = null;
let onSystemChange: (() => void) | null = null;

// "system" must keep tracking the OS after the initial apply — otherwise a
// live OS setting change (e.g. macOS auto-dark at sunset) wouldn't move the
// app. Subscribe on first use, unsubscribe when the preference stops being
// system-driven.
function trackSystem(pref: ThemePreference): void {
    if (pref !== "system" || typeof matchMedia === "undefined") {
        if (onSystemChange) {
            mediaQuery?.removeEventListener("change", onSystemChange);
            onSystemChange = null;
            mediaQuery = null;
        }
        return;
    }
    if (onSystemChange) return;
    mediaQuery = matchMedia("(prefers-color-scheme: dark)");
    onSystemChange = () => applyTheme(get(theme));
    mediaQuery.addEventListener("change", onSystemChange);
}

// Electron's static server binds to a random port every launch
// (static-server.ts's listen(0, ...)), so the page origin changes on every
// relaunch — localStorage (origin-scoped) never survives that. Electron
// persists the preference to a userData file instead, via IPC; see
// ElectronApp's theme-store.ts.
async function readStoredTheme(): Promise<ThemePreference | null> {
    if (isElectron()) {
        try {
            const stored = await window.electronApp!.theme.get();
            return stored === "light" || stored === "dark" || stored === "system" ? stored : null;
        } catch {
            return null;
        }
    }
    if (typeof localStorage === "undefined") return null;
    try {
        const stored = localStorage.getItem(STORAGE_KEY);
        return stored === "light" || stored === "dark" || stored === "system" ? stored : null;
    } catch {
        return null;
    }
}

async function writeStoredTheme(pref: ThemePreference): Promise<void> {
    if (isElectron()) {
        try {
            await window.electronApp!.theme.set(pref);
        } catch {
            // Losing the preference just means it's re-detected next load, not fatal.
        }
        return;
    }
    if (typeof localStorage === "undefined") return;
    try {
        localStorage.setItem(STORAGE_KEY, pref);
    } catch {
        // Storage can be unavailable (private browsing, quota) — losing the
        // preference just means it's re-detected next load, not fatal.
    }
}

// Called once from +layout.svelte's onMount, alongside initI18n. By then
// app.html's inline script has already applied the stored theme pre-paint,
// so this mainly (re)establishes the store value and the system listener.
export async function initTheme(): Promise<void> {
    const stored = await readStoredTheme();
    setTheme(stored ?? "system", { persist: false });
}

// Applies immediately (no reload needed — the class/color-scheme toggle is
// cheap) and persists unless told not to.
export function setTheme(pref: ThemePreference, opts: { persist?: boolean } = {}): void {
    theme.set(pref);
    applyTheme(pref);
    trackSystem(pref);
    if (opts.persist !== false) void writeStoredTheme(pref);
}
