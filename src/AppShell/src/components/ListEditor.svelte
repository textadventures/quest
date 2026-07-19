<script lang="ts">
    import { addListItem, removeListItem, updateListItem } from "$lib/editor-store";

    interface Props {
        elementKey: string;
        attribute: string;
        value: string | null;
        addPrompt?: string;
    }

    let { elementKey, attribute, value, addPrompt = "Add item…" }: Props = $props();

    let items = $derived.by<{key: string, value: string}[]>(() => {
        try { return JSON.parse(value ?? "[]"); } catch { return []; }
    });

    let editingItem = $state<{key: string, value: string} | null>(null);
    let newItemValue = $state("");

    function commitEdit() {
        if (editingItem) {
            updateListItem(elementKey, attribute, editingItem.key, editingItem.value);
            editingItem = null;
        }
    }

    function onAdd() {
        if (newItemValue.trim()) {
            addListItem(elementKey, attribute, newItemValue.trim());
            newItemValue = "";
        }
    }
</script>

<div class="flex flex-col gap-1 w-full">
    {#each items as item (item.key)}
        <div class="flex items-center gap-1">
            {#if editingItem?.key === item.key}
                <input
                    type="text"
                    class="input text-xs py-0.5 px-1.5 flex-1"
                    value={editingItem.value}
                    oninput={(e) => { if (editingItem) editingItem.value = (e.target as HTMLInputElement).value; }}
                    onkeydown={(e) => {
                        if (e.key === "Enter") commitEdit();
                        else if (e.key === "Escape") editingItem = null;
                    }}
                    onblur={commitEdit}
                />
            {:else}
                <button
                    type="button"
                    class="text-xs flex-1 text-left px-1.5 py-0.5 hover:text-primary-600-400"
                    onclick={() => { editingItem = { key: item.key, value: item.value }; }}
                >{item.value}</button>
            {/if}
            <button
                type="button"
                class="btn btn-sm preset-outlined-error-500 text-xs px-1.5 py-0.5"
                onclick={() => removeListItem(elementKey, attribute, item.key)}
            >✕</button>
        </div>
    {/each}
    <div class="flex items-center gap-1 mt-0.5">
        <input
            type="text"
            class="input text-xs py-0.5 px-1.5 flex-1"
            placeholder={addPrompt}
            data-staging
            bind:value={newItemValue}
            onkeydown={(e) => { if (e.key === "Enter") onAdd(); }}
        />
        <button
            type="button"
            class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5"
            onclick={onAdd}
        >Add</button>
    </div>
</div>
