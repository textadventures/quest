<script lang="ts">
    import { onMount } from "svelte";
    import { page } from "$app/state";
    import { base } from "$app/paths";
    import { fetchGameDetails, type GameDetails } from "$lib/home-catalog";
    import { isElectron } from "$lib/runtime";

    const isElectronApp = isElectron();

    let details = $state<GameDetails | null>(null);
    let error = $state(false);
    let loading = $state(true);

    // Electron: open a dedicated player window with no filesystem bridge
    // (see ipc/player.ts), rather than navigating this editor window itself
    // to /player/?id= — that in-place nav would otherwise carry this
    // window's own window.electronApp (fs/dialog access) straight into a
    // downloaded, third-party game's page, and Quest games can eval their
    // own <javascript> resources (see wasm-player.js's WebPlayer.runJs).
    function handleElectronPlay() {
        if (!details) return;
        void window.electronApp!.player.openWindow({ id: details.id });
    }

    onMount(async () => {
        const id = page.params.id;
        if (!id) {
            error = true;
            loading = false;
            return;
        }
        try {
            details = await fetchGameDetails(id);
        } catch {
            error = true;
        } finally {
            loading = false;
        }
    });
</script>

<!-- Always dark (see +layout.svelte) — surface-950/400/800/300 are the fixed
     dark-side members of Skeleton's paired tokens, not auto-switching ones,
     since the OS could be in light mode regardless. -->
<div class="flex flex-col items-center min-h-svh bg-surface-950 text-surface-100 p-8 gap-6">
    <a href="{base}/" class="anchor self-start">&larr; Back to Play</a>

    {#if loading}
        <div class="flex flex-col items-center gap-3 py-12">
            <div class="size-10 rounded-full border-4 border-surface-800 border-t-primary-500 animate-spin"></div>
        </div>
    {:else if error || !details}
        <p class="text-error-500 text-sm">Couldn't load this game's details.</p>
    {:else}
        <div class="flex flex-col md:flex-row gap-6 max-w-2xl w-full">
            {#if details.cover || details.thumbnail}
                <img src={details.cover ?? details.thumbnail} alt="" class="w-40 rounded-lg self-center md:self-start" />
            {/if}
            <div class="flex flex-col gap-3 flex-1">
                <h1 class="text-2xl font-semibold">{details.name}</h1>
                {#if details.author}
                    <p class="text-surface-400 text-sm">by {details.author}</p>
                {/if}
                {#if details.tags.length > 0}
                    <div class="flex flex-wrap gap-2">
                        {#each details.tags as tag (tag)}
                            <span class="text-xs px-2 py-1 rounded-full bg-surface-800 text-surface-300">{tag}</span>
                        {/each}
                    </div>
                {/if}
                {#if details.description}
                    <p class="text-sm whitespace-pre-line">{details.description}</p>
                {/if}
                <div class="flex gap-3 mt-2">
                    {#if isElectronApp}
                        <button type="button" class="btn preset-filled-primary-500" onclick={handleElectronPlay}>Play</button>
                    {:else}
                        <a href="{base}/player/?id={details.id}" class="btn preset-filled-primary-500">Play</a>
                    {/if}
                    <a href={details.url} target="_blank" rel="noopener" class="btn preset-outlined-primary-500">View on textadventures.co.uk</a>
                </div>
            </div>
        </div>
    {/if}
</div>
