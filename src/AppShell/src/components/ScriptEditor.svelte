<script lang="ts">
    import { SvelteSet } from "svelte/reactivity";
    import ScriptEditor from "./ScriptEditor.svelte";
    import AddScriptModal from "./AddScriptModal.svelte";
    import AssetPicker from "./AssetPicker.svelte";
    import {
        scriptVersion,
        scriptClipboardHasContent,
        getScriptData,
        getScriptCode,
        setScriptCode,
        copyScripts,
        cutScripts,
        deleteScripts,
        pasteScripts,
        setScriptParameter,
        setIfExpression,
        setElseIfExpression,
        addScript,
        deleteScript,
        moveScript,
        addElse,
        addElseIf,
        removeElse,
        removeElseIf,
        getScriptCommandCategories,
        getObjectNames,
        getExitNames,
        getIfExpressionTemplates,
        getIfExpressionTemplateData,
        makeScriptEditable,
    } from "$lib/editor-store";
    import type {
        ScriptBlockData,
        ScriptNodeData,
        ScriptControlData,
        ScriptCategoryInfo,
        IfExpressionTemplateData,
        IfExpressionTemplate,
    } from "$lib/types";

    interface Props {
        elementKey: string;
        attribute: string;
        containerPath?: string;
        // Pre-loaded data for nested blocks (avoids extra WASM round-trips)
        initialData?: ScriptNodeData[] | null;
        depth?: number;
        isLocked?: boolean;
    }

    let {
        elementKey,
        attribute,
        containerPath = "",
        initialData = null,
        depth = 0,
        isLocked = false,
    }: Props = $props();

    let scriptData = $state<ScriptBlockData | null>(null);
    let categories = $state<ScriptCategoryInfo[]>([]);
    let showAddModal = $state(false);
    let isRoot = $derived(initialData === null);
    let codeViewMode = $state(false);
    let scriptCode = $state("");
    const selectedIndices = new SvelteSet<number>();
    // Set before a move mutation so the $effect restores it instead of clearing
    let nextSelection: Set<number> | null = null;
    // Tracks which expression controls the user has explicitly forced into expression mode,
    // overriding the default simple-mode detection based on value shape.
    const expressionOverrides = new SvelteSet<string>();
    let objectNames = $state<string[]>([]);
    let exitNames = $state<string[]>([]);
    let ifTemplates = $state<IfExpressionTemplate[]>([]);

    // Load on mount and whenever scriptVersion bumps (undo/redo)
    $effect(() => {
        const version = $scriptVersion; // track for reactivity on undo/redo
        if (isRoot) {
            scriptData = version >= 0 ? getScriptData(elementKey, attribute) : null;
            selectedIndices.clear();
            for (const i of nextSelection ?? []) selectedIndices.add(i);
            nextSelection = null;
        }
        if (categories.length === 0) {
            void getScriptCommandCategories().then(cats => {
                if (cats) categories = cats.categories;
            });
        }
        if (objectNames.length === 0) {
            const names = getObjectNames();
            if (names && names.length > 0) objectNames = names;
        }
        if (exitNames.length === 0) {
            const names = getExitNames();
            if (names && names.length > 0) exitNames = names;
        }
        if (ifTemplates.length === 0) {
            const templates = getIfExpressionTemplates();
            if (templates && templates.length > 0) ifTemplates = templates;
        }
    });

    function scripts(): ScriptNodeData[] {
        if (!isRoot && initialData !== null) return initialData;
        return scriptData?.scripts ?? [];
    }

    function refresh() {
        if (isRoot) {
            scriptData = getScriptData(elementKey, attribute);
        }
        expressionOverrides.clear();
    }

    function mutate(fn: () => string): boolean {
        const result = fn();
        if (result === "ok") {
            refresh();
            return true;
        }
        return false;
    }

    // Expression / simple-mode helpers

    function exprKey(scriptIndex: number, attr: string): string {
        return `${containerPath}/${scriptIndex}/${attr}`;
    }

    function namesForControl(ctrl: ScriptControlData): string[] {
        return ctrl.objectType === "exit" ? exitNames : objectNames;
    }

    function isSimpleValue(ctrl: ScriptControlData): boolean {
        const v = ctrl.value ?? "";
        switch (ctrl.simpleEditor) {
            case "boolean":
                return v === "true" || v === "false";
            case "number":
            case "numberdouble":
                return v !== "" && Number.isFinite(Number(v));
            case "objects":
                // Bare identifier (object name) or empty — not a complex expression
                return v === "" || /^[a-zA-Z_][a-zA-Z0-9_]*$/.test(v);
            default:
                // textbox, dropdown, file: simple when value is a quoted string literal
                return v.length >= 2 && v.startsWith('"') && v.endsWith('"');
        }
    }

    function inSimpleMode(ctrl: ScriptControlData, scriptIndex: number): boolean {
        if (!ctrl.simpleEditor) return false;
        const key = exprKey(scriptIndex, ctrl.attribute!);
        if (expressionOverrides.has(key)) return false;
        return isSimpleValue(ctrl);
    }

    function toSimpleDisplay(ctrl: ScriptControlData): string {
        const v = ctrl.value ?? "";
        switch (ctrl.simpleEditor) {
            case "boolean":
            case "number":
            case "numberdouble":
            case "objects":
                return v;
            default: {
                if (v.length < 2) return "";
                return v.slice(1, -1).replace(/<br\/>/g, "\n").replace(/\\"/g, '"');
            }
        }
    }

    function fromSimpleToExpression(simpleValue: string, simpleEditor: string): string {
        switch (simpleEditor) {
            case "boolean":
            case "number":
            case "numberdouble":
            case "objects":
                return simpleValue;
            default: {
                const escaped = simpleValue.replace(/"/g, '\\"').replace(/\n/g, "<br/>");
                return `"${escaped}"`;
            }
        }
    }

    function setExpressionOverride(scriptIndex: number, attr: string) {
        expressionOverrides.add(exprKey(scriptIndex, attr));
    }

    function clearExpressionOverride(scriptIndex: number, attr: string) {
        expressionOverrides.delete(exprKey(scriptIndex, attr));
    }

    function onSwitchToExpression(scriptIndex: number, ctrl: ScriptControlData) {
        setExpressionOverride(scriptIndex, ctrl.attribute!);
    }

    function onSwitchToSimple(scriptIndex: number, ctrl: ScriptControlData) {
        clearExpressionOverride(scriptIndex, ctrl.attribute!);
        // If the current value is not a valid simple value, reset to a default simple value
        if (!isSimpleValue(ctrl)) {
            switch (ctrl.simpleEditor) {
                case "boolean":
                    onSetParam(scriptIndex, ctrl.attribute!, "true");
                    break;
                case "number":
                case "numberdouble":
                    onSetParam(scriptIndex, ctrl.attribute!, "0");
                    break;
                default:
                    onSetParam(scriptIndex, ctrl.attribute!, '""');
            }
        }
    }

    function onSimpleValueChange(scriptIndex: number, ctrl: ScriptControlData, simpleValue: string) {
        onSetParam(scriptIndex, ctrl.attribute!, fromSimpleToExpression(simpleValue, ctrl.simpleEditor!));
    }

    function buildTemplateExpression(tmpl: IfExpressionTemplateData, changedName: string, changedValue: string): string {
        let result = tmpl.originalPattern;
        for (const ctrl of tmpl.controls) {
            const value = ctrl.name === changedName ? changedValue : (ctrl.value ?? "");
            result = result.replace(`#${ctrl.name}#`, value);
        }
        return result;
    }

    function onToggleCodeView() {
        codeViewMode = !codeViewMode;
        if (codeViewMode) {
            scriptCode = getScriptCode(elementKey, attribute);
        } else {
            refresh();
        }
    }

    function onCodeViewSave(value: string) {
        const result = setScriptCode(elementKey, attribute, value);
        if (result === "ok") {
            scriptCode = value;
        }
    }

    function sortedSelection(): number[] {
        return [...selectedIndices].sort((a, b) => a - b);
    }

    function toggleSelection(i: number, checked: boolean) {
        if (checked) selectedIndices.add(i); else selectedIndices.delete(i);
    }

    function onCopySelected() {
        // no scriptVersion bump — selection intentionally preserved after copy
        copyScripts(elementKey, attribute, containerPath, sortedSelection());
    }

    function onCutSelected() {
        // selectedIndices cleared by $effect when scriptVersion bumps
        cutScripts(elementKey, attribute, containerPath, sortedSelection());
    }

    function onDeleteSelected() {
        deleteScripts(elementKey, attribute, containerPath, sortedSelection());
    }

    function onPaste() {
        pasteScripts(elementKey, attribute, containerPath);
    }

    function onMoveUpSelected() {
        const [idx] = sortedSelection();
        if (idx <= 0) return;
        nextSelection = new Set([idx - 1]);
        mutate(() => moveScript(elementKey, attribute, containerPath, idx, idx - 1));
    }

    function onMoveDownSelected() {
        const [idx] = sortedSelection();
        if (idx >= scripts().length - 1) return;
        nextSelection = new Set([idx + 1]);
        mutate(() => moveScript(elementKey, attribute, containerPath, idx, idx + 1));
    }

    function onAddScript(createString: string) {
        mutate(() => addScript(elementKey, attribute, containerPath, createString));
    }

    function onDelete(index: number) {
        mutate(() => deleteScript(elementKey, attribute, containerPath, index));
    }

    function onMoveUp(index: number) {
        if (index === 0) return;
        mutate(() => moveScript(elementKey, attribute, containerPath, index, index - 1));
    }

    function onMoveDown(index: number) {
        const list = scripts();
        if (index >= list.length - 1) return;
        mutate(() => moveScript(elementKey, attribute, containerPath, index, index + 1));
    }

    function onSetParam(scriptIndex: number, paramName: string, value: string) {
        mutate(() => setScriptParameter(elementKey, attribute, containerPath, scriptIndex, paramName, value));
    }

    function onSetIfExpr(scriptIndex: number, expression: string) {
        mutate(() => setIfExpression(elementKey, attribute, containerPath, scriptIndex, expression));
    }

    function onSetElseIfExpr(scriptIndex: number, elseIfIndex: number, expression: string) {
        mutate(() => setElseIfExpression(elementKey, attribute, containerPath, scriptIndex, elseIfIndex, expression));
    }

    function onAddElse(scriptIndex: number) {
        mutate(() => addElse(elementKey, attribute, containerPath, scriptIndex));
    }

    function onAddElseIf(scriptIndex: number) {
        mutate(() => addElseIf(elementKey, attribute, containerPath, scriptIndex));
    }

    function onRemoveElse(scriptIndex: number) {
        mutate(() => removeElse(elementKey, attribute, containerPath, scriptIndex));
    }

    function onRemoveElseIf(scriptIndex: number, elseIfIndex: number) {
        mutate(() => removeElseIf(elementKey, attribute, containerPath, scriptIndex, elseIfIndex));
    }

    // Nested container path helpers
    function thenPath(scriptIndex: number): string {
        return containerPath ? `${containerPath}/${scriptIndex}/then` : `${scriptIndex}/then`;
    }
    function elsePath(scriptIndex: number): string {
        return containerPath ? `${containerPath}/${scriptIndex}/else` : `${scriptIndex}/else`;
    }
    function elseIfPath(scriptIndex: number, elseIfIndex: number): string {
        const base = containerPath ? `${containerPath}/${scriptIndex}` : `${scriptIndex}`;
        return `${base}/elseif/${elseIfIndex}`;
    }
    function paramPath(scriptIndex: number, paramAttr: string): string {
        const base = containerPath ? `${containerPath}/${scriptIndex}` : `${scriptIndex}`;
        return `${base}/param/${paramAttr}`;
    }

    const indentClass = $derived(depth > 0 ? "ml-4 border-l border-surface-300-700 pl-2" : "");
</script>

<div class={indentClass}>
    {#if isRoot && isLocked}
        <div class="flex items-center gap-2 py-1 px-2 mb-1 text-xs text-surface-400-500 italic border border-surface-200-800 rounded">
            <span class="flex-1">This script is inherited — read-only.</span>
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0 not-italic"
                onclick={onToggleCodeView}
            >{codeViewMode ? "Visual view" : "Code view"}</button>
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0 not-italic"
                onclick={() => makeScriptEditable(elementKey, attribute)}
            >Make editable copy</button>
        </div>
    {/if}
    {#if codeViewMode}
        <textarea
            autocapitalize="off"
            class="textarea text-xs font-mono w-full"
            rows={10}
            readonly={isLocked}
            value={scriptCode}
            onchange={(e) => onCodeViewSave((e.target as HTMLTextAreaElement).value)}
        ></textarea>
    {:else}
        <div role="region" inert={isLocked || undefined} class={isLocked ? "opacity-60" : ""}>
            {#each scripts() as script, i (script.id)}
                <div class="group relative border border-surface-200-800 rounded mb-1 bg-surface-50-950 flex items-start">
                    {#if isRoot}
                        <label class="flex items-start pt-1.5 pl-1.5 pr-0.5 cursor-pointer flex-shrink-0">
                            <input
                                type="checkbox"
                                class="checkbox size-3.5"
                                checked={selectedIndices.has(i)}
                                onchange={(e) => toggleSelection(i, (e.target as HTMLInputElement).checked)}
                            />
                        </label>
                    {/if}
                    <div class="flex-1 min-w-0">
                        <!-- Script row actions (hover). Root-level rows have a checkbox +
                             selection toolbar below (Cut/Copy/Delete/Move) that already
                             covers this on touch, so only force these always-visible
                             (pointer-coarse:opacity-100) for nested if/for/while blocks,
                             which have no checkbox and no other way to reorder/delete. -->
                        <div class="absolute right-1 top-1 flex gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity z-10 {isRoot ? "" : "pointer-coarse:opacity-100"}">
                            <button
                                type="button"
                                class="btn btn-sm preset-outlined-primary-500 px-1 py-0 text-xs leading-none"
                                title="Move up"
                                disabled={i === 0}
                                onclick={() => onMoveUp(i)}
                            >↑</button>
                            <button
                                type="button"
                                class="btn btn-sm preset-outlined-primary-500 px-1 py-0 text-xs leading-none"
                                title="Move down"
                                disabled={i === scripts().length - 1}
                                onclick={() => onMoveDown(i)}
                            >↓</button>
                            <button
                                type="button"
                                class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none"
                                title="Delete"
                                onclick={() => onDelete(i)}
                            >×</button>
                        </div>

                        {#if script.type === "if"}
                            {@render ifBlock(script, i)}
                        {:else}
                            {@render normalBlock(script, i)}
                        {/if}
                    </div>
                </div>
            {/each}

            <!-- Selection toolbar -->
            {#if isRoot && selectedIndices.size > 0}
                {@const sel = sortedSelection()}
                <div class="flex items-center gap-1 mb-1 px-1 py-1 bg-surface-100-900 rounded border border-surface-200-800 text-xs">
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                        onclick={onCutSelected}
                    >Cut</button>
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                        onclick={onCopySelected}
                    >Copy</button>
                    <button
                        type="button"
                        class="btn btn-sm preset-tonal-error text-xs py-0.5"
                        onclick={onDeleteSelected}
                    >Delete</button>
                    {#if sel.length === 1}
                        <span class="w-px h-4 bg-surface-300-700 mx-0.5"></span>
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                            disabled={sel[0] === 0}
                            onclick={onMoveUpSelected}
                        >↑ Move up</button>
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                            disabled={sel[0] === scripts().length - 1}
                            onclick={onMoveDownSelected}
                        >↓ Move down</button>
                    {/if}
                    <span class="ml-auto text-surface-400-500">{sel.length} selected</span>
                </div>
            {/if}
        </div>
    {/if}

    {#if !(isRoot && isLocked)}
        <!-- Add script row -->
        <div class="flex items-center gap-1 mt-1 flex-wrap">
            {#if !codeViewMode && categories.length > 0}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={() => (showAddModal = true)}
                >+ Add script</button>
            {:else if !codeViewMode && isRoot}
                <span class="text-xs text-surface-400-500 italic">Loading commands…</span>
            {/if}
            {#if isRoot && $scriptClipboardHasContent && !codeViewMode}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={onPaste}
                >Paste</button>
            {/if}
            {#if isRoot}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={onToggleCodeView}
                >{codeViewMode ? "Visual editor" : "Code view"}</button>
            {/if}
        </div>

        {#if showAddModal}
            <AddScriptModal
                {categories}
                onAdd={onAddScript}
                onClose={() => (showAddModal = false)}
            />
        {/if}
    {/if}
</div>

{#snippet normalBlock(script: ScriptNodeData, i: number)}
    <div class="px-2 py-1 pr-16 flex flex-wrap items-center gap-x-1 gap-y-0.5 text-xs">
        {#each script.controls ?? [] as ctrl, ci (ci)}
            {@render inlineControl(ctrl, i)}
        {/each}
    </div>
{/snippet}

{#snippet inlineControl(ctrl: ScriptControlData, scriptIndex: number)}
    {#if ctrl.controlType === "label"}
        <span class="text-surface-600-400 select-none">{ctrl.caption ?? ""}</span>
    {:else if ctrl.controlType === "script" && ctrl.attribute !== null}
        <!-- Nested script block for commands like for/while/foreach/firsttime -->
        <div class="w-full mt-0.5">
            {#if ctrl.caption}
                <span class="text-surface-500-400 text-xs italic">{ctrl.caption}:</span>
            {/if}
            <ScriptEditor
                {elementKey}
                {attribute}
                containerPath={paramPath(scriptIndex, ctrl.attribute)}
                initialData={ctrl.scripts ?? []}
                depth={depth + 1}
            />
        </div>
    {:else if ctrl.attribute !== null}
        {@render valueControl(ctrl, scriptIndex)}
    {/if}
{/snippet}

{#snippet valueControl(ctrl: ScriptControlData, scriptIndex: number)}
    {#if ctrl.controlType === "expression" && ctrl.simpleEditor !== null}
        {@const simple = inSimpleMode(ctrl, scriptIndex)}
        {#if ctrl.simpleEditor === "boolean"}
            <!-- Boolean: yes / no / expression dropdown (no separate widget) -->
            <select
                class="select text-xs py-0 px-1 max-w-32"
                value={simple ? ctrl.value : "expression"}
                onchange={(e) => {
                    const v = (e.target as HTMLSelectElement).value;
                    if (v === "expression") {
                        onSwitchToExpression(scriptIndex, ctrl);
                    } else {
                        clearExpressionOverride(scriptIndex, ctrl.attribute!);
                        onSetParam(scriptIndex, ctrl.attribute!, v === "yes" ? "true" : "false");
                    }
                }}
            >
                <option value="true">yes</option>
                <option value="false">no</option>
                <option value="expression">expression</option>
            </select>
            {#if !simple}
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0 px-1 min-w-16 max-w-48 flex-1"
                    value={ctrl.value ?? ""}
                    onchange={(e) => onSetParam(scriptIndex, ctrl.attribute!, (e.target as HTMLInputElement).value)}
                />
            {/if}
        {:else}
            <!-- Textbox / dropdown / number: labelled mode toggle + widget -->
            <select
                class="select text-xs py-0 px-1 max-w-28"
                value={simple ? (ctrl.simpleLabel ?? "simple") : "expression"}
                onchange={(e) => {
                    const v = (e.target as HTMLSelectElement).value;
                    if (v === "expression") {
                        onSwitchToExpression(scriptIndex, ctrl);
                    } else {
                        onSwitchToSimple(scriptIndex, ctrl);
                    }
                }}
            >
                <option value={ctrl.simpleLabel ?? "simple"}>{ctrl.simpleLabel ?? "simple"}</option>
                <option value="expression">expression</option>
            </select>
            {#if simple}
                {#if ctrl.simpleEditor === "objects"}
                    <select
                        class="select text-xs py-0 px-1 max-w-40"
                        value={ctrl.value ?? ""}
                        onchange={(e) => onSimpleValueChange(scriptIndex, ctrl, (e.target as HTMLSelectElement).value)}
                    >
                        <option value=""></option>
                        {#each namesForControl(ctrl) as name (name)}
                            <option value={name}>{name}</option>
                        {/each}
                    </select>
                {:else if ctrl.simpleEditor === "dropdown" && ctrl.options}
                    <select
                        class="select text-xs py-0 px-1 max-w-32"
                        value={toSimpleDisplay(ctrl)}
                        onchange={(e) => onSimpleValueChange(scriptIndex, ctrl, (e.target as HTMLSelectElement).value)}
                    >
                        {#each ctrl.options as opt, oi (oi)}
                            <option value={opt.value}>{opt.label}</option>
                        {/each}
                    </select>
                {:else if ctrl.simpleEditor === "number" || ctrl.simpleEditor === "numberdouble"}
                    <input
                        type="number"
                        class="input text-xs py-0 px-1 min-w-12 max-w-24"
                        value={ctrl.value ?? "0"}
                        onchange={(e) => onSetParam(scriptIndex, ctrl.attribute!, (e.target as HTMLInputElement).value)}
                    />
                {:else if ctrl.simpleEditor === "file"}
                    <AssetPicker
                        value={toSimpleDisplay(ctrl)}
                        source={ctrl.source}
                        onchange={(v) => onSimpleValueChange(scriptIndex, ctrl, v)}
                        class="input text-xs py-0 px-1 min-w-16 max-w-48"
                    />
                {:else}
                    <!-- textbox (default for message, text, colour, etc.) -->
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input text-xs py-0 px-1 min-w-16 max-w-48 flex-1"
                        value={toSimpleDisplay(ctrl)}
                        onchange={(e) => onSimpleValueChange(scriptIndex, ctrl, (e.target as HTMLInputElement).value)}
                    />
                {/if}
            {:else}
                <!-- Expression mode: raw expression text input -->
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0 px-1 min-w-16 max-w-48 flex-1"
                    value={ctrl.value ?? ""}
                    onchange={(e) => onSetParam(scriptIndex, ctrl.attribute!, (e.target as HTMLInputElement).value)}
                />
            {/if}
        {/if}
    {:else if ctrl.controlType === "expression" || ctrl.controlType === "textbox" || ctrl.controlType === "richtext"}
        <input
            type="text"
            autocapitalize="off"
            class="input text-xs py-0 px-1 min-w-16 max-w-48 flex-1"
            value={ctrl.value ?? ""}
            onchange={(e) => onSetParam(scriptIndex, ctrl.attribute!, (e.target as HTMLInputElement).value)}
        />
    {:else if ctrl.controlType === "checkbox"}
        <input
            type="checkbox"
            class="checkbox"
            checked={ctrl.value === "True" || ctrl.value === "true"}
            onchange={(e) => onSetParam(scriptIndex, ctrl.attribute!, (e.target as HTMLInputElement).checked.toString())}
        />
        {#if ctrl.caption}<span class="text-surface-600-400">{ctrl.caption}</span>{/if}
    {:else if ctrl.controlType === "dropdown" && ctrl.options}
        <select
            class="select text-xs py-0 px-1 max-w-32"
            value={ctrl.value ?? ""}
            onchange={(e) => onSetParam(scriptIndex, ctrl.attribute!, (e.target as HTMLSelectElement).value)}
        >
            {#each ctrl.options as opt, oi (oi)}
                <option value={opt.value}>{opt.label}</option>
            {/each}
        </select>
    {:else}
        <span class="text-surface-400-500 italic text-xs">[{ctrl.controlType}]</span>
    {/if}
{/snippet}

{#snippet ifBlock(script: ScriptNodeData, i: number)}
    {@const tmplData = getIfExpressionTemplateData(script.expression ?? "")}
    {@const exprKey = `if/${i}`}
    {@const inTemplateMode = tmplData !== null && !expressionOverrides.has(exprKey)}
    <div class="px-2 py-1 pr-16 text-xs">
        <!-- If condition -->
        <div class="flex items-center gap-1 flex-wrap">
            <span class="text-surface-600-400 font-medium select-none">if</span>
            {#if ifTemplates.length > 0}
                <!-- Template / expression mode toggle -->
                <select
                    class="select text-xs py-0 px-1 max-w-40"
                    value={inTemplateMode ? tmplData!.templateName : "expression"}
                    onchange={(e) => {
                        const v = (e.target as HTMLSelectElement).value;
                        if (v === "expression") {
                            expressionOverrides.add(exprKey);
                        } else {
                            expressionOverrides.delete(exprKey);
                            const tpl = ifTemplates.find(t => t.name === v);
                            if (tpl) onSetIfExpr(i, tpl.createExpression);
                        }
                    }}
                >
                    <option value="expression">expression</option>
                    {#each ifTemplates as tpl (tpl.name)}
                        <option value={tpl.name}>{tpl.name}</option>
                    {/each}
                </select>
            {/if}
            {#if inTemplateMode && tmplData}
                <!-- Template controls (e.g. object picker for Got(#object#)) -->
                {#each tmplData.controls as ctrl (ctrl.name)}
                    {#if ctrl.simpleEditor === "objects"}
                        <select
                            class="select text-xs py-0 px-1 max-w-40"
                            value={ctrl.value ?? ""}
                            onchange={(e) => {
                                const newVal = (e.target as HTMLSelectElement).value;
                                onSetIfExpr(i, buildTemplateExpression(tmplData!, ctrl.name, newVal));
                            }}
                        >
                            <option value=""></option>
                            {#each objectNames as name (name)}
                                <option value={name}>{name}</option>
                            {/each}
                        </select>
                    {:else}
                        <input
                            type="text"
                            autocapitalize="off"
                            class="input text-xs py-0 px-1 min-w-16 max-w-32 flex-1"
                            placeholder={ctrl.simpleLabel ?? ctrl.name}
                            value={ctrl.value ?? ""}
                            onchange={(e) => {
                                const newVal = (e.target as HTMLInputElement).value;
                                onSetIfExpr(i, buildTemplateExpression(tmplData!, ctrl.name, newVal));
                            }}
                        />
                    {/if}
                {/each}
            {:else}
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0 px-1 min-w-24 max-w-64 flex-1"
                    value={script.expression ?? ""}
                    onchange={(e) => onSetIfExpr(i, (e.target as HTMLInputElement).value)}
                />
            {/if}
            <span class="text-surface-600-400 select-none">then</span>
        </div>

        <!-- Then block -->
        <div class="mt-1">
            <ScriptEditor
                {elementKey}
                {attribute}
                containerPath={thenPath(i)}
                initialData={script.thenScripts ?? []}
                depth={depth + 1}
            />
        </div>

        <!-- Else-if blocks -->
        {#each script.elseIfClauses ?? [] as elseIf, ei (elseIf.id)}
            {@const eiTmplData = getIfExpressionTemplateData(elseIf.expression ?? "")}
            {@const eiExprKey = `elseif/${i}/${ei}`}
            {@const eiInTemplateMode = eiTmplData !== null && !expressionOverrides.has(eiExprKey)}
            <div class="mt-1 flex items-center gap-1 flex-wrap">
                <span class="text-surface-600-400 font-medium select-none">else if</span>
                {#if ifTemplates.length > 0}
                    <select
                        class="select text-xs py-0 px-1 max-w-40"
                        value={eiInTemplateMode ? eiTmplData!.templateName : "expression"}
                        onchange={(e) => {
                            const v = (e.target as HTMLSelectElement).value;
                            if (v === "expression") {
                                expressionOverrides.add(eiExprKey);
                            } else {
                                expressionOverrides.delete(eiExprKey);
                                const tpl = ifTemplates.find(t => t.name === v);
                                if (tpl) onSetElseIfExpr(i, ei, tpl.createExpression);
                            }
                        }}
                    >
                        <option value="expression">expression</option>
                        {#each ifTemplates as tpl (tpl.name)}
                            <option value={tpl.name}>{tpl.name}</option>
                        {/each}
                    </select>
                {/if}
                {#if eiInTemplateMode && eiTmplData}
                    {#each eiTmplData.controls as ctrl (ctrl.name)}
                        {#if ctrl.simpleEditor === "objects"}
                            <select
                                class="select text-xs py-0 px-1 max-w-40"
                                value={ctrl.value ?? ""}
                                onchange={(e) => {
                                    const newVal = (e.target as HTMLSelectElement).value;
                                    onSetElseIfExpr(i, ei, buildTemplateExpression(eiTmplData!, ctrl.name, newVal));
                                }}
                            >
                                <option value=""></option>
                                {#each objectNames as name (name)}
                                    <option value={name}>{name}</option>
                                {/each}
                            </select>
                        {:else}
                            <input
                                type="text"
                                autocapitalize="off"
                                class="input text-xs py-0 px-1 min-w-16 max-w-32 flex-1"
                                placeholder={ctrl.simpleLabel ?? ctrl.name}
                                value={ctrl.value ?? ""}
                                onchange={(e) => {
                                    const newVal = (e.target as HTMLInputElement).value;
                                    onSetElseIfExpr(i, ei, buildTemplateExpression(eiTmplData!, ctrl.name, newVal));
                                }}
                            />
                        {/if}
                    {/each}
                {:else}
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input text-xs py-0 px-1 min-w-24 max-w-64 flex-1"
                        value={elseIf.expression}
                        onchange={(e) => onSetElseIfExpr(i, ei, (e.target as HTMLInputElement).value)}
                    />
                {/if}
                <span class="text-surface-600-400 select-none">then</span>
                <button
                    type="button"
                    class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none ml-auto"
                    title="Remove else if"
                    onclick={() => onRemoveElseIf(i, ei)}
                >×</button>
            </div>
            <div class="mt-1">
                <ScriptEditor
                    {elementKey}
                    {attribute}
                    containerPath={elseIfPath(i, ei)}
                    initialData={elseIf.scripts}
                    depth={depth + 1}
                />
            </div>
        {/each}

        <!-- Else block -->
        {#if script.elseScripts !== null && script.elseScripts !== undefined}
            <div class="mt-1 flex items-center gap-1">
                <span class="text-surface-600-400 font-medium select-none">else</span>
                <button
                    type="button"
                    class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none ml-auto"
                    title="Remove else"
                    onclick={() => onRemoveElse(i)}
                >×</button>
            </div>
            <div class="mt-1">
                <ScriptEditor
                    {elementKey}
                    {attribute}
                    containerPath={elsePath(i)}
                    initialData={script.elseScripts}
                    depth={depth + 1}
                />
            </div>
        {/if}

        <!-- Add else if / Add else buttons -->
        <div class="flex gap-1 mt-1">
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                onclick={() => onAddElseIf(i)}
            >+ else if</button>
            {#if script.elseScripts === null || script.elseScripts === undefined}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={() => onAddElse(i)}
                >+ else</button>
            {/if}
        </div>
    </div>
{/snippet}
