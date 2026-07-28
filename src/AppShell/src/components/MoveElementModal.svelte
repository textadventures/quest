<script lang="ts">
    import { getMovePossibleParents } from "$lib/editor-store";
    import Combobox from "$components/Combobox.svelte";
    import type { ControlOption } from "$lib/types";

    interface Props {
        elementKey: string;
        onconfirm: (parent: string) => void;
        oncancel: () => void;
    }

    const { elementKey, onconfirm, oncancel }: Props = $props();

    // "_objects" un-parents the element back to the top level (mirrors MoveElement's
    // own special-case for that key) — always offered first regardless of what
    // GetMovePossibleParents itself returns.
    let options: ControlOption[] = $derived([
        { value: "_objects", label: "(top level)" },
        ...getMovePossibleParents(elementKey).map(name => ({ value: name, label: name })),
    ]);

    let target = $state("");

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
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50 p-4"
    onclick={onBackdropClick}
    onkeydown={handleKeydown}
>
    <div class="card bg-white rounded-xl shadow-xl w-full max-w-80 p-6 flex flex-col gap-4">
        <h2 class="text-base font-semibold">Move "{elementKey}"</h2>

        <div class="flex flex-col gap-1">
            <span class="text-xs text-surface-500-400">New parent</span>
            <Combobox
                {options}
                value={target}
                onchange={(v) => { target = v; }}
                class="input bg-white px-2 py-1 text-sm w-full"
            />
        </div>

        <div class="flex justify-end gap-2">
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>Cancel</button>
            <button
                class="btn btn-sm preset-filled-primary-500"
                onclick={confirm}
                disabled={!target}
            >
                Move
            </button>
        </div>
    </div>
</div>
