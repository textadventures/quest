<script lang="ts">
    import { showBackupBanner, dismissBackupBanner, backupGame } from "$lib/editor-store";

    let backingUp = $state(false);

    async function handleBackup() {
        backingUp = true;
        try { await backupGame(); } finally { backingUp = false; }
    }
</script>

{#if $showBackupBanner}
    <div class="flex items-center gap-3 px-4 py-2 bg-primary-100-900 border-b border-primary-300-700 text-sm">
        <span class="flex-1">This game is only stored in this browser — use Backup to save a copy to disk.</span>
        <button
            type="button"
            class="btn btn-sm preset-filled-primary-500"
            onclick={handleBackup}
            disabled={backingUp}
        >{backingUp ? "Backing up…" : "Backup…"}</button>
        <button type="button" class="btn btn-sm preset-tonal" onclick={dismissBackupBanner}>Dismiss</button>
    </div>
{/if}
