<script lang="ts">
    import { fullAttributeData, selectedKey, removeAttribute, addInheritedType, removeInheritedType, getTypeNames, setAttribute, setObjectReference, changeAttributeType, setPatternAttribute, getObjectNames } from "$lib/editor-store";
    import type { AttributeDataItem } from "$lib/types";
    import { Switch } from "@skeletonlabs/skeleton-svelte";
    import X from "@lucide/svelte/icons/x";
    import ScriptEditor from "./ScriptEditor.svelte";
    import ListEditor from "./ListEditor.svelte";
    import DictionaryEditor from "./DictionaryEditor.svelte";
    import ScriptDictionaryEditor from "./ScriptDictionaryEditor.svelte";

    const TYPE_OPTIONS = [
        { value: "string",           label: "String" },
        { value: "boolean",          label: "Boolean" },
        { value: "int",              label: "Integer" },
        { value: "double",           label: "Double" },
        { value: "script",           label: "Script" },
        { value: "stringlist",       label: "String List" },
        { value: "object",           label: "Object" },
        { value: "simplepattern",    label: "Command pattern" },
        { value: "stringdictionary", label: "String dictionary" },
        { value: "scriptdictionary", label: "Script dictionary" },
        { value: "null",             label: "Null" },
    ];

    let typeNames = $state<string[]>([]);
    let objectNames = $state<string[]>([]);
    let addTypeValue = $state("");
    let newAttrName = $state("");

    // Track selected attribute by name so selection survives data refreshes
    let selectedAttrName = $state<string | null>(null);

    let selectedAttr = $derived(
        selectedAttrName
            ? ($fullAttributeData?.attributes.find(a => a.name === selectedAttrName) ?? null)
            : null
    );

    // Editing state for string/pattern/number values
    let editingValue = $state("");
    let editingValueOriginal = $state("");
    let editingBool = $state(false);
    let editingObject = $state("");

    // Reset when element changes (selectedKey changes)
    let prevSelectedKey = $state<string | null>(null);
    $effect(() => {
        const key = $selectedKey;
        if (key !== prevSelectedKey) {
            prevSelectedKey = key;
            selectedAttrName = null;
            editingValue = "";
            editingValueOriginal = "";
            editingBool = false;
            editingObject = "";
            newAttrName = "";
        }
    });

    // Sync editing state when selectedAttr changes
    $effect(() => {
        const attr = selectedAttr;
        if (!attr) return;
        if (attr.type === "boolean") {
            editingBool = attr.value?.toLowerCase() === "true";
            editingValue = "";
        } else if (attr.type === "object") {
            editingObject = attr.value ?? "";
            editingValue = "";
        } else if (isTextEditable(attr)) {
            const v = attr.value ?? "";
            editingValue = v;
            editingValueOriginal = v;
        } else {
            editingValue = "";
            editingValueOriginal = "";
        }
    });

    $effect(() => {
        typeNames = getTypeNames();
        objectNames = getObjectNames() ?? [];
    });

    // Resizable splitter (pointer events, not mouse — same pattern as
    // routes/edit/+page.svelte's handleSplitterPointerDown, so iPad-width
    // touch devices can drag it too)
    let panelWidth = $state(360);

    function onSplitterPointerDown(e: PointerEvent) {
        e.preventDefault();
        const startX = e.clientX;
        const startWidth = panelWidth;
        document.body.style.cursor = "col-resize";
        document.body.style.userSelect = "none";

        function onMove(moveEvent: PointerEvent) {
            const next = startWidth + (startX - moveEvent.clientX);
            panelWidth = Math.max(180, Math.min(900, next));
        }
        function onUp() {
            document.removeEventListener("pointermove", onMove);
            document.removeEventListener("pointerup", onUp);
            document.body.style.cursor = "";
            document.body.style.userSelect = "";
        }
        document.addEventListener("pointermove", onMove);
        document.addEventListener("pointerup", onUp);
    }

    // Scroll the (possibly stacked-below) assignment panel into view whenever
    // an attribute is selected, so picking a row doesn't leave the editor
    // off-screen below a long attributes table on a phone. A no-op when the
    // panel is already fully visible (desktop side-by-side layout).
    let panelEl = $state<HTMLDivElement | undefined>();
    $effect(() => {
        if (selectedAttrName && panelEl) panelEl.scrollIntoView({ block: "nearest" });
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
        if (selectedAttrName === attr.name) selectedAttrName = null;
    }

    let attrTbodyEl = $state<HTMLTableSectionElement | null>(null);

    function onSelectAttribute(attr: AttributeDataItem) {
        selectedAttrName = attr.name;
    }

    function focusAttrRow(name: string) {
        const el = attrTbodyEl?.querySelector<HTMLTableRowElement>(`[data-attr="${CSS.escape(name)}"]`);
        el?.focus();
        el?.scrollIntoView({ block: "nearest" });
    }

    function onAttrRowKeydown(e: KeyboardEvent, attr: AttributeDataItem) {
        const attrs = $fullAttributeData?.attributes ?? [];
        const idx = attrs.findIndex(a => a.name === attr.name);
        if (e.key === "ArrowDown") {
            e.preventDefault();
            if (idx < attrs.length - 1) { onSelectAttribute(attrs[idx + 1]); focusAttrRow(attrs[idx + 1].name); }
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            if (idx > 0) { onSelectAttribute(attrs[idx - 1]); focusAttrRow(attrs[idx - 1].name); }
        } else if (e.key === "Home") {
            e.preventDefault();
            if (attrs.length > 0) { onSelectAttribute(attrs[0]); focusAttrRow(attrs[0].name); }
        } else if (e.key === "End") {
            e.preventDefault();
            if (attrs.length > 0) { onSelectAttribute(attrs[attrs.length - 1]); focusAttrRow(attrs[attrs.length - 1].name); }
        } else if (e.key === "Delete" && $selectedKey && canDeleteAttribute(attr)) {
            e.preventDefault();
            removeAttribute($selectedKey, attr.name);
            const next = attrs[idx + 1] ?? attrs[idx - 1];
            if (next) { onSelectAttribute(next); focusAttrRow(next.name); } else { selectedAttrName = null; }
        }
    }

    function onChangeType(newType: string) {
        if ($selectedKey && selectedAttrName) {
            changeAttributeType($selectedKey, selectedAttrName, newType);
        }
    }

    function commitTextEdit() {
        if (!$selectedKey || !selectedAttr) return;
        if (editingValue === editingValueOriginal) return;
        const attr = selectedAttr;
        if (attr.type === "simplepattern") {
            setPatternAttribute($selectedKey, attr.name, editingValue);
        } else if (attr.type === "int") {
            setAttribute($selectedKey, attr.name, "number", editingValue);
        } else if (attr.type === "double") {
            setAttribute($selectedKey, attr.name, "numberdouble", editingValue);
        } else if (attr.type === "string") {
            setAttribute($selectedKey, attr.name, "textbox", editingValue);
        }
        editingValueOriginal = editingValue;
    }

    function onBoolChange(checked: boolean) {
        if ($selectedKey && selectedAttr) {
            setAttribute($selectedKey, selectedAttr.name, "checkbox", checked ? "true" : "false");
        }
    }

    function onObjectChange(value: string) {
        if ($selectedKey && selectedAttr) {
            setObjectReference($selectedKey, selectedAttr.name, value);
        }
    }

    function onAddAttribute() {
        if ($selectedKey && newAttrName.trim()) {
            const name = newAttrName.trim();
            setAttribute($selectedKey, name, "textbox", "");
            newAttrName = "";
            selectedAttrName = name;
            // Focus the new row after Svelte renders it
            Promise.resolve().then(() => requestAnimationFrame(() => focusAttrRow(name)));
        }
    }

    const UNDELETABLE_ATTRIBUTES = new Set(["name", "type", "elementtype"]);

    function canDeleteAttribute(attr: AttributeDataItem): boolean {
        return !attr.isInherited && !UNDELETABLE_ATTRIBUTES.has(attr.name);
    }

    function isTextEditable(attr: AttributeDataItem): boolean {
        return attr.type === "string" || attr.type === "int" || attr.type === "double" || attr.type === "simplepattern";
    }

    function displayValue(attr: AttributeDataItem): string {
        if (attr.value === null) return "(null)";
        if (attr.type === "stringlist") {
            try {
                const items = JSON.parse(attr.value) as {key: string, value: string}[];
                return items.map(i => i.value).join(", ") || "(empty list)";
            } catch { /* fall through */ }
        }
        if (attr.type === "stringdictionary") {
            try {
                const items = JSON.parse(attr.value) as {key: string, value: string}[];
                return items.map(i => `${i.key}=${i.value}`).join(", ") || "(empty dict)";
            } catch { /* fall through */ }
        }
        if (attr.type === "scriptdictionary") {
            try {
                const items = JSON.parse(attr.value) as {key: string, value: string}[];
                return items.map(i => `${i.key}=${i.value}`).join(", ") || "(empty dict)";
            } catch { /* fall through */ }
        }
        return attr.value;
    }

</script>

<div class="flex flex-col flex-1 min-h-0 text-xs">
    <!-- Inherited types section. The "Inherited types" title and the "Add
         type…" control stay always visible (flex-shrink-0); only the rows
         themselves get a capped, independently-scrollable region below @2xl,
         so a long list of inherited types can't crowd out the attributes
         list and assignment panel below it (both need real allocated space
         of their own once stacked — see the split pane below). -->
    <div class="flex-shrink-0 border-b border-surface-200-800">
        <div class="px-3 py-1.5 border-b border-surface-100-900">
            <span class="font-semibold text-surface-500-400 uppercase tracking-wide">Inherited types</span>
        </div>
        <div class="max-h-28 overflow-y-auto @2xl:max-h-none @2xl:overflow-visible">
            <table class="w-full">
                <thead>
                    <tr class="text-surface-400-500 border-b border-surface-100-900">
                        <th class="text-left py-1 px-3 font-medium">Name</th>
                        <th class="text-left py-1 px-3 font-medium">Source</th>
                        <th class="w-6"></th>
                    </tr>
                </thead>
                <tbody>
                    {#each $fullAttributeData?.inheritedTypes ?? [] as t (t.name)}
                        <tr class="border-b border-surface-100-900">
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
                        <tr><td colspan="3" class="py-1 px-3 text-surface-400-500 italic">No inherited types</td></tr>
                    {/each}
                </tbody>
            </table>
        </div>
        <div class="px-3 py-1.5">
            <div class="flex items-center gap-2 max-w-xs">
                <select
                    class="select text-xs py-0 px-1.5 h-6 flex-1"
                    bind:value={addTypeValue}
                >
                    <option value="">Add type…</option>
                    {#each availableTypes() as t (t)}
                        <option value={t}>{t}</option>
                    {/each}
                </select>
                <button
                    type="button"
                    disabled={!addTypeValue}
                    onclick={onAddType}
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0 h-6 flex-shrink-0"
                >Add</button>
            </div>
        </div>
    </div>

    <!-- Attributes section: split pane. Stacks list-above-panel below the
         @2xl container breakpoint (the properties pane's own width, not the
         viewport — it can be narrow on desktop too when the splitter is
         dragged in). Bounded (flex-1 min-h-0) at every width, in both
         directions, so the list and the assignment panel each get their own
         real allocated space and internal scroll — never "panel appended
         after the whole list", which on a long list meant scrolling past
         every row just to reach the panel you just opened. -->
    <div class="flex flex-col @2xl:flex-row flex-1 min-h-0">
        <!-- Left: attributes list -->
        <div class="flex flex-col flex-1 min-h-0 min-w-0 overflow-hidden">
            <div class="px-3 py-1.5 border-b border-surface-100-900 flex-shrink-0">
                <span class="font-semibold text-surface-500-400 uppercase tracking-wide">Attributes</span>
            </div>

            <!-- Scrollable table -->
            <div class="overflow-y-auto flex-1">
                <table class="w-full">
                    <thead class="sticky top-0 bg-surface-50-950 z-10">
                        <tr class="text-surface-400-500 border-b border-surface-200-800">
                            <th class="text-left py-1 px-3 font-medium">Name</th>
                            <th class="text-left py-1 px-3 font-medium">Value</th>
                            <th class="hidden @lg:table-cell text-left py-1 px-3 font-medium">Source</th>
                            <th class="w-6"></th>
                        </tr>
                    </thead>
                    <tbody bind:this={attrTbodyEl}>
                        {#each $fullAttributeData?.attributes ?? [] as attr (attr.name)}
                            {@const dimmed = attr.isInherited || attr.isDefaultType}
                            {@const isSelected = selectedAttrName === attr.name}
                            <tr
                                class="border-b border-surface-100-900 cursor-pointer outline-none
                                    focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-primary-500
                                    {isSelected ? "bg-primary-100-900" : "hover:bg-surface-100-900"}
                                    {dimmed ? "text-surface-400-500" : ""}"
                                tabindex={isSelected ? 0 : -1}
                                data-attr={attr.name}
                                onclick={() => onSelectAttribute(attr)}
                                onkeydown={(e) => onAttrRowKeydown(e, attr)}
                            >
                                <td class="py-0.5 px-3 truncate max-w-36 {!dimmed ? "font-medium" : ""}" title={attr.name}>{attr.name}</td>
                                <td class="py-0.5 px-3 max-w-40 truncate" title={attr.value ?? ""}>{displayValue(attr)}</td>
                                <td class="hidden @lg:table-cell py-0.5 px-3 text-surface-400-500 truncate" title={attr.source}>{attr.source}</td>
                                <td class="py-0.5 pr-2 text-right">
                                    {#if canDeleteAttribute(attr)}
                                        <button
                                            type="button"
                                            tabindex="-1"
                                            class="text-error-500 hover:text-error-700"
                                            onclick={(e) => onDeleteAttribute(attr, e)}
                                            title="Remove attribute (or press Delete)"
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

        <!-- Splitter: desktop only (the @2xl split above only applies past this
             breakpoint, where there's a row to drag between) -->
        <!-- svelte-ignore a11y_no_static_element_interactions -->
        <div
            class="hidden @2xl:block w-1 flex-shrink-0 cursor-col-resize bg-surface-200-800 hover:bg-primary-400 transition-colors"
            onpointerdown={onSplitterPointerDown}
        ></div>

        <!-- Right (desktop) / bottom (stacked) panel: reserves a real, always-
             visible share of the split area's height once a row is selected
             (evenly with the list, both scrolling internally) — pinned in
             place rather than trailing after the list, which on a long list
             meant scrolling past every row to reach the panel you just
             opened. Content-sized (not flex-1) when nothing is selected, so
             the idle "Select an attribute…" message doesn't reserve that
             space up front. -->
        <div
            bind:this={panelEl}
            class="w-full @2xl:w-[var(--panel-width)] @2xl:flex-shrink-0 @2xl:min-h-0 flex flex-col overflow-hidden"
            class:flex-1={!!selectedAttr}
            class:min-h-0={!!selectedAttr}
            class:flex-shrink-0={!selectedAttr}
            style="--panel-width: {panelWidth}px"
        >
            <div class="px-3 py-1.5 border-b border-surface-100-900 font-semibold text-surface-500-400 uppercase tracking-wide flex-shrink-0 flex items-center justify-between">
                <span>Assignment</span>
                {#if selectedAttr}
                    <button
                        type="button"
                        class="normal-case text-surface-400-500 hover:text-surface-900-50"
                        onclick={() => { selectedAttrName = null; }}
                        title="Close"
                        aria-label="Close"
                    ><X size={14} /></button>
                {/if}
            </div>
            {#if selectedAttr}
                {@const attr = selectedAttr}
                <div class="p-3 flex flex-col gap-2 overflow-y-auto">
                    <div class="font-medium text-surface-700-300 truncate flex-shrink-0" title={attr.name}>{attr.name}</div>

                    {#if attr.isInherited || attr.isDefaultType}
                        <p class="text-surface-400-500 italic text-xs flex-shrink-0">Inherited from <span class="font-medium">{attr.source}</span></p>
                    {/if}

                    <!-- Type selector -->
                    <div class="flex flex-col gap-1 flex-shrink-0">
                        <span class="text-surface-400-500 uppercase tracking-wide text-xs">Type</span>
                        <select
                            class="select text-xs py-0 px-1.5 h-7"
                            value={attr.type}
                            onchange={(e) => onChangeType((e.target as HTMLSelectElement).value)}
                            disabled={attr.isDefaultType}
                        >
                            {#each TYPE_OPTIONS as opt (opt.value)}
                                <option value={opt.value}>{opt.label}</option>
                            {/each}
                        </select>
                    </div>

                    <!-- Value editor -->
                    <div class="flex flex-col gap-1">
                        <span class="text-surface-400-500 uppercase tracking-wide text-xs flex-shrink-0">Value</span>
                        {#if attr.type === "null"}
                            <p class="text-surface-400-500 italic">(no value)</p>
                        {:else if attr.type === "boolean"}
                            <Switch
                                checked={editingBool}
                                disabled={attr.isDefaultType}
                                onCheckedChange={(e) => { editingBool = e.checked; onBoolChange(editingBool); }}
                            >
                                <Switch.Control><Switch.Thumb /></Switch.Control>
                                <Switch.HiddenInput />
                                <Switch.Label>{editingBool ? "True" : "False"}</Switch.Label>
                            </Switch>
                        {:else if attr.type === "script"}
                            <div class="min-h-48">
                                {#if $selectedKey}
                                    <ScriptEditor elementKey={$selectedKey} attribute={attr.name} isLocked={attr.isInherited || attr.isDefaultType} />
                                {/if}
                            </div>
                        {:else if attr.type === "scriptdictionary"}
                            {#if $selectedKey}
                                <ScriptDictionaryEditor elementKey={$selectedKey} attribute={attr.name} value={attr.value} isLocked={attr.isInherited || attr.isDefaultType} />
                            {/if}
                        {:else if attr.type === "stringlist"}
                            {#if $selectedKey}
                                <ListEditor elementKey={$selectedKey} attribute={attr.name} value={attr.value} />
                            {/if}
                        {:else if attr.type === "stringdictionary"}
                            {#if $selectedKey}
                                <DictionaryEditor elementKey={$selectedKey} attribute={attr.name} value={attr.value} />
                            {/if}
                        {:else if attr.type === "object"}
                            <select
                                class="select text-xs py-0 px-1.5 h-7"
                                bind:value={editingObject}
                                disabled={attr.isDefaultType}
                                onchange={(e) => onObjectChange((e.target as HTMLSelectElement).value)}
                            >
                                {#each objectNames as name (name)}
                                    <option value={name}>{name}</option>
                                {/each}
                            </select>
                        {:else if attr.type === "int" || attr.type === "double"}
                            <input
                                type="number"
                                class="input text-xs py-1 px-1.5 w-full"
                                step={attr.type === "double" ? "any" : "1"}
                                value={editingValue}
                                disabled={attr.isDefaultType}
                                oninput={(e) => { editingValue = (e.target as HTMLInputElement).value; }}
                                onblur={commitTextEdit}
                                onkeydown={(e) => { if (e.key === "Enter") { e.preventDefault(); commitTextEdit(); } }}
                            />
                        {:else}
                            <!-- string, simplepattern -->
                            <textarea
                                autocapitalize="off"
                                class="input text-xs py-1 px-1.5 w-full resize-none"
                                rows="4"
                                value={editingValue}
                                disabled={attr.isDefaultType}
                                oninput={(e) => { editingValue = (e.target as HTMLTextAreaElement).value; }}
                                onblur={commitTextEdit}
                                onkeydown={(e) => { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); commitTextEdit(); } }}
                            ></textarea>
                        {/if}
                    </div>
                </div>
            {:else}
                <p class="p-3 text-surface-400-500 italic">Select an attribute to edit it.</p>
            {/if}
        </div>
    </div>

    <!-- Add new attribute -->
    <div class="px-3 py-1.5 flex-shrink-0 border-t border-surface-200-800">
        <div class="flex items-center gap-2 max-w-xs">
            <input
                type="text"
                autocapitalize="off"
                placeholder="Add attribute..."
                class="input text-xs py-0 px-1.5 h-6 flex-1 min-w-0"
                data-staging
                bind:value={newAttrName}
                onkeydown={(e) => { if (e.key === "Enter") onAddAttribute(); }}
            />
            <button
                type="button"
                disabled={!newAttrName.trim()}
                onclick={onAddAttribute}
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0 h-6 flex-shrink-0"
            >Add</button>
        </div>
    </div>
</div>
