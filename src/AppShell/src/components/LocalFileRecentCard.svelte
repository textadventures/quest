<script lang="ts">
    import type { RecentGame } from "$lib/filesystem/electron-adapter";
    import Gamepad2 from "@lucide/svelte/icons/gamepad-2";

    // Electron only — there's no browser equivalent (see PlayCatalog.svelte),
    // so unlike RecentGameCard this never needs a non-Electron branch.
    let { game, onplay, onremove }: { game: RecentGame; onplay: () => void; onremove: () => void } = $props();

    function folderName(dirPath: string): string {
        return dirPath.split(/[\\/]/).pop() || dirPath;
    }

    function relativeTime(ms: number): string {
        const mins = Math.round((Date.now() - ms) / 60000);
        if (mins < 1) return "just now";
        if (mins < 60) return `${mins}m ago`;
        const hours = Math.round(mins / 60);
        if (hours < 24) return `${hours}h ago`;
        return `${Math.round(hours / 24)}d ago`;
    }

    function handleRemove(e: MouseEvent) {
        e.preventDefault();
        e.stopPropagation();
        onremove();
    }
</script>

<div class="relative">
    <button
        type="button"
        onclick={onplay}
        class="flex flex-col w-full text-left rounded-lg border border-surface-800 overflow-hidden hover:border-primary-500 transition-colors"
    >
        <div class="aspect-[3/4] bg-surface-800 flex items-center justify-center overflow-hidden">
            <Gamepad2 size={40} class="text-surface-600" />
        </div>
        <div class="p-2 flex flex-col gap-1">
            <div class="text-sm font-semibold truncate">{game.filename}</div>
            <div class="text-xs text-surface-400 truncate">{folderName(game.dirPath)}</div>
            <div class="text-xs text-surface-500">{relativeTime(game.lastOpened)}</div>
        </div>
    </button>
    <button
        type="button"
        class="absolute top-1 right-1 flex items-center justify-center size-6 rounded-full bg-surface-950/80 text-surface-300 hover:text-error-500 transition-colors"
        title="Remove from Recently Played"
        aria-label="Remove from Recently Played"
        onclick={handleRemove}
    >
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="size-3.5">
            <path d="M18 6 6 18" />
            <path d="m6 6 12 12" />
        </svg>
    </button>
</div>
