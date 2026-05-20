<script lang="ts">
    import { selectedKey, selectedData, setAttribute, setDropdownType } from "$lib/editor-store";
    import type { ControlInfo } from "$lib/types";
    import ScriptEditor from "./ScriptEditor.svelte";

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
    <div class="px-3 py-2 text-xs font-semibold uppercase text-surface-500-400 border-b border-surface-200-800 bg-surface-100-900">
        Properties
    </div>

    {#if $selectedKey === null}
        <p class="px-3 py-4 text-sm text-surface-400-500">Select an object to view its properties.</p>
    {:else if $selectedData === null}
        <p class="px-3 py-4 text-sm text-surface-400-500">No properties available.</p>
    {:else}
        {#if $selectedData.tabs.length > 0}
            <div class="flex border-b border-surface-200-800 bg-surface-100-900 overflow-x-auto flex-shrink-0">
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
        <div class="flex items-center gap-2 px-3 py-1.5 border-b border-surface-100-900 min-h-8">
            <span class="text-xs text-surface-600-400 w-32 flex-shrink-0 overflow-hidden text-ellipsis whitespace-nowrap" title={ctrl.caption ?? ctrl.attribute}>
                {ctrl.caption ?? ctrl.attribute}
            </span>

            {#if ctrl.controlType === "checkbox"}
                <input
                    type="checkbox"
                    class="checkbox"
                    checked={boolValue(ctrl.attribute)}
                    onchange={(e) => onCheckboxChange(ctrl.attribute!, (e.target as HTMLInputElement).checked)}
                />
            {:else if ctrl.controlType === "number"}
                <input
                    type="number"
                    class="input text-xs py-0.5 px-1.5 w-full"
                    value={attrValue(ctrl.attribute) ?? ""}
                    onchange={(e) => onNumberChange(ctrl.attribute!, "number", (e.target as HTMLInputElement).value)}
                />
            {:else if ctrl.controlType === "numberdouble"}
                <input
                    type="number"
                    step="any"
                    class="input text-xs py-0.5 px-1.5 w-full"
                    value={attrValue(ctrl.attribute) ?? ""}
                    onchange={(e) => onNumberChange(ctrl.attribute!, "numberdouble", (e.target as HTMLInputElement).value)}
                />
            {:else if ctrl.controlType === "dropdown" && ctrl.options}
                <select
                    class="select text-xs py-0.5 px-1.5 w-full"
                    value={attrValue(ctrl.attribute) ?? ""}
                    onchange={(e) => onDropdownChange(ctrl.attribute!, (e.target as HTMLSelectElement).value)}
                >
                    {#each ctrl.options as opt, oi (oi)}
                        <option value={opt.value}>{opt.label}</option>
                    {/each}
                </select>
            {:else if ctrl.controlType === "dropdowntypes" && ctrl.options && ctrl.attribute}
                <select
                    class="select text-xs py-0.5 px-1.5 w-full"
                    value={attrValue(ctrl.attribute) ?? "*"}
                    onchange={(e) => $selectedKey && setDropdownType($selectedKey, ctrl.attribute!, (e.target as HTMLSelectElement).value)}
                >
                    {#each ctrl.options as opt, oi (oi)}
                        <option value={opt.value}>{opt.label}</option>
                    {/each}
                </select>
            {:else if ctrl.controlType === "textbox" || ctrl.controlType === "richtext"}
                <input
                    type="text"
                    class="input text-xs py-0.5 px-1.5 w-full"
                    value={attrValue(ctrl.attribute) ?? ""}
                    onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLInputElement).value)}
                />
            {:else if ctrl.controlType === "script" && ctrl.attribute !== null && $selectedKey !== null}
                <div class="flex-1 min-w-0 overflow-hidden">
                    <ScriptEditor elementKey={$selectedKey} attribute={ctrl.attribute} />
                </div>
            {:else}
                {#if attrValue(ctrl.attribute) !== null}
                    <span class="text-xs overflow-hidden text-ellipsis whitespace-nowrap" title={attrValue(ctrl.attribute) ?? ""}>
                        {attrValue(ctrl.attribute)}
                    </span>
                {:else}
                    <em class="text-xs text-surface-400-500">null</em>
                {/if}
            {/if}
        </div>
    {/if}
{/snippet}
