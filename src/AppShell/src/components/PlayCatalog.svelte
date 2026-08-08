<script lang="ts">
    import { onMount } from "svelte";
    import { base } from "$app/paths";
    import { goto } from "$app/navigation";
    import { fetchCatalog, type CatalogCategory, type UpdateInfo } from "$lib/home-catalog";
    import { isElectron } from "$lib/runtime";
    import UpdateBanner from "$components/UpdateBanner.svelte";
    import GameCard from "$components/GameCard.svelte";
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

    onMount(load);

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
            <a href="{base}/play/local" class="btn preset-outlined-surface-500 whitespace-nowrap">Open a local file…</a>
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
