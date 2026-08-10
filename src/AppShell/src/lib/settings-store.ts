import { writable } from "svelte/store";

// Controls SettingsModal.svelte (mounted once in +layout.svelte). Kept
// separate from editor-store.ts because Settings must be reachable from
// Home/Play/Create routes, where no game/WASM bridge is loaded at all.
export const settingsModalOpen = writable(false);
