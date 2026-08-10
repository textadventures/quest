<script lang="ts">
    import { showFileChangedExternallyBanner, dismissFileChangedExternallyBanner, reloadGameFromDisk } from "$lib/editor-store";

    let reloading = $state(false);

    async function handleReload() {
        reloading = true;
        try {
            // Only dismisses on an actual reload — a cancelled discard-confirm
            // (unsaved edits) leaves the banner up, since the file is still
            // out of sync with what's on screen.
            if (await reloadGameFromDisk()) dismissFileChangedExternallyBanner();
        } finally {
            reloading = false;
        }
    }
</script>

{#if $showFileChangedExternallyBanner}
    <div class="flex items-center gap-3 px-4 py-2 bg-warning-100-900 border-b border-warning-300-700 text-sm">
        <span class="flex-1">This file was changed outside the editor, by another program.</span>
        <button
            type="button"
            class="btn btn-sm preset-filled-warning-500"
            onclick={handleReload}
            disabled={reloading}
        >{reloading ? "Reloading…" : "Reload from disk"}</button>
        <button type="button" class="btn btn-sm preset-tonal" onclick={dismissFileChangedExternallyBanner}>Dismiss</button>
    </div>
{/if}
