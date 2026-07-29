<script lang="ts">
    import { selectedKey, selectedData, makeElementLocal } from "$lib/editor-store";

    let copying = $state(false);

    async function handleCopy() {
        const key = $selectedKey;
        if (!key) return;
        copying = true;
        try { await makeElementLocal(key); } finally { copying = false; }
    }
</script>

{#if $selectedData?.isLibraryElement}
    <div class="flex items-center gap-3 px-4 py-2 bg-warning-100-900 border-b border-warning-300-700 text-sm">
        <span class="flex-1">
            This element comes from a library{$selectedData.filename ? ` (${$selectedData.filename})` : ""} and can't be edited directly.
        </span>
        <button
            type="button"
            class="btn btn-sm preset-filled-warning-500"
            onclick={handleCopy}
            disabled={copying}
        >{copying ? "Copying…" : "Copy into your game"}</button>
    </div>
{/if}
