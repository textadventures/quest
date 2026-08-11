<script lang="ts">
    import { base } from "$app/paths";
    import { languageName, type CatalogGame } from "$lib/home-catalog";
    import { t } from "$lib/i18n";

    let { game }: { game: CatalogGame } = $props();

    function ratingStars(rating: number): string {
        const rounded = Math.max(0, Math.min(5, Math.round(rating)));
        return "★".repeat(rounded) + "☆".repeat(5 - rounded);
    }
</script>

<a
    href="{base}/play/{game.id}"
    class="flex flex-col rounded-lg border border-surface-800 overflow-hidden hover:border-primary-500 transition-colors"
>
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
</a>
