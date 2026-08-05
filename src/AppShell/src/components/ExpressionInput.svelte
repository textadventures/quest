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
    // These fields commonly sit inside a scrolled panel (a script row, a property tab). An
    // absolutely-positioned popover is still clipped by that panel's overflow regardless of
    // z-index, so the popover is portaled to <body> (see `portal` below) and positioned with
    // `fixed` viewport coordinates computed from the button's rect instead.
    let popoverStyle = $state("");

    function portal(node: HTMLElement) {
        document.body.appendChild(node);
        return {
            destroy() {
                node.remove();
            },
        };
    }

    // Opens downward by default, flipping upward when there isn't room below — mirrors
    // Combobox.svelte's heuristic. Re-run on scroll/resize while open so the popover tracks the
    // button if the underlying panel scrolls.
    function updatePosition() {
        if (!buttonEl) return;
        const rect = buttonEl.getBoundingClientRect();
        const approxPopoverHeight = 300;
        const spaceBelow = window.innerHeight - rect.bottom;
        const spaceAbove = rect.top;
        const openUpward = spaceBelow < approxPopoverHeight && spaceAbove > spaceBelow;
        const top = openUpward ? rect.top - 4 : rect.bottom + 4;
        popoverStyle = `position: fixed; left: ${rect.right}px; top: ${top}px; transform: translate(-100%, ${openUpward ? "-100%" : "0"});`;
    }

    // A built-in function and a library-defined one can share a name (e.g. "Ask" — a native
    // ExpressionOwner.Ask(caption) plus a CoreFunctions.aslx Ask(question, callback)). Within an
    // expression that's not actually ambiguous: NcalcExpressionEvaluator tries the native
    // ExpressionOwner method by name first and only falls through to WorldModel.Procedure() if no
    // method of that name exists at all — so the built-in always shadows a same-named library
    // function here, and listing both would (a) violate the {#each} block's per-name key and
    // (b) offer an entry that, if inserted, wouldn't run what it appears to call.
    function dedupeByName(fns: ExpressionFunctionInfo[]): ExpressionFunctionInfo[] {
        // Plain object, not Map — this is a throwaway local lookup discarded when the function
        // returns, not component state, so there's no Svelte-reactivity concern here.
        const byName: Record<string, ExpressionFunctionInfo> = {};
        for (const fn of fns) {
            const existing = byName[fn.name];
            if (!existing || (existing.isUserDefined && !fn.isUserDefined)) {
                byName[fn.name] = fn;
            }
        }
        return Object.values(byName);
    }

    function toggle() {
        if (!open) {
            functions = dedupeByName(getExpressionFunctions());
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
    // Game-authored functions are more likely to be what the author wants to call than a
    // built-in or library one, so they get their own section ahead of everything else.
    let filteredGameFunctions = $derived(filteredFunctions.filter(f => f.isUserDefined && !f.isLibrary));
    let filteredLibraryFunctions = $derived(filteredFunctions.filter(f => !f.isUserDefined || f.isLibrary));

    $effect(() => {
        if (!open) return;
        updatePosition();
        function onOutside(e: MouseEvent) {
            const target = e.target as Node;
            if (buttonEl?.contains(target) || popoverRootEl?.contains(target)) return;
            close();
        }
        function onKeydown(e: KeyboardEvent) {
            if (e.key === "Escape") close();
        }
        function onReposition() {
            updatePosition();
        }
        document.addEventListener("mousedown", onOutside);
        document.addEventListener("keydown", onKeydown);
        // capture:true so this also fires for scrolls inside nested scroll containers, not just
        // the window itself.
        window.addEventListener("scroll", onReposition, true);
        window.addEventListener("resize", onReposition);
        return () => {
            document.removeEventListener("mousedown", onOutside);
            document.removeEventListener("keydown", onKeydown);
            window.removeEventListener("scroll", onReposition, true);
            window.removeEventListener("resize", onReposition);
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
    <button
        bind:this={buttonEl}
        type="button"
        class="btn btn-sm preset-outlined-primary-500 px-1 py-0.5 flex-shrink-0"
        title="Insert object or function"
        onclick={toggle}
    ><Wand2 size={13} aria-hidden="true" /></button>
    {#if open}
        <div
            bind:this={popoverRootEl}
            use:portal
            style={popoverStyle}
            class="z-50 w-64 max-h-72 flex flex-col rounded border border-surface-200-800 bg-white dark:bg-surface-800 shadow-lg"
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
                {#if filteredGameFunctions.length > 0}
                    <div class="px-2 pt-1.5 pb-0.5 text-[10px] font-semibold uppercase text-surface-600-400 sticky top-0 bg-white dark:bg-surface-800">Game functions</div>
                    {#each filteredGameFunctions as fn (fn.name)}
                        <button
                            type="button"
                            class="block w-full text-left px-2 py-1 text-xs hover:bg-surface-100-900 truncate"
                            onclick={() => insertFunction(fn)}
                        >{fn.name}<span class="text-surface-600-400">({fn.parameters.join(", ")})</span></button>
                    {/each}
                {/if}
                {#if filteredLibraryFunctions.length > 0}
                    <div class="px-2 pt-1.5 pb-0.5 text-[10px] font-semibold uppercase text-surface-600-400 sticky top-0 bg-white dark:bg-surface-800">Library &amp; built-in functions</div>
                    {#each filteredLibraryFunctions as fn (fn.name)}
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
