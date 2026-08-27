<script lang="ts">
    import { SvelteSet } from "svelte/reactivity";
    import ScriptEditor from "./ScriptEditor.svelte";
    import AddScriptModal from "./AddScriptModal.svelte";
    import AssetPicker from "./AssetPicker.svelte";
    import CodeEditor from "./CodeEditor.svelte";
    import Combobox from "./Combobox.svelte";
    import ExpressionInput from "./ExpressionInput.svelte";
    import { t } from "$lib/i18n";
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
        getPageNames,
        getExpressionTemplates,
        getExpressionTemplateData,
        makeScriptEditable,
        getExpressionFunctions,
        addScriptListItem,
        removeScriptListItem,
        updateScriptListItem,
        setScriptListItemCount,
        addScriptDictCase,
        removeScriptDictCase,
        renameScriptDictCase,
    } from "$lib/editor-store";
    import { defaultCodeView } from "$lib/code-view-store";
    import { measureTextPx } from "$lib/text-measure";
    import type {
        ScriptBlockData,
        ScriptNodeData,
        ScriptControlData,
        ScriptCategoryInfo,
        ExpressionTemplateData,
        ExpressionTemplateControlData,
        ExpressionTemplate,
        ExpressionFunctionInfo,
        CaseScriptData,
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
    // Same idea as expressionOverrides, but for a single parameter *inside* an expression
    // template (e.g. RandomChance's "percentile") rather than a whole script command param -
    // keyed by `${overrideKey}::${ctrl.name}`, see expressionField/templateParamKey.
    const templateParamOverrides = new SvelteSet<string>();
    // Tracks which switch cases (scriptdictionary control entries) are expanded, keyed by
    // `${scriptIndex}:${paramAttribute}:${caseKey}` - see switchCasesEditor.
    const expandedCases = new SvelteSet<string>();
    // Staged "add case" key text, keyed by `${scriptIndex}:${paramAttribute}` - one row of
    // scriptdictionary controls can only appear once per script node, so this is unambiguous.
    let newCaseKeys = $state<Record<string, string>>({});
    let objectNames = $state<string[]>([]);
    let exitNames = $state<string[]>([]);
    let pageNames = $state<string[]>([]);
    // Functions callable from a Call function script (WorldModel.Procedure() only resolves aslx
    // Function elements, not native built-ins) — kept as full ExpressionFunctionInfo, not just
    // names, so the picker can group game-authored functions ahead of library ones and the
    // parameter-list editor can look up a selected function's declared arity/param names.
    let functionInfos = $state<ExpressionFunctionInfo[]>([]);
    // Expression templates ("insert a common expression" pickers), keyed by expressionType.
    // "if" is synthesized by IfExpressionControlDefinition for if/else-if conditions; "set" and
    // "foreach" come from <usetemplates> on ordinary "expression" controls (e.g. "Set variable"'s
    // value, "for each"'s list) - see CoreEditorScriptsVariables.aslx / CoreEditorScriptsScripts.aslx.
    const EXPRESSION_TEMPLATE_TYPES = ["if", "set", "foreach"];
    let templatesByType = $state<Record<string, ExpressionTemplate[]>>({});

    function templatesFor(expressionType: string | null | undefined): ExpressionTemplate[] {
        return (expressionType && templatesByType[expressionType]) || [];
    }

    // Only root instances show the code-view toggle (nested if/for/case
    // sub-editors share the root's `attribute` script, so their own
    // codeViewMode is never read — see the isRoot guards below and on the
    // scriptCode effect). The setting is a standing preference, not a
    // per-panel choice: changing it flips every root ScriptEditor currently
    // on screen, overriding whatever any of them was individually showing.
    $effect(() => {
        if (isRoot) codeViewMode = $defaultCodeView;
    });

    // Load on mount and whenever scriptVersion bumps (undo/redo)
    $effect(() => {
        const version = $scriptVersion; // track for reactivity on undo/redo
        if (isRoot) {
            scriptData = version >= 0 ? getScriptData(elementKey, attribute) : null;
        }
        // Any cut/delete/undo just removed the currently-selected rows, so a selection can't
        // survive a scriptVersion bump — except a move, which pre-sets nextSelection (see
        // onMoveUp/DownSelected) to shift it instead. Nested blocks (if/for/while ... then,
        // else, etc.) have their own checkboxes and selection toolbar too, so this applies to
        // every level, not just the root.
        selectedIndices.clear();
        for (const i of nextSelection ?? []) selectedIndices.add(i);
        nextSelection = null;
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
        if (pageNames.length === 0) {
            const names = getPageNames();
            if (names && names.length > 0) pageNames = names;
        }
        if (functionInfos.length === 0) {
            const infos = getExpressionFunctions().filter(f => f.isUserDefined);
            if (infos.length > 0) functionInfos = infos;
        }
        for (const type of EXPRESSION_TEMPLATE_TYPES) {
            if (!(type in templatesByType)) {
                const templates = getExpressionTemplates(type);
                if (templates) templatesByType[type] = templates;
            }
        }
    });

    // Code view has its own separately-fetched buffer (scriptCode), so it needs its own
    // reactive refresh when the selected element/attribute changes (e.g. picking a different
    // command in the tree while code view is already open) - the scriptData effect above only
    // covers the visual editor's data.
    $effect(() => {
        if (isRoot && codeViewMode) {
            scriptCode = getScriptCode(elementKey, attribute);
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
        templateParamOverrides.clear();
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
        if (ctrl.objectType === "exit") return exitNames;
        if (ctrl.objectType === "page") return pageNames;
        return objectNames;
    }

    // Loosened to a minimal structural shape (rather than ScriptControlData specifically) so
    // this same simple/expression-shape check works for both an ordinary script command param
    // and a control nested inside an expression template (e.g. RandomChance's "percentile") -
    // see inTemplateParamSimpleMode below, which is the only other caller of this shape.
    type SimpleValueLike = { simpleEditor: string | null; value: string | null };

    function isSimpleValue(ctrl: SimpleValueLike): boolean {
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

    // Same intent as onSwitchToSimple's inline switch below, but as a pure function (a template
    // control's toggle has no scriptIndex/onSetParam to hang that logic off) and with "objects"
    // broken out from the default case: an object reference resets to a bare "" (nothing
    // selected), not the quoted empty string a textbox/dropdown simple editor wants.
    function defaultSimpleValue(simpleEditor: string): string {
        switch (simpleEditor) {
            case "number":
            case "numberdouble":
                return "0";
            case "objects":
                return "";
            default:
                return '""';
        }
    }

    function templateParamKey(overrideKey: string, ctrlName: string): string {
        return `${overrideKey}::${ctrlName}`;
    }

    function inTemplateParamSimpleMode(overrideKey: string, ctrl: ExpressionTemplateControlData): boolean {
        if (!ctrl.simpleEditor) return false;
        if (templateParamOverrides.has(templateParamKey(overrideKey, ctrl.name))) return false;
        return isSimpleValue(ctrl);
    }

    // Svelte action: sizes a <select> (the condition/template picker, or a template control's
    // object picker) to fit its own currently selected text rather than a fixed Tailwind max-w
    // class - a plain width class either truncates a long value or, since a native <select>
    // without an explicit width sizes to its widest *option* rather than the selected one, stays
    // wide even once a short option is picked (e.g. Got(#object#) offering an object list that
    // includes some long name).
    //
    // A canvas-measured text width plus a guessed flat reserve for the select's own native
    // dropdown-arrow chrome was tried here first and still clipped the last character or so in
    // practice - that chrome's actual footprint is drawn by the OS/browser itself, not something
    // any fixed constant can promise to match everywhere. Instead, this asks the browser
    // directly: an offscreen probe <select> - same classes, single option matching the real
    // text, width left to size itself naturally - lays out exactly as wide as this browser wants
    // to render that text with its own arrow, on this exact system. `minCh`/`maxCh` stay
    // expressed in roughly character terms for callers, converted using this element's own
    // "0"-glyph width (measureTextPx), since only they still need an actual estimate.
    function autoWidthSelect(node: HTMLSelectElement, opts: { text: string; minCh: number; maxCh: number }) {
        const probe = document.createElement("select");
        probe.className = node.className;
        probe.style.width = "auto";
        probe.style.position = "fixed";
        probe.style.visibility = "hidden";
        probe.style.left = "-9999px";
        probe.setAttribute("aria-hidden", "true");
        probe.tabIndex = -1;
        document.body.appendChild(probe);

        function apply(o: typeof opts) {
            probe.innerHTML = "";
            const opt = document.createElement("option");
            opt.textContent = o.text || " ";
            probe.appendChild(opt);
            const naturalWidth = probe.getBoundingClientRect().width;
            const chPx = measureTextPx("0", getComputedStyle(node).font) || 8;
            const minPx = o.minCh * chPx;
            const maxPx = o.maxCh * chPx;
            node.style.width = `${Math.max(minPx, Math.min(maxPx, naturalWidth))}px`;
        }
        apply(opts);
        return {
            update: apply,
            destroy() {
                probe.remove();
            },
        };
    }

    // Grows a multiline textbox to exactly fit its content (one line when empty/single-line,
    // taller as more lines are typed) instead of a fixed row count that leaves blank space.
    function autoResizeTextarea(node: HTMLTextAreaElement, value: string) {
        void value; // only needed so Svelte calls `update` below when it changes externally
        function resize() {
            node.style.height = "auto";
            node.style.height = `${node.scrollHeight}px`;
        }
        resize();
        node.addEventListener("input", resize);
        return {
            update(newValue: string) {
                // Skip when this fires for the field's own keystrokes (already resized live by
                // the "input" listener) and only re-measure for genuinely external changes.
                if (newValue !== node.value) {
                    resize();
                }
            },
            destroy() {
                node.removeEventListener("input", resize);
            },
        };
    }

    function toSimpleDisplay(ctrl: SimpleValueLike): string {
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

    function buildTemplateExpression(tmpl: ExpressionTemplateData, changedName: string, changedValue: string): string {
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

    // Game-authored functions first (most likely to be what the author wants to call), then
    // library/built-in ones — mirrors ExpressionInput's Functions grouping.
    function functionPickerOptions(): {value: string; label: string; group: string}[] {
        const game = functionInfos.filter(f => !f.isLibrary);
        const library = functionInfos.filter(f => f.isLibrary);
        return [
            ...game.map(f => ({ value: f.name, label: f.name, group: t("expressionInput.gameFunctionsHeading") })),
            ...library.map(f => ({ value: f.name, label: f.name, group: t("scriptEditor.libraryFunctionsGroup") })),
        ];
    }

    // Sets the Call function picker's function-name attribute, then — if the chosen name
    // matches a known function — resizes the sibling parameter list to that function's declared
    // arity. Functions have a fixed parameter count (FunctionCallScript matches positionally
    // against ParamNames, and modern game versions require an exact count), so this is safe:
    // there's no "variable number of parameters" case to preserve a manual +/- for here, beyond
    // names the picker doesn't recognise at all (kept on the manual list UI in paramListEditor).
    function onSelectFunction(scriptIndex: number, paramName: string, siblingControls: ScriptControlData[], value: string) {
        const paramsCtrl = siblingControls.find(c => c.isFunctionParams);
        const fn = functionInfos.find(f => f.name === value);
        mutate(() => {
            const result = setScriptParameter(elementKey, attribute, containerPath, scriptIndex, paramName, value);
            if (result !== "ok" || !fn || !paramsCtrl?.attribute) return result;
            return setScriptListItemCount(elementKey, attribute, containerPath, scriptIndex, paramsCtrl.attribute, fn.parameters.length);
        });
    }

    // The function a functionparams list control is currently showing labelled boxes for, or
    // undefined if it's on the manual +/- fallback (name doesn't match a known function, or the
    // list hasn't been resized to match yet — see onSelectFunction). Shared by paramListEditor
    // (which boxes to render) and inlineControl's "With parameters:" label (hidden once the
    // boxes are self-labelled, since the label adds nothing at that point).
    function matchedFunctionFor(ctrl: ScriptControlData, siblingControls: ScriptControlData[]): ExpressionFunctionInfo | undefined {
        if (!ctrl.isFunctionParams) return undefined;
        const pickerValue = siblingControls.find(c => c.isFunctionPicker)?.value;
        const fn = functionInfos.find(f => f.name === pickerValue);
        if (!fn) return undefined;
        const items = parseListItems(ctrl.value);
        return fn.parameters.length === items.length ? fn : undefined;
    }

    function onAddParam(scriptIndex: number, paramAttribute: string, value: string) {
        mutate(() => addScriptListItem(elementKey, attribute, containerPath, scriptIndex, paramAttribute, value));
    }

    function onRemoveParam(scriptIndex: number, paramAttribute: string, key: string) {
        mutate(() => removeScriptListItem(elementKey, attribute, containerPath, scriptIndex, paramAttribute, key));
    }

    function onUpdateParam(scriptIndex: number, paramAttribute: string, key: string, value: string) {
        mutate(() => updateScriptListItem(elementKey, attribute, containerPath, scriptIndex, paramAttribute, key, value));
    }

    function parseListItems(value: string | null): {key: string, value: string}[] {
        try { return JSON.parse(value ?? "[]"); } catch { return []; }
    }

    function caseGroupKey(scriptIndex: number, paramAttribute: string): string {
        return `${scriptIndex}:${paramAttribute}`;
    }

    function onAddCase(scriptIndex: number, paramAttribute: string) {
        const groupKey = caseGroupKey(scriptIndex, paramAttribute);
        const key = (newCaseKeys[groupKey] ?? "").trim();
        if (!key) return;
        const added = mutate(() => addScriptDictCase(elementKey, attribute, containerPath, scriptIndex, paramAttribute, key));
        if (added) {
            expandedCases.add(`${groupKey}:${key}`);
            newCaseKeys[groupKey] = "";
        }
    }

    function onRemoveCase(scriptIndex: number, paramAttribute: string, key: string) {
        mutate(() => removeScriptDictCase(elementKey, attribute, containerPath, scriptIndex, paramAttribute, key));
    }

    function onRenameCase(scriptIndex: number, paramAttribute: string, oldKey: string, newKey: string) {
        const trimmed = newKey.trim();
        if (!trimmed || trimmed === oldKey) return;
        const groupKey = caseGroupKey(scriptIndex, paramAttribute);
        const renamed = mutate(() => renameScriptDictCase(elementKey, attribute, containerPath, scriptIndex, paramAttribute, oldKey, trimmed));
        if (renamed && expandedCases.has(`${groupKey}:${oldKey}`)) {
            expandedCases.delete(`${groupKey}:${oldKey}`);
            expandedCases.add(`${groupKey}:${trimmed}`);
        }
    }

    function toggleCaseExpanded(scriptIndex: number, paramAttribute: string, key: string) {
        const fullKey = `${caseGroupKey(scriptIndex, paramAttribute)}:${key}`;
        if (expandedCases.has(fullKey)) expandedCases.delete(fullKey); else expandedCases.add(fullKey);
    }

    function scriptsForCase(ctrl: ScriptControlData, key: string): ScriptNodeData[] {
        return ctrl.cases?.find((c: CaseScriptData) => c.key === key)?.scripts ?? [];
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
    function casePath(scriptIndex: number, paramAttr: string, key: string): string {
        const base = containerPath ? `${containerPath}/${scriptIndex}` : `${scriptIndex}`;
        // The key is percent-encoded since it's an arbitrary case-match expression that may
        // contain "/" - see ResolveContainer's "case" segment on the WasmEditorBridge side.
        return `${base}/case/${paramAttr}/${encodeURIComponent(key)}`;
    }

    const indentClass = $derived(depth > 0 ? "ml-4 border-l border-surface-300-700 pl-2" : "");
</script>

<div class={indentClass}>
    {#if isRoot && isLocked}
        <div class="flex items-center gap-2 py-1 px-2 mb-1 text-xs text-surface-600-400 italic border border-surface-200-800 rounded">
            <span class="flex-1">{t("scriptEditor.inheritedReadOnly")}</span>
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0 not-italic"
                onclick={onToggleCodeView}
            >{codeViewMode ? t("scriptEditor.visualView") : t("scriptEditor.codeView")}</button>
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0 not-italic"
                onclick={() => makeScriptEditable(elementKey, attribute)}
            >{t("common.makeEditableCopy")}</button>
        </div>
    {/if}
    {#if codeViewMode}
        <CodeEditor
            value={scriptCode}
            language="quest-script"
            readonly={isLocked}
            onChange={onCodeViewSave}
            minHeight="auto"
        />
    {:else}
        <div role="region" inert={isLocked || undefined} class={isLocked ? "opacity-60" : ""}>
            {#each scripts() as script, i (script.id)}
                <div class="group relative border border-surface-200-800 rounded mb-1 bg-surface-50-950 flex items-start">
                    <label class="flex items-start pt-1.5 pl-1.5 pr-0.5 cursor-pointer flex-shrink-0">
                        <input
                            type="checkbox"
                            class="checkbox size-3.5"
                            checked={selectedIndices.has(i)}
                            onchange={(e) => toggleSelection(i, (e.target as HTMLInputElement).checked)}
                        />
                    </label>
                    <div class="flex-1 min-w-0">
                        <!-- Script row actions (hover). Every row also has a checkbox + selection
                             toolbar below (Cut/Copy/Delete/Move) that already covers this on
                             touch, so these stay hover-only at all nesting levels. -->
                        <div class="absolute right-1 top-1 flex gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity z-10">
                            <button
                                type="button"
                                class="btn btn-sm preset-outlined-primary-500 px-1 py-0 text-xs leading-none"
                                title={t("common.moveUp")}
                                disabled={i === 0}
                                onclick={() => onMoveUp(i)}
                            >↑</button>
                            <button
                                type="button"
                                class="btn btn-sm preset-outlined-primary-500 px-1 py-0 text-xs leading-none"
                                title={t("common.moveDown")}
                                disabled={i === scripts().length - 1}
                                onclick={() => onMoveDown(i)}
                            >↓</button>
                            <button
                                type="button"
                                class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none"
                                title={t("common.delete")}
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

            <!-- Selection toolbar: one horizontally-scrollable row (rather than
                 the buttons getting clipped, or wrapping onto more lines)
                 when it doesn't fit a narrow screen. -->
            {#if selectedIndices.size > 0}
                {@const sel = sortedSelection()}
                <div class="flex items-center gap-1 mb-1 px-1 py-1 bg-surface-100-900/60 rounded border border-surface-200-800 text-xs overflow-x-auto">
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5 flex-shrink-0"
                        onclick={onCutSelected}
                    >{t("common.cut")}</button>
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5 flex-shrink-0"
                        onclick={onCopySelected}
                    >{t("common.copy")}</button>
                    <button
                        type="button"
                        class="btn btn-sm preset-tonal-error text-xs py-0.5 flex-shrink-0"
                        onclick={onDeleteSelected}
                    >{t("common.delete")}</button>
                    {#if sel.length === 1}
                        <span class="w-px h-4 bg-surface-300-700 mx-0.5 flex-shrink-0"></span>
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5 flex-shrink-0"
                            disabled={sel[0] === 0}
                            onclick={onMoveUpSelected}
                        >↑ {t("common.moveUp")}</button>
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5 flex-shrink-0"
                            disabled={sel[0] === scripts().length - 1}
                            onclick={onMoveDownSelected}
                        >↓ {t("common.moveDown")}</button>
                    {/if}
                    <span class="ml-auto pl-2 flex-shrink-0 text-surface-600-400">{t("scriptEditor.selectedCount", { count: sel.length })}</span>
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
                >+ {t("scriptEditor.addScript")}</button>
            {:else if !codeViewMode && isRoot}
                <span class="text-xs text-surface-600-400 italic">{t("scriptEditor.loadingCommands")}</span>
            {/if}
            {#if $scriptClipboardHasContent && !codeViewMode}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={onPaste}
                >{t("common.paste")}</button>
            {/if}
            {#if isRoot}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={onToggleCodeView}
                >{codeViewMode ? t("scriptEditor.visualEditor") : t("scriptEditor.codeView")}</button>
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
    <div class="px-2 py-1 pr-16 pointer-coarse:pr-2 flex flex-wrap items-center gap-x-1 gap-y-0.5 text-xs">
        {#each script.controls ?? [] as ctrl, ci (ci)}
            {@render inlineControl(ctrl, i, script.controls ?? [], ci)}
        {/each}
    </div>
{/snippet}

{#snippet inlineControl(ctrl: ScriptControlData, scriptIndex: number, siblingControls: ScriptControlData[], controlIndex: number)}
    {#if ctrl.breakBefore}
        <!-- <breakbefore/> - a full-width, zero-height flex item forces everything after it
             onto a new row within the controls' flex-wrap container, without needing to
             restructure the controls around it. -->
        <span class="basis-full h-0"></span>
    {/if}
    {#if ctrl.controlType === "label"}
        {@const nextCtrl = siblingControls[controlIndex + 1]}
        {@const labelIsRedundant = nextCtrl?.isFunctionParams && matchedFunctionFor(nextCtrl, siblingControls) !== undefined}
        {#if !labelIsRedundant}
            <span class="text-surface-600-400 select-none">{ctrl.caption ?? ""}</span>
        {/if}
    {:else if ctrl.controlType === "script" && ctrl.attribute !== null}
        <!-- Nested script block for commands like for/while/foreach/firsttime -->
        <div class="w-full mt-0.5">
            {#if ctrl.caption}
                <span class="text-surface-600-400 text-xs italic">{ctrl.caption}:</span>
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
        {@render valueControl(ctrl, scriptIndex, siblingControls)}
    {/if}
{/snippet}

{#snippet valueControl(ctrl: ScriptControlData, scriptIndex: number, siblingControls: ScriptControlData[])}
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
                <option value="true">{t("scriptEditor.yesOption")}</option>
                <option value="false">{t("scriptEditor.noOption")}</option>
                <option value="expression">{t("scriptEditor.expressionOption")}</option>
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
                <option value={ctrl.simpleLabel ?? t("scriptEditor.simpleOption")}>{ctrl.simpleLabel ?? t("scriptEditor.simpleOption")}</option>
                <option value="expression">{t("scriptEditor.expressionOption")}</option>
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
                {:else if ctrl.simpleEditor === "dropdown" && ctrl.options && ctrl.freetext}
                    <!-- <freetext/> - lets the user type a value not in the list, e.g. a
                         drawing command's colour parameter. -->
                    <Combobox
                        value={toSimpleDisplay(ctrl)}
                        options={ctrl.options}
                        onchange={(v) => onSimpleValueChange(scriptIndex, ctrl, v)}
                        class="input text-xs py-0 px-1 max-w-32"
                    />
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
                        min={ctrl.minimum ?? undefined}
                        max={ctrl.maximum ?? undefined}
                        step={ctrl.increment ?? (ctrl.simpleEditor === "numberdouble" ? "any" : undefined)}
                        class="input text-xs py-0 px-1 min-w-12 max-w-24"
                        value={ctrl.value ?? "0"}
                        onchange={(e) => onSetParam(scriptIndex, ctrl.attribute!, (e.target as HTMLInputElement).value)}
                    />
                {:else if ctrl.simpleEditor === "file"}
                    <AssetPicker
                        value={toSimpleDisplay(ctrl)}
                        source={ctrl.source}
                        filterName={ctrl.fileFilterName}
                        onchange={(v) => onSimpleValueChange(scriptIndex, ctrl, v)}
                        class="input text-xs py-0 px-1 min-w-16 max-w-48"
                    />
                {:else if ctrl.multiline}
                    <!-- multiline textbox (e.g. Print's message) - keeps embedded newlines,
                         auto-sized to the height of its content rather than a fixed row count -->
                    <textarea
                        autocapitalize="off"
                        rows="1"
                        class="input text-xs py-0.5 px-1 min-w-16 w-full resize-none overflow-hidden"
                        value={toSimpleDisplay(ctrl)}
                        use:autoResizeTextarea={toSimpleDisplay(ctrl)}
                        onchange={(e) => onSimpleValueChange(scriptIndex, ctrl, (e.target as HTMLTextAreaElement).value)}
                    ></textarea>
                {:else}
                    <!-- textbox (default for message, text, colour, etc.) -->
                    <input
                        type="text"
                        autocapitalize="off"
                        class={"input text-xs py-0 px-1 min-w-16 flex-1" + (ctrl.expand ? " w-full" : " max-w-48")}
                        value={toSimpleDisplay(ctrl)}
                        onchange={(e) => onSimpleValueChange(scriptIndex, ctrl, (e.target as HTMLInputElement).value)}
                    />
                {/if}
            {:else}
                <!-- Expression mode: raw expression text input. ExpressionInput's own root
                     element isn't a flex item that grows on its own, so it needs a growing
                     wrapper here to actually fill the row when expand is set. -->
                <div class={ctrl.expand ? "flex-1 min-w-0" : "flex-none"}>
                    <ExpressionInput
                        value={ctrl.value ?? ""}
                        onchange={(v) => onSetParam(scriptIndex, ctrl.attribute!, v)}
                        {objectNames}
                        class={"input text-xs py-0 px-1 min-w-16" + (ctrl.expand ? " w-full" : " max-w-48")}
                    />
                </div>
            {/if}
        {/if}
    {:else if ctrl.controlType === "expression"}
        {@render expressionField(
            ctrl.value ?? "",
            (v) => onSetParam(scriptIndex, ctrl.attribute!, v),
            ctrl.useTemplates ?? null,
            exprKey(scriptIndex, ctrl.attribute!),
            "input text-xs py-0 px-1 min-w-16 max-w-48 flex-1",
        )}
    {:else if ctrl.controlType === "textbox" && ctrl.isFunctionPicker}
        {@const functionOptions = functionPickerOptions()}
        <Combobox
            value={ctrl.value ?? ""}
            options={functionOptions}
            onchange={(v) => onSelectFunction(scriptIndex, ctrl.attribute!, siblingControls, v)}
            class="input text-xs py-0 px-1"
            wrapperClass="min-w-32 max-w-96 flex-1"
        />
    {:else if ctrl.controlType === "textbox" || ctrl.controlType === "richtext"}
        <input
            type="text"
            autocapitalize="off"
            class="input text-xs py-0 px-1 min-w-16 max-w-48 flex-1"
            value={ctrl.value ?? ""}
            onchange={(e) => onSetParam(scriptIndex, ctrl.attribute!, (e.target as HTMLInputElement).value)}
        />
    {:else if ctrl.controlType === "list"}
        {@render paramListEditor(ctrl, scriptIndex, siblingControls)}
    {:else if ctrl.controlType === "scriptdictionary"}
        {@render switchCasesEditor(ctrl, scriptIndex)}
    {:else if ctrl.controlType === "checkbox"}
        <input
            type="checkbox"
            class="checkbox"
            checked={ctrl.value === "True" || ctrl.value === "true"}
            onchange={(e) => onSetParam(scriptIndex, ctrl.attribute!, (e.target as HTMLInputElement).checked.toString())}
        />
        {#if ctrl.caption}<span class="text-surface-600-400">{ctrl.caption}</span>{/if}
    {:else if ctrl.controlType === "dropdown" && ctrl.options && ctrl.freetext}
        <Combobox
            value={ctrl.value ?? ""}
            options={ctrl.options}
            onchange={(v) => onSetParam(scriptIndex, ctrl.attribute!, v)}
            class="input text-xs py-0 px-1 max-w-32"
        />
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
        <span class="text-surface-600-400 italic text-xs">[{ctrl.controlType}]</span>
    {/if}
{/snippet}

{#snippet paramListEditor(ctrl: ScriptControlData, scriptIndex: number, siblingControls: ScriptControlData[])}
    {@const items = parseListItems(ctrl.value)}
    {@const matchedFn = matchedFunctionFor(ctrl, siblingControls)}
    {#if matchedFn}
        <!-- Known, fixed-arity function (see onSelectFunction): one labelled box per declared
             parameter, kept in sync with the signature, instead of the generic +/- list. -->
        <span class="flex flex-wrap items-center gap-2">
            {#each matchedFn.parameters as paramName, pi (pi)}
                <span class="flex items-center gap-1">
                    <span class="text-surface-600-400 text-[10px] whitespace-nowrap">{paramName}:</span>
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input text-xs py-0 px-1 w-20"
                        value={items[pi].value}
                        onchange={(e) => onUpdateParam(scriptIndex, ctrl.attribute!, items[pi].key, (e.target as HTMLInputElement).value)}
                    />
                </span>
            {/each}
        </span>
    {:else}
        <span class="flex flex-wrap items-center gap-1">
            {#each items as item (item.key)}
                <span class="flex items-center gap-0.5">
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input text-xs py-0 px-1 w-20"
                        value={item.value}
                        onchange={(e) => onUpdateParam(scriptIndex, ctrl.attribute!, item.key, (e.target as HTMLInputElement).value)}
                    />
                    <button
                        type="button"
                        class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none"
                        title={t("scriptEditor.removeParameter")}
                        onclick={() => onRemoveParam(scriptIndex, ctrl.attribute!, item.key)}
                    >×</button>
                </span>
            {/each}
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs py-0 px-1.5 leading-none"
                onclick={() => onAddParam(scriptIndex, ctrl.attribute!, "")}
            >+ {t("scriptEditor.addParam")}</button>
        </span>
    {/if}
{/snippet}

{#snippet switchCasesEditor(ctrl: ScriptControlData, scriptIndex: number)}
    {@const paramAttribute = ctrl.attribute!}
    {@const groupKey = caseGroupKey(scriptIndex, paramAttribute)}
    {@const items = parseListItems(ctrl.value)}
    <div class="w-full flex flex-col gap-1 mt-0.5">
        {#each items as item (item.key)}
            {@const expanded = expandedCases.has(`${groupKey}:${item.key}`)}
            <div class="border border-surface-200-800 rounded">
                <div class="flex items-center gap-1 px-2 py-1">
                    <button
                        type="button"
                        class="text-surface-600-400 hover:text-primary-600-400 text-xs"
                        onclick={() => toggleCaseExpanded(scriptIndex, paramAttribute, item.key)}
                    >{expanded ? "▼" : "▶"}</button>
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input text-xs py-0 px-1 min-w-16 max-w-48"
                        aria-label={t("scriptEditor.caseKeyAriaLabel")}
                        value={item.key}
                        onchange={(e) => onRenameCase(scriptIndex, paramAttribute, item.key, (e.target as HTMLInputElement).value)}
                    />
                    <button
                        type="button"
                        class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none"
                        title={t("scriptEditor.removeCase")}
                        onclick={() => onRemoveCase(scriptIndex, paramAttribute, item.key)}
                    >×</button>
                </div>
                {#if expanded}
                    <div class="pl-2 pb-1 border-t border-surface-100-900">
                        <ScriptEditor
                            {elementKey}
                            {attribute}
                            containerPath={casePath(scriptIndex, paramAttribute, item.key)}
                            initialData={scriptsForCase(ctrl, item.key)}
                            depth={depth + 1}
                        />
                    </div>
                {/if}
            </div>
        {/each}
        <div class="flex items-center gap-1">
            <input
                type="text"
                autocapitalize="off"
                class="input text-xs py-0 px-1 min-w-16 max-w-48"
                placeholder={t("scriptEditor.addCaseKeyPlaceholder")}
                aria-label={t("scriptEditor.addCaseKeyAriaLabel")}
                value={newCaseKeys[groupKey] ?? ""}
                oninput={(e) => (newCaseKeys[groupKey] = (e.target as HTMLInputElement).value)}
                onkeydown={(e) => { if (e.key === "Enter") onAddCase(scriptIndex, paramAttribute); }}
            />
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5"
                disabled={!(newCaseKeys[groupKey] ?? "").trim()}
                onclick={() => onAddCase(scriptIndex, paramAttribute)}
            >{t("common.add")}</button>
        </div>
    </div>
{/snippet}

{#snippet expressionField(
    value: string,
    onchange: (v: string) => void,
    expressionType: string | null,
    overrideKey: string,
    inputClass: string,
)}
    {@const templates = templatesFor(expressionType)}
    {@const tmplData = expressionType && templates.length > 0 ? getExpressionTemplateData(value, expressionType) : null}
    {@const inTemplateMode = tmplData !== null && !expressionOverrides.has(overrideKey)}
    {#if templates.length > 0}
        <!-- Template / expression mode toggle - sized to the selected label (not a fixed
             max-width) for the same reason as the object picker below: a native <select>
             otherwise sizes to its widest *option* (e.g. "player answers a yes/no question"),
             staying that wide even once a much shorter condition like "random chance" is picked. -->
        <select
            class="select text-xs py-0 px-1"
            use:autoWidthSelect={{
                text: inTemplateMode ? tmplData!.templateName : t("scriptEditor.expressionOption"),
                minCh: 8,
                maxCh: 36,
            }}
            value={inTemplateMode ? tmplData!.templateName : "expression"}
            onchange={(e) => {
                const v = (e.target as HTMLSelectElement).value;
                if (v === "expression") {
                    expressionOverrides.add(overrideKey);
                } else {
                    expressionOverrides.delete(overrideKey);
                    const tpl = templates.find(t => t.name === v);
                    if (tpl) onchange(tpl.createExpression);
                }
            }}
        >
            <option value="expression">{t("scriptEditor.expressionOption")}</option>
            {#each templates as tpl (tpl.name)}
                <option value={tpl.name}>{tpl.name}</option>
            {/each}
        </select>
    {/if}
    {#if inTemplateMode && tmplData}
        <!-- Template controls (e.g. object picker for Got(#object#)) -->
        {#each tmplData.controls as ctrl (ctrl.name)}
            {#if ctrl.controlType === "label"}
                <!-- Static caption between controls, e.g. RandomChance's "% of the time" -->
                <span class="text-surface-600-400 select-none">{ctrl.caption ?? ""}</span>
            {:else if ctrl.simpleEditor === "boolean"}
                {@const boolSimple = inTemplateParamSimpleMode(overrideKey, ctrl)}
                <!-- Boolean: yes / no / expression dropdown, no separate widget - mirrors
                     valueControl's non-template boolean handling. -->
                <select
                    class="select text-xs py-0 px-1 max-w-32"
                    value={boolSimple ? ctrl.value : "expression"}
                    onchange={(e) => {
                        const v = (e.target as HTMLSelectElement).value;
                        const paramKey = templateParamKey(overrideKey, ctrl.name);
                        if (v === "expression") {
                            templateParamOverrides.add(paramKey);
                        } else {
                            templateParamOverrides.delete(paramKey);
                            onchange(buildTemplateExpression(tmplData!, ctrl.name, v));
                        }
                    }}
                >
                    <option value="true">{t("scriptEditor.yesOption")}</option>
                    <option value="false">{t("scriptEditor.noOption")}</option>
                    <option value="expression">{t("scriptEditor.expressionOption")}</option>
                </select>
                {#if !boolSimple}
                    <ExpressionInput
                        value={ctrl.value ?? ""}
                        onchange={(v) => onchange(buildTemplateExpression(tmplData!, ctrl.name, v))}
                        {objectNames}
                        class="input text-xs py-0 px-1"
                        minCh={16}
                        maxCh={64}
                    />
                {/if}
            {:else if ctrl.simpleEditor}
                {@const paramKey = templateParamKey(overrideKey, ctrl.name)}
                {@const paramSimple = inTemplateParamSimpleMode(overrideKey, ctrl)}
                <!-- Labelled mode toggle (e.g. "object"/"flag name" vs "expression") - every
                     simple editor gets one, since even a plain quoted-string field (e.g.
                     GetBoolean's "flag name") needs an escape hatch to an arbitrary expression,
                     not just object/number pickers whose own widget can't hold one at all. -->
                <select
                    class="select text-xs py-0 px-1 max-w-28"
                    value={paramSimple ? (ctrl.simpleLabel ?? "simple") : "expression"}
                    onchange={(e) => {
                        const v = (e.target as HTMLSelectElement).value;
                        if (v === "expression") {
                            templateParamOverrides.add(paramKey);
                        } else {
                            templateParamOverrides.delete(paramKey);
                            // Switching back to the simple widget only takes effect visually if
                            // the stored value is already simple-shaped (see
                            // inTemplateParamSimpleMode) - an arbitrary leftover expression (e.g.
                            // a function call) would otherwise leave the toggle showing
                            // "object"/"flag name" while the expression box stays put, since
                            // paramSimple is re-derived from the value's shape on every render.
                            // Reset to a blank/zero default so the switch is visible immediately,
                            // mirroring onSwitchToSimple's non-template twin.
                            if (!isSimpleValue(ctrl)) {
                                onchange(buildTemplateExpression(tmplData!, ctrl.name, defaultSimpleValue(ctrl.simpleEditor!)));
                            }
                        }
                    }}
                >
                    <option value={ctrl.simpleLabel ?? t("scriptEditor.simpleOption")}>{ctrl.simpleLabel ?? t("scriptEditor.simpleOption")}</option>
                    <option value="expression">{t("scriptEditor.expressionOption")}</option>
                </select>
                {#if paramSimple && ctrl.simpleEditor === "objects"}
                    <select
                        class="select text-xs py-0 px-1"
                        use:autoWidthSelect={{ text: ctrl.value ?? "", minCh: 4, maxCh: 46 }}
                        value={ctrl.value ?? ""}
                        onchange={(e) => {
                            const newVal = (e.target as HTMLSelectElement).value;
                            onchange(buildTemplateExpression(tmplData!, ctrl.name, newVal));
                        }}
                    >
                        <option value=""></option>
                        {#each objectNames as name (name)}
                            <option value={name}>{name}</option>
                        {/each}
                    </select>
                {:else if paramSimple && (ctrl.simpleEditor === "number" || ctrl.simpleEditor === "numberdouble")}
                    <input
                        type="number"
                        min={ctrl.minimum ?? undefined}
                        max={ctrl.maximum ?? undefined}
                        step={ctrl.increment ?? (ctrl.simpleEditor === "numberdouble" ? "any" : undefined)}
                        class="input text-xs py-0 px-1 min-w-16 max-w-32 flex-1"
                        placeholder={ctrl.simpleLabel ?? ctrl.name}
                        value={ctrl.value ?? ""}
                        onchange={(e) => {
                            const newVal = (e.target as HTMLInputElement).value;
                            onchange(buildTemplateExpression(tmplData!, ctrl.name, newVal));
                        }}
                    />
                {:else if paramSimple}
                    <!-- textbox (default): quoted NCalc string literal <-> plain text, matching
                         valueControl's own textbox handling. -->
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input text-xs py-0 px-1 min-w-16 max-w-48 flex-1"
                        placeholder={ctrl.simpleLabel ?? ctrl.name}
                        value={toSimpleDisplay(ctrl)}
                        onchange={(e) => {
                            const newVal = fromSimpleToExpression((e.target as HTMLInputElement).value, ctrl.simpleEditor!);
                            onchange(buildTemplateExpression(tmplData!, ctrl.name, newVal));
                        }}
                    />
                {:else}
                    <ExpressionInput
                        value={ctrl.value ?? ""}
                        onchange={(v) => onchange(buildTemplateExpression(tmplData!, ctrl.name, v))}
                        {objectNames}
                        class="input text-xs py-0 px-1"
                        minCh={16}
                        maxCh={64}
                    />
                {/if}
            {:else}
                <!-- No <simple>/<simpleeditor> at all (e.g. HasAttribute's "object" field, or
                     #object#.#property# = #value#'s "value") - there's no simple mode to toggle
                     to, so always show the full expression editor, same as this snippet's own
                     top-level fallback below. -->
                <ExpressionInput
                    value={ctrl.value ?? ""}
                    onchange={(v) => onchange(buildTemplateExpression(tmplData!, ctrl.name, v))}
                    {objectNames}
                    class="input text-xs py-0 px-1"
                    minCh={16}
                    maxCh={64}
                />
            {/if}
        {/each}
    {:else}
        <ExpressionInput {value} {onchange} {objectNames} class={inputClass} />
    {/if}
{/snippet}

{#snippet ifBlock(script: ScriptNodeData, i: number)}
    <div class="px-2 py-1 pr-16 pointer-coarse:pr-2 text-xs">
        <!-- If condition -->
        <div class="flex items-center gap-1 flex-wrap">
            <span class="text-surface-600-400 font-medium select-none">{t("scriptEditor.ifKeyword")}</span>
            {@render expressionField(
                script.expression ?? "",
                (v) => onSetIfExpr(i, v),
                "if",
                `if/${i}`,
                "input text-xs py-0 px-1 min-w-24 max-w-64 flex-1",
            )}
            <span class="text-surface-600-400 select-none">{t("scriptEditor.thenKeyword")}</span>
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
            <div class="mt-1 flex items-center gap-1 flex-wrap">
                <span class="text-surface-600-400 font-medium select-none">{t("scriptEditor.elseIfKeyword")}</span>
                {@render expressionField(
                    elseIf.expression ?? "",
                    (v) => onSetElseIfExpr(i, ei, v),
                    "if",
                    `elseif/${i}/${ei}`,
                    "input text-xs py-0 px-1 min-w-24 max-w-64 flex-1",
                )}
                <span class="text-surface-600-400 select-none">{t("scriptEditor.thenKeyword")}</span>
                <button
                    type="button"
                    class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none ml-auto"
                    title={t("scriptEditor.removeElseIf")}
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
                <span class="text-surface-600-400 font-medium select-none">{t("scriptEditor.elseKeyword")}</span>
                <button
                    type="button"
                    class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none ml-auto"
                    title={t("scriptEditor.removeElse")}
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
            >+ {t("scriptEditor.addElseIf")}</button>
            {#if script.elseScripts === null || script.elseScripts === undefined}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={() => onAddElse(i)}
                >+ {t("scriptEditor.addElse")}</button>
            {/if}
        </div>
    </div>
{/snippet}
