<script lang="ts">
    import { onMount, onDestroy } from "svelte";
    import { base } from "$app/paths";
    import { goto } from "$app/navigation";
    import { page } from "$app/state";
    import { fetchCatalog, type CatalogCategory, type UpdateInfo } from "$lib/home-catalog";
    import { isElectron } from "$lib/runtime";
    import { listRecentGames, removeRecentGame, type RecentGame } from "$lib/filesystem/electron-adapter";
    import { playElectronFile, pickAndPlayElectronFile, closeLocalPlayChannel } from "$lib/filesystem/local-play";
    import { pickFile } from "$lib/filesystem/file-picker";
    import { listRecentCatalogPlays, removeRecentCatalogPlay, type RecentCatalogPlay } from "$lib/recent-catalog-plays";
    import UpdateBanner from "$components/UpdateBanner.svelte";
    import GameCard from "$components/GameCard.svelte";
    import RecentGameCard from "$components/RecentGameCard.svelte";
    import LocalFileRecentCard from "$components/LocalFileRecentCard.svelte";
    import ChevronDown from "@lucide/svelte/icons/chevron-down";

    const isElectronApp = isElectron();

    let categories = $state<CatalogCategory[] | null>(null);
    let update = $state<UpdateInfo | null>(null);
    let error = $state(false);
    let loading = $state(true);

    async function load() {
        loading = true;
        error = false;
        try {
            const result = await fetchCatalog();
            categories = result.categories;
            update = result.update;
        } catch {
            error = true;
        } finally {
            loading = false;
        }
    }

    // Recently played — merged from two sources: catalog games (tracked client-side in
    // recent-catalog-plays.ts, works everywhere) and local files (Electron only — tracked
    // via electron-adapter.ts's "play"-kind RecentGame list; there's no persistent file
    // handle across sessions in a plain browser to track this with). Both render as cards
    // in the same grid (RecentGameCard / LocalFileRecentCard) — catalog entries first,
    // local files after, not interleaved by timestamp (the two lists come from unrelated
    // sources with no shared ordering guarantee worth relying on).
    let recentCatalogPlays = $state<RecentCatalogPlay[]>([]);
    let recentLocalPlays = $state<RecentGame[]>([]);

    function refreshRecentCatalogPlays() {
        recentCatalogPlays = listRecentCatalogPlays();
    }

    async function refreshRecentLocalPlays() {
        if (!isElectronApp) return;
        recentLocalPlays = await listRecentGames("play");
    }

    function handleRemoveRecentCatalog(id: string) {
        removeRecentCatalogPlay(id);
        refreshRecentCatalogPlays();
    }

    async function handleRemoveRecentLocal(game: RecentGame) {
        await removeRecentGame(game.dirPath, game.filename, "play");
        await refreshRecentLocalPlays();
    }

    // Electron: a single click launches the game — no second "Start" click needed,
    // because the player window is created by the *main* process (see ipc/player.ts's
    // player:openWindow), not by this renderer's own window.open(). Renderer-driven
    // window.open() is what forces the browser build's two-click flow below (a native
    // file dialog and a script-driven window.open() can't share one click's activation
    // grant — see handleBrowserStart); main-process window creation isn't subject to
    // that at all, so there's no separate destination page needed here either way.
    let electronOpenBusy = $state(false);
    let electronOpenError = $state<string | null>(null);

    async function handleOpenLocal() {
        electronOpenError = null;
        electronOpenBusy = true;
        try {
            await pickAndPlayElectronFile(refreshRecentLocalPlays);
        } catch (err) {
            electronOpenError = String(err);
        } finally {
            electronOpenBusy = false;
        }
    }

    async function playLocalRecent(game: RecentGame) {
        electronOpenError = null;
        electronOpenBusy = true;
        try {
            await playElectronFile(game.dirPath, game.filename, refreshRecentLocalPlays);
        } catch (err) {
            electronOpenError = String(err);
        } finally {
            electronOpenBusy = false;
        }
    }

    // Electron: a file-association open of a play-kind file (.quest/.asl/.cas) lands
    // here as a query string — either baked into this window's initial URL for a cold
    // start (see ElectronApp's main.ts, initialUrlPath) or via a goto() from
    // +layout.svelte's onOpenPlayFile listener once this page is already mounted. Same
    // nonce-guarded pattern as open/+page.svelte's action=open-recent effect, so a
    // repeat firing on the same file (nonce unchanged) doesn't relaunch the player.
    // There's no user gesture here to attach a picker button to, so this has to be a
    // effect rather than a click handler — but it needs no page of its own, root is
    // always mounted when Electron is running (PUBLIC_SHOW_HOME is always true there).
    let handledPlayNonce = "";
    $effect(() => {
        if (!isElectronApp) return;
        const params = page.url.searchParams;
        if (params.get("action") !== "play-file") return;
        const nonce = params.get("t") ?? "";
        if (nonce === handledPlayNonce) return;
        const dirPath = params.get("dir");
        const filename = params.get("file");
        if (!dirPath || !filename) return;
        handledPlayNonce = nonce;
        electronOpenError = null;
        electronOpenBusy = true;
        void playElectronFile(dirPath, filename, refreshRecentLocalPlays)
            .catch((err) => { electronOpenError = String(err); })
            .finally(() => { electronOpenBusy = false; });
    });

    // ── Browser build only (isElectronApp false) ────────────────────────────
    // Two deliberate clicks, not one click-through-then-another: picking the file and
    // starting the game are each their own genuine user gesture, so each gets its own
    // fresh browser activation — a single click can't do both (see handleBrowserStart)
    // because a native file dialog and a script-driven window.open() fight over the
    // same click's single-use activation grant.
    let pickedFile = $state<File | null>(null);
    let pickedBytes = $state<Uint8Array | null>(null);
    let pickError = $state<string | null>(null);
    let starting = $state(false);
    let startError = $state<string | null>(null);

    // Kept across handoffs (not just a local var) so a new one can close the previous —
    // otherwise a stale channel would *also* answer a new window's 'ready' broadcast
    // (with the wrong bytes), and would go on answering a refresh of the now-superseded
    // old window. Independent of local-play.ts's own Electron-only channel singleton —
    // the browser build never touches that module.
    let browserPlayChannel: BroadcastChannel | null = null;

    async function handlePickFile() {
        pickError = null;
        startError = null;
        const file = await pickFile(".quest,.aslx,.asl,.cas");
        if (!file) return;
        try {
            pickedBytes = new Uint8Array(await file.arrayBuffer());
            pickedFile = file;
        } catch (err) {
            pickError = String(err);
        }
    }

    function handleClearPicked() {
        pickedFile = null;
        pickedBytes = null;
        pickError = null;
        startError = null;
    }

    // window.open() must be the very first thing this does — it's what spends this
    // click's activation, and awaiting anything beforehand (there's nothing to await
    // here; the bytes are already read in handlePickFile) would let the popup blocker
    // silently no-op it. Hands the bytes to the new tab over a BroadcastChannel — see
    // wasm-player.js's `source=local` boot branch. No resource-request handling on this
    // path: a raw picked File has no directory to resolve sibling assets against (unlike
    // the Electron path above), so this only really supports self-contained .quest packages.
    function handleBrowserStart() {
        if (!pickedFile || !pickedBytes) return;
        startError = null;

        browserPlayChannel?.close();

        const popup = window.open(`${base}/player/?source=local`, "_blank");
        if (!popup) {
            startError = "Please allow pop-ups for this site to play the game.";
            return;
        }

        starting = true;
        const bytes = pickedBytes;
        const filename = pickedFile.name;
        const bc = new BroadcastChannel("quest-play-local");
        browserPlayChannel = bc;
        // Deliberately left open (not closed after the first message) — WasmPlayer
        // re-broadcasts 'ready' on every load of that tab, including a plain refresh,
        // so this needs to keep answering for as long as this page is still open,
        // exactly like the never-closed editor-preview channel it mirrors.
        bc.onmessage = ({ data }) => {
            if (data.type === "ready") {
                bc.postMessage({ type: "game", bytes, filename });
                starting = false;
                handleClearPicked();
            }
        };
    }

    onMount(() => {
        void load();
        refreshRecentCatalogPlays();
        void refreshRecentLocalPlays();
    });

    // PlayCatalog.svelte unmounts on every navigation away from "/" (e.g. to a game's
    // /play/{id} details page, or to the Create tab), unlike the always-mounted root
    // layout, so channel cleanup here is load-bearing, not just defensive.
    onDestroy(() => {
        browserPlayChannel?.close();
        closeLocalPlayChannel();
    });

    let searchQuery = $state("");
    function handleSearch(e: SubmitEvent) {
        e.preventDefault();
        const q = searchQuery.trim();
        if (!q) return;
        void goto(`${base}/play/search?q=${encodeURIComponent(q)}`);
    }

    // Tag categories only (curated sections like Latest Games have slug ===
    // null — see CatalogCategory) — already in the same descending-game-count
    // order fetchCatalog's categories come back in, so the dropdown matches
    // the page's own "See all" ordering below.
    const tagCategories = $derived((categories ?? []).filter((c) => c.slug != null));

    function handleCategorySelect(e: Event) {
        const slug = (e.currentTarget as HTMLSelectElement).value;
        if (!slug) return;
        void goto(`${base}/play/category/${slug}`);
    }
</script>

<!-- Always dark (see +layout.svelte) — surface-950/400/800 are the fixed
     dark-side members of Skeleton's paired tokens, not auto-switching ones,
     since the OS could be in light mode regardless. The background lives on
     this outer, unconstrained-width div — max-w-5xl below only centers the
     content column, so it must not also carry the background, or anything
     wider than 5xl shows the page's real (light-mode) background down the
     sides instead of dark. -->
<div class="min-h-svh bg-surface-950 text-surface-100">
    {#if isElectronApp && update}
        <UpdateBanner {update} />
    {/if}
    <div class="flex flex-col gap-8 w-full max-w-5xl mx-auto p-8">
        <div class="flex flex-wrap gap-2 items-stretch justify-center w-full max-w-2xl mx-auto">
            <form class="flex gap-2 flex-1 min-w-[240px]" onsubmit={handleSearch}>
                <input
                    type="search"
                    bind:value={searchQuery}
                    placeholder="Search games…"
                    title="Try category:puzzle, language:de, platform:quest-gamebook"
                    class="input flex-1 bg-surface-900 border-surface-700 text-surface-100 placeholder:text-surface-500"
                />
                <button type="submit" class="btn preset-outlined-primary-500">Search</button>
            </form>
            {#if tagCategories.length > 0}
                <div class="relative">
                    <select
                        class="input appearance-none pr-8 h-full bg-surface-900 border-surface-700 text-surface-100"
                        onchange={handleCategorySelect}
                    >
                        <option value="">Browse category…</option>
                        {#each tagCategories as category (category.slug)}
                            <option value={category.slug}>{category.title}</option>
                        {/each}
                    </select>
                    <ChevronDown size={14} class="pointer-events-none absolute right-2.5 top-1/2 -translate-y-1/2 text-surface-400" />
                </div>
            {/if}
            {#if isElectronApp}
                <button
                    type="button"
                    class="btn preset-outlined-surface-500 whitespace-nowrap"
                    onclick={handleOpenLocal}
                    disabled={electronOpenBusy}
                >
                    {electronOpenBusy ? "Opening…" : "Open a game file…"}
                </button>
            {:else if !pickedFile}
                <button type="button" class="btn preset-outlined-surface-500 whitespace-nowrap" onclick={handlePickFile}>
                    Open a game file&hellip;
                </button>
            {:else}
                <div class="flex items-center gap-2">
                    <span class="text-sm text-surface-300 truncate max-w-[20ch]">{pickedFile.name}</span>
                    <button type="button" class="btn btn-sm preset-outlined-surface-500" onclick={handleClearPicked} disabled={starting}>
                        Change
                    </button>
                    <button type="button" class="btn preset-filled-primary-500" onclick={handleBrowserStart} disabled={starting}>
                        {starting ? "Starting…" : "Start ▶"}
                    </button>
                </div>
            {/if}
        </div>

        {#if electronOpenError}
            <p class="text-error-500 text-sm text-center">{electronOpenError}</p>
        {/if}
        {#if !isElectronApp && pickError}
            <p class="text-error-500 text-sm text-center">{pickError}</p>
        {/if}
        {#if !isElectronApp && startError}
            <p class="text-error-500 text-sm text-center">{startError}</p>
        {/if}

        {#if recentCatalogPlays.length > 0 || (isElectronApp && recentLocalPlays.length > 0)}
            <section>
                <h2 class="text-lg font-semibold mb-3">Recently played</h2>
                <div class="grid grid-cols-[repeat(auto-fill,minmax(150px,1fr))] gap-4">
                    {#each recentCatalogPlays as game (game.id)}
                        <RecentGameCard {game} onremove={() => handleRemoveRecentCatalog(game.id)} />
                    {/each}
                    {#if isElectronApp}
                        {#each recentLocalPlays as game (game.dirPath + "/" + game.filename)}
                            <LocalFileRecentCard {game} onplay={() => playLocalRecent(game)} onremove={() => handleRemoveRecentLocal(game)} />
                        {/each}
                    {/if}
                </div>
            </section>
        {/if}

        {#if loading}
            <div class="flex flex-col items-center gap-3 py-12">
                <div class="size-10 rounded-full border-4 border-surface-800 border-t-primary-500 animate-spin"></div>
                <p class="text-surface-400 text-sm">Loading games&hellip;</p>
            </div>
        {:else if error}
            <div class="flex flex-col items-center gap-3 py-12 text-center">
                <p class="text-error-500 text-sm">Couldn't load the games list.</p>
                <button type="button" class="btn preset-tonal" onclick={load}>Try again</button>
            </div>
        {:else if categories}
            {#each categories as category (category.title)}
                <section>
                    <div class="flex items-baseline justify-between mb-3">
                        <h2 class="text-lg font-semibold">{category.title}</h2>
                        {#if category.slug}
                            <a href="{base}/play/category/{category.slug}" class="anchor text-sm">See all &rarr;</a>
                        {/if}
                    </div>
                    <div class="grid grid-cols-[repeat(auto-fill,minmax(150px,1fr))] gap-4">
                        {#each category.games as game (game.id)}
                            <GameCard {game} />
                        {/each}
                    </div>
                </section>
            {/each}
        {/if}
    </div>
</div>
