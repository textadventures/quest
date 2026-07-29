<script lang="ts">
    import { selectedKey, selectedData, treeNodes, isGamebook, setAttribute, setDropdownType, setMultiType, setObjectReference, addDictItem, removeDictItem, updateDictItem, getObjectNames, selectNode, createObjectSilent, openAddModal, createIncludedLibrary, createJavascript } from "$lib/editor-store";
    import { showToast } from "$lib/toast";
    import type { ControlInfo, TextProcessorCommand } from "$lib/types";
    import type { TreeNode } from "$lib/types";
    import ChevronLeft from "@lucide/svelte/icons/chevron-left";
    import ArrowRight from "@lucide/svelte/icons/arrow-right";
    import ScriptEditor from "./ScriptEditor.svelte";
    import ScriptDictionaryEditor from "./ScriptDictionaryEditor.svelte";
    import Combobox from "./Combobox.svelte";
    import AttributesEditor from "./AttributesEditor.svelte";
    import ListEditor from "./ListEditor.svelte";
    import ElementsList from "./ElementsList.svelte";
    import AssetPicker from "./AssetPicker.svelte";
    import ExitsEditor from "./ExitsEditor.svelte";
    import VerbsEditor from "./VerbsEditor.svelte";
    import AddElementModal from "./AddElementModal.svelte";
    import LibraryElementBanner from "./LibraryElementBanner.svelte";

    let { onback }: { onback?: () => void } = $props();

    // Same derivation as Toolbar's selectedNode — used for the mobile back header's label.
    let selectedNode = $derived<TreeNode | null>(
        $treeNodes.find(n => n.key === $selectedKey) ?? null
    );

    // The "_advanced" tree header has no elementType of its own (EditorController
    // registers it with type: null), so $selectedData is always null for it and it
    // would otherwise fall into the generic "No properties available" state below.
    // It still needs to be a real entry point — its sub-category headers (Functions,
    // Timers, Library, ...) stay hidden from the tree until they have their first
    // element (TreePanel's HIDE_WHEN_EMPTY), so this is the only place to add the
    // first one of each.
    const ALL_ADVANCED_ADDERS: { label: string; action: () => void; gamebook: boolean }[] = [
        { label: "Add Function", action: () => openAddModal("function", null), gamebook: true },
        { label: "Add Timer", action: () => openAddModal("timer", null), gamebook: false },
        { label: "Add Walkthrough", action: () => openAddModal("walkthrough", null), gamebook: false },
        { label: "Add Library", action: () => createIncludedLibrary(), gamebook: true },
        { label: "Add Template", action: () => openAddModal("template", null), gamebook: false },
        { label: "Add Dynamic Template", action: () => openAddModal("dynamictemplate", null), gamebook: false },
        { label: "Add Type", action: () => openAddModal("type", null), gamebook: false },
        { label: "Add JavaScript", action: () => createJavascript(), gamebook: true },
    ];
    // Gamebook mode only supports Function/Library/JavaScript — Timer/Walkthrough
    // don't apply to a flat page-based game, and Template/Object Type are in
    // EditorController's m_ignoredTypes for gamebook (adding one would create an
    // invisible, orphaned element).
    let ADVANCED_ADDERS = $derived(ALL_ADVANCED_ADDERS.filter(a => !$isGamebook || a.gamebook));

    let activeTab = $state<string | null>(null);
    let lastKey = $state<string | null>(null);
    let editingItem = $state<{attribute: string, key: string, value: string} | null>(null);
    let newDictItems = $state<Record<string, {key: string, value: string}>>({});
    let attributeErrors = $state<Record<string, string>>({});
    // Refetched whenever the selection changes, for dictionary controls whose keys are
    // object names (e.g. gamebook page "Options" links) rather than free text.
    let dictSourceObjectNames = $state<string[]>([]);
    $effect(() => {
        if ($selectedKey) dictSourceObjectNames = getObjectNames() ?? [];
    });
    // Which gamebookoptions control (keyed by attribute) has its "new page" dialog open.
    let newPageModalFor = $state<string | null>(null);

    $effect(() => {
        const key = $selectedKey;
        const data = $selectedData;
        if (key !== lastKey) {
            lastKey = key;
            attributeErrors = {};
            activeTab = (data && data.tabs.length > 0) ? data.tabs[0].caption : null;
        } else if (activeTab !== null) {
            // Keep activeTab valid after a data refresh (tab list is stable for the same node)
            const stillExists = data?.tabs.some(t => t.caption === activeTab);
            if (!stillExists) activeTab = (data && data.tabs.length > 0) ? data.tabs[0].caption : null;
        } else {
            activeTab = (data && data.tabs.length > 0) ? data.tabs[0].caption : null;
        }
    });

    function recordResult(attribute: string, result: string) {
        const error = result.startsWith("error:") ? result.slice("error:".length) : "";
        attributeErrors = { ...attributeErrors, [attribute]: error };
        // The inline error below the field only stays visible while this element/tab is in
        // view — a toast survives switching to a different tab or element entirely.
        if (error) showToast(error, "error");
    }

    function onTextChange(attribute: string, controlType: string, value: string) {
        if ($selectedKey) recordResult(attribute, setAttribute($selectedKey, attribute, controlType, value));
    }

    function onCheckboxChange(attribute: string, checked: boolean) {
        if ($selectedKey) recordResult(attribute, setAttribute($selectedKey, attribute, "checkbox", checked.toString()));
    }

    function onNumberChange(attribute: string, controlType: string, value: string) {
        if ($selectedKey) recordResult(attribute, setAttribute($selectedKey, attribute, controlType, value));
    }

    function onDropdownChange(attribute: string, value: string) {
        if ($selectedKey) recordResult(attribute, setAttribute($selectedKey, attribute, "dropdown", value));
    }

    function getControlsForView(): ControlInfo[] {
        const data = $selectedData;
        if (!data) return [];
        if (data.tabs.length > 0) {
            return data.tabs.find(t => t.caption === activeTab)?.controls ?? [];
        }
        return data.controls;
    }

    function partitionControls(controls: ControlInfo[]): { main: ControlInfo[]; advanced: ControlInfo[] } {
        const advanced = controls.filter(c => c.advanced);
        if (advanced.length === 0 || advanced.length === controls.length) {
            return { main: controls, advanced: [] };
        }
        return { main: controls.filter(c => !c.advanced), advanced };
    }

    function attrValue(attribute: string): string | null {
        return $selectedData?.attributes[attribute] ?? null;
    }

    function boolValue(attribute: string): boolean {
        const v = attrValue(attribute);
        return v === "True" || v === "true";
    }

    function insertTextProcessorText(attribute: string, controlType: string, insertBefore: string, insertAfter: string, event: MouseEvent) {
        const wrapper = (event.target as HTMLElement).closest(".richtext-wrap");
        const textarea = wrapper?.querySelector("textarea") as HTMLTextAreaElement | null;
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
        if (node instanceof HTMLInputElement || node instanceof HTMLTextAreaElement) node.select();
    }

    function tabClass(caption: string | null): string {
        return activeTab === caption
            ? "px-3 py-1.5 text-xs whitespace-nowrap transition-colors text-primary-600-400 border-b-2 border-primary-500 font-medium"
            : "px-3 py-1.5 text-xs whitespace-nowrap transition-colors text-surface-500-400 hover:text-surface-900-100";
    }
</script>

<div class="@container flex flex-col flex-1 bg-surface-50-950 overflow-hidden">
    <div class="px-3 py-2 border-b border-surface-200-800">
        {#if onback}
            <button
                type="button"
                class="flex items-center gap-1 -ml-1 px-1 text-sm font-medium text-surface-900-50"
                onclick={onback}
            ><ChevronLeft size={16} /> {selectedNode?.text ?? "Properties"}</button>
        {:else}
            <span class="text-xs font-semibold uppercase text-surface-500-400">Properties</span>
        {/if}
    </div>
    <LibraryElementBanner />

    {#if $selectedKey === null}
        <p class="px-3 py-4 text-sm text-surface-400-500">Select an object to view its properties.</p>
    {:else if $selectedKey === "_advanced"}
        <div class="flex flex-col items-start gap-1.5 px-3 py-3">
            {#each ADVANCED_ADDERS as adder (adder.label)}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={adder.action}
                >+ {adder.label}</button>
            {/each}
        </div>
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
        {@const readOnly = $selectedData.isLibraryElement}
        {#if hasAttributesPanel}
            {@const { main, advanced } = partitionControls(viewControls.filter(c => c.controlType !== "attributes"))}
            <div class="flex-1 overflow-hidden flex flex-col min-h-0 {readOnly ? "pointer-events-none opacity-60" : ""}">
                <AttributesEditor>
                    {#snippet extraControls()}
                        {#each main as ctrl, i (i)}
                            {@render controlRow(ctrl)}
                        {/each}
                        {@render advancedExpander(advanced)}
                    {/snippet}
                </AttributesEditor>
            </div>
        {:else}
            {@const { main, advanced } = partitionControls(viewControls)}
            <div class="flex-1 overflow-y-auto {readOnly ? "pointer-events-none opacity-60" : ""}">
                {#each main as ctrl, i (i)}
                    {@render controlRow(ctrl)}
                {/each}
                {@render advancedExpander(advanced)}
            </div>
        {/if}
    {/if}
</div>

{#snippet textProcessorPanel(commands: TextProcessorCommand[], attribute: string, controlType: string)}
    <div class="flex flex-col gap-0.5 shrink-0 max-h-32 overflow-y-auto @2xl:max-h-none @2xl:overflow-visible">
        {#each commands as cmd (cmd.command)}
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
            <div class="richtext-wrap flex flex-col @2xl:flex-row gap-2 w-full">
                <textarea
                    autocapitalize="off"
                    class="input text-xs py-0.5 px-1.5 flex-1 min-h-32 resize-y"
                    value={attrValue(ctrl.attribute!) ?? ""}
                    onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLTextAreaElement).value)}
                ></textarea>
                {@render textProcessorPanel(ctrl.textProcessorCommands, ctrl.attribute!, ctrl.controlType)}
            </div>
        {:else}
            <textarea
                autocapitalize="off"
                class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                value={attrValue(ctrl.attribute!) ?? ""}
                onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLTextAreaElement).value)}
            ></textarea>
        {/if}
    {:else if ctrl.controlType === "textbox"}
        <input
            type="text"
            autocapitalize="off"
            class={"input text-xs py-0.5 px-1.5 w-full" + (ctrl.attribute && attributeErrors[ctrl.attribute] ? " !border-error-500" : "")}
            value={attrValue(ctrl.attribute!) ?? ""}
            onchange={(e) => onTextChange(ctrl.attribute!, ctrl.controlType, (e.target as HTMLInputElement).value)}
        />
    {:else if ctrl.controlType === "gameid"}
        <div class="flex items-center gap-2 w-full">
            <input
                type="text"
                autocapitalize="off"
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
        <AssetPicker
            value={attrValue(ctrl.attribute!) ?? ""}
            source={ctrl.source}
            onchange={(v) => onTextChange(ctrl.attribute!, ctrl.controlType, v)}
            containerClass="w-full"
        />
    {:else if ctrl.controlType === "list" && ctrl.attribute && $selectedKey}
        <ListEditor elementKey={$selectedKey} attribute={ctrl.attribute} value={attrValue(ctrl.attribute)} addPrompt={ctrl.addPrompt ?? undefined} />
    {:else if (ctrl.controlType === "stringdictionary" || ctrl.controlType === "gamebookoptions") && ctrl.attribute}
        {@const items = (() => { try { return JSON.parse(attrValue(ctrl.attribute) ?? "[]") as {key: string, value: string}[]; } catch { return []; } })()}
        {@const dk = ctrl.attribute}
        {@const isObjectSource = ctrl.source === "object"}
        {@const excludedNames = new Set((ctrl.sourceExclude ?? "").split(/[;,]/).map(s => s.trim()).filter(Boolean))}
        {@const availableObjectNames = isObjectSource ? dictSourceObjectNames.filter(n => !excludedNames.has(n) && !items.some(i => i.key === n)) : []}
        <div class="flex flex-col gap-1 w-full">
            {#each items as item (item.key)}
                {@const isEditing = editingItem?.attribute === dk && editingItem?.key === item.key}
                <div class="flex items-center gap-1">
                    <span class="text-xs text-surface-500-400 w-24 flex-shrink-0 truncate" title={item.key}>{item.key}</span>
                    {#if isEditing}
                        <input
                            type="text"
                            autocapitalize="off"
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
                            onclick={() => { editingItem = { attribute: dk, key: item.key, value: item.value }; }}
                        >{item.value}</button>
                    {/if}
                    {#if isObjectSource}
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500 text-xs px-1.5 py-0.5 flex-shrink-0"
                            title="Go to {item.key}"
                            onclick={() => selectNode(item.key)}
                        ><ArrowRight size={11} /></button>
                    {/if}
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-error-500 text-xs px-1.5 py-0.5 flex-shrink-0"
                        onclick={() => $selectedKey && removeDictItem($selectedKey, dk, item.key)}
                    >✕</button>
                </div>
            {/each}
            <div class="flex items-center gap-1 mt-0.5">
                {#if isObjectSource}
                    <select
                        class="select text-xs py-0.5 px-1.5 w-24 flex-shrink-0"
                        aria-label={ctrl.keyPrompt ?? "Key"}
                        title={ctrl.keyPrompt ?? undefined}
                        data-staging
                        value={newDictItems[dk]?.key ?? ""}
                        onchange={(e) => { newDictItems[dk] = { ...(newDictItems[dk] ?? { key: "", value: "" }), key: (e.target as HTMLSelectElement).value }; }}
                    >
                        <option value="">Select…</option>
                        {#each availableObjectNames as name (name)}
                            <option value={name}>{name}</option>
                        {/each}
                    </select>
                {:else}
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input text-xs py-0.5 px-1.5 w-24 flex-shrink-0"
                        placeholder={ctrl.keyPrompt ?? "Key"}
                        data-staging
                        value={newDictItems[dk]?.key ?? ""}
                        oninput={(e) => { newDictItems[dk] = { ...(newDictItems[dk] ?? { key: "", value: "" }), key: (e.target as HTMLInputElement).value }; }}
                    />
                {/if}
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0.5 px-1.5 flex-1"
                    placeholder={ctrl.valuePrompt ?? "Value"}
                    data-staging
                    value={newDictItems[dk]?.value ?? ""}
                    oninput={(e) => { newDictItems[dk] = { ...(newDictItems[dk] ?? { key: "", value: "" }), value: (e.target as HTMLInputElement).value }; }}
                    onkeydown={(e) => {
                        if (e.key === "Enter" && $selectedKey && newDictItems[dk]?.key?.trim()) {
                            addDictItem($selectedKey, dk, newDictItems[dk].key.trim(), newDictItems[dk].value ?? "");
                            newDictItems[dk] = { key: "", value: "" };
                        }
                    }}
                />
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0"
                    onclick={() => {
                        if ($selectedKey && newDictItems[dk]?.key?.trim()) {
                            addDictItem($selectedKey, dk, newDictItems[dk].key.trim(), newDictItems[dk].value ?? "");
                            newDictItems[dk] = { key: "", value: "" };
                        }
                    }}
                >Add</button>
                {#if ctrl.controlType === "gamebookoptions"}
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0 whitespace-nowrap"
                        onclick={() => { newPageModalFor = dk; }}
                    >+ New Page</button>
                {/if}
            </div>
        </div>
        {#if newPageModalFor === dk}
            <AddElementModal
                elementType="page"
                parent={null}
                onconfirm={(name) => {
                    newPageModalFor = null;
                    if (!$selectedKey) return;
                    const result = createObjectSilent(name, null);
                    if (result.startsWith("error:")) {
                        showToast(result.slice("error:".length));
                        return;
                    }
                    const value = newDictItems[dk]?.value?.trim() || result;
                    addDictItem($selectedKey, dk, result, value);
                    newDictItems[dk] = { key: "", value: "" };
                    editingItem = { attribute: dk, key: result, value };
                }}
                oncancel={() => { newPageModalFor = null; }}
            />
        {/if}
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
                    <div class="richtext-wrap flex flex-col @2xl:flex-row gap-2 w-full">
                        <textarea
                            autocapitalize="off"
                            class="input text-xs py-0.5 px-1.5 flex-1 min-h-32 resize-y"
                            value={attrValue(ctrl.subAttribute) ?? ""}
                            onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                        ></textarea>
                        {@render textProcessorPanel(ctrl.textProcessorCommands, ctrl.subAttribute, "richtext")}
                    </div>
                {:else}
                    <textarea
                        autocapitalize="off"
                        class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                        value={attrValue(ctrl.subAttribute) ?? ""}
                        onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                    ></textarea>
                {/if}
            {:else if subEditorType === "textbox" && ctrl.subAttribute !== null}
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0.5 px-1.5 w-full"
                    value={attrValue(ctrl.subAttribute) ?? ""}
                    onchange={(e) => onTextChange(ctrl.subAttribute!, "textbox", (e.target as HTMLInputElement).value)}
                />
            {:else if subEditorType === "script" && ctrl.subAttribute !== null && $selectedKey !== null}
                <ScriptEditor elementKey={$selectedKey} attribute={ctrl.subAttribute} />
            {:else if subEditorType === "boolean" && ctrl.subAttribute !== null}
                <label class="flex items-center gap-2">
                    <input
                        type="checkbox"
                        class="checkbox flex-shrink-0"
                        checked={boolValue(ctrl.subAttribute)}
                        onchange={(e) => onCheckboxChange(ctrl.subAttribute!, (e.target as HTMLInputElement).checked)}
                    />
                    <span class="text-xs text-surface-600-400">{ctrl.checkboxCaption ?? ctrl.subAttribute}</span>
                </label>
            {/if}
        </div>
    {:else if ctrl.controlType === "script" && ctrl.attribute !== null && $selectedKey !== null}
        <div class="flex-1 min-w-0 overflow-hidden">
            <ScriptEditor elementKey={$selectedKey} attribute={ctrl.attribute} />
        </div>
    {:else if ctrl.controlType === "scriptdictionary" && ctrl.attribute && $selectedKey}
        <ScriptDictionaryEditor
            elementKey={$selectedKey}
            attribute={ctrl.attribute}
            value={attrValue(ctrl.attribute)}
            keySource={ctrl.source === "object" ? "object" : "text"}
        />
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

{#snippet advancedExpander(controls: ControlInfo[])}
    {#if controls.length > 0}
        <details class="mt-2 border-t border-surface-200-800">
            <summary class="px-3 pt-2.5 pb-1.5 text-xs font-semibold uppercase text-surface-500-400 cursor-pointer select-none">
                Advanced
            </summary>
            {#each controls as ctrl, i (i)}
                {@render controlRow(ctrl)}
            {/each}
        </details>
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
    {:else if ctrl.controlType === "elementslist" && $selectedKey}
        <ElementsList
            elementKey={$selectedKey}
            elementType={ctrl.elementType ?? "object"}
            objectType={ctrl.objectType}
            listFilter={ctrl.listFilter}
        />
    {:else if ctrl.controlType === "exits" && $selectedKey}
        <ExitsEditor elementKey={$selectedKey} />
    {:else if ctrl.controlType === "verbs" && $selectedKey}
        <VerbsEditor elementKey={$selectedKey} />
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
                        <div class="richtext-wrap flex flex-col @2xl:flex-row gap-2 w-full">
                            <textarea
                                autocapitalize="off"
                                class="input text-xs py-0.5 px-1.5 flex-1 min-h-32 resize-y"
                                value={attrValue(ctrl.subAttribute) ?? ""}
                                onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                            ></textarea>
                            {@render textProcessorPanel(ctrl.textProcessorCommands, ctrl.subAttribute, "richtext")}
                        </div>
                    {:else}
                        <textarea
                            autocapitalize="off"
                            class="input text-xs py-0.5 px-1.5 w-full min-h-24 resize-y"
                            value={attrValue(ctrl.subAttribute) ?? ""}
                            onchange={(e) => onTextChange(ctrl.subAttribute!, "richtext", (e.target as HTMLTextAreaElement).value)}
                        ></textarea>
                    {/if}
                {:else if subEditorType === "textbox" && ctrl.subAttribute !== null}
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input text-xs py-0.5 px-1.5 w-full"
                        value={attrValue(ctrl.subAttribute) ?? ""}
                        onchange={(e) => onTextChange(ctrl.subAttribute!, "textbox", (e.target as HTMLInputElement).value)}
                    />
                {:else if subEditorType === "script" && ctrl.subAttribute !== null && $selectedKey !== null}
                    <ScriptEditor elementKey={$selectedKey} attribute={ctrl.subAttribute} />
                {:else if subEditorType === "boolean" && ctrl.subAttribute !== null}
                    <label class="flex items-center gap-2">
                        <input
                            type="checkbox"
                            class="checkbox flex-shrink-0"
                            checked={boolValue(ctrl.subAttribute)}
                            onchange={(e) => onCheckboxChange(ctrl.subAttribute!, (e.target as HTMLInputElement).checked)}
                        />
                        <span class="text-xs text-surface-600-400">{ctrl.checkboxCaption ?? ctrl.subAttribute}</span>
                    </label>
                {/if}
            </div>
        {:else}
            {@const label = ctrl.caption ?? ctrl.attribute}
            {@const isMultiline = ctrl.controlType === "richtext" || ctrl.controlType === "script" || ctrl.controlType === "list" || ctrl.controlType === "stringdictionary" || ctrl.controlType === "scriptdictionary" || ctrl.controlType === "gamebookoptions"}
            {@const stacksBelowLabel = label.length > 20 || isMultiline}
            {#if stacksBelowLabel}
                <div class="flex flex-col gap-1 px-3 py-1.5">
                    <span class="text-xs text-surface-600-400">{label}:</span>
                    {@render controlOnly(ctrl)}
                    {#if ctrl.attribute && attributeErrors[ctrl.attribute]}
                        <p class="text-xs text-error-500">{attributeErrors[ctrl.attribute]}</p>
                    {/if}
                </div>
            {:else}
                <div class="flex flex-col gap-1 px-3 py-1.5">
                    <div class="flex items-center gap-2 min-h-8">
                        <span class="text-xs text-surface-600-400 w-32 flex-shrink-0">{label}:</span>
                        {@render controlOnly(ctrl)}
                    </div>
                    {#if ctrl.attribute && attributeErrors[ctrl.attribute]}
                        <p class="text-xs text-error-500">{attributeErrors[ctrl.attribute]}</p>
                    {/if}
                </div>
            {/if}
        {/if}
    {/if}
{/snippet}
