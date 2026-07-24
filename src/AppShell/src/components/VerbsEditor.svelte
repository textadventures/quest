<script lang="ts">
    import { fullAttributeData, scriptVersion, setAttribute, removeAttribute, changeAttributeType, getVerbAttributesInfo, addVerb } from "$lib/editor-store";
    import type { AttributeDataItem } from "$lib/types";
    import ScriptEditor from "./ScriptEditor.svelte";
    import ScriptDictionaryEditor from "./ScriptDictionaryEditor.svelte";
    import Combobox from "./Combobox.svelte";
    import X from "@lucide/svelte/icons/x";

    interface Props {
        elementKey: string;
    }

    let { elementKey }: Props = $props();

    const TYPE_OPTIONS = [
        { value: "string",           label: "Print a message" },
        { value: "script",           label: "Run a script" },
        { value: "scriptdictionary", label: "Require another object" },
    ];

    // Property name -> friendly display pattern for every verb in the game. Game-wide (not
    // scoped to this object), so it only needs refreshing when the verb list itself could have
    // changed — on undo/redo (scriptVersion) or when a verb was just added to this object.
    let verbInfo = $state<{attribute: string, displayPattern: string}[]>([]);

    $effect(() => {
        void $scriptVersion;
        void elementKey;
        verbInfo = getVerbAttributesInfo();
    });

    let verbPatternByAttribute = $derived(new Map(verbInfo.map(v => [v.attribute, v.displayPattern])));

    // Only the object's own (non-inherited) attributes that happen to be verbs — matches the
    // Attributes tab's own "added to this object" semantics.
    let verbs = $derived(
        ($fullAttributeData?.attributes ?? []).filter(a => !a.isInherited && verbPatternByAttribute.has(a.name))
    );

    let availableVerbPatterns = $derived(
        Array.from(new Set(verbInfo.map(v => v.displayPattern)))
            .sort((a, b) => a.localeCompare(b))
            .map(p => ({ value: p, label: p }))
    );

    let selectedAttrName = $state<string | null>(null);
    let selectedAttr = $derived(
        selectedAttrName ? (verbs.find(v => v.name === selectedAttrName) ?? null) : null
    );

    let prevElementKey = $state<string | null>(null);
    $effect(() => {
        if (elementKey !== prevElementKey) {
            prevElementKey = elementKey;
            selectedAttrName = null;
            newVerbPattern = "";
            addError = null;
        }
    });

    // The "Print a message" value buffer. Deliberately NOT kept in sync via a $effect watching
    // selectedAttr — that reactive chain (selectedAttr -> verbs -> fullAttributeData store, plus
    // verbPatternByAttribute derived from the separately-fetched verbInfo) re-runs far more often
    // than selectedAttr actually changes, which was stomping the buffer back to the committed
    // value on every keystroke and made the textarea appear uneditable. Set explicitly instead,
    // at the three points where the edited value actually should change.
    let editingMessage = $state("");
    let editingMessageOriginal = $state("");

    function syncMessageBuffer(attr: AttributeDataItem | null) {
        editingMessage = attr && attr.type === "string" ? (attr.value ?? "") : "";
        editingMessageOriginal = editingMessage;
    }

    function commitMessage() {
        if (!selectedAttr || editingMessage === editingMessageOriginal) return;
        setAttribute(elementKey, selectedAttr.name, "textbox", editingMessage);
        editingMessageOriginal = editingMessage;
    }

    function onSelectVerb(attr: AttributeDataItem) {
        selectedAttrName = attr.name;
        syncMessageBuffer(attr);
    }

    function onDeleteVerb(attr: AttributeDataItem, e: MouseEvent) {
        e.stopPropagation();
        removeAttribute(elementKey, attr.name);
        if (selectedAttrName === attr.name) selectedAttrName = null;
    }

    function onChangeType(newType: string) {
        if (!selectedAttrName) return;
        changeAttributeType(elementKey, selectedAttrName, newType);
        // ChangeAttributeType resets the backing attribute to a fresh empty string for "string" —
        // reflect that immediately rather than waiting on the async refreshSelectedData() round trip.
        if (newType === "string") {
            editingMessage = "";
            editingMessageOriginal = "";
        }
    }

    // Resizable splitter (matches AttributesEditor's, pointer events not mouse
    // so iPad-width touch devices can drag it) — defaults wider than that one
    // since the verbs list only needs a couple of narrow columns, so most of
    // the width is better spent on the Behaviour panel (script/dictionary
    // editors) rather than left as blank space.
    let panelWidth = $state(480);

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

    // Scroll the (possibly stacked-below) behaviour panel into view whenever a
    // verb is selected — no-op when it's already fully visible (desktop
    // side-by-side layout).
    let panelEl = $state<HTMLDivElement | undefined>();
    $effect(() => {
        if (selectedAttrName && panelEl) panelEl.scrollIntoView({ block: "nearest" });
    });

    let newVerbPattern = $state("");
    let addError = $state<string | null>(null);

    function onAddVerb() {
        const pattern = newVerbPattern.trim();
        if (!pattern) return;
        const result = addVerb(elementKey, pattern);
        if (result.startsWith("error:")) {
            addError = result.slice("error:".length);
            return;
        }
        addError = null;
        newVerbPattern = "";
        verbInfo = getVerbAttributesInfo();
        if (result.startsWith("ok:")) {
            selectedAttrName = result.slice("ok:".length);
            // AddVerb always sets the new verb's value to an empty string ("Print a message").
            editingMessage = "";
            editingMessageOriginal = "";
        }
    }
</script>

<div class="flex flex-col @2xl:flex-row text-xs">
    <!-- Left: verbs list, with "Add Verb" directly beneath it. Deliberately NOT stretched to
         fill the tab's height (unlike AttributesEditor, which is normally packed with rows) —
         verbs lists are usually short or empty, and forcing this column to fill all available
         height pinned "Add Verb" far below the list under a wall of blank space. This sizes to
         content instead, scrolling with the rest of the tab if the list ever gets long. -->
    <div class="flex flex-col flex-1 min-w-0">
        <div class="px-3 py-1.5 border-b border-surface-100-900">
            <span class="font-semibold text-surface-500-400 uppercase tracking-wide">Verbs</span>
        </div>
        <div>
            <table class="w-full">
                <thead class="sticky top-0 bg-surface-50-950 z-10">
                    <tr class="text-surface-400-500 border-b border-surface-200-800">
                        <th class="text-left py-1 px-3 font-medium">Verb</th>
                        <th class="text-left py-1 px-3 font-medium">Behaviour</th>
                        <th class="w-6"></th>
                    </tr>
                </thead>
                <tbody>
                    {#each verbs as attr (attr.name)}
                        {@const isSelected = selectedAttrName === attr.name}
                        {@const display = verbPatternByAttribute.get(attr.name) ?? attr.name}
                        <tr
                            class="border-b border-surface-100-900 cursor-pointer
                                {isSelected ? "bg-primary-100-900" : "hover:bg-surface-100-900"}"
                            onclick={() => onSelectVerb(attr)}
                        >
                            <td class="py-0.5 px-3 font-medium truncate max-w-36" title={display}>{display}</td>
                            <td class="py-0.5 px-3 text-surface-400-500">
                                {TYPE_OPTIONS.find(t => t.value === attr.type)?.label ?? attr.type}
                            </td>
                            <td class="py-0.5 pr-2 text-right">
                                <button
                                    type="button"
                                    class="text-error-500 hover:text-error-700"
                                    onclick={(e) => onDeleteVerb(attr, e)}
                                    title="Remove verb"
                                >✕</button>
                            </td>
                        </tr>
                    {:else}
                        <tr><td colspan="3" class="py-2 px-3 text-surface-400-500 italic">No verbs added yet</td></tr>
                    {/each}
                </tbody>
            </table>
        </div>

        <!-- Add verb -->
        <div class="px-3 py-1.5 flex-shrink-0 border-t border-surface-200-800 flex flex-col gap-1">
            <div class="flex items-center gap-2">
                <Combobox
                    value={newVerbPattern}
                    options={availableVerbPatterns}
                    onchange={(v) => { newVerbPattern = v; }}
                    oninput={(v) => { newVerbPattern = v; }}
                    class="input text-xs py-0.5 px-1.5 flex-1"
                />
                <button
                    type="button"
                    disabled={!newVerbPattern.trim()}
                    onclick={onAddVerb}
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0"
                >Add Verb</button>
            </div>
            {#if addError}
                <p class="text-xs text-error-500">{addError}</p>
            {/if}
        </div>
    </div>

    <!-- Splitter: desktop only -->
    <!-- svelte-ignore a11y_no_static_element_interactions -->
    <div
        class="hidden @2xl:block w-1 flex-shrink-0 cursor-col-resize bg-surface-200-800 hover:bg-primary-400 transition-colors"
        onpointerdown={onSplitterPointerDown}
    ></div>

    <!-- Right: selected verb's behaviour -->
    <div
        bind:this={panelEl}
        class="w-full @2xl:w-[var(--panel-width)] @2xl:flex-shrink-0 flex flex-col overflow-hidden border-l border-surface-200-800"
        style="--panel-width: {panelWidth}px"
    >
        <div class="px-3 py-1.5 border-b border-surface-100-900 font-semibold text-surface-500-400 uppercase tracking-wide flex-shrink-0 flex items-center justify-between">
            <span>Behaviour</span>
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
                <div class="font-medium text-surface-700-300 truncate flex-shrink-0" title={verbPatternByAttribute.get(attr.name) ?? attr.name}>
                    {verbPatternByAttribute.get(attr.name) ?? attr.name}
                </div>

                <div class="flex flex-col gap-1 flex-shrink-0">
                    <span class="text-surface-400-500 uppercase tracking-wide text-xs">Type</span>
                    <select
                        class="select text-xs py-0 px-1.5 h-7"
                        value={attr.type}
                        onchange={(e) => onChangeType((e.target as HTMLSelectElement).value)}
                    >
                        {#each TYPE_OPTIONS as opt (opt.value)}
                            <option value={opt.value}>{opt.label}</option>
                        {/each}
                    </select>
                </div>

                <div class="flex flex-col gap-1">
                    <span class="text-surface-400-500 uppercase tracking-wide text-xs flex-shrink-0">Value</span>
                    {#if attr.type === "script"}
                        <div class="min-h-48">
                            <ScriptEditor elementKey={elementKey} attribute={attr.name} />
                        </div>
                    {:else if attr.type === "scriptdictionary"}
                        <ScriptDictionaryEditor elementKey={elementKey} attribute={attr.name} value={attr.value} keySource="object" />
                    {:else}
                        <textarea
                            autocapitalize="off"
                            class="input text-xs py-1 px-1.5 w-full resize-none"
                            rows="4"
                            value={editingMessage}
                            oninput={(e) => { editingMessage = (e.target as HTMLTextAreaElement).value; }}
                            onblur={commitMessage}
                            onkeydown={(e) => { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); commitMessage(); } }}
                        ></textarea>
                    {/if}
                </div>
            </div>
        {:else}
            <p class="p-3 text-surface-400-500 italic">Select a verb to edit its behaviour.</p>
        {/if}
    </div>
</div>
