<script lang="ts">
    import { page } from "$app/state";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { searchGames, type CatalogGame } from "$lib/home-catalog";
    import GameCard from "$components/GameCard.svelte";
    import GamesPager from "$components/GamesPager.svelte";

    let queryInput = $state("");
    let games = $state<CatalogGame[]>([]);
    let totalCount = $state(0);
    let pageCount = $state(0);
    let currentPage = $state(1);
    let loading = $state(false);
    let error = $state(false);
    let searched = $state(false);

    async function load(q: string, pageNum: number) {
        loading = true;
        error = false;
        try {
            const result = await searchGames(q, pageNum);
            games = result.games;
            totalCount = result.totalCount;
            pageCount = result.pageCount;
            currentPage = result.page;
            searched = true;
        } catch {
            error = true;
        } finally {
            loading = false;
        }
    }

    // Re-runs on both a fresh mount and a q/page change while this route stays
    // mounted, since it reads off $app/state's page rather than onMount — lets
    // a repeat search (or Back/Forward) re-fetch without a full remount.
    $effect(() => {
        const q = page.url.searchParams.get("q") ?? "";
        queryInput = q;
        if (!q.trim()) {
            searched = false;
            games = [];
            return;
        }
        const requestedPage = Number(page.url.searchParams.get("page") ?? "1");
        void load(q, Number.isFinite(requestedPage) && requestedPage > 0 ? requestedPage : 1);
    });

    function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        const q = queryInput.trim();
        if (!q) return;
        void goto(`${base}/play/search?q=${encodeURIComponent(q)}`);
    }

    function goToPage(p: number) {
        const q = page.url.searchParams.get("q") ?? "";
        void goto(`${base}/play/search?q=${encodeURIComponent(q)}&page=${p}`);
    }
</script>

<!-- Always dark, matching the rest of /play — see +layout.svelte's isPlayContext. -->
<div class="min-h-svh bg-surface-950 text-surface-100">
    <div class="flex flex-col gap-6 w-full max-w-5xl mx-auto p-8">
        <a href="{base}/" class="anchor self-start">&larr; Back to Play</a>

        <form class="flex gap-2 w-full max-w-md mx-auto" onsubmit={handleSubmit}>
            <input
                type="search"
                bind:value={queryInput}
                placeholder="Search games…"
                title="Try category:puzzle, language:de, platform:quest-gamebook"
                class="input bg-surface-900 border-surface-700 text-surface-100 placeholder:text-surface-500"
            />
            <button type="submit" class="btn preset-filled-primary-500">Search</button>
        </form>

        {#if loading}
            <div class="flex flex-col items-center gap-3 py-12">
                <div class="size-10 rounded-full border-4 border-surface-800 border-t-primary-500 animate-spin"></div>
            </div>
        {:else if error}
            <div class="flex flex-col items-center gap-3 py-12 text-center">
                <p class="text-error-500 text-sm">Couldn't load search results.</p>
                <button type="button" class="btn preset-tonal" onclick={() => load(queryInput.trim(), currentPage)}>Try again</button>
            </div>
        {:else if searched}
            <p class="text-surface-400 text-sm">{totalCount} result{totalCount === 1 ? "" : "s"}</p>
            {#if games.length === 0}
                <p class="text-surface-400 text-sm">No games found.</p>
            {:else}
                <div class="grid grid-cols-[repeat(auto-fill,minmax(150px,1fr))] gap-4">
                    {#each games as game (game.id)}
                        <GameCard {game} />
                    {/each}
                </div>
                <GamesPager page={currentPage} {pageCount} onPage={goToPage} />
            {/if}
        {/if}
    </div>
</div>
