<script lang="ts">
    import "../app.css";
    import type { Snippet } from "svelte";
    import { onMount } from "svelte";
    import { get } from "svelte/store";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { page } from "$app/state";
    import { PUBLIC_WEBEDITOR_VERSION, PUBLIC_SHOW_HOME } from "$env/static/public";
    import { isLoaded, saveGame, saveGameAs } from "$lib/editor-store";
    import { isElectron } from "$lib/runtime";
    import HomeHeader from "$components/HomeHeader.svelte";
    import HomeTabs from "$components/HomeTabs.svelte";

    let { children }: { children: Snippet } = $props();

    const showHome = PUBLIC_SHOW_HOME === "true";
    const rootPath = base || "/";

    // Root doubles up as both the Play tab (nothing loaded) and the editor
    // canvas (a game loaded) — only that route needs the isLoaded check to
    // pick between them. /open and /play/[id] never show the editor canvas,
    // so the tab bar always belongs there regardless of isLoaded — which
    // matters because isLoaded has no reason to reset to false just because
    // the user navigated (e.g. browser back) away from the loaded editor.
    const showTabs = $derived(showHome && !(page.url.pathname === rootPath && $isLoaded));

    // Play (and its game-detail pages) are always dark, matching the look the
    // standalone Home page had before this was folded into WebEditor — Create
    // (/open) keeps following the editor's own light/dark system preference,
    // since forcing that too would mean redoing its markup (also used as-is
    // by textadventures.co.uk's plain editor-open flow) to match.
    const isPlayContext = $derived(page.url.pathname === rootPath || page.url.pathname.startsWith(`${base}/play/`));

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
                    // Edits autosave continuously, so switching games just needs the
                    // previous one flushed first — nothing is discarded, unlike the
                    // old confirm()-to-discard prompt this replaced.
                    void (async () => {
                        if (get(isLoaded)) await saveGame();
                        // goto() to the exact URL already showing is a no-op in SvelteKit —
                        // no navigation event fires, so nothing happens when the menu
                        // action is triggered while already sitting on /open. A per-click
                        // nonce guarantees a real navigation every time; routes/open/+page.svelte
                        // reacts to it via $app/state's page (not onMount), which re-runs on
                        // every navigation, remount or not.
                        const query = action === "open-file" ? `?action=open&t=${Date.now()}` : "";
                        void goto(`${base}/open${query}`);
                    })();
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
        // flush-then-navigate pattern as open-folder above, just carrying a
        // specific game to load; routes/open/+page.svelte's $effect reads
        // dir/file/t off the query string and does the actual load.
        const unsubscribeRecent = window.electronApp!.menu.onOpenRecent((game) => {
            void (async () => {
                if (get(isLoaded)) await saveGame();
                const query = `?action=open-recent&dir=${encodeURIComponent(game.dirPath)}&file=${encodeURIComponent(game.filename)}&t=${Date.now()}`;
                void goto(`${base}/open${query}`);
            })();
        });
        return () => {
            unsubscribeAction();
            unsubscribeRecent();
        };
    });
</script>

{#if showTabs}
    <div class={isPlayContext ? "bg-surface-950" : ""}>
        <HomeHeader forceDark={isPlayContext} />
        <HomeTabs forceDark={isPlayContext} />
    </div>
{/if}
{@render children()}
