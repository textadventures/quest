<script lang="ts">
    import { onMount } from "svelte";
    import { page } from "$app/state";
    import { base } from "$app/paths";
    import { fetchGameDetails, type GameDetails } from "$lib/home-catalog";

    let details = $state<GameDetails | null>(null);
    let error = $state(false);
    let loading = $state(true);

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

<div class="flex flex-col items-center min-h-svh p-8 gap-6">
    <a href="{base}/" class="anchor self-start">&larr; Back to Play</a>

    {#if loading}
        <div class="flex flex-col items-center gap-3 py-12">
            <div class="size-10 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
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
                    <p class="text-surface-500-400 text-sm">by {details.author}</p>
                {/if}
                {#if details.tags.length > 0}
                    <div class="flex flex-wrap gap-2">
                        {#each details.tags as tag (tag)}
                            <span class="text-xs px-2 py-1 rounded-full bg-surface-200-800 text-surface-600-300">{tag}</span>
                        {/each}
                    </div>
                {/if}
                {#if details.description}
                    <p class="text-sm whitespace-pre-line">{details.description}</p>
                {/if}
                <div class="flex gap-3 mt-2">
                    <a href="{base}/player/?id={details.id}" class="btn preset-filled-primary-500">Play</a>
                    <a href={details.url} target="_blank" rel="noopener" class="btn preset-outlined-primary-500">View on textadventures.co.uk</a>
                </div>
            </div>
        </div>
    {/if}
</div>
