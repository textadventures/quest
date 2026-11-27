<script lang="ts">
    import { onMount, onDestroy } from "svelte";
    import { base } from "#lib/base-path.js";
    import { goto } from "$app/navigation";
    import { page } from "$app/state";
    import { fetchCatalog, type CatalogCategory, type UpdateInfo } from "#lib/home-catalog.js";
    import { isElectron } from "#lib/runtime.js";
    import { listRecentGames, removeRecentGame, electronDroppedFilePath, FileNotFoundError, describeElectronError, type RecentGame } from "#lib/filesystem/electron-adapter.js";
    import { playElectronFile, pickAndPlayElectronFile, closeLocalPlayChannel } from "#lib/filesystem/local-play.js";
    import { pickFiles } from "#lib/filesystem/file-picker.js";
    import { splitPickedGameFiles } from "#lib/filesystem/picked-game-files.js";
    import { listRecentCatalogPlays, removeRecentCatalogPlay, type RecentCatalogPlay } from "#lib/recent-catalog-plays.svelte.js";
    import UpdateBanner from "#components/UpdateBanner.svelte";
    import GameCard from "#components/GameCard.svelte";
    import RecentGameCard from "#components/RecentGameCard.svelte";
    import LocalFileRecentCard from "#components/LocalFileRecentCard.svelte";
    import FolderOpen from "@lucide/svelte/icons/folder-open";
    import SearchIcon from "@lucide/svelte/icons/search";
    import { t, tPlural } from "#lib/i18n/index.js";

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
    // in the same grid (RecentGameCard / LocalFileRecentCard), interleaved by timestamp —
    // lastPlayed and lastOpened are both plain Date.now() epoch-ms, so they compare directly.
    let recentCatalogPlays = $state<RecentCatalogPlay[]>([]);
    let recentLocalPlays = $state<RecentGame[]>([]);

    type RecentEntry =
        | { kind: "catalog"; key: string; timestamp: number; game: RecentCatalogPlay }
        | { kind: "local"; key: string; timestamp: number; game: RecentGame };

    const recentEntries = $derived<RecentEntry[]>([
        ...recentCatalogPlays.map((game): RecentEntry => ({ kind: "catalog", key: game.id, timestamp: game.lastPlayed, game })),
        ...recentLocalPlays.map((game): RecentEntry => ({ kind: "local", key: game.dirPath + "/" + game.filename, timestamp: game.lastOpened, game })),
    ].sort((a, b) => b.timestamp - a.timestamp));

    async function refreshRecentCatalogPlays() {
        recentCatalogPlays = await listRecentCatalogPlays();
    }

    async function refreshRecentLocalPlays() {
        if (!isElectronApp) return;
        recentLocalPlays = await listRecentGames("play");
    }

    async function handleRemoveRecentCatalog(id: string) {
        await removeRecentCatalogPlay(id);
        await refreshRecentCatalogPlays();
    }

    async function handleRemoveRecentLocal(game: RecentGame) {
        if (isMissing(game)) clearElectronOpenError();
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
    // Set alongside electronOpenError when a Recently Played file has gone; its
    // card then shows that in place of the error message.
    let missingRecent = $state<RecentGame | null>(null);

    function showElectronOpenError(err: unknown) {
        if (err instanceof FileNotFoundError) {
            electronOpenError = t("common.fileNotFound", { filename: err.filename, folder: err.dirPath });
            missingRecent = recentLocalPlays.find((g) => g.dirPath === err.dirPath && g.filename === err.filename) ?? null;
        } else {
            electronOpenError = describeElectronError(err);
            missingRecent = null;
        }
    }

    // By path, not identity: the recent list can be re-read in between.
    function isMissing(game: RecentGame): boolean {
        return missingRecent?.dirPath === game.dirPath && missingRecent.filename === game.filename;
    }

    function clearElectronOpenError() {
        electronOpenError = null;
        missingRecent = null;
    }

    async function handleOpenLocal() {
        clearElectronOpenError();
        electronOpenBusy = true;
        try {
            await pickAndPlayElectronFile(refreshRecentLocalPlays);
        } catch (err) {
            showElectronOpenError(err);
        } finally {
            electronOpenBusy = false;
        }
    }

    async function playLocalRecent(game: RecentGame) {
        clearElectronOpenError();
        electronOpenBusy = true;
        try {
            await playElectronFile(game.dirPath, game.filename, refreshRecentLocalPlays);
        } catch (err) {
            showElectronOpenError(err);
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
    // always mounted when Electron is running.
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
        clearElectronOpenError();
        electronOpenBusy = true;
        void playElectronFile(dirPath, filename, refreshRecentLocalPlays)
            .catch(showElectronOpenError)
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
    // Library files picked along with an .aslx game (see picked-game-files.ts). Raw, not a deep
    // $state proxy, since it goes over a BroadcastChannel, which can't clone a proxy.
    let pickedAdjacentFiles = $state.raw<Record<string, Uint8Array>>({});
    const pickedLibraryCount = $derived(Object.keys(pickedAdjacentFiles).length);
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
        const files = await pickFiles(PLAY_EXTENSIONS.join(","));
        if (files.length) await acceptBrowserFiles(files);
    }

    async function acceptBrowserFiles(files: File[]) {
        pickError = null;
        startError = null;
        try {
            const picked = await splitPickedGameFiles(files);
            if (!picked) {
                pickError = t("playCatalog.noGameInFiles");
                return;
            }
            pickedBytes = picked.bytes;
            pickedAdjacentFiles = picked.adjacentFiles;
            pickedFile = picked.file;
        } catch (err) {
            pickError = String(err);
        }
    }

    function handleClearPicked() {
        pickedFile = null;
        pickedBytes = null;
        pickedAdjacentFiles = {};
        pickError = null;
        startError = null;
    }

    // window.open() must be the very first thing this does — it's what spends this
    // click's activation, and awaiting anything beforehand (there's nothing to await
    // here; the bytes are already read in handlePickFile) would let the popup blocker
    // silently no-op it. Hands the bytes to the new tab over a BroadcastChannel — see
    // wasm-player.js's `source=local` boot branch — with any library files picked along with
    // the game. No resource-request handling on this path: a raw picked File has no directory
    // to resolve sibling assets such as images against (unlike the Electron path above).
    function handleBrowserStart() {
        if (!pickedFile || !pickedBytes) return;
        startError = null;

        browserPlayChannel?.close();

        const popup = window.open(`${base}/player/?source=local`, "_blank");
        if (!popup) {
            startError = t("playCatalog.popupBlockedError");
            return;
        }

        starting = true;
        const bytes = pickedBytes;
        const filename = pickedFile.name;
        const adjacentFiles = pickedAdjacentFiles;
        const bc = new BroadcastChannel("quest-play-local");
        browserPlayChannel = bc;
        // Deliberately left open (not closed after the first message) — WasmPlayer
        // re-broadcasts 'ready' on every load of that tab, including a plain refresh,
        // so this needs to keep answering for as long as this page is still open,
        // exactly like the never-closed editor-preview channel it mirrors.
        bc.onmessage = ({ data }) => {
            if (data.type === "ready") {
                bc.postMessage({ type: "game", bytes, filename, adjacentFiles, picked: true });
                starting = false;
                handleClearPicked();
            }
        };
    }

    onMount(() => {
        void load();
        void refreshRecentCatalogPlays();
        void refreshRecentLocalPlays();
    });

    // PlayCatalog.svelte unmounts on every navigation away from "/" (e.g. to a game's
    // /play/{id} details page, or to the Create tab), unlike the always-mounted root
    // layout, so channel cleanup here is load-bearing, not just defensive.
    onDestroy(() => {
        browserPlayChannel?.close();
        closeLocalPlayChannel();
    });

    // ── Drag and drop (both builds) ─────────────────────────────────────────
    // A game file dropped anywhere on the page. Electron plays it straight away,
    // like the picker; the browser build treats it as picked and still needs the
    // Start click, since a drop doesn't grant the activation window.open() needs.
    const PLAY_EXTENSIONS = [".quest", ".aslx", ".asl", ".cas"];

    function isPlayableFilename(name: string): boolean {
        const lower = name.toLowerCase();
        return PLAY_EXTENSIONS.some((ext) => lower.endsWith(ext));
    }

    // dragenter/dragleave fire for every child crossed, so count them rather
    // than toggling, or the overlay flickers as the pointer moves.
    let dragDepth = $state(0);
    const dragActive = $derived(dragDepth > 0);

    function isFileDrag(e: DragEvent): boolean {
        return e.dataTransfer?.types.includes("Files") ?? false;
    }

    function handleDragEnter(e: DragEvent) {
        if (!isFileDrag(e)) return;
        e.preventDefault();
        dragDepth++;
    }

    function handleDragOver(e: DragEvent) {
        if (!isFileDrag(e)) return;
        // Without this the drop event never fires, and Electron would navigate
        // the window to the dropped file instead.
        e.preventDefault();
        if (e.dataTransfer) e.dataTransfer.dropEffect = "copy";
    }

    function handleDragLeave(e: DragEvent) {
        if (!isFileDrag(e)) return;
        dragDepth = Math.max(0, dragDepth - 1);
    }

    async function handleDrop(e: DragEvent) {
        if (!isFileDrag(e)) return;
        e.preventDefault();
        dragDepth = 0;
        const files = [...(e.dataTransfer?.files ?? [])];
        if (!files.length) return;
        const unsupported = files.find((f) => !isPlayableFilename(f.name));
        if (unsupported) {
            const message = t("playCatalog.unsupportedFile", { filename: unsupported.name });
            if (isElectronApp) { missingRecent = null; electronOpenError = message; } else { pickError = message; }
            return;
        }
        if (!isElectronApp) {
            await acceptBrowserFiles(files);
            return;
        }
        // Electron reads a game's libraries from its folder itself, so only the game is needed
        // from a drop of a game and its libraries.
        const file = files.length === 1 ? files[0] : (await splitPickedGameFiles(files))?.file ?? files[0];
        const dropped = electronDroppedFilePath(file);
        if (!dropped) {
            missingRecent = null;
            electronOpenError = t("playCatalog.unsupportedFile", { filename: file.name });
            return;
        }
        clearElectronOpenError();
        electronOpenBusy = true;
        try {
            await playElectronFile(dropped.dirPath, dropped.filename, refreshRecentLocalPlays);
        } catch (err) {
            showElectronOpenError(err);
        } finally {
            electronOpenBusy = false;
        }
    }

    let searchQuery = $state("");
    function handleSearch(e: SubmitEvent) {
        e.preventDefault();
        const q = searchQuery.trim();
        if (!q) return;
        void goto(`${base}/play/search?q=${encodeURIComponent(q)}`);
    }

    // Tag categories only (curated sections like Latest Games have slug ===
    // null — see CatalogCategory), in the order fetchCatalog returns them, so
    // the chips match the order of the sections further down the page.
    const tagCategories = $derived((categories ?? []).filter((c): c is CatalogCategory & { slug: string } => c.slug != null));
</script>

<!-- Always dark (see +layout.svelte) — surface-950/400/800 are the fixed
     dark-side members of Skeleton's paired tokens, not auto-switching ones,
     since the OS could be in light mode regardless. The play-dark class
     scopes app.css's fixed-shade form-control overrides. The background lives
     on this outer, unconstrained-width div — max-w-5xl below only centers the
     content column, so it must not also carry the background, or anything
     wider than 5xl shows the page's real (light-mode) background down the
     sides instead of dark. -->
<!-- The drag handlers are on the whole page so a game file can be dropped
     anywhere; there's no keyboard equivalent to provide, since the Open tile
     and button do the same thing. -->
<!-- svelte-ignore a11y_no_static_element_interactions -->
<div
    class="play-dark min-h-svh bg-surface-950 text-surface-100"
    ondragenter={handleDragEnter}
    ondragover={handleDragOver}
    ondragleave={handleDragLeave}
    ondrop={handleDrop}
>
    <!-- Not in the Microsoft Store build: the Store updates the app itself,
         and the banner's link is the GitHub installer, a different install. -->
    {#if isElectronApp && update && !window.electronApp?.windowsStore}
        <UpdateBanner {update} />
    {/if}

    {#if dragActive}
        <div class="pointer-events-none fixed inset-3 z-50 flex items-center justify-center rounded-2xl border-2 border-dashed border-primary-500 bg-surface-950/85">
            <div class="flex flex-col items-center gap-3 text-surface-100">
                <FolderOpen size={40} class="text-primary-400" />
                <p class="text-lg font-semibold">{t("playCatalog.dropToPlay")}</p>
            </div>
        </div>
    {/if}

    <div class="flex flex-col gap-12 w-full max-w-5xl mx-auto px-4 py-8 sm:px-6">
        {#if recentEntries.length > 0}
            <section aria-labelledby="recently-played-heading">
                <h2 id="recently-played-heading" class="text-xl font-semibold mb-4">{t("playCatalog.recentlyPlayed")}</h2>
                {@render openErrors()}
                <!-- One scrolling row rather than a wrapping grid, so a long
                     history never pushes Discover off the first screen. -->
                <div class="flex gap-4 overflow-x-auto pb-2">
                    <div class="w-[150px] shrink-0 flex">
                        {@render openFileTile()}
                    </div>
                    {#each recentEntries as entry (entry.key)}
                        <div class="w-[150px] shrink-0">
                            {#if entry.kind === "catalog"}
                                <RecentGameCard game={entry.game} onremove={() => handleRemoveRecentCatalog(entry.game.id)} />
                            {:else}
                                <LocalFileRecentCard
                                    game={entry.game}
                                    missing={isMissing(entry.game)}
                                    onplay={() => playLocalRecent(entry.game)}
                                    onremove={() => handleRemoveRecentLocal(entry.game)}
                                />
                            {/if}
                        </div>
                    {/each}
                </div>
            </section>
        {/if}

        <section aria-labelledby="discover-heading" class="flex flex-col gap-5 rounded-2xl border border-surface-800 bg-surface-900 p-5 sm:p-7">
            <div class="flex flex-wrap items-baseline justify-between gap-2">
                <h2 id="discover-heading" class="text-xl font-semibold">{t("playCatalog.discoverTitle")}</h2>
                <a href="https://textadventures.co.uk" target="_blank" rel="noopener" class="anchor text-sm">{t("playCatalog.discoverSource")}</a>
            </div>

            <form
                role="search"
                class="flex items-center gap-2 h-12 pl-4 pr-1.5 rounded-xl border border-surface-700 bg-surface-950 focus-within:border-primary-500 transition-colors"
                onsubmit={handleSearch}
            >
                <SearchIcon size={18} class="shrink-0 text-surface-400" aria-hidden="true" />
                <input
                    type="search"
                    bind:value={searchQuery}
                    aria-label={t("search.searchButton")}
                    placeholder={t("search.searchPlaceholder")}
                    title={t("search.searchHint")}
                    class="flex-1 min-w-0 h-full bg-transparent border-0 outline-none text-base text-surface-100 placeholder:text-surface-500"
                />
                <button type="submit" class="btn preset-filled-primary-500 h-9 shrink-0">{t("search.searchButton")}</button>
            </form>

            {#if tagCategories.length > 0}
                <nav aria-label={t("playCatalog.browseCategories")} class="flex flex-wrap gap-2">
                    {#each tagCategories as category (category.slug)}
                        <a
                            href="{base}/play/category/{category.slug}"
                            class="inline-flex items-center h-9 px-3.5 rounded-full border border-surface-700 text-sm text-surface-200 hover:border-surface-500 hover:text-surface-50 transition-colors"
                        >{category.title}</a>
                    {/each}
                </nav>
            {/if}
        </section>

        {#if recentEntries.length === 0}
            <!-- Nothing played yet: opening a file shrinks to one line under
                 Discover rather than leading the page. -->
            <div class="flex flex-col gap-3">
                <div data-testid="open-file-bar" class="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-dashed border-surface-700 px-5 py-3.5">
                    <p class="flex items-center gap-3 text-sm text-surface-300">
                        <FolderOpen size={20} class="shrink-0 text-primary-400" aria-hidden="true" />
                        {t("playCatalog.openFileHint")}
                    </p>
                    {@render openFileControls()}
                </div>
                {@render openErrors()}
            </div>
        {/if}

        {#if loading}
            <div class="flex flex-col items-center gap-3 py-12">
                <div class="size-10 rounded-full border-4 border-surface-800 border-t-primary-500 animate-spin"></div>
                <p class="text-surface-300 text-sm">{t("playCatalog.loadingGames")}</p>
            </div>
        {:else if error}
            <div class="flex flex-col items-center gap-3 py-12 text-center">
                <p class="text-error-300 text-sm">{t("playCatalog.loadError")}</p>
                <button type="button" class="btn preset-tonal" onclick={load}>{t("search.tryAgain")}</button>
            </div>
        {:else if categories}
            {#each categories as category (category.title)}
                <section>
                    <div class="flex items-baseline justify-between mb-3">
                        <h2 class="text-lg font-semibold">{category.title}</h2>
                        {#if category.slug}
                            <a href="{base}/play/category/{category.slug}" class="anchor text-sm">{t("playCatalog.seeAll")}</a>
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

<!-- Errors from opening a local file. A missing Recently Played file is shown
     on its own card instead (see LocalFileRecentCard's missing state). -->
{#snippet openErrors()}
    {#if electronOpenError && !missingRecent}
        <p class="text-error-300 text-sm mb-3" role="alert">{electronOpenError}</p>
    {/if}
    {#if !isElectronApp && pickError}
        <p class="text-error-300 text-sm mb-3" role="alert">{pickError}</p>
    {/if}
    {#if !isElectronApp && startError}
        <p class="text-error-300 text-sm mb-3" role="alert">{startError}</p>
    {/if}
{/snippet}

<!-- First tile of the Recently Played row. Fills the row's height, so it
     matches the cards beside it and never changes size as its label does. -->
{#snippet openFileTile()}
    {#if !isElectronApp && pickedFile}
        <div class="flex flex-col items-center justify-center gap-3 w-full rounded-lg border-2 border-primary-500 p-3 text-center">
            <FolderOpen size={28} class="text-primary-400" aria-hidden="true" />
            <span class="text-sm font-semibold break-all line-clamp-3">{pickedFile.name}</span>
            {#if pickedLibraryCount > 0}
                <span class="text-xs text-surface-300">{tPlural("playCatalog.withLibraries", pickedLibraryCount)}</span>
            {/if}
            <button type="button" class="btn preset-filled-primary-500 w-full" onclick={handleBrowserStart} disabled={starting}>
                {starting ? t("playCatalog.starting") : t("playCatalog.startButton") + " ▶"}
            </button>
            <button type="button" class="btn btn-sm preset-tonal" onclick={handleClearPicked} disabled={starting}>
                {t("playCatalog.change")}
            </button>
        </div>
    {:else}
        <button
            type="button"
            class="flex flex-col items-center justify-center gap-3 w-full rounded-lg border-2 border-dashed border-surface-700 p-3 text-center text-surface-300 hover:border-primary-500 transition-colors"
            onclick={isElectronApp ? handleOpenLocal : handlePickFile}
            disabled={electronOpenBusy}
        >
            <span class="flex items-center justify-center size-12 rounded-full bg-primary-950 text-primary-300">
                <FolderOpen size={24} aria-hidden="true" />
            </span>
            <span class="text-sm font-semibold text-surface-50">
                {electronOpenBusy ? t("playCatalog.opening") : t("playCatalog.openGameFile")}
            </span>
            <span class="text-xs text-surface-300">{t("playCatalog.fileTypes")}</span>
            <span class="text-xs text-surface-300">{t("playCatalog.dropHint")}</span>
        </button>
    {/if}
{/snippet}

<!-- The same choices as the tile, as a row of buttons for the slim
     first-run bar. -->
{#snippet openFileControls()}
    {#if isElectronApp}
        <button type="button" class="btn preset-tonal min-w-44" onclick={handleOpenLocal} disabled={electronOpenBusy}>
            {electronOpenBusy ? t("playCatalog.opening") : t("playCatalog.openGameFile")}
        </button>
    {:else if !pickedFile}
        <button type="button" class="btn preset-tonal" onclick={handlePickFile}>
            {t("playCatalog.openGameFile")}
        </button>
    {:else}
        <div class="flex items-center gap-2">
            <span class="text-sm text-surface-300 truncate max-w-[20ch]">{pickedFile.name}</span>
            {#if pickedLibraryCount > 0}
                <span class="text-xs text-surface-300">{tPlural("playCatalog.withLibraries", pickedLibraryCount)}</span>
            {/if}
            <button type="button" class="btn btn-sm preset-tonal" onclick={handleClearPicked} disabled={starting}>
                {t("playCatalog.change")}
            </button>
            <button type="button" class="btn preset-filled-primary-500" onclick={handleBrowserStart} disabled={starting}>
                {starting ? t("playCatalog.starting") : t("playCatalog.startButton") + " ▶"}
            </button>
        </div>
    {/if}
{/snippet}
