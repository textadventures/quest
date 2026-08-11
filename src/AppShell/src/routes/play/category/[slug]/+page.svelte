<script lang="ts">
    import { page } from "$app/state";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { fetchCategory, type CatalogGame } from "$lib/home-catalog";
    import GameCard from "$components/GameCard.svelte";
    import GamesPager from "$components/GamesPager.svelte";
    import { t } from "$lib/i18n";

    let title = $state<string | null>(null);
    let games = $state<CatalogGame[]>([]);
    let totalCount = $state(0);
    let pageCount = $state(0);
    let currentPage = $state(1);
    let loading = $state(true);
    let error = $state(false);

    async function load(slug: string, pageNum: number) {
        loading = true;
        error = false;
        try {
            const result = await fetchCategory(slug, pageNum);
            title = result.title;
            games = result.games;
            totalCount = result.totalCount;
            pageCount = result.pageCount;
            currentPage = result.page;
        } catch {
            error = true;
        } finally {
            loading = false;
        }
    }

    // Re-runs on both a fresh mount and a slug/page change while this route
    // stays mounted (SvelteKit reuses the component across param changes),
    // since it reads off $app/state's page rather than onMount.
    $effect(() => {
        const slug = page.params.slug;
        if (!slug) return;
        const requestedPage = Number(page.url.searchParams.get("page") ?? "1");
        void load(slug, Number.isFinite(requestedPage) && requestedPage > 0 ? requestedPage : 1);
    });

    function goToPage(p: number) {
        void goto(`${base}/play/category/${page.params.slug}?page=${p}`);
    }
</script>

<!-- Always dark, matching the rest of /play — see +layout.svelte's isPlayContext. -->
<div class="min-h-svh bg-surface-950 text-surface-100">
    <div class="flex flex-col gap-6 w-full max-w-5xl mx-auto p-8">
        <a href="{base}/" class="anchor self-start">{t("search.backToPlay")}</a>

        {#if loading}
            <div class="flex flex-col items-center gap-3 py-12">
                <div class="size-10 rounded-full border-4 border-surface-800 border-t-primary-500 animate-spin"></div>
            </div>
        {:else if error}
            <div class="flex flex-col items-center gap-3 py-12 text-center">
                <p class="text-error-500 text-sm">{t("categoryPage.loadError")}</p>
                <button
                    type="button"
                    class="btn preset-tonal"
                    onclick={() => load(page.params.slug!, currentPage)}
                >{t("search.tryAgain")}</button>
            </div>
        {:else}
            <h1 class="text-lg font-semibold">
                {title}
                <span class="text-surface-400 font-normal text-sm">({totalCount})</span>
            </h1>
            <div class="grid grid-cols-[repeat(auto-fill,minmax(150px,1fr))] gap-4">
                {#each games as game (game.id)}
                    <GameCard {game} />
                {/each}
            </div>
            <GamesPager page={currentPage} {pageCount} onPage={goToPage} />
        {/if}
    </div>
</div>
