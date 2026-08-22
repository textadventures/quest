import { writable } from "svelte/store";
import { isElectron } from "./runtime";

// Whether ScriptEditor instances default to raw code view instead of the
// visual block editor. Changing it also flips every ScriptEditor currently
// mounted (see ScriptEditor.svelte's effect on this store) — a setting the
// user opts into as a standing preference, not a per-panel toggle.
export const defaultCodeView = writable<boolean>(false);

const STORAGE_KEY = "questviva-default-code-view";

// Electron's static server binds to a random port every launch (see
// theme-store.ts), so localStorage (origin-scoped) never survives that —
// persist to a userData file via IPC instead there.
async function readStoredDefaultCodeView(): Promise<boolean | null> {
    if (isElectron()) {
        try {
            return await window.electronApp!.defaultCodeView.get();
        } catch {
            return null;
        }
    }
    if (typeof localStorage === "undefined") return null;
    try {
        const stored = localStorage.getItem(STORAGE_KEY);
        return stored === null ? null : stored === "true";
    } catch {
        return null;
    }
}

async function writeStoredDefaultCodeView(value: boolean): Promise<void> {
    if (isElectron()) {
        try {
            await window.electronApp!.defaultCodeView.set(value);
        } catch {
            // Losing the preference just means it's re-detected next load, not fatal.
        }
        return;
    }
    if (typeof localStorage === "undefined") return;
    try {
        localStorage.setItem(STORAGE_KEY, String(value));
    } catch {
        // Storage can be unavailable (private browsing, quota) — losing the
        // preference just means it's re-detected next load, not fatal.
    }
}

// Called once from +layout.svelte's onMount, alongside initTheme/initI18n.
export async function initDefaultCodeView(): Promise<void> {
    const stored = await readStoredDefaultCodeView();
    if (stored !== null) defaultCodeView.set(stored);
}

export function setDefaultCodeView(value: boolean): void {
    defaultCodeView.set(value);
    void writeStoredDefaultCodeView(value);
}
