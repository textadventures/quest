<script lang="ts">
    import { addScriptDictItem, removeScriptDictItem, makeScriptDictEditable } from "$lib/editor-store";
    import ScriptEditor from "./ScriptEditor.svelte";

    interface Props {
        elementKey: string;
        attribute: string;
        value: string | null;
        isLocked?: boolean;
    }

    let { elementKey, attribute, value, isLocked = false }: Props = $props();

    let keys = $derived.by<string[]>(() => {
        try {
            const items = JSON.parse(value ?? "[]") as {key: string, value: string}[];
            return items.map(i => i.key);
        } catch { return []; }
    });

    let expandedKeys = $state(new Set<string>());
    let newKey = $state("");

    function onAdd() {
        const k = newKey.trim();
        if (k) {
            const result = addScriptDictItem(elementKey, attribute, k);
            if (result === "ok") expandedKeys = new Set([...expandedKeys, k]);
            newKey = "";
        }
    }

    function toggleExpanded(key: string) {
        const next = new Set(expandedKeys);
        if (next.has(key)) next.delete(key); else next.add(key);
        expandedKeys = next;
    }
</script>

<div class="flex flex-col gap-1 w-full">
    {#if isLocked}
        <div class="flex items-center gap-2 py-1 px-2 mb-1 text-xs text-surface-400-500 italic border border-surface-200-800 rounded">
            <span class="flex-1">This script dictionary is inherited — read-only.</span>
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0 not-italic"
                onclick={() => makeScriptDictEditable(elementKey, attribute)}
            >Make editable copy</button>
        </div>
    {/if}
    {#if isLocked}
        {#each keys as key (key)}
            <div class="border border-surface-200-800 rounded px-2 py-1 text-xs text-surface-400-500 opacity-60">{key} <span class="italic">(script)</span></div>
        {/each}
    {:else}
        {#each keys as key (key)}
            <div class="border border-surface-200-800 rounded">
                <div class="flex items-center gap-1 px-2 py-1">
                    <button
                        type="button"
                        class="flex-1 text-left text-xs font-medium hover:text-primary-600-400"
                        onclick={() => toggleExpanded(key)}
                    >
                        <span class="mr-1 text-surface-400-500">{expandedKeys.has(key) ? "▼" : "▶"}</span>{key}
                    </button>
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-error-500 text-xs px-1.5 py-0.5 flex-shrink-0"
                        onclick={() => removeScriptDictItem(elementKey, attribute, key)}
                    >✕</button>
                </div>
                {#if expandedKeys.has(key)}
                    <div class="px-2 pb-2 border-t border-surface-100-900">
                        <ScriptEditor elementKey={elementKey} attribute="{attribute}[{key}]" />
                    </div>
                {/if}
            </div>
        {/each}
        <div class="flex items-center gap-1 mt-0.5">
            <input
                type="text"
                class="input text-xs py-0.5 px-1.5 flex-1"
                placeholder="Add entry key…"
                bind:value={newKey}
                onkeydown={(e) => { if (e.key === "Enter") onAdd(); }}
            />
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0"
                onclick={onAdd}
            >Add</button>
        </div>
    {/if}
</div>
