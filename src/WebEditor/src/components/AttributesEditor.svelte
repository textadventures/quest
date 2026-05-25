<script lang="ts">
    import { fullAttributeData, selectedKey, removeAttribute, addInheritedType, removeInheritedType, getTypeNames, setAttribute } from "$lib/editor-store";
    import type { AttributeDataItem } from "$lib/types";

    let typeNames = $state<string[]>([]);
    let addTypeValue = $state("");
    let newAttrName = $state("");
    let newAttrValue = $state("");
    let selectedAttr = $state<AttributeDataItem | null>(null);
    let editingValue = $state("");

    $effect(() => {
        typeNames = getTypeNames();
    });

    $effect(() => {
        // Reset selection when element changes
        $fullAttributeData;
        selectedAttr = null;
        editingValue = "";
    });

    function availableTypes(): string[] {
        const inherited = new Set($fullAttributeData?.inheritedTypes.map(t => t.name) ?? []);
        return typeNames.filter(t => !inherited.has(t));
    }

    function onDeleteInheritedType(typeName: string) {
        if ($selectedKey) removeInheritedType($selectedKey, typeName);
    }

    function onAddType() {
        if ($selectedKey && addTypeValue) {
            addInheritedType($selectedKey, addTypeValue);
            addTypeValue = "";
        }
    }

    function onDeleteAttribute(attr: AttributeDataItem, e: MouseEvent) {
        e.stopPropagation();
        if ($selectedKey) removeAttribute($selectedKey, attr.name);
        if (selectedAttr?.name === attr.name) selectedAttr = null;
    }

    function onSelectAttribute(attr: AttributeDataItem) {
        selectedAttr = attr;
        editingValue = attr.value ?? "";
    }

    function commitEdit() {
        if ($selectedKey && selectedAttr) {
            setAttribute($selectedKey, selectedAttr.name, "textbox", editingValue);
        }
    }

    function onAddAttribute() {
        if ($selectedKey && newAttrName.trim()) {
            setAttribute($selectedKey, newAttrName.trim(), "textbox", newAttrValue);
            newAttrName = "";
            newAttrValue = "";
        }
    }

    function displayValue(attr: AttributeDataItem): string {
        if (attr.value === null) return "(null)";
        if (attr.value.startsWith("[")) {
            try {
                const items = JSON.parse(attr.value) as {key: string, value: string}[];
                return `(list: ${items.length} item${items.length === 1 ? "" : "s"})`;
            } catch { /* not a list */ }
        }
        return attr.value;
    }

    function isEditable(attr: AttributeDataItem): boolean {
        return attr.value !== "(script)" && !attr.value?.startsWith("[");
    }
</script>

<div class="flex flex-col gap-0 p-3 text-xs">
    <!-- Inherited types -->
    <div class="mb-4">
        <div class="flex items-center gap-2 mb-1">
            <span class="font-semibold text-surface-500-400 uppercase tracking-wide">Inherited types</span>
        </div>
        <div class="flex items-center gap-1 mb-1">
            <select
                class="select text-xs py-0.5 px-1.5 flex-1"
                bind:value={addTypeValue}
            >
                <option value="">Add type…</option>
                {#each availableTypes() as t}
                    <option value={t}>{t}</option>
                {/each}
            </select>
            <button
                type="button"
                disabled={!addTypeValue}
                onclick={onAddType}
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5"
            >Add</button>
        </div>
        <table class="w-full border-collapse">
            <thead>
                <tr class="text-surface-400-500 border-b border-surface-200-800">
                    <th class="text-left py-1 pr-3 font-medium">Name</th>
                    <th class="text-left py-1 pr-3 font-medium">Source</th>
                    <th class="w-6"></th>
                </tr>
            </thead>
            <tbody>
                {#each $fullAttributeData?.inheritedTypes ?? [] as t}
                    <tr class="border-b border-surface-100-900">
                        <td class="py-0.5 pr-3">{t.name}</td>
                        <td class="py-0.5 pr-3 text-surface-400-500">{t.source}</td>
                        <td class="py-0.5 text-right">
                            {#if !t.isDefaultType}
                                <button
                                    type="button"
                                    class="text-error-500 hover:text-error-700 px-1"
                                    onclick={() => onDeleteInheritedType(t.name)}
                                    title="Remove type"
                                >✕</button>
                            {/if}
                        </td>
                    </tr>
                {:else}
                    <tr><td colspan="3" class="py-1 text-surface-400-500 italic">No inherited types</td></tr>
                {/each}
            </tbody>
        </table>
    </div>

    <!-- Attributes -->
    <div>
        <div class="flex items-center gap-2 mb-1">
            <span class="font-semibold text-surface-500-400 uppercase tracking-wide">Attributes</span>
        </div>

        <!-- Add new attribute -->
        <div class="flex items-center gap-1 mb-2">
            <input
                type="text"
                placeholder="Name"
                class="input text-xs py-0.5 px-1.5 w-32"
                bind:value={newAttrName}
                onkeydown={(e) => { if (e.key === "Enter") onAddAttribute(); }}
            />
            <input
                type="text"
                placeholder="Value"
                class="input text-xs py-0.5 px-1.5 flex-1"
                bind:value={newAttrValue}
                onkeydown={(e) => { if (e.key === "Enter") onAddAttribute(); }}
            />
            <button
                type="button"
                disabled={!newAttrName.trim()}
                onclick={onAddAttribute}
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5"
            >Add</button>
        </div>

        <!-- Inline editor for selected attribute -->
        {#if selectedAttr && isEditable(selectedAttr)}
            <div class="flex items-center gap-1 mb-2 p-2 bg-surface-100-900 rounded border border-surface-200-800">
                <span class="text-surface-600-400 w-32 flex-shrink-0 truncate" title={selectedAttr.name}>{selectedAttr.name}:</span>
                <input
                    type="text"
                    class="input text-xs py-0.5 px-1.5 flex-1"
                    bind:value={editingValue}
                    onblur={commitEdit}
                    onkeydown={(e) => {
                        if (e.key === "Enter") commitEdit();
                        if (e.key === "Escape") { selectedAttr = null; }
                    }}
                />
            </div>
        {/if}

        <table class="w-full border-collapse">
            <thead>
                <tr class="text-surface-400-500 border-b border-surface-200-800">
                    <th class="text-left py-1 pr-3 font-medium w-40">Name</th>
                    <th class="text-left py-1 pr-3 font-medium">Value</th>
                    <th class="text-left py-1 pr-3 font-medium w-24">Source</th>
                    <th class="w-6"></th>
                </tr>
            </thead>
            <tbody>
                {#each $fullAttributeData?.attributes ?? [] as attr}
                    {@const dimmed = attr.isInherited || attr.isDefaultType}
                    {@const isSelected = selectedAttr?.name === attr.name}
                    <tr
                        class="border-b border-surface-100-900 cursor-pointer {dimmed ? 'text-surface-400-500' : 'hover:bg-surface-100-900'} {isSelected ? 'bg-primary-50-950' : ''}"
                        onclick={() => onSelectAttribute(attr)}
                    >
                        <td class="py-0.5 pr-3 truncate max-w-40 {!dimmed ? 'font-medium' : ''}" title={attr.name}>{attr.name}</td>
                        <td class="py-0.5 pr-3 max-w-48 truncate" title={attr.value ?? ""}>{displayValue(attr)}</td>
                        <td class="py-0.5 pr-3 text-surface-400-500 truncate" title={attr.source}>{attr.source}</td>
                        <td class="py-0.5 text-right">
                            {#if !attr.isInherited}
                                <button
                                    type="button"
                                    class="text-error-500 hover:text-error-700 px-1"
                                    onclick={(e) => onDeleteAttribute(attr, e)}
                                    title="Remove attribute"
                                >✕</button>
                            {/if}
                        </td>
                    </tr>
                {:else}
                    <tr><td colspan="4" class="py-1 text-surface-400-500 italic">No attributes</td></tr>
                {/each}
            </tbody>
        </table>
    </div>
</div>
