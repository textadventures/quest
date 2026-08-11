<script lang="ts">
    import { base } from "$app/paths";
    import { languageName } from "$lib/home-catalog";
    import type { RecentCatalogPlay } from "$lib/recent-catalog-plays";
    import { isElectron } from "$lib/runtime";
    import { t } from "$lib/i18n";

    let { game, onremove }: { game: RecentCatalogPlay; onremove: () => void } = $props();

    const isElectronApp = isElectron();

    function ratingStars(rating: number): string {
        const rounded = Math.max(0, Math.min(5, Math.round(rating)));
        return "★".repeat(rounded) + "☆".repeat(5 - rounded);
    }

    // Electron: a dedicated player window, same call play/[id]/+page.svelte's own Play
    // button makes — this card launches straight into play, it never goes through the
    // catalog details page. Browser: a plain link to the same effect (see below), since
    // there's no renderer-owned window to open programmatically there.
    function handlePlay() {
        void window.electronApp!.player.openWindow({ id: game.id });
    }

    function handleRemove(e: MouseEvent) {
        e.preventDefault();
        e.stopPropagation();
        onremove();
    }
</script>

{#snippet cardBody()}
    <div class="aspect-[3/4] bg-surface-800 flex items-center justify-center overflow-hidden">
        {#if game.cover || game.thumbnail}
            <img src={game.cover ?? game.thumbnail} alt="" loading="lazy" class="w-full h-full object-cover" />
        {/if}
    </div>
    <div class="p-2 flex flex-col gap-1">
        <div class="text-sm font-semibold truncate">{game.name}</div>
        {#if game.author}
            <div class="text-xs text-surface-400 truncate">{t("gameCard.byAuthor", { author: game.author })}</div>
        {/if}
        <div class="flex flex-wrap gap-1">
            <span class="text-[10px] leading-none px-1.5 py-1 rounded-full bg-surface-800 text-surface-300">
                {game.isGamebook ? t("common.gamebook") : t("common.textAdventure")}
            </span>
            {#if game.language !== "en"}
                <span class="text-[10px] leading-none px-1.5 py-1 rounded-full bg-surface-800 text-surface-300">
                    {languageName(game.language)}
                </span>
            {/if}
        </div>
        {#if game.rating > 0}
            <div class="text-xs text-primary-500">{ratingStars(game.rating)}</div>
        {/if}
    </div>
{/snippet}

<div class="relative">
    {#if isElectronApp}
        <button
            type="button"
            onclick={handlePlay}
            class="flex flex-col w-full text-left rounded-lg border border-surface-800 overflow-hidden hover:border-primary-500 transition-colors"
        >
            {@render cardBody()}
        </button>
    {:else}
        <a
            href="{base}/player/?id={game.id}"
            class="flex flex-col rounded-lg border border-surface-800 overflow-hidden hover:border-primary-500 transition-colors"
        >
            {@render cardBody()}
        </a>
    {/if}
    <button
        type="button"
        class="absolute top-1 right-1 flex items-center justify-center size-6 rounded-full bg-surface-950/80 text-surface-300 hover:text-error-500 transition-colors"
        title={t("recentGameCard.removeTitle")}
        aria-label={t("recentGameCard.removeTitle")}
        onclick={handleRemove}
    >
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="size-3.5">
            <path d="M18 6 6 18" />
            <path d="m6 6 12 12" />
        </svg>
    </button>
</div>
