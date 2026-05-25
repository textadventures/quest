<script lang="ts">
    import { selectedKey, selectedData, setAttribute, setDropdownType, setMultiType, setObjectReference, addDictItem, removeDictItem, updateDictItem } from "$lib/editor-store";
    import type { ControlInfo, TextProcessorCommand } from "$lib/types";
    import ScriptEditor from "./ScriptEditor.svelte";
    import Combobox from "./Combobox.svelte";
    import AttributesEditor from "./AttributesEditor.svelte";
    import ListEditor from "./ListEditor.svelte";

    let activeTab = $state<string | null>(null);
    let lastKey = $state<string | null>(null);
    let editingItem = $state<{attribute: string, key: string, value: string} | null>(null);
    let newDictItems = $state<Record<string, {key: string, value: string}>>({});

    $effect(() => {
        const key = $selectedKey;
        const data = $selectedData;
        if (key !== lastKey) {
            lastKey = key;
            activeTab = (data && data.tabs.length > 0) ? data.tabs[0].caption : null;
        } else if (activeTab !== null) {
            // Keep activeTab valid after a data refresh (tab list is stable for the same node)
            const stillExists = data?.tabs.some(t => t.caption === activeTab);
            if (!stillExists) activeTab = (data && data.tabs.length > 0) ? data.tabs[0].caption : null;
        } else {
            activeTab = (data && data.tabs.length > 0) ? data.tabs[0].caption : null;
        }
    });

    function onTextChange(attribute: string, controlType: string, value: string) {
        if ($selectedKey) setAttribute($selectedKey, attribute, controlType, value);
    }

    function onCheckboxChange(attribute: string, checked: boolean) {
        if ($selectedKey) setAttribute($selectedKey, attribute, "checkbox", checked.toString());
    }

    function onNumberChange(attribute: string, controlType: string, value: string) {
        if ($selectedKey) setAttribute($selectedKey, attribute, controlType, value);
    }

    function onDropdownChange(attribute: string, value: string) {
        if ($selectedKey) setAttribute($selectedKey, attribute, "dropdown", value);
    }

    function getControlsForView(): ControlInfo[] {
        const data = $selectedData;
        if (!data) return [];
        if (data.tabs.length > 0) {
            return data.tabs.find(t => t.caption === activeTab)?.controls ?? [];
        }
        return data.controls;
    }

    function attrValue(attribute: string): string | null {
        return $selectedData?.attributes[attribute] ?? null;
    }

    function boolValue(attribute: string): boolean {
        const v = attrValue(attribute);
        return v === "True" || v === "true";
    }

    function insertTextProcessorText(attribute: string, controlType: string, insertBefore: string, insertAfter: string, event: MouseEvent) {
        const wrapper = (event.target as HTMLElement).closest('.richtext-wrap');
        const textarea = wrapper?.querySelector('textarea') as HTMLTextAreaElement | null;
        if (!textarea) return;
        const start = textarea.selectionStart ?? 0;
        const end = textarea.selectionEnd ?? 0;
        const selectedText = textarea.value.substring(start, end);
        textarea.value = textarea.value.substring(0, start) + insertBefore + selectedText + insertAfter + textarea.value.substring(end);
        textarea.selectionStart = start + insertBefore.length;
        textarea.selectionEnd = start + insertBefore.length + selectedText.length;
        textarea.focus();
        onTextChange(attribute, controlType, textarea.value);
    }

    function focusOnMount(node: HTMLElement) {
        node.focus();
    }

    function tabClass(caption: string | null): string {
        return activeTab === caption
            ? "px-3 py-1.5 text-xs whitespace-nowrap transition-colors text-primary-600-400 border-b-2 border-primary-500 font-medium"
            : "px-3 py-1.5 text-xs whitespace-nowrap transition-colors text-surface-500-400 hover:text-surface-900-100";
    }
</script>

<div class="flex flex-col flex-1 bg-surface-50-950 overflow-hidden">
    <div class="px-3 py-2 text-xs font-semibold uppercase text-surface-500-400 border-b border-surface-200-800">
        Properties
    </div>

    {#if $selectedKey === null}
        <p class="px-3 py-4 text-sm text-surface-400-500">Select an object to view its properties.</p>
    {:else if $selectedData === null}
        <p class="px-3 py-4 text-sm text-surface-400-500">No properties available.</p>
    {:else}
        {#if $selectedData.tabs.length > 0}
            <div class="flex border-b border-surface-200-800 overflow-x-auto flex-shrink-0">
                {#each $selectedData.tabs as tab, ti (ti)}
                    <button
                        type="button"
                        class={tabClass(tab.caption)}
                        onclick={() => { activeTab = tab.caption; }}
                    >
                        {tab.caption ?? "Tab"}
                    </button>
                {/each}
            </div>
        {/if}

        {@const viewControls = getControlsForView()}
        {@const hasAttributesPanel = viewControls.some(c => c.controlType === "attributes")}
        {#if hasAttributesPanel}
            <div class="flex-1 overflow-hidden flex flex-col min-h-0">
                {#each viewControls.filter(c => c.controlType !== "attributes") as ctrl, i (i)}
                    {@render controlRow(ctrl)}
                {/each}
                <AttributesEditor />
            </div>
        {:else}
            <div class="flex-1 overflow-y-auto">
                {#each viewControls as ctrl, i (i)}
                    {@render controlRow(ctrl)}
                {/each}
            </div>
        {/if}
    {/if}
</div>

{#snippet textProcessorPanel(commands: TextProcessorCommand[], attribute: string, controlType: string)}
    <div class="flex flex-col gap-0.5 shrink-0">
        {#each commands as cmd}
            <div class="flex items-center gap-1">
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 w-28 justify-start"
                    onclick={(e) => insertTextProcessorText(attribute, controlType, cmd.insertBefore, cmd.insertAfter, e)}
                >{cmd.command}</button>
                <span class="text-xs text-surface-400-500 whitespace-nowrap">{cmd.info}</span>
            </div>
        {/each}
        <a href="https://docs.textadventures.co.uk/quest/text_processor.html" target="_blank" class="text-xs text-primary-500 underline mt-1">Text Processor help</a>
    </div>
{/snippet}

{#snippet controlOnly(ctrl: ControlInfo)}
    {#if ctrl.controlType === "number"}
        <input
            type="number"
            class="input text-xs py-0.5 px-1.5 w-auto"
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => onNumberChange(ctrl.attribute!, "number", (e.target as HTMLInputElement).value)}
        />
    {:else if ctrl.controlType === "numberdouble"}
        <input
            type="number"
            step="any"
            class="input text-xs py-0.5 px-1.5 w-auto"
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => onNumberChange(ctrl.attribute!, "numberdouble", (e.target as HTMLInputElement).value)}
        />
    {:else if ctrl.controlType === "dropdown" && ctrl.options}
        <Combobox
            value={attrValue(ctrl.attribute!) ?? ""}
            options={ctrl.options}
            onchange={(v) => onDropdownChange(ctrl.attribute!, v)}
            class="input text-xs py-0.5 px-1.5 w-auto min-w-24"
        />
    {:else if ctrl.controlType === "dropdowntypes" && ctrl.options && ctrl.attribute}
        <select
            class="select text-xs py-0.5 px-1.5 w-auto"
            value={attrValue(ctrl.attribute) ?? "*"}
            onchange={(e) => $selectedKey && setDropdownType($selectedKey, ctrl.attribute!, (e.target as HTMLSelectElement).value)}
        >
            {#each ctrl.options as opt, oi (oi)}
                <option value={opt.value}>{opt.label}</option>
            {/each}
        </select>
    {:else if ctrl.controlType === "richtext"}
        {#if ctrl.textProcessorCommands?.length}
            <div class="richtext-wrap flex gap-2 w-full">
                <textarea
                    class="input text-xs py-0.5 px-1.5 flex-1 min-h-32 resize-y"
                    value={attrValue(ctrl.attribute!) ?? ""}
                    onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLTextAreaElement).value)}
                ></textarea>
                {@render textProcessorPanel(ctrl.textProcessorCommands, ctrl.attribute!, ctrl.controlType)}
            </div>
        {:else}
            <textarea
                class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                value={attrValue(ctrl.attribute!) ?? ""}
                onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLTextAreaElement).value)}
            ></textarea>
        {/if}
    {:else if ctrl.controlType === "textbox"}
        <input
            type="text"
            class="input text-xs py-0.5 px-1.5 w-full"
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLInputElement).value)}
        />
    {:else if ctrl.controlType === "gameid"}
        <div class="flex items-center gap-2 w-full">
            <input
                type="text"
                class="input text-xs py-0.5 px-1.5 flex-1"
                readonly
                value={attrValue(ctrl.attribute!) ?? ""}
            />
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 whitespace-nowrap"
                onclick={() => onTextChange(ctrl.attribute!, "textbox", crypto.randomUUID())}
            >Generate</button>
        </div>
    {:else if ctrl.controlType === "file"}
        <em class="text-xs text-surface-400-500">File picker not yet implemented</em>
    {:else if ctrl.controlType === "list" && ctrl.attribute && $selectedKey}
        <ListEditor elementKey={$selectedKey} attribute={ctrl.attribute} value={attrValue(ctrl.attribute)} addPrompt={ctrl.addPrompt ?? undefined} />
    {:else if ctrl.controlType === "stringdictionary" && ctrl.attribute}
        {@const items = (() => { try { return JSON.parse(attrValue(ctrl.attribute) ?? "[]") as {key: string, value: string}[] } catch { return [] } })()}
        {@const dk = ctrl.attribute}
        <div class="flex flex-col gap-1 w-full">
            {#each items as item (item.key)}
                {@const isEditing = editingItem?.attribute === dk && editingItem?.key === item.key}
                <div class="flex items-center gap-1">
                    <span class="text-xs text-surface-500-400 w-24 flex-shrink-0 truncate" title={item.key}>{item.key}</span>
                    {#if isEditing}
                        <input
                            type="text"
                            class="input text-xs py-0.5 px-1.5 flex-1"
                            use:focusOnMount
                            value={editingItem!.value}
                            oninput={(e) => { if (editingItem) editingItem.value = (e.target as HTMLInputElement).value; }}
                            onkeydown={(e) => {
                                if (e.key === "Enter" && $selectedKey && editingItem) {
                                    updateDictItem($selectedKey, dk, editingItem.key, editingItem.value);
                                    editingItem = null;
                                } else if (e.key === "Escape") {
                                    editingItem = null;
                                }
                            }}
                            onblur={() => {
                                if ($selectedKey && editingItem) {
                                    updateDictItem($selectedKey, dk, editingItem.key, editingItem.value);
                                    editingItem = null;
                                }
                            }}
                        />
                    {:else}
                        <button
                            type="button"
                            class="text-xs flex-1 text-left px-1.5 py-0.5 hover:text-primary-600-400 truncate"
                            onclick={() => { editingItem = {attribute: dk, key: item.key, value: item.value}; }}
                        >{item.value}</button>
                    {/if}
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-error-500 text-xs px-1.5 py-0.5 flex-shrink-0"
                        onclick={() => $selectedKey && removeDictItem($selectedKey, dk, item.key)}
                    >✕</button>
                </div>
            {/each}
            <div class="flex items-center gap-1 mt-0.5">
                <input
                    type="text"
                    class="input text-xs py-0.5 px-1.5 w-24 flex-shrink-0"
                    placeholder={ctrl.caption ? "Key" : "Key"}
                    value={newDictItems[dk]?.key ?? ""}
                    oninput={(e) => { newDictItems[dk] = {...(newDictItems[dk] ?? {key: "", value: ""}), key: (e.target as HTMLInputElement).value}; }}
                />
                <input
                    type="text"
                    class="input text-xs py-0.5 px-1.5 flex-1"
                    placeholder="Value"
                    value={newDictItems[dk]?.value ?? ""}
                    oninput={(e) => { newDictItems[dk] = {...(newDictItems[dk] ?? {key: "", value: ""}), value: (e.target as HTMLInputElement).value}; }}
                    onkeydown={(e) => {
                        if (e.key === "Enter" && $selectedKey && newDictItems[dk]?.key?.trim()) {
                            addDictItem($selectedKey, dk, newDictItems[dk].key.trim(), newDictItems[dk].value ?? "");
                            newDictItems[dk] = {key: "", value: ""};
                        }
                    }}
                />
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0"
                    onclick={() => {
                        if ($selectedKey && newDictItems[dk]?.key?.trim()) {
                            addDictItem($selectedKey, dk, newDictItems[dk].key.trim(), newDictItems[dk].value ?? "");
                            newDictItems[dk] = {key: "", value: ""};
                        }
                    }}
                >Add</button>
            </div>
        </div>
    {:else if ctrl.controlType === "objects" && ctrl.options}
        <Combobox
            value={attrValue(ctrl.attribute!) ?? ""}
            options={ctrl.options}
            onchange={(v) => $selectedKey && setObjectReference($selectedKey, ctrl.attribute!, v)}
            class="input text-xs py-0.5 px-1.5 w-auto min-w-24"
        />
    {:else if ctrl.controlType === "multi" && ctrl.options}
        {@const selectedType = attrValue(ctrl.attribute!) ?? "null"}
        {@const subEditorType = ctrl.subEditors?.find(e => e.value === selectedType)?.label ?? selectedType}
        <div class="flex flex-col gap-1 w-full">
            <select
                class="select text-xs py-0.5 px-1.5 w-auto self-start"
                value={selectedType}
                onchange={(e) => $selectedKey && setMultiType($selectedKey, ctrl.subAttribute!, (e.target as HTMLSelectElement).value)}
            >
                {#each ctrl.options as opt (opt.value)}
                    <option value={opt.value}>{opt.label}</option>
                {/each}
            </select>
            {#if subEditorType === "richtext" && ctrl.subAttribute !== null}
                {#if ctrl.textProcessorCommands?.length}
                    <div class="richtext-wrap flex gap-2 w-full">
                        <textarea
                            class="input text-xs py-0.5 px-1.5 flex-1 min-h-32 resize-y"
                            value={attrValue(ctrl.subAttribute) ?? ""}
                            onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                        ></textarea>
                        {@render textProcessorPanel(ctrl.textProcessorCommands, ctrl.subAttribute, "richtext")}
                    </div>
                {:else}
                    <textarea
                        class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                        value={attrValue(ctrl.subAttribute) ?? ""}
                        onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                    ></textarea>
                {/if}
            {:else if subEditorType === "textbox" && ctrl.subAttribute !== null}
                <input
                    type="text"
                    class="input text-xs py-0.5 px-1.5 w-full"
                    value={attrValue(ctrl.subAttribute) ?? ""}
                    onchange={(e) => onTextChange(ctrl.subAttribute!, "textbox", (e.target as HTMLInputElement).value)}
                />
            {:else if subEditorType === "script" && ctrl.subAttribute !== null && $selectedKey !== null}
                <ScriptEditor elementKey={$selectedKey} attribute={ctrl.subAttribute} />
            {/if}
        </div>
    {:else if ctrl.controlType === "script" && ctrl.attribute !== null && $selectedKey !== null}
        <div class="flex-1 min-w-0 overflow-hidden">
            <ScriptEditor elementKey={$selectedKey} attribute={ctrl.attribute} />
        </div>
    {:else}
        {#if attrValue(ctrl.attribute!) !== null}
            <span class="text-xs overflow-hidden text-ellipsis whitespace-nowrap" title={attrValue(ctrl.attribute!) ?? ""}>
                {attrValue(ctrl.attribute!)}
            </span>
        {:else}
            <em class="text-xs text-surface-400-500">null</em>
        {/if}
    {/if}
{/snippet}

{#snippet controlRow(ctrl: ControlInfo)}
    {#if ctrl.controlType === "attributes"}
        <AttributesEditor />
    {:else if ctrl.controlType === "title"}
        <div class="px-3 pt-3 pb-1 text-xs font-semibold text-surface-500-400 uppercase tracking-wide">
            {ctrl.caption ?? ""}
        </div>
    {:else if ctrl.controlType === "label"}
        <div class="px-3 py-1 text-xs text-surface-500-400 italic">
            {ctrl.caption ?? ""}
        </div>
    {:else if ctrl.attribute !== null}
        {#if ctrl.controlType === "checkbox"}
            <label class="flex items-center gap-2 px-3 py-1.5 min-h-8 cursor-pointer">
                <input
                    type="checkbox"
                    class="checkbox flex-shrink-0"
                    checked={boolValue(ctrl.attribute)}
                    onchange={(e) => onCheckboxChange(ctrl.attribute!, (e.target as HTMLInputElement).checked)}
                />
                <span class="text-xs text-surface-600-400">
                    {ctrl.caption ?? ctrl.attribute}
                </span>
            </label>
        {:else if ctrl.controlType === "multi" && ctrl.options}
            {@const label = ctrl.caption ?? ctrl.attribute}
            {@const selectedType = attrValue(ctrl.attribute!) ?? "null"}
            {@const subEditorType = ctrl.subEditors?.find(e => e.value === selectedType)?.label ?? selectedType}
            <div class="flex flex-col gap-1 px-3 py-1.5">
                <div class="flex items-center gap-2">
                    <span class="text-xs text-surface-600-400 whitespace-nowrap">{label}:</span>
                    <select
                        class="select text-xs py-0.5 px-1.5 w-auto"
                        value={selectedType}
                        onchange={(e) => $selectedKey && setMultiType($selectedKey, ctrl.subAttribute!, (e.target as HTMLSelectElement).value)}
                    >
                        {#each ctrl.options as opt (opt.value)}
                            <option value={opt.value}>{opt.label}</option>
                        {/each}
                    </select>
                </div>
                {#if subEditorType === "richtext" && ctrl.subAttribute !== null}
                    {#if ctrl.textProcessorCommands?.length}
                        <div class="richtext-wrap flex gap-2 w-full">
                            <textarea
                                class="input text-xs py-0.5 px-1.5 flex-1 min-h-32 resize-y"
                                value={attrValue(ctrl.subAttribute) ?? ""}
                                onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                            ></textarea>
                            {@render textProcessorPanel(ctrl.textProcessorCommands, ctrl.subAttribute, "richtext")}
                        </div>
                    {:else}
                        <textarea
                            class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                            value={attrValue(ctrl.subAttribute) ?? ""}
                            onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                        ></textarea>
                    {/if}
                {:else if subEditorType === "textbox" && ctrl.subAttribute !== null}
                    <input
                        type="text"
                        class="input text-xs py-0.5 px-1.5 w-full"
                        value={attrValue(ctrl.subAttribute) ?? ""}
                        onchange={(e) => onTextChange(ctrl.subAttribute!, "textbox", (e.target as HTMLInputElement).value)}
                    />
                {:else if subEditorType === "script" && ctrl.subAttribute !== null && $selectedKey !== null}
                    <ScriptEditor elementKey={$selectedKey} attribute={ctrl.subAttribute} />
                {/if}
            </div>
        {:else}
            {@const label = ctrl.caption ?? ctrl.attribute}
            {@const isLong = label.length > 20}
            {@const isMultiline = ctrl.controlType === "richtext" || ctrl.controlType === "script" || ctrl.controlType === "list" || ctrl.controlType === "stringdictionary"}
            {#if isLong}
                <div class="flex flex-col gap-1 px-3 py-1.5">
                    <span class="text-xs text-surface-600-400">{label}:</span>
                    {@render controlOnly(ctrl)}
                </div>
            {:else}
                <div class="flex {isMultiline ? 'items-start' : 'items-center'} gap-2 px-3 py-1.5 min-h-8">
                    <span class="text-xs text-surface-600-400 w-32 flex-shrink-0 {isMultiline ? 'pt-0.5' : ''}">{label}:</span>
                    {@render controlOnly(ctrl)}
                </div>
            {/if}
        {/if}
    {/if}
{/snippet}
