<script lang="ts">
    import { exportHtml, type ExportHtmlMode } from "$lib/editor-store";
    import { t } from "$lib/i18n";
    import { trapFocus } from "$lib/actions/trapFocus";

    interface Props {
        oncancel: () => void;
    }
    const { oncancel }: Props = $props();

    let mode = $state<ExportHtmlMode>("cdn");
    let exporting = $state(false);
    let error = $state("");
    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { dialogEl?.focus(); });

    async function handleExport() {
        exporting = true;
        error = "";
        try {
            await exportHtml(mode);
            oncancel();
        } catch (err) {
            error = String(err);
        } finally {
            exporting = false;
        }
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") oncancel();
    }
</script>

<div
    bind:this={dialogEl}
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50 p-4"
    onclick={onBackdropClick}
    onkeydown={handleKeydown}
    use:trapFocus
>
    <div class="card bg-surface-50-950 rounded-xl shadow-xl w-full max-w-[28rem] p-6 flex flex-col gap-4">
        <div class="flex items-center justify-between">
            <h2 class="text-base font-semibold">{t("exportHtmlModal.title")}</h2>
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>{t("common.close")}</button>
        </div>

        <p class="text-sm text-surface-600-400">
            {t("exportHtmlModal.description")}
        </p>

        <div class="flex flex-col gap-3" role="radiogroup" aria-label={t("exportHtmlModal.title")}>
            <label class="flex items-start gap-2 text-sm cursor-pointer">
                <input type="radio" class="mt-0.5" name="export-html-mode" value="cdn" bind:group={mode} />
                <span>
                    <span class="font-medium">{t("exportHtmlModal.optionCdnTitle")}</span>
                    <span class="text-surface-600-400"> — {t("exportHtmlModal.optionCdnDetail")}</span>
                </span>
            </label>
            <label class="flex items-start gap-2 text-sm cursor-pointer">
                <input type="radio" class="mt-0.5" name="export-html-mode" value="zip" bind:group={mode} />
                <span>
                    <span class="font-medium">{t("exportHtmlModal.optionZipTitle")}</span>
                    <span class="text-surface-600-400"> — {t("exportHtmlModal.optionZipDetail")}</span>
                </span>
            </label>
        </div>

        <p class="text-xs text-surface-500">{t("exportHtmlModal.footnote")}</p>

        {#if error}<p class="text-xs text-error-500">{error}</p>{/if}

        <button
            type="button"
            class="btn btn-sm preset-filled-primary-500 self-start"
            onclick={handleExport}
            disabled={exporting}
        >{exporting ? t("exportHtmlModal.exporting") : t("exportHtmlModal.exportButton")}</button>
    </div>
</div>
