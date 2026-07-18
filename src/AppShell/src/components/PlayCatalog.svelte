<script lang="ts">
    import { onMount, onDestroy } from "svelte";
    import { base } from "$app/paths";
    import { fetchCatalog, type CatalogCategory } from "$lib/home-catalog";
    import { pickFile } from "$lib/filesystem/file-picker";
    import { isElectron } from "$lib/runtime";
    import { openElectronPlayFile, loadElectronFile } from "$lib/filesystem/electron-adapter";

    const isElectronApp = isElectron();

    let categories = $state<CatalogCategory[] | null>(null);
    let error = $state(false);
    let loading = $state(true);

    async function load() {
        loading = true;
        error = false;
        try {
            categories = await fetchCatalog();
        } catch {
            error = true;
        } finally {
            loading = false;
        }
    }

    onMount(load);

    function ratingStars(rating: number): string {
        const rounded = Math.max(0, Math.min(5, Math.round(rating)));
        return "★".repeat(rounded) + "☆".repeat(5 - rounded);
    }

    // Kept across handoffs (not just a local var) so a new one can close the
    // previous — otherwise a stale channel would *also* answer a new
    // window's 'ready' broadcast (with the wrong bytes), and would go on
    // answering a refresh of the now-superseded old window.
    let playChannel: BroadcastChannel | null = null;

    function blobToDataUrl(blob: Blob): Promise<string> {
        return new Promise((resolve) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result as string);
            reader.readAsDataURL(blob);
        });
    }

    let electronBusy = $state(false);
    let electronError = $state<string | null>(null);

    // Electron: a single click launches the game — no second "Start" click
    // needed, because the player window here is created by the *main*
    // process (see ipc/player.ts's player:openWindow), not by this
    // renderer's own window.open(). Renderer-driven window.open() is what
    // forces the browser build's two-click flow below (a native file dialog
    // and a script-driven window.open() can't share one click's activation
    // grant — see handleBrowserStart); main-process window creation isn't
    // subject to that at all. Also wires 'resource-request' (not just
    // 'ready') over the handoff channel, backed by a real ElectronFileAdapter
    // — the same mechanism editor-store.ts's previewInWasmPlayer uses — so a
    // loose .aslx that references sibling images/sounds in its folder keeps
    // working, not just self-contained .quest packages.
    async function handleElectronPlay() {
        electronError = null;
        const picked = await openElectronPlayFile();
        if (!picked) return;

        electronBusy = true;
        try {
            const { bytes, adapter } = await loadElectronFile(picked.dirPath, picked.filename);

            playChannel?.close();
            const bc = new BroadcastChannel("quest-play-local");
            playChannel = bc;
            bc.onmessage = async ({ data }) => {
                if (data.type === "ready") {
                    bc.postMessage({ type: "game", bytes, filename: picked.filename });
                } else if (data.type === "resource-request") {
                    const blob = await adapter.getAsset(data.name);
                    if (blob) {
                        const dataUrl = await blobToDataUrl(blob);
                        bc.postMessage({ type: "resource-response", id: data.id, dataUrl });
                    }
                }
            };

            const opened = await window.electronApp!.player.openWindow();
            if (!opened) {
                electronError = "Couldn't open the player window.";
                bc.close();
                playChannel = null;
            }
        } catch (err) {
            electronError = String(err);
        } finally {
            electronBusy = false;
        }
    }

    // ── Browser build only (isElectronApp false) ────────────────────────────
    // Two deliberate clicks, not one click-through-then-another: picking the
    // file and starting the game are each their own genuine user gesture, so
    // each gets its own fresh browser activation — a single click can't do
    // both (see handleBrowserStart) because a native file dialog and a
    // script-driven window.open() fight over the same click's single-use
    // activation grant.
    let pickedFile = $state<File | null>(null);
    let pickedBytes = $state<Uint8Array | null>(null);
    let pickError = $state<string | null>(null);
    let starting = $state(false);
    let startError = $state<string | null>(null);

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

    // window.open() must be the very first thing this does — it's what
    // spends this click's activation, and awaiting anything beforehand
    // (there's nothing to await here; the bytes are already read in
    // handlePickFile) would let the popup blocker silently no-op it. Hands
    // the bytes to the new tab over a BroadcastChannel — see wasm-player.js's
    // `source=local` boot branch. No resource-request handling on this path:
    // a raw picked File has no directory to resolve sibling assets against
    // (unlike the Electron path above), so this only really supports
    // self-contained .quest packages.
    function handleBrowserStart() {
        if (!pickedFile || !pickedBytes) return;
        startError = null;

        playChannel?.close();

        const popup = window.open(`${base}/player/?source=local`, "_blank");
        if (!popup) {
            startError = "Please allow pop-ups for this site to play the game.";
            return;
        }

        starting = true;
        const bytes = pickedBytes;
        const filename = pickedFile.name;
        const bc = new BroadcastChannel("quest-play-local");
        playChannel = bc;
        // Deliberately left open (not closed after the first message) —
        // WasmPlayer re-broadcasts 'ready' on every load of that tab,
        // including a plain refresh, so this needs to keep answering for as
        // long as this Play tab is still open, exactly like the
        // never-closed editor-preview channel it mirrors.
        bc.onmessage = ({ data }) => {
            if (data.type === "ready") {
                bc.postMessage({ type: "game", bytes, filename });
                starting = false;
                handleClearPicked();
            }
        };
    }

    onDestroy(() => playChannel?.close());
</script>

<!-- Always dark (see +layout.svelte) — surface-950/400/800 are the fixed
     dark-side members of Skeleton's paired tokens, not auto-switching ones,
     since the OS could be in light mode regardless. The background lives on
     this outer, unconstrained-width div — max-w-5xl below only centers the
     content column, so it must not also carry the background, or anything
     wider than 5xl shows the page's real (light-mode) background down the
     sides instead of dark. -->
<div class="min-h-svh bg-surface-950 text-surface-100">
    <div class="flex flex-col gap-8 w-full max-w-5xl mx-auto p-8">
        <div class="flex flex-col items-center gap-2">
            {#if isElectronApp}
                <button type="button" class="btn preset-outlined-primary-500" onclick={handleElectronPlay} disabled={electronBusy}>
                    {electronBusy ? "Opening…" : "Open a game file…"}
                </button>
                {#if electronError}
                    <p class="text-error-500 text-sm">{electronError}</p>
                {/if}
            {:else if !pickedFile}
                <button type="button" class="btn preset-outlined-primary-500" onclick={handlePickFile}>
                    Open a game file&hellip;
                </button>
            {:else}
                <div class="flex items-center gap-3">
                    <span class="text-sm text-surface-300 truncate max-w-[20ch]">{pickedFile.name}</span>
                    <button type="button" class="btn btn-sm preset-outlined-surface-500" onclick={handleClearPicked} disabled={starting}>
                        Change
                    </button>
                    <button type="button" class="btn preset-filled-primary-500" onclick={handleBrowserStart} disabled={starting}>
                        {starting ? "Starting…" : "Start ▶"}
                    </button>
                </div>
            {/if}
            {#if !isElectronApp && pickError}
                <p class="text-error-500 text-sm">{pickError}</p>
            {/if}
            {#if !isElectronApp && startError}
                <p class="text-error-500 text-sm">{startError}</p>
            {/if}
        </div>

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
                    <h2 class="text-lg font-semibold mb-3">{category.title}</h2>
                    <div class="grid grid-cols-[repeat(auto-fill,minmax(150px,1fr))] gap-4">
                        {#each category.games as game (game.id)}
                            <a
                                href="{base}/play/{game.id}"
                                class="flex flex-col rounded-lg border border-surface-800 overflow-hidden hover:border-primary-500 transition-colors"
                            >
                                <div class="aspect-[3/4] bg-surface-800 flex items-center justify-center overflow-hidden">
                                    {#if game.cover || game.thumbnail}
                                        <img src={game.cover ?? game.thumbnail} alt="" loading="lazy" class="w-full h-full object-cover" />
                                    {/if}
                                </div>
                                <div class="p-2">
                                    <div class="text-sm font-semibold truncate">{game.name}</div>
                                    {#if game.author}
                                        <div class="text-xs text-surface-400 truncate">by {game.author}</div>
                                    {/if}
                                    {#if game.rating > 0}
                                        <div class="text-xs text-primary-500 mt-1">{ratingStars(game.rating)}</div>
                                    {/if}
                                </div>
                            </a>
                        {/each}
                    </div>
                </section>
            {/each}
        {/if}
    </div>
</div>
