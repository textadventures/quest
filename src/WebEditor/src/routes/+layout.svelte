<script lang="ts">
    import "../app.css";
    import type { Snippet } from "svelte";
    import { onMount } from "svelte";
    import { get } from "svelte/store";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { PUBLIC_WEBEDITOR_VERSION } from "$env/static/public";
    import { isDirty, isLoaded, saveGame, saveGameAs } from "$lib/editor-store";
    import { isElectron } from "$lib/runtime";

    let { children }: { children: Snippet } = $props();

    // Printed once on load, same style as WasmPlayer's console banner — the
    // header no longer shows the version, so this is the only place to find it.
    console.log(
        "%cQuest Viva Editor %c" + (PUBLIC_WEBEDITOR_VERSION || "dev") + "\n%chttps://questviva.com",
        "font-weight:700;font-size:14px;color:#0ea5e9",
        "font-weight:400;color:#64748b",
        "color:#64748b"
    );

    // Backs the desktop app's native File menu (see ElectronApp's main.ts /
    // preload.ts) — the menu itself can't touch editor-store, so it sends an
    // action here instead. Registered at the layout level (not routes/+page.svelte)
    // because it needs to fire — and navigate to /open — from anywhere, including
    // when a game is already loaded and the toolbar has no "open a different
    // project" button of its own.
    onMount(() => {
        if (!isElectron()) return;
        const unsubscribeAction = window.electronApp!.menu.onAction((action) => {
            switch (action) {
                case "new-game":
                case "open-file": {
                    if (get(isDirty) && !confirm("You have unsaved changes. Discard them and continue?")) return;
                    // goto() to the exact URL already showing is a no-op in SvelteKit —
                    // no navigation event fires, so nothing happens when the menu
                    // action is triggered while already sitting on /open. A per-click
                    // nonce guarantees a real navigation every time; routes/open/+page.svelte
                    // reacts to it via $app/state's page (not onMount), which re-runs on
                    // every navigation, remount or not.
                    const query = action === "open-file" ? `?action=open&t=${Date.now()}` : "";
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
        // Native "Open Recent" submenu (ElectronApp's main.ts) — same
        // dirty-check + nonce-navigation pattern as open-folder above, just
        // carrying a specific game to load; routes/open/+page.svelte's
        // $effect reads dir/file/t off the query string and does the actual load.
        const unsubscribeRecent = window.electronApp!.menu.onOpenRecent((game) => {
            if (get(isDirty) && !confirm("You have unsaved changes. Discard them and continue?")) return;
            const query = `?action=open-recent&dir=${encodeURIComponent(game.dirPath)}&file=${encodeURIComponent(game.filename)}&t=${Date.now()}`;
            void goto(`${base}/open${query}`);
        });
        return () => {
            unsubscribeAction();
            unsubscribeRecent();
        };
    });
</script>

{@render children()}
