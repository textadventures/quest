<script lang="ts">
    // Asks the author what to do when the open game changed outside the editor and the editor
    // couldn't simply reload it (see checkForExternalChanges in editor-store.ts): there were
    // unsaved edits here, the changed file doesn't load, or the file is gone.
    import { goto } from "$app/navigation";
    import { base } from "#lib/base-path.js";
    import {
        reloadGameFromDisk,
        keepMyChanges,
        savesHeld,
        externalChangeError,
        gameFileMissing,
    } from "#lib/editor-store.js";
    import { t } from "#lib/i18n/index.js";

    let busy = $state(false);

    async function run(action: () => Promise<unknown>) {
        busy = true;
        try {
            await action();
        } finally {
            busy = false;
        }
    }
</script>

{#if $gameFileMissing || $savesHeld}
    <div
        class="flex flex-wrap items-center gap-3 px-4 py-2 bg-warning-100-900 border-b border-warning-300-700 text-sm"
        role="alert"
        data-testid="file-changed-banner"
    >
        {#if $gameFileMissing}
            <span class="flex-1">{t("fileChangedBanner.missingMessage")}</span>
            <button
                type="button"
                class="btn btn-xs preset-filled-warning-500"
                onclick={() => run(keepMyChanges)}
                disabled={busy}
            >{t("fileChangedBanner.saveAgain")}</button>
            <!-- Nothing to save first: saves are held while the file is missing. -->
            <button
                type="button"
                class="btn btn-xs preset-tonal"
                onclick={() => goto(`${base}/open`)}
                disabled={busy}
            >{t("fileChangedBanner.closeGame")}</button>
        {:else if $externalChangeError}
            <span class="flex-1">{t("fileChangedBanner.loadErrorMessage", { error: $externalChangeError })}</span>
            <button
                type="button"
                class="btn btn-xs preset-tonal"
                onclick={() => run(keepMyChanges)}
                disabled={busy}
            >{t("fileChangedBanner.keepMyChanges")}</button>
        {:else}
            <span class="flex-1">{t("fileChangedBanner.conflictMessage")}</span>
            <button
                type="button"
                class="btn btn-xs preset-filled-warning-500"
                onclick={() => run(reloadGameFromDisk)}
                disabled={busy}
            >{t("fileChangedBanner.useChangedFile")}</button>
            <button
                type="button"
                class="btn btn-xs preset-tonal"
                onclick={() => run(keepMyChanges)}
                disabled={busy}
            >{t("fileChangedBanner.keepMyChanges")}</button>
        {/if}
    </div>
{/if}
