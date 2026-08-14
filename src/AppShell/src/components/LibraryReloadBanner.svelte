<script lang="ts">
    import { showLibraryReloadBanner, dismissLibraryReloadBanner, reloadGame } from "$lib/editor-store";
    import { t } from "$lib/i18n";

    let reloading = $state(false);

    async function handleReload() {
        reloading = true;
        try { await reloadGame(); } finally { reloading = false; }
    }
</script>

{#if $showLibraryReloadBanner}
    <div class="flex items-center gap-3 px-4 py-2 bg-primary-100-900 border-b border-primary-300-700 text-sm">
        <span class="flex-1">{t("libraryReloadBanner.message")}</span>
        <button
            type="button"
            class="btn btn-sm preset-filled-primary-500"
            onclick={handleReload}
            disabled={reloading}
        >{reloading ? t("common.reloading") : t("libraryReloadBanner.reloadButton")}</button>
        <button type="button" class="btn btn-sm preset-tonal" onclick={dismissLibraryReloadBanner}>{t("common.dismiss")}</button>
    </div>
{/if}
