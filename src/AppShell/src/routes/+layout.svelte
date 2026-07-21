<script lang="ts">
    import "../app.css";
    import type { Snippet } from "svelte";
    import { onMount } from "svelte";
    import { get } from "svelte/store";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { page } from "$app/state";
    import { PUBLIC_APPSHELL_VERSION, PUBLIC_SHOW_HOME } from "$env/static/public";
    import { isLoaded, saveGame, saveGameAs } from "$lib/editor-store";
    import { isElectron } from "$lib/runtime";
    import HomeHeader from "$components/HomeHeader.svelte";
    import HomeTabs from "$components/HomeTabs.svelte";

    let { children }: { children: Snippet } = $props();

    const showHome = PUBLIC_SHOW_HOME === "true";
    const rootPath = base || "/";

    // The editor lives at /edit, never at root or /open — so the tab bar's
    // visibility is a pure route check now, with no isLoaded involved. (It
    // used to also check isLoaded, back when root doubled as both the Play
    // tab and the editor canvas; that meant the tab bar and the page's own
    // rendering could disagree about which one was actually showing.)
    const showTabs = $derived(showHome && page.url.pathname !== `${base}/edit`);

    // Measured so pages that vertically center their content (e.g. /open)
    // can subtract the actual header+tabs height via the --home-bar-height
    // custom property below, instead of centering within the full 100svh
    // and landing too far down. bind:clientHeight re-measures on resize.
    let homeBarHeight = $state(0);
    $effect(() => {
        if (showTabs) document.documentElement.style.setProperty("--home-bar-height", `${homeBarHeight}px`);
        else document.documentElement.style.removeProperty("--home-bar-height");
    });

    // Play (and its game-detail pages) are always dark, matching the look the
    // standalone Home page had before this was folded into AppShell — Create
    // (/open) keeps following the editor's own light/dark system preference,
    // since forcing that too would mean redoing its markup (also used as-is
    // by textadventures.co.uk's plain editor-open flow) to match.
    const isPlayContext = $derived(page.url.pathname === rootPath || page.url.pathname.startsWith(`${base}/play/`));

    // Printed once on load, same style as WasmPlayer's console banner — the
    // header no longer shows the version, so this is the only place to find it.
    console.log(
        "%cQuest Viva Editor %c" + (PUBLIC_APPSHELL_VERSION || "dev") + "\n%chttps://questviva.com",
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
        // A play-kind file association (.quest/.asl/.cas) opened while this
        // window already exists (ElectronApp's main.ts routeOpenedFile, via
        // preload.ts's player.onOpenPlayFile) — routed to root with the same
        // query shape a cold-start open uses (see main.ts's initialUrlPath),
        // so PlayCatalog.svelte only needs one $effect to handle both. Never
        // touches the editor's own loaded game, so no saveGame() flush first.
        const unsubscribePlayFile = window.electronApp!.player.onOpenPlayFile((file) => {
            const query = `?action=play-file&dir=${encodeURIComponent(file.dirPath)}&file=${encodeURIComponent(file.filename)}&t=${Date.now()}`;
            void goto(`${rootPath}${query}`);
        });
        return () => {
            unsubscribeAction();
            unsubscribeRecent();
            unsubscribePlayFile();
        };
    });
</script>

{#if showTabs}
    <div class={isPlayContext ? "bg-surface-950" : ""} bind:clientHeight={homeBarHeight}>
        <HomeHeader forceDark={isPlayContext} />
        <HomeTabs forceDark={isPlayContext} />
    </div>
{/if}
{@render children()}
