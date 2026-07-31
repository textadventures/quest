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
    let rootEl = $state<HTMLDivElement | undefined>();
    let open = $state(false);
    let filter = $state("");
    let functions = $state<ExpressionFunctionInfo[]>([]);

    function toggle() {
        if (!open) {
            functions = getExpressionFunctions();
            filter = "";
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
            if (!rootEl?.contains(e.target as Node)) close();
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

<div class="relative flex items-center gap-1 min-w-0 flex-1" bind:this={rootEl}>
    <input
        bind:this={inputEl}
        type="text"
        autocapitalize="off"
        class={className || "input text-xs py-0.5 px-1.5 w-full"}
        {value}
        onchange={(e) => onchange((e.target as HTMLInputElement).value)}
    />
    <button
        type="button"
        class="btn btn-sm preset-outlined-primary-500 px-1 py-0.5 flex-shrink-0"
        title="Insert object or function"
        onclick={toggle}
    ><Wand2 size={13} aria-hidden="true" /></button>
    {#if open}
        <div class="absolute z-50 right-0 top-full mt-1 w-64 max-h-72 flex flex-col rounded border border-surface-200-800 bg-white dark:bg-surface-800 shadow-lg">
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
