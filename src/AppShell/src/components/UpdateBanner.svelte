<script lang="ts">
    import type { UpdateInfo } from "$lib/home-catalog";

    let { update }: { update: UpdateInfo } = $props();

    const dismissedKey = "questviva-update-dismissed-version";

    let manuallyDismissed = $state(false);

    // Dismissing only suppresses this specific version's banner — a later
    // release with a newer latestVersion re-triggers it.
    const dismissed = $derived(
        manuallyDismissed ||
        (typeof localStorage !== "undefined" && localStorage.getItem(dismissedKey) === update.latestVersion)
    );

    function handleDismiss() {
        localStorage.setItem(dismissedKey, update.latestVersion);
        manuallyDismissed = true;
    }
</script>

{#if !dismissed}
    <div class="flex items-center gap-3 px-4 py-2 bg-primary-100-900 border-b border-primary-300-700 text-sm">
        <span class="flex-1">Quest Viva {update.latestVersion} is available.</span>
        <a href={update.url} target="_blank" rel="noopener" class="btn btn-sm preset-filled-primary-500">Download</a>
        <button type="button" class="btn btn-sm preset-tonal" onclick={handleDismiss}>Later</button>
    </div>
{/if}
