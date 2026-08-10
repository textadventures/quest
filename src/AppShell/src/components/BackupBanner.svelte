<script lang="ts">
    import { showBackupBanner, dismissBackupBanner, backupGame } from "$lib/editor-store";
    import { t } from "$lib/i18n";

    let backingUp = $state(false);

    async function handleBackup() {
        backingUp = true;
        try { await backupGame(); } finally { backingUp = false; }
    }
</script>

{#if $showBackupBanner}
    <div class="flex items-center gap-3 px-4 py-2 bg-primary-100-900 border-b border-primary-300-700 text-sm">
        <span class="flex-1">{t("backupBanner.message")}</span>
        <button
            type="button"
            class="btn btn-sm preset-filled-primary-500"
            onclick={handleBackup}
            disabled={backingUp}
        >{backingUp ? t("backupBanner.backingUp") : t("toolbar.backup")}</button>
        <button type="button" class="btn btn-sm preset-tonal" onclick={dismissBackupBanner}>{t("common.dismiss")}</button>
    </div>
{/if}
