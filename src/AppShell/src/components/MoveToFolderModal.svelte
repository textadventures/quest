<script lang="ts">
    import { getFunctionFolders } from "$lib/editor-store";
    import { t } from "$lib/i18n";
    import { trapFocus } from "$lib/actions/trapFocus";
    import Combobox from "$components/Combobox.svelte";
    import type { ControlOption } from "$lib/types";

    interface Props {
        elementKey: string;
        onconfirm: (folder: string) => void;
        oncancel: () => void;
    }

    const { elementKey, onconfirm, oncancel }: Props = $props();

    // "" un-assigns the folder, putting the function back at the top level — always offered
    // first. Existing folder names are offered below; typing anything else creates a new one
    // (Combobox already commits free-typed text on blur/Enter, same as MoveElementModal).
    let options: ControlOption[] = $derived([
        { value: "", label: t("moveToFolderModal.topLevel") },
        ...getFunctionFolders().map(name => ({ value: name, label: name })),
    ]);

    let target = $state("");
    let hasChosen = $state(false);
    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { dialogEl?.focus(); });

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Enter" && hasChosen) confirm();
        if (e.key === "Escape") oncancel();
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    function confirm() {
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
    use:trapFocus
>
    <div class="card bg-white rounded-xl shadow-xl w-full max-w-80 p-6 flex flex-col gap-4">
        <h2 class="text-base font-semibold">{t("moveToFolderModal.heading", { name: elementKey })}</h2>

        <div class="flex flex-col gap-1">
            <span class="text-xs text-surface-600-400">{t("moveToFolderModal.folderLabel")}</span>
            <Combobox
                {options}
                value={target}
                onchange={(v) => { target = v; hasChosen = true; }}
                class="input bg-white px-2 py-1 text-sm w-full"
            />
        </div>

        <div class="flex justify-end gap-2">
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>{t("common.cancel")}</button>
            <button
                class="btn btn-sm preset-filled-primary-500"
                onclick={confirm}
                disabled={!hasChosen}
            >
                {t("moveToFolderModal.moveButton")}
            </button>
        </div>
    </div>
</div>
