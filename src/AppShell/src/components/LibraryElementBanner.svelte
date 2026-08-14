<script lang="ts">
    import { selectedKey, selectedData, makeElementLocal } from "$lib/editor-store";
    import { t } from "$lib/i18n";

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
            {t("libraryElementBanner.message", { filename: $selectedData.filename ? ` (${$selectedData.filename})` : "" })}
        </span>
        <button
            type="button"
            class="btn btn-sm preset-filled-warning-500"
            onclick={handleCopy}
            disabled={copying}
        >{copying ? t("libraryElementBanner.copying") : t("libraryElementBanner.copyIntoGame")}</button>
    </div>
{/if}
