<script lang="ts">
    import { onMount } from "svelte";
    import { base } from "$app/paths";
    import { fetchCatalog, type CatalogCategory } from "$lib/home-catalog";

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

        <p class="text-surface-400 text-sm text-center">
            Have a game file or a link already? <a href="{base}/player/" class="anchor">Open the player</a>.
        </p>
    </div>
</div>
