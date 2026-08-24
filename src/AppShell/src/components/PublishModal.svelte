<script lang="ts">
    import { publishGame, canPublishToServer } from "$lib/editor-store";
    import { t } from "$lib/i18n";
    import { trapFocus } from "$lib/actions/trapFocus";

    interface Props {
        oncancel: () => void;
    }
    const { oncancel }: Props = $props();

    let includeWalkthrough = $state(false);
    let publishing = $state(false);
    let error = $state("");
    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { dialogEl?.focus(); });

    async function handlePublish() {
        publishing = true;
        error = "";
        try {
            await publishGame(includeWalkthrough);
            oncancel();
        } catch (err) {
            error = String(err);
        } finally {
            publishing = false;
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
            <h2 class="text-base font-semibold">{t("publishModal.title")}</h2>
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>{t("common.close")}</button>
        </div>

        <p class="text-sm text-surface-600-400">
            {$canPublishToServer ? t("publishModal.descriptionServer") : t("publishModal.descriptionLocal")}
        </p>

        <label class="flex items-center gap-2 text-sm">
            <input type="checkbox" bind:checked={includeWalkthrough} />
            {t("publishModal.includeWalkthrough")}
        </label>

        {#if error}<p class="text-xs text-error-500">{error}</p>{/if}

        <button
            type="button"
            class="btn btn-sm preset-filled-primary-500 self-start"
            onclick={handlePublish}
            disabled={publishing}
        >{publishing ? t("publishModal.publishing") : $canPublishToServer ? t("publishModal.publishToServer") : t("publishModal.publishButton")}</button>
    </div>
</div>
