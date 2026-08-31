<script lang="ts">
    import { addListItem, removeListItem, updateListItem, recordWalkthrough, playWalkthrough } from "$lib/editor-store";
    import { t } from "$lib/i18n";

    interface Props {
        elementKey: string;
        attribute: string;
        value: string | null;
        addPrompt?: string;
        isWalkthrough?: boolean;
    }

    let { elementKey, attribute, value, addPrompt = t("listEditor.defaultAddPrompt"), isWalkthrough = false }: Props = $props();

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
    {#if isWalkthrough}
        <div class="flex justify-end gap-1 mb-1">
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5"
                onclick={() => playWalkthrough(elementKey)}
                disabled={items.length === 0}
                title={t("listEditor.playTitle")}
            >▶ {t("listEditor.playButton")}</button>
            <button
                type="button"
                class="btn btn-sm preset-outlined-error-500 text-xs px-2 py-0.5"
                onclick={() => recordWalkthrough(elementKey)}
                title={t("listEditor.recordTitle")}
            >● {t("listEditor.recordButton")}</button>
        </div>
    {/if}
    {#each items as item (item.key)}
        <div class="flex items-center gap-1">
            {#if editingItem?.key === item.key}
                <input
                    type="text"
                    autocapitalize="off"
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
                title={t("common.delete")}
                aria-label={t("common.delete")}
                onclick={() => removeListItem(elementKey, attribute, item.key)}
            >✕</button>
        </div>
    {/each}
    <div class="flex items-center gap-1 mt-0.5">
        <input
            type="text"
            autocapitalize="off"
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
        >{t("common.add")}</button>
    </div>
</div>
