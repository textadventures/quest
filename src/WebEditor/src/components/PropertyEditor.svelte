<script lang="ts">
    import { selectedKey, selectedData, setAttribute, setDropdownType, setMultiType, setObjectReference } from "$lib/editor-store";
    import type { ControlInfo } from "$lib/types";
    import ScriptEditor from "./ScriptEditor.svelte";
    import Combobox from "./Combobox.svelte";

    let activeTab = $state<string | null>(null);
    let lastKey = $state<string | null>(null);

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

        <div class="flex-1 overflow-y-auto">
            {#each getControlsForView() as ctrl, i (i)}
                {@render controlRow(ctrl)}
            {/each}
        </div>
    {/if}
</div>

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
        <textarea
            class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLTextAreaElement).value)}
        ></textarea>
    {:else if ctrl.controlType === "textbox"}
        <input
            type="text"
            class="input text-xs py-0.5 px-1.5 w-full"
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLInputElement).value)}
        />
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
                <textarea
                    class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                    value={attrValue(ctrl.subAttribute) ?? ""}
                    onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                ></textarea>
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
    {#if ctrl.controlType === "title"}
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
                    <textarea
                        class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                        value={attrValue(ctrl.subAttribute) ?? ""}
                        onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                    ></textarea>
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
            {@const isMultiline = ctrl.controlType === "richtext" || ctrl.controlType === "script"}
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
