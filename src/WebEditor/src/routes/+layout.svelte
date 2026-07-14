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
                    void goto(action === "open-folder" ? `${base}/open?action=open` : `${base}/open`);
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
