<script lang="ts">
    import { getMovePossibleParents, treeNodes } from "$lib/editor-store";
    import { t } from "$lib/i18n";
    import Combobox from "$components/Combobox.svelte";
    import type { ControlOption } from "$lib/types";

    interface Props {
        elementKey: string;
        onconfirm: (parent: string) => void;
        oncancel: () => void;
    }

    const { elementKey, onconfirm, oncancel }: Props = $props();

    // "_objects"/"_walkthrough" un-parent the element back to the top level (mirrors
    // MoveElement's own special-casing of those keys) — always offered first
    // regardless of what GetMovePossibleParents itself returns. Which sentinel
    // applies depends on the moved element's own type.
    let topLevelKey = $derived(
        $treeNodes.find(n => n.key === elementKey)?.nodeType === "walkthrough" ? "_walkthrough" : "_objects"
    );
    let options: ControlOption[] = $derived([
        { value: topLevelKey, label: t("moveElementModal.topLevel") },
        ...getMovePossibleParents(elementKey).map(name => ({ value: name, label: name })),
    ]);

    let target = $state("");
    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { dialogEl?.focus(); });

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Enter" && target) confirm();
        if (e.key === "Escape") oncancel();
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    function confirm() {
        if (!target) return;
        onconfirm(target);
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
>
    <div class="card bg-white rounded-xl shadow-xl w-full max-w-80 p-6 flex flex-col gap-4">
        <h2 class="text-base font-semibold">{t("moveElementModal.heading", { name: elementKey })}</h2>

        <div class="flex flex-col gap-1">
            <span class="text-xs text-surface-600-400">{t("moveElementModal.newParentLabel")}</span>
            <Combobox
                {options}
                value={target}
                onchange={(v) => { target = v; }}
                class="input bg-white px-2 py-1 text-sm w-full"
            />
        </div>

        <div class="flex justify-end gap-2">
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>{t("common.cancel")}</button>
            <button
                class="btn btn-sm preset-filled-primary-500"
                onclick={confirm}
                disabled={!target}
            >
                {t("moveElementModal.moveButton")}
            </button>
        </div>
    </div>
</div>
