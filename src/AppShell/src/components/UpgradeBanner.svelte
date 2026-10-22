<script lang="ts">
    import { upgradeNotice, dismissUpgradeNotice, downloadOriginalGame } from "#lib/editor-store.js";
    import { DOCS_ASL_VERSIONS_URL } from "#lib/docs-links.js";
    import { t } from "#lib/i18n/index.js";
</script>

{#if $upgradeNotice}
    <div class="flex flex-wrap items-center gap-3 px-4 py-2 bg-warning-100-900 border-b border-warning-300-700 text-sm">
        <span class="flex-1">{t("upgradeBanner.message")}</span>
        <a class="anchor" href={DOCS_ASL_VERSIONS_URL} target="_blank" rel="noopener noreferrer">{t("upgradeBanner.learnMore")}</a>
        <button type="button" class="btn btn-xs preset-filled-warning-500" onclick={downloadOriginalGame}>{t("upgradeBanner.downloadOriginal")}</button>
        <button type="button" class="btn btn-xs preset-tonal" onclick={dismissUpgradeNotice}>{t("common.dismiss")}</button>
    </div>
{/if}
