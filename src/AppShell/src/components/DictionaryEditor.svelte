<script lang="ts">
    import { addDictItem, removeDictItem, updateDictItem } from "$lib/editor-store";

    interface Props {
        elementKey: string;
        attribute: string;
        value: string | null;
    }

    let { elementKey, attribute, value }: Props = $props();

    let items = $derived.by<{key: string, value: string}[]>(() => {
        try { return JSON.parse(value ?? "[]"); } catch { return []; }
    });

    let editingKey = $state<string | null>(null);
    let editingValue = $state("");

    let newKey = $state("");
    let newValue = $state("");

    function startEdit(key: string, currentValue: string) {
        editingKey = key;
        editingValue = currentValue;
    }

    function commitEdit() {
        if (editingKey) {
            updateDictItem(elementKey, attribute, editingKey, editingValue);
        }
        editingKey = null;
        editingValue = "";
    }

    function onAdd() {
        if (newKey.trim()) {
            addDictItem(elementKey, attribute, newKey.trim(), newValue);
            newKey = "";
            newValue = "";
        }
    }
</script>

<div class="flex flex-col gap-1 w-full">
    {#each items as item (item.key)}
        <div class="flex items-center gap-1">
            <span class="text-xs text-surface-500-400 px-1 flex-shrink-0">{item.key}:</span>
            {#if editingKey === item.key}
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0.5 px-1.5 flex-1"
                    value={editingValue}
                    oninput={(e) => { editingValue = (e.target as HTMLInputElement).value; }}
                    onkeydown={(e) => {
                        if (e.key === "Enter") commitEdit();
                        else if (e.key === "Escape") { editingKey = null; }
                    }}
                    onblur={commitEdit}
                />
            {:else}
                <button
                    type="button"
                    class="text-xs flex-1 text-left px-1.5 py-0.5 hover:text-primary-600-400 truncate"
                    onclick={() => startEdit(item.key, item.value)}
                >{item.value}</button>
            {/if}
            <button
                type="button"
                class="btn btn-sm preset-outlined-error-500 text-xs px-1.5 py-0.5 flex-shrink-0"
                onclick={() => removeDictItem(elementKey, attribute, item.key)}
            >✕</button>
        </div>
    {/each}
    <div class="flex items-center gap-1 mt-0.5">
        <input
            type="text"
            autocapitalize="off"
            class="input text-xs py-0.5 px-1.5 w-20 flex-shrink-0"
            placeholder="Key"
            data-staging
            bind:value={newKey}
            onkeydown={(e) => { if (e.key === "Enter") onAdd(); }}
        />
        <input
            type="text"
            autocapitalize="off"
            class="input text-xs py-0.5 px-1.5 flex-1"
            placeholder="Value"
            data-staging
            bind:value={newValue}
            onkeydown={(e) => { if (e.key === "Enter") onAdd(); }}
        />
        <button
            type="button"
            class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0"
            onclick={onAdd}
        >Add</button>
    </div>
</div>
