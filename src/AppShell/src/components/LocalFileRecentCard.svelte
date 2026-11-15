<script lang="ts">
    import type { RecentGame } from "#lib/filesystem/electron-adapter.js";
    import { resolveAndCacheCover } from "#lib/local-cover.js";
    import Gamepad2 from "@lucide/svelte/icons/gamepad-2";
    import Laptop from "@lucide/svelte/icons/laptop";
    import TriangleAlert from "@lucide/svelte/icons/triangle-alert";
    import { t } from "#lib/i18n/index.js";

    // Electron only — there's no browser equivalent (see PlayCatalog.svelte),
    // so unlike RecentGameCard this never needs a non-Electron branch.
    // missing: the last attempt to play this found the file gone. The card then
    // says so in place of its cover, and offers Remove rather than Play.
    let { game, missing = false, onplay, onremove }: { game: RecentGame; missing?: boolean; onplay: () => void; onremove: () => void } = $props();

    // game.coverDataUrl is cached on the RecentGame record (resolved once at play time, or
    // here as a one-off self-heal for a legacy entry — see local-cover.ts) — the common case
    // is a synchronous read, no engine boot needed. undefined (never resolved yet) is the only
    // case that falls back to resolving live; starts null so the Gamepad2 placeholder below
    // shows while that's in flight. Keyed off dirPath+filename so a change of `game` (grid
    // re-sorted by recency) re-checks rather than keeping a stale image.
    let coverUrl = $state<string | null>(null);
    $effect(() => {
        const { dirPath, filename, coverDataUrl } = game;
        if (coverDataUrl !== undefined) {
            coverUrl = coverDataUrl;
            return;
        }
        coverUrl = null;
        void resolveAndCacheCover(dirPath, filename).then((url) => {
            if (game.dirPath === dirPath && game.filename === filename) coverUrl = url;
        });
    });

    function folderName(dirPath: string): string {
        return dirPath.split(/[\\/]/).pop() || dirPath;
    }

    function relativeTime(ms: number): string {
        const mins = Math.round((Date.now() - ms) / 60000);
        if (mins < 1) return t("common.relativeTime.justNow");
        if (mins < 60) return t("common.relativeTime.minutesAgo", { mins });
        const hours = Math.round(mins / 60);
        if (hours < 24) return t("common.relativeTime.hoursAgo", { hours });
        return t("common.relativeTime.daysAgo", { days: Math.round(hours / 24) });
    }

    function handleRemove(e: MouseEvent) {
        e.preventDefault();
        e.stopPropagation();
        onremove();
    }
</script>

<div class="relative">
    {#if missing}
        <div class="flex flex-col w-full rounded-lg border border-error-800 overflow-hidden">
            <div class="aspect-[3/4] bg-error-950/40 flex flex-col items-center justify-center gap-2 p-3 text-center" role="alert">
                <TriangleAlert size={28} class="text-error-400" aria-hidden="true" />
                <span class="text-sm font-semibold text-error-300">{t("playCatalog.fileNotFound")}</span>
                <span class="text-xs text-error-200">{t("playCatalog.fileNotFoundHint")}</span>
            </div>
            <div class="p-2 flex flex-col gap-2">
                <div class="text-sm font-semibold truncate text-surface-400" title={game.dirPath}>{game.filename}</div>
                <button type="button" class="btn btn-sm preset-tonal self-start" onclick={handleRemove}>
                    {t("playCatalog.remove")}
                </button>
            </div>
        </div>
    {:else}
        <button
            type="button"
            onclick={onplay}
            class="flex flex-col w-full text-left rounded-lg border border-surface-800 overflow-hidden hover:border-primary-500 transition-colors"
        >
            <div class="aspect-[3/4] bg-surface-800 flex items-center justify-center overflow-hidden">
                {#if coverUrl}
                    <img src={coverUrl} alt="" loading="lazy" class="w-full h-full object-cover" />
                {:else}
                    <Gamepad2 size={40} class="text-surface-600" />
                {/if}
            </div>
            <div class="p-2 flex flex-col gap-1">
                <div class="text-sm font-semibold truncate">{game.filename}</div>
                <div class="flex items-center gap-1 text-xs text-surface-400 min-w-0">
                    <Laptop size={12} class="shrink-0" aria-hidden="true" />
                    <span class="truncate">{folderName(game.dirPath)}</span>
                </div>
                <div class="text-xs text-surface-500">{relativeTime(game.lastOpened)}</div>
            </div>
        </button>
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
