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
        newAttrName = "";
        newAttrValue = "";
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
        editingValue = (attr.value === null || attr.value === "(script)" || isComplex(attr)) ? "" : attr.value;
    }

    function commitEdit() {
        if ($selectedKey && selectedAttr && !isComplex(selectedAttr) && selectedAttr.value !== "(script)") {
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

    function isComplex(attr: AttributeDataItem): boolean {
        if (attr.value === null || attr.value === "(script)") return true;
        if (attr.value.startsWith("[")) {
            try { JSON.parse(attr.value); return true; } catch { /* not JSON */ }
        }
        return false;
    }

    function displayValue(attr: AttributeDataItem): string {
        if (attr.value === null) return "";
        if (attr.value === "(script)") return "(script)";
        if (attr.value.startsWith("[")) {
            try {
                const items = JSON.parse(attr.value) as {key: string, value: string}[];
                return `(list: ${items.length})`;
            } catch { /* not a list */ }
        }
        return attr.value;
    }
</script>

<div class="flex flex-col flex-1 min-h-0 text-xs">
    <!-- Inherited types section -->
    <div class="flex-shrink-0 border-b border-surface-200-800">
        <div class="flex items-center gap-2 px-3 py-1.5 border-b border-surface-100-900">
            <span class="font-semibold text-surface-500-400 uppercase tracking-wide flex-1">Inherited types</span>
            <select
                class="select text-xs py-0 px-1.5 h-6"
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
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0 h-6"
            >Add</button>
        </div>
        <table class="w-full">
            <thead>
                <tr class="text-surface-400-500 border-b border-surface-100-900">
                    <th class="text-left py-1 px-3 font-medium">Name</th>
                    <th class="text-left py-1 px-3 font-medium">Value</th>
                    <th class="text-left py-1 px-3 font-medium">Source</th>
                    <th class="w-6"></th>
                </tr>
            </thead>
            <tbody>
                {#each $fullAttributeData?.inheritedTypes ?? [] as t}
                    <tr class="border-b border-surface-100-900">
                        <td class="py-0.5 px-3">{t.name}</td>
                        <td class="py-0.5 px-3">{t.name}</td>
                        <td class="py-0.5 px-3 text-surface-400-500">{t.source}</td>
                        <td class="py-0.5 pr-2 text-right">
                            {#if !t.isDefaultType}
                                <button
                                    type="button"
                                    class="text-error-500 hover:text-error-700"
                                    onclick={() => onDeleteInheritedType(t.name)}
                                    title="Remove type"
                                >✕</button>
                            {/if}
                        </td>
                    </tr>
                {:else}
                    <tr><td colspan="4" class="py-1 px-3 text-surface-400-500 italic">No inherited types</td></tr>
                {/each}
            </tbody>
        </table>
    </div>

    <!-- Attributes section: split pane -->
    <div class="flex flex-1 min-h-0">
        <!-- Left: attributes list -->
        <div class="flex flex-col flex-1 min-w-0 overflow-hidden border-r border-surface-200-800">
            <!-- Add new attribute -->
            <div class="flex items-center gap-1 px-3 py-1.5 border-b border-surface-100-900 flex-shrink-0">
                <span class="font-semibold text-surface-500-400 uppercase tracking-wide mr-1">Attributes</span>
                <input
                    type="text"
                    placeholder="Name"
                    class="input text-xs py-0 px-1.5 h-6 w-28 flex-shrink-0"
                    bind:value={newAttrName}
                    onkeydown={(e) => { if (e.key === "Enter") onAddAttribute(); }}
                />
                <input
                    type="text"
                    placeholder="Value"
                    class="input text-xs py-0 px-1.5 h-6 flex-1 min-w-0"
                    bind:value={newAttrValue}
                    onkeydown={(e) => { if (e.key === "Enter") onAddAttribute(); }}
                />
                <button
                    type="button"
                    disabled={!newAttrName.trim()}
                    onclick={onAddAttribute}
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0 h-6 flex-shrink-0"
                >Add</button>
            </div>

            <!-- Scrollable table -->
            <div class="overflow-y-auto flex-1">
                <table class="w-full">
                    <thead class="sticky top-0 bg-surface-50-950 z-10">
                        <tr class="text-surface-400-500 border-b border-surface-200-800">
                            <th class="text-left py-1 px-3 font-medium">Name</th>
                            <th class="text-left py-1 px-3 font-medium">Value</th>
                            <th class="text-left py-1 px-3 font-medium">Source</th>
                            <th class="w-6"></th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each $fullAttributeData?.attributes ?? [] as attr}
                            {@const dimmed = attr.isInherited || attr.isDefaultType}
                            {@const isSelected = selectedAttr?.name === attr.name}
                            <tr
                                class="border-b border-surface-100-900 cursor-pointer
                                    {isSelected ? 'bg-primary-100-900' : dimmed ? 'hover:bg-surface-100-900' : 'hover:bg-surface-100-900'}
                                    {dimmed ? 'text-surface-400-500' : ''}"
                                onclick={() => onSelectAttribute(attr)}
                            >
                                <td class="py-0.5 px-3 truncate max-w-36 {!dimmed ? 'font-medium' : ''}" title={attr.name}>{attr.name}</td>
                                <td class="py-0.5 px-3 max-w-40 truncate" title={attr.value ?? ""}>{displayValue(attr)}</td>
                                <td class="py-0.5 px-3 text-surface-400-500 truncate" title={attr.source}>{attr.source}</td>
                                <td class="py-0.5 pr-2 text-right">
                                    {#if !attr.isInherited}
                                        <button
                                            type="button"
                                            class="text-error-500 hover:text-error-700"
                                            onclick={(e) => onDeleteAttribute(attr, e)}
                                            title="Remove attribute"
                                        >✕</button>
                                    {/if}
                                </td>
                            </tr>
                        {:else}
                            <tr><td colspan="4" class="py-2 px-3 text-surface-400-500 italic">No attributes</td></tr>
                        {/each}
                    </tbody>
                </table>
            </div>
        </div>

        <!-- Right: assignment panel -->
        <div class="w-56 flex-shrink-0 flex flex-col overflow-y-auto">
            <div class="px-3 py-1.5 border-b border-surface-100-900 font-semibold text-surface-500-400 uppercase tracking-wide flex-shrink-0">
                Assignment
            </div>
            {#if selectedAttr}
                <div class="p-3 flex flex-col gap-2">
                    <div class="font-medium text-surface-700-300 truncate" title={selectedAttr.name}>{selectedAttr.name}</div>
                    {#if selectedAttr.isInherited || selectedAttr.isDefaultType}
                        <p class="text-surface-400-500 italic text-xs">Inherited from <span class="font-medium">{selectedAttr.source}</span></p>
                    {/if}
                    {#if selectedAttr.value === "(script)"}
                        <p class="text-surface-400-500 italic">Edit via the Scripts tab</p>
                    {:else if isComplex(selectedAttr)}
                        <p class="text-surface-400-500 italic">Complex type — edit via the relevant tab</p>
                    {:else}
                        <textarea
                            class="input text-xs py-1 px-1.5 w-full resize-none"
                            rows="4"
                            value={editingValue}
                            oninput={(e) => { editingValue = (e.target as HTMLTextAreaElement).value; }}
                            onblur={commitEdit}
                            onkeydown={(e) => { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); commitEdit(); } }}
                        ></textarea>
                    {/if}
                </div>
            {:else}
                <p class="p-3 text-surface-400-500 italic">Select an attribute to edit it.</p>
            {/if}
        </div>
    </div>
</div>
