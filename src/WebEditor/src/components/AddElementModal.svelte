<script lang="ts">
    import { validateName } from "$lib/editor-store";

    interface Props {
        elementType: "room" | "object" | "function" | "timer" | "walkthrough" | "template" | "dynamictemplate" | "type";
        parent?: string | null;
        onconfirm: (name: string) => void;
        oncancel: () => void;
    }

    const { elementType, parent = null, onconfirm, oncancel }: Props = $props();

    const labels: Record<string, string> = {
        room: "Room",
        object: "Object",
        function: "Function",
        timer: "Timer",
        walkthrough: "Walkthrough",
        template: "Template",
        dynamictemplate: "Dynamic Template",
        type: "Type",
    };

    let dialogEl: HTMLDivElement;
    let inputEl: HTMLInputElement;

    $effect(() => { inputEl?.focus(); });

    let name = $state("");
    let error = $state("");

    $effect(() => {
        const result = name ? validateName(name) : "";
        error = result === "ok" ? "" : result;
    });

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Enter" && name && !error) confirm();
        if (e.key === "Escape") oncancel();
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    function confirm() {
        if (!name || error) return;
        const result = validateName(name);
        if (result !== "ok") { error = result; return; }
        onconfirm(name);
    }
</script>

<div
    bind:this={dialogEl}
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50"
    onclick={onBackdropClick}
    onkeydown={handleKeydown}
>
    <div class="card bg-white rounded-xl shadow-xl w-80 p-6 flex flex-col gap-4">
        <h2 class="text-base font-semibold">
            Add {labels[elementType]}{parent ? ` in "${parent}"` : ""}
        </h2>

        <div class="flex flex-col gap-1">
            <label for="element-name" class="text-xs text-surface-500-400">Name</label>
            <input
                id="element-name"
                type="text"
                class={"input bg-white px-2 py-1 text-sm" + (error ? " !border-error-500" : "")}
                bind:this={inputEl}
                bind:value={name}
                placeholder="Enter a name..."
            />
            {#if error}
                <p class="text-xs text-error-500">{error}</p>
            {/if}
        </div>

        <div class="flex justify-end gap-2">
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>Cancel</button>
            <button
                class="btn btn-sm preset-filled-primary-500"
                onclick={confirm}
                disabled={!name || !!error}
            >
                Add {labels[elementType]}
            </button>
        </div>
    </div>
</div>
