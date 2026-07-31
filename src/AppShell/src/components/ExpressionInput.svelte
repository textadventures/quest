<script lang="ts">
    import Wand2 from "@lucide/svelte/icons/wand-2";
    import { getExpressionFunctions } from "$lib/editor-store";
    import type { ExpressionFunctionInfo } from "$lib/types";

    interface Props {
        value: string;
        onchange: (value: string) => void;
        // Object names to offer in the "Objects" section — callers that already have this list
        // loaded (e.g. ScriptEditor's script-block state) should pass it through rather than
        // have every field independently re-fetch it.
        objectNames: string[];
        class?: string;
    }

    let { value, onchange, objectNames, class: className = "" }: Props = $props();

    let inputEl = $state<HTMLInputElement | undefined>();
    let buttonEl = $state<HTMLButtonElement | undefined>();
    let popoverRootEl = $state<HTMLDivElement | undefined>();
    let open = $state(false);
    let filter = $state("");
    let functions = $state<ExpressionFunctionInfo[]>([]);
    // Defaults to opening downward, but flips upward when there isn't room below — same
    // heuristic as Combobox.svelte, needed since these fields often sit near the bottom of a
    // scrolled panel (e.g. a script's last parameter, or the last property in a tab).
    let openUpward = $state(false);

    function updateOpenDirection() {
        if (!buttonEl) return;
        const rect = buttonEl.getBoundingClientRect();
        const spaceBelow = window.innerHeight - rect.bottom;
        const spaceAbove = rect.top;
        openUpward = spaceBelow < 300 && spaceAbove > spaceBelow;
    }

    function toggle() {
        if (!open) {
            functions = getExpressionFunctions();
            filter = "";
            updateOpenDirection();
        }
        open = !open;
    }

    function close() {
        open = false;
    }

    // Splices text into the field at the current cursor position (falling back to the end if
    // the field never had focus), then restores focus/cursor there once Svelte has re-rendered
    // the now-longer value.
    function insertAtCursor(text: string) {
        const start = inputEl?.selectionStart ?? value.length;
        const end = inputEl?.selectionEnd ?? value.length;
        const newValue = value.slice(0, start) + text + value.slice(end);
        const cursorPos = start + text.length;
        close();
        onchange(newValue);
        requestAnimationFrame(() => {
            inputEl?.focus();
            inputEl?.setSelectionRange(cursorPos, cursorPos);
        });
    }

    function insertFunction(fn: ExpressionFunctionInfo) {
        insertAtCursor(`${fn.name}(${fn.parameters.join(", ")})`);
    }

    let filteredObjects = $derived(
        filter === "" ? objectNames : objectNames.filter(n => n.toLowerCase().includes(filter.toLowerCase()))
    );
    let filteredFunctions = $derived(
        filter === "" ? functions : functions.filter(f => f.name.toLowerCase().includes(filter.toLowerCase()))
    );

    $effect(() => {
        if (!open) return;
        function onOutside(e: MouseEvent) {
            const target = e.target as Node;
            if (buttonEl?.contains(target) || popoverRootEl?.contains(target)) return;
            close();
        }
        function onKeydown(e: KeyboardEvent) {
            if (e.key === "Escape") close();
        }
        document.addEventListener("mousedown", onOutside);
        document.addEventListener("keydown", onKeydown);
        return () => {
            document.removeEventListener("mousedown", onOutside);
            document.removeEventListener("keydown", onKeydown);
        };
    });
</script>

<div class="flex items-center gap-1 min-w-0 flex-1">
    <input
        bind:this={inputEl}
        type="text"
        autocapitalize="off"
        class={className || "input text-xs py-0.5 px-1.5 w-full"}
        {value}
        onchange={(e) => onchange((e.target as HTMLInputElement).value)}
    />
    <div class="relative flex-shrink-0">
        <button
            bind:this={buttonEl}
            type="button"
            class="btn btn-sm preset-outlined-primary-500 px-1 py-0.5"
            title="Insert object or function"
            onclick={toggle}
        ><Wand2 size={13} aria-hidden="true" /></button>
        {#if open}
            <div
                bind:this={popoverRootEl}
                class="absolute z-50 right-0 w-64 max-h-72 flex flex-col rounded border border-surface-200-800 bg-white dark:bg-surface-800 shadow-lg {openUpward ? "bottom-full mb-1" : "top-full mt-1"}"
            >
                <input
                    type="text"
                    autocapitalize="off"
                    placeholder="Filter…"
                    class="input text-xs py-1 px-2 rounded-none border-0 border-b border-surface-200-800"
                    bind:value={filter}
                />
                <div class="overflow-y-auto flex-1">
                    {#if filteredObjects.length > 0}
                        <div class="px-2 pt-1.5 pb-0.5 text-[10px] font-semibold uppercase text-surface-600-400 sticky top-0 bg-white dark:bg-surface-800">Objects</div>
                        {#each filteredObjects as name (name)}
                            <button
                                type="button"
                                class="block w-full text-left px-2 py-1 text-xs hover:bg-surface-100-900 truncate"
                                onclick={() => insertAtCursor(name)}
                            >{name}</button>
                        {/each}
                    {/if}
                    {#if filteredFunctions.length > 0}
                        <div class="px-2 pt-1.5 pb-0.5 text-[10px] font-semibold uppercase text-surface-600-400 sticky top-0 bg-white dark:bg-surface-800">Functions</div>
                        {#each filteredFunctions as fn (fn.name)}
                            <button
                                type="button"
                                class="block w-full text-left px-2 py-1 text-xs hover:bg-surface-100-900 truncate"
                                onclick={() => insertFunction(fn)}
                            >{fn.name}<span class="text-surface-600-400">({fn.parameters.join(", ")})</span></button>
                        {/each}
                    {/if}
                    {#if filteredObjects.length === 0 && filteredFunctions.length === 0}
                        <p class="px-2 py-2 text-xs text-surface-600-400 italic">No matches.</p>
                    {/if}
                </div>
            </div>
        {/if}
    </div>
</div>
