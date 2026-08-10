<script lang="ts">
    import { showLibraryReloadBanner, dismissLibraryReloadBanner, reloadGame } from "$lib/editor-store";

    let reloading = $state(false);

    async function handleReload() {
        reloading = true;
        try { await reloadGame(); } finally { reloading = false; }
    }
</script>

{#if $showLibraryReloadBanner}
    <div class="flex items-center gap-3 px-4 py-2 bg-primary-100-900 border-b border-primary-300-700 text-sm">
        <span class="flex-1">Reload the editor to update panes for the library change you just made.</span>
        <button
            type="button"
            class="btn btn-sm preset-filled-primary-500"
            onclick={handleReload}
            disabled={reloading}
        >{reloading ? "Reloading…" : "Reload"}</button>
        <button type="button" class="btn btn-sm preset-tonal" onclick={dismissLibraryReloadBanner}>Dismiss</button>
    </div>
{/if}
