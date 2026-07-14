<script lang="ts">
    import "../app.css";
    import type { Snippet } from "svelte";
    import { onMount } from "svelte";
    import { get } from "svelte/store";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { isDirty, isLoaded, saveGame, saveGameAs } from "$lib/editor-store";
    import { isElectron } from "$lib/runtime";

    let { children }: { children: Snippet } = $props();

    // Backs the desktop app's native File menu (see ElectronApp's main.ts /
    // preload.ts) — the menu itself can't touch editor-store, so it sends an
    // action here instead. Registered at the layout level (not routes/+page.svelte)
    // because it needs to fire — and navigate to /open — from anywhere, including
    // when a game is already loaded and the toolbar has no "open a different
    // project" button of its own.
    onMount(() => {
        if (!isElectron()) return;
        return window.electronApp!.menu.onAction((action) => {
            switch (action) {
                case "new-game":
                case "open-folder": {
                    if (get(isDirty) && !confirm("You have unsaved changes. Discard them and continue?")) return;
                    // goto() to the exact URL already showing is a no-op in SvelteKit —
                    // no navigation event fires, so nothing happens when the menu
                    // action is triggered while already sitting on /open. A per-click
                    // nonce guarantees a real navigation every time; routes/open/+page.svelte
                    // reacts to it via $app/state's page (not onMount), which re-runs on
                    // every navigation, remount or not.
                    const query = action === "open-folder" ? `?action=open&t=${Date.now()}` : "";
                    void goto(`${base}/open${query}`);
                    break;
                }
                case "save":
                    if (get(isLoaded)) void saveGame();
                    break;
                case "save-as":
                    if (get(isLoaded)) void saveGameAs();
                    break;
            }
        });
    });
</script>

{@render children()}
