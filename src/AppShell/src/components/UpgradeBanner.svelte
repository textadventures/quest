<script lang="ts">
    import { upgradeNotice, dismissUpgradeNotice, downloadOriginalGame, legacyUiHacks, dismissLegacyUiNotice, keepClassicInterface, treeNodes } from "#lib/editor-store.js";
    import { DOCS_ASL_VERSIONS_URL, DOCS_MOVING_FROM_CLASSIC_URL } from "#lib/docs-links.js";
    import { t } from "#lib/i18n/index.js";

    const MAX_HACKS_SHOWN = 3;
    const gameKey = $derived($treeNodes.find(n => n.nodeType === "game")?.key);
</script>

{#if $upgradeNotice}
    <div class="flex flex-wrap items-center gap-3 px-4 py-2 bg-warning-100-900 border-b border-warning-300-700 text-sm">
        <span class="flex-1">{t("upgradeBanner.message")}</span>
        <a class="anchor" href={DOCS_ASL_VERSIONS_URL} target="_blank" rel="noopener noreferrer">{t("upgradeBanner.learnMore")}</a>
        <button type="button" class="btn btn-xs preset-filled-warning-500" onclick={downloadOriginalGame}>{t("upgradeBanner.downloadOriginal")}</button>
        <button type="button" class="btn btn-xs preset-tonal" onclick={dismissUpgradeNotice}>{t("common.dismiss")}</button>
    </div>
{/if}
{#if $legacyUiHacks.length > 0}
    <div class="flex flex-wrap items-center gap-3 px-4 py-2 bg-warning-100-900 border-b border-warning-300-700 text-sm">
        <div class="flex-1 min-w-0">
            <p>{t("upgradeBanner.legacyUiMessage")}</p>
            <ul class="mt-1">
                {#each $legacyUiHacks.slice(0, MAX_HACKS_SHOWN) as hack, i (i)}
                    <li class="break-words">{hack.source}: <code>{hack.detail}</code></li>
                {/each}
            </ul>
            {#if $legacyUiHacks.length > MAX_HACKS_SHOWN}
                <p class="mt-1">{t("upgradeBanner.legacyUiMore", { count: $legacyUiHacks.length - MAX_HACKS_SHOWN })}</p>
            {/if}
        </div>
        <a class="anchor" href={DOCS_MOVING_FROM_CLASSIC_URL} target="_blank" rel="noopener noreferrer">{t("upgradeBanner.learnMore")}</a>
        {#if gameKey}
            <button type="button" class="btn btn-xs preset-filled-warning-500" onclick={() => keepClassicInterface(gameKey)}>{t("upgradeBanner.keepClassicInterface")}</button>
        {/if}
        <button type="button" class="btn btn-xs preset-tonal" onclick={dismissLegacyUiNotice}>{t("common.dismiss")}</button>
    </div>
{/if}
