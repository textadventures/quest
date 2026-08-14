<script lang="ts">
    import { t } from "$lib/i18n";

    let { page, pageCount, onPage }: { page: number; pageCount: number; onPage: (page: number) => void } = $props();
</script>

{#if pageCount > 1}
    <div class="flex items-center justify-center gap-3">
        <button
            type="button"
            class="btn btn-sm preset-outlined-surface-500"
            disabled={page <= 1}
            onclick={() => onPage(page - 1)}
        >{t("pager.prev")}</button>
        <span class="text-sm text-surface-400">{t("pager.pageOf", { page, pageCount })}</span>
        <button
            type="button"
            class="btn btn-sm preset-outlined-surface-500"
            disabled={page >= pageCount}
            onclick={() => onPage(page + 1)}
        >{t("pager.next")}</button>
    </div>
{/if}
