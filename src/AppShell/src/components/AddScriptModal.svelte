<script lang="ts">
    import type { ScriptCategoryInfo, ScriptCommandInfo } from "$lib/types";
    import Search from "@lucide/svelte/icons/search";
    import X from "@lucide/svelte/icons/x";

    interface Props {
        categories: ScriptCategoryInfo[];
        onAdd: (createString: string) => void;
        onClose: () => void;
    }

    let { categories, onAdd, onClose }: Props = $props();

    let dialogEl: HTMLDivElement;

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

    // ── Filtering ──────────────────────────────────────────────────────────────
    // While the box has text, the category sidebar + single-category list is
    // replaced by one flat list of matches across every category, each tagged
    // with its category name, so authors don't need to know where to look.

    let filterText = $state("");
    let filterInputEl = $state<HTMLInputElement | undefined>();
    $effect(() => { filterInputEl?.focus(); });
    const isFiltering = $derived(filterText.trim().length > 0);

    interface MatchedCommand extends ScriptCommandInfo {
        categoryName: string;
    }

    const filteredCommands = $derived.by((): MatchedCommand[] => {
        const query = filterText.trim().toLowerCase();
        if (!query) return [];
        const matches: MatchedCommand[] = [];
        for (const cat of categories) {
            for (const cmd of cat.commands) {
                if (cmd.add.toLowerCase().includes(query)) {
                    matches.push({ ...cmd, categoryName: cat.name });
                }
            }
        }
        return matches;
    });

    // Auto-select first command whenever the active list changes (category
    // switch, or entering/leaving/typing further into the filter)
    $effect(() => {
        if (isFiltering) {
            selectedCommand = filteredCommands[0] ?? null;
        } else {
            const cat = selectedCategory;
            selectedCommand = cat && cat.commands.length > 0 ? cat.commands[0] : null;
        }
    });

    function clearFilter() {
        filterText = "";
        filterInputEl?.focus();
    }

    // ── Keyboard navigation ────────────────────────────────────────────────────
    // Both the flat filtered list and the per-category list are custom listboxes
    // (plain buttons, not a native <select>), so Up/Down to move the selection
    // isn't free — wire it up here rather than only supporting mouse clicks.

    const activeList = $derived<ScriptCommandInfo[]>(isFiltering ? filteredCommands : (selectedCategory?.commands ?? []));
    const selectedIndex = $derived(
        selectedCommand ? activeList.findIndex((c) => c.createString === selectedCommand!.createString) : -1
    );

    let rowEls = $state<(HTMLButtonElement | undefined)[]>([]);

    function moveSelection(delta: number) {
        const list = activeList;
        if (list.length === 0) return;
        const nextIndex = Math.min(Math.max(selectedIndex + delta, 0), list.length - 1);
        selectedCommand = list[nextIndex];
        rowEls[nextIndex]?.scrollIntoView({ block: "nearest" });
    }

    // Up/Down while focus is inside the category sidebar moves between categories instead of
    // between commands — otherwise it's indistinguishable from being focused on the command
    // list, since both are plain buttons and the general handler below can't tell them apart.
    let categoryButtonEls = $state<(HTMLButtonElement | undefined)[]>([]);

    function onCategoryListKeydown(e: KeyboardEvent) {
        if (e.key === "ArrowRight") {
            // Jump across to the command list, landing on whichever row is already selected.
            e.preventDefault();
            e.stopPropagation();
            rowEls[selectedIndex]?.focus();
            return;
        }
        if (e.key !== "ArrowDown" && e.key !== "ArrowUp") return;
        e.preventDefault();
        e.stopPropagation();
        const delta = e.key === "ArrowDown" ? 1 : -1;
        const nextIndex = Math.min(Math.max(selectedCategoryIndex + delta, 0), categories.length - 1);
        selectedCategoryIndex = nextIndex;
        categoryButtonEls[nextIndex]?.focus();
    }

    // Left, with focus in the (non-filtered) command list, jumps back to the category sidebar —
    // there's no equivalent list to jump to from the flat filtered list, since it has no sidebar.
    function onCategoryCommandListKeydown(e: KeyboardEvent) {
        if (e.key !== "ArrowLeft") return;
        e.preventDefault();
        e.stopPropagation();
        categoryButtonEls[selectedCategoryIndex]?.focus();
    }

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
        if (e.key === "Escape") {
            if (filterText) {
                e.stopPropagation();
                clearFilter();
                return;
            }
            onClose();
        }
        if (e.key === "Enter" && selectedCommand) onOk();
        if (e.key === "ArrowDown") { e.preventDefault(); moveSelection(1); }
        if (e.key === "ArrowUp") { e.preventDefault(); moveSelection(-1); }
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) onClose();
    }
</script>

<div
    bind:this={dialogEl}
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50 p-4"
    onclick={onBackdropClick}
    onkeydown={onKeydown}
>
    <div class="bg-white rounded-xl shadow-xl w-full max-w-[600px] h-[min(520px,85dvh)] flex flex-col overflow-hidden ring-1 ring-surface-200-800">

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

        <!-- Filter box -->
        <div class="px-5 py-2 border-b border-surface-200-800 flex-shrink-0">
            <div class="relative">
                <Search class="absolute left-2.5 top-1/2 -translate-y-1/2 size-3.5 text-surface-400 pointer-events-none" />
                <input
                    bind:this={filterInputEl}
                    type="text"
                    autocapitalize="off"
                    bind:value={filterText}
                    placeholder="Filter commands..."
                    aria-label="Filter script commands"
                    class="input text-sm py-1.5 pl-8 pr-7 w-full"
                />
                {#if filterText}
                    <button
                        type="button"
                        class="absolute right-2 top-1/2 -translate-y-1/2 size-4 flex items-center justify-center text-surface-400 hover:text-surface-900-50"
                        onclick={clearFilter}
                        aria-label="Clear filter"
                    ><X class="size-3.5" /></button>
                {/if}
            </div>
        </div>

        <!-- Body: categories + commands, or a flat filtered list -->
        <div class="flex flex-1 min-h-0">
            {#if isFiltering}
                <!-- Flat filtered list across all categories -->
                <div class="flex-1 overflow-y-auto pb-2" role="listbox" aria-label="Matching script commands">
                    {#if filteredCommands.length === 0}
                        <div class="px-5 py-3 text-sm text-surface-400">No matching commands</div>
                    {/if}
                    {#each filteredCommands as cmd, idx (cmd.createString)}
                        {@const isSelected = selectedCommand?.createString === cmd.createString}
                        <button
                            bind:this={rowEls[idx]}
                            type="button"
                            role="option"
                            aria-selected={isSelected}
                            tabindex={isSelected ? 0 : -1}
                            onclick={() => (selectedCommand = cmd)}
                            ondblclick={() => { selectedCommand = cmd; onOk(); }}
                            class="w-full text-left px-5 py-2 text-sm flex items-center gap-3 transition-colors {
                                isSelected
                                    ? "bg-primary-100 dark:bg-primary-900/40 text-primary-700 dark:text-primary-300 font-medium"
                                    : "text-surface-700-300 hover:bg-surface-100-900"
                            }"
                        >
                            <span class="w-4 flex-shrink-0 text-primary-500 {isSelected ? "opacity-100" : "opacity-0"}">●</span>
                            <span class="flex-1">{cmd.add}</span>
                            <span class="flex-shrink-0 text-xs text-surface-400">{cmd.categoryName}</span>
                        </button>
                    {/each}
                </div>
            {:else}
                <!-- Category sidebar -->
                <div
                    class="w-40 border-r border-surface-200-800 overflow-y-auto flex-shrink-0"
                    role="listbox"
                    aria-label="Script command categories"
                    tabindex="-1"
                    onkeydown={onCategoryListKeydown}
                >
                    {#each categories as cat, ci (ci)}
                        {@const isSelected = selectedCategoryIndex === ci}
                        <button
                            bind:this={categoryButtonEls[ci]}
                            type="button"
                            role="option"
                            aria-selected={isSelected}
                            tabindex={isSelected ? 0 : -1}
                            onclick={() => { selectedCategoryIndex = ci; }}
                            class="w-full text-left px-4 py-2.5 text-sm transition-colors {
                                isSelected
                                    ? "font-semibold text-primary-600-400 bg-primary-100 dark:bg-primary-900/40"
                                    : "text-surface-700-300 hover:bg-surface-100-900"
                            }"
                        >{cat.name}</button>
                    {/each}
                </div>

                <!-- Commands list -->
                <div
                    class="flex-1 overflow-y-auto pb-2"
                    role="listbox"
                    aria-label="Script commands in {selectedCategory?.name ?? ""}"
                    tabindex="-1"
                    onkeydown={onCategoryCommandListKeydown}
                >
                    {#if selectedCategory}
                        {#each selectedCategory.commands as cmd, idx (cmd.createString)}
                            {@const isSelected = selectedCommand?.createString === cmd.createString}
                            <button
                                bind:this={rowEls[idx]}
                                type="button"
                                role="option"
                                aria-selected={isSelected}
                                tabindex={isSelected ? 0 : -1}
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
            {/if}
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
