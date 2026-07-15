<script lang="ts">
    import type { ScriptCategoryInfo, ScriptCommandInfo } from "$lib/types";

    interface Props {
        categories: ScriptCategoryInfo[];
        onAdd: (createString: string) => void;
        onClose: () => void;
    }

    let { categories, onAdd, onClose }: Props = $props();

    let dialogEl: HTMLDivElement;
    $effect(() => { dialogEl?.focus(); });

    const SHORTCUT_KEYWORDS: string[] = [
        "msg",
        "(function)AddToInventory",
        "(function)MoveObject",
        "(function)MakeObjectVisible",
        "(function)MakeObjectInvisible",
        "if",
    ];
    const SHORTCUT_LABELS: Record<string, string> = {
        "msg": "Print",
        "(function)AddToInventory": "Inventory",
        "(function)MoveObject": "Move",
        "(function)MakeObjectVisible": "Show",
        "(function)MakeObjectInvisible": "Hide",
        "if": "If",
    };

    const shortcuts = $derived(
        SHORTCUT_KEYWORDS.flatMap((keyword) => {
            for (const cat of categories) {
                const cmd = cat.commands.find((c) => c.keyword === keyword);
                if (cmd) return [{ label: SHORTCUT_LABELS[keyword] ?? cmd.add, createString: cmd.createString }];
            }
            return [];
        })
    );

    let selectedCategoryIndex = $state(0);
    let selectedCommand = $state<ScriptCommandInfo | null>(null);

    const selectedCategory = $derived(categories[selectedCategoryIndex] ?? null);

    // Auto-select first command when category changes
    $effect(() => {
        const cat = selectedCategory;
        selectedCommand = cat && cat.commands.length > 0 ? cat.commands[0] : null;
    });

    function onShortcutClick(createString: string) {
        onAdd(createString);
        onClose();
    }

    function onOk() {
        if (selectedCommand) {
            onAdd(selectedCommand.createString);
            onClose();
        }
    }

    function onKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") onClose();
        if (e.key === "Enter" && selectedCommand) onOk();
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) onClose();
    }
</script>

<!-- svelte-ignore a11y_no_noninteractive_element_interactions -->
<div
    bind:this={dialogEl}
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50"
    onclick={onBackdropClick}
    onkeydown={onKeydown}
>
    <div class="bg-white rounded-xl shadow-xl w-[600px] h-[520px] flex flex-col overflow-hidden ring-1 ring-surface-200-800">

        <!-- Header -->
        <div class="px-5 py-3 flex items-center justify-between flex-shrink-0 border-b border-surface-200-800">
            <h2 class="font-semibold text-base">Add Script Command</h2>
            <button
                type="button"
                onclick={onClose}
                class="text-surface-400-500 hover:text-surface-900-50 text-xl leading-none px-1 transition-colors"
                aria-label="Close"
            >×</button>
        </div>

        <!-- Shortcut buttons -->
        {#if shortcuts.length > 0}
            <div class="px-5 py-3 border-b border-surface-200-800 flex gap-2 flex-wrap flex-shrink-0">
                <span class="text-xs font-medium text-surface-500-400 self-center mr-1">Quick add:</span>
                {#each shortcuts as shortcut (shortcut.createString)}
                    <button
                        type="button"
                        onclick={() => onShortcutClick(shortcut.createString)}
                        class="btn btn-sm preset-outlined-primary-500 rounded-full px-4 py-1 text-sm font-medium"
                    >{shortcut.label}</button>
                {/each}
            </div>
        {/if}

        <!-- Body: categories + commands -->
        <div class="flex flex-1 min-h-0">
            <!-- Category sidebar -->
            <div class="w-40 border-r border-surface-200-800 overflow-y-auto flex-shrink-0">
                {#each categories as cat, ci (ci)}
                    <button
                        type="button"
                        onclick={() => { selectedCategoryIndex = ci; }}
                        class="w-full text-left px-4 py-2.5 text-sm transition-colors {
                            selectedCategoryIndex === ci
                                ? "font-semibold text-primary-600-400 bg-primary-100 dark:bg-primary-900/40"
                                : "text-surface-700-300 hover:bg-surface-100-900"
                        }"
                    >{cat.name}</button>
                {/each}
            </div>

            <!-- Commands list -->
            <div class="flex-1 overflow-y-auto pb-2">
                {#if selectedCategory}
                    {#each selectedCategory.commands as cmd (cmd.createString)}
                        {@const isSelected = selectedCommand?.createString === cmd.createString}
                        <button
                            type="button"
                            onclick={() => (selectedCommand = cmd)}
                            ondblclick={() => { selectedCommand = cmd; onOk(); }}
                            class="w-full text-left px-5 py-2 text-sm flex items-center gap-3 transition-colors {
                                isSelected
                                    ? "bg-primary-100 dark:bg-primary-900/40 text-primary-700 dark:text-primary-300 font-medium"
                                    : "text-surface-700-300 hover:bg-surface-100-900"
                            }"
                        >
                            <span class="w-4 flex-shrink-0 text-primary-500 {isSelected ? "opacity-100" : "opacity-0"}">●</span>
                            {cmd.add}
                        </button>
                    {/each}
                {/if}
            </div>
        </div>

        <!-- Footer -->
        <div class="px-5 py-3 border-t border-surface-200-800 flex justify-end gap-3 flex-shrink-0">
            <button type="button" onclick={onClose} class="btn preset-tonal">Cancel</button>
            <button
                type="button"
                onclick={onOk}
                class="btn preset-filled-primary-500"
                disabled={!selectedCommand}
            >OK</button>
        </div>
    </div>
</div>
