<script lang="ts">
    import {
        showFileChangedExternallyBanner,
        dismissFileChangedExternallyBanner,
        reloadGameFromDisk,
        keepMyChanges,
        savesHeld,
        isDirty,
        isEditingField,
    } from "#lib/editor-store.js";
    import { t } from "#lib/i18n/index.js";

    let busy = $state(false);

    // With nothing unsaved there's nothing to lose by reloading, so it's a plain Reload. With
    // unsaved edits (or a save already held because the file changed), the author picks which
    // copy wins: neither is chosen for them.
    const conflict = $derived($savesHeld || $isDirty || $isEditingField);

    async function handleUseChangedFile() {
        busy = true;
        try {
            await reloadGameFromDisk();
        } finally {
            busy = false;
        }
    }

    async function handleKeepMine() {
        busy = true;
        try {
            await keepMyChanges();
        } finally {
            busy = false;
        }
    }
</script>

{#if $showFileChangedExternallyBanner}
    <div
        class="flex flex-wrap items-center gap-3 px-4 py-2 bg-warning-100-900 border-b border-warning-300-700 text-sm"
        role="alert"
        data-testid="file-changed-banner"
    >
        <span class="flex-1">{conflict ? t("fileChangedBanner.conflictMessage") : t("fileChangedBanner.message")}</span>
        {#if conflict}
            <button
                type="button"
                class="btn btn-xs preset-filled-warning-500"
                onclick={handleUseChangedFile}
                disabled={busy}
            >{t("fileChangedBanner.useChangedFile")}</button>
            <button
                type="button"
                class="btn btn-xs preset-tonal"
                onclick={handleKeepMine}
                disabled={busy}
            >{t("fileChangedBanner.keepMyChanges")}</button>
        {:else}
            <button
                type="button"
                class="btn btn-xs preset-filled-warning-500"
                onclick={handleUseChangedFile}
                disabled={busy}
            >{busy ? t("common.reloading") : t("fileChangedBanner.reloadFromDisk")}</button>
            <button type="button" class="btn btn-xs preset-tonal" onclick={dismissFileChangedExternallyBanner}>{t("common.dismiss")}</button>
        {/if}
    </div>
{/if}
