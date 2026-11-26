<script lang="ts">
    // Reminds the author to back up a local draft, from the foot of the tree. A card here rather
    // than a banner above the editor, so showing it never moves the editor or the toolbar: only
    // the tree above it gets a little shorter, and as the card is at the bottom, the tree's items
    // keep their place. While the tree isn't on screen, the toolbar's "Saved" chip stands in for
    // it (see Toolbar.svelte), and clicking that brings the tree back and focuses Download here.
    import { tick } from "svelte";
    import {
        backupGame, remindBackupLater, backupReminderFocusPending, requestStoragePersistence,
    } from "#lib/editor-store.js";
    import { t } from "#lib/i18n/index.js";

    let backingUp = $state(false);
    let cardEl = $state<HTMLElement>();
    let downloadButton = $state<HTMLButtonElement>();

    async function handleBackup() {
        backingUp = true;
        try { await backupGame(); } finally { backingUp = false; }
    }

    // Asks the browser to keep the storage the first time the card is actually on screen, not
    // just mounted (on a phone the tree stays mounted while hidden): Firefox prompts, and the
    // prompt makes more sense with the card's explanation beside it.
    $effect(() => {
        if (!cardEl) return;
        const observer = new IntersectionObserver((entries) => {
            if (entries.some(e => e.isIntersecting)) {
                observer.disconnect();
                void requestStoragePersistence();
            }
        });
        observer.observe(cardEl);
        return () => observer.disconnect();
    });

    // A tick first, so the edit page has brought the tree back (it reacts to the same store).
    $effect(() => {
        if (!$backupReminderFocusPending || !downloadButton) return;
        void tick().then(() => {
            if (!downloadButton || downloadButton.offsetParent === null) return;
            downloadButton.focus();
            backupReminderFocusPending.set(false);
        });
    });
</script>

<section
    bind:this={cardEl}
    aria-label={t("backupReminder.label")}
    data-testid="backup-reminder"
    class="shrink-0 m-1.5 p-2.5 flex flex-col gap-2 rounded-md border border-primary-300-700 bg-primary-50-950 text-xs"
>
    <p>{t("backupReminder.message")}</p>
    <div class="flex flex-wrap gap-1.5">
        <button
            bind:this={downloadButton}
            type="button"
            class="btn btn-xs preset-filled-primary-500"
            onclick={handleBackup}
            disabled={backingUp}
        >{backingUp ? t("backupReminder.backingUp") : t("backupReminder.download")}</button>
        <button type="button" class="btn btn-xs preset-tonal" onclick={remindBackupLater}>{t("backupReminder.remindLater")}</button>
    </div>
</section>
