<script lang="ts">
    import { showFileChangedExternallyBanner, dismissFileChangedExternallyBanner, reloadGameFromDisk } from "$lib/editor-store";
    import { t } from "$lib/i18n";

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
        <span class="flex-1">{t("fileChangedBanner.message")}</span>
        <button
            type="button"
            class="btn btn-sm preset-filled-warning-500"
            onclick={handleReload}
            disabled={reloading}
        >{reloading ? t("common.reloading") : t("fileChangedBanner.reloadFromDisk")}</button>
        <button type="button" class="btn btn-sm preset-tonal" onclick={dismissFileChangedExternallyBanner}>{t("common.dismiss")}</button>
    </div>
{/if}
