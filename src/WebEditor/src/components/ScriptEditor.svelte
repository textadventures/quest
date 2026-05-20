<script lang="ts">
    import ScriptEditor from "./ScriptEditor.svelte";
    import {
        scriptVersion,
        getScriptData,
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
    } from "$lib/editor-store";
    import type {
        ScriptBlockData,
        ScriptNodeData,
        ScriptControlData,
        ScriptCategoryInfo,
    } from "$lib/types";

    interface Props {
        elementKey: string;
        attribute: string;
        containerPath?: string;
        // Pre-loaded data for nested blocks (avoids extra WASM round-trips)
        initialData?: ScriptNodeData[] | null;
        depth?: number;
    }

    let {
        elementKey,
        attribute,
        containerPath = "",
        initialData = null,
        depth = 0,
    }: Props = $props();

    let scriptData = $state<ScriptBlockData | null>(null);
    let categories = $state<ScriptCategoryInfo[]>([]);
    let addKeyword = $state("");
    let isRoot = $derived(initialData === null);

    // Load on mount and whenever scriptVersion bumps (undo/redo)
    $effect(() => {
        const version = $scriptVersion; // track for reactivity on undo/redo
        if (isRoot) {
            scriptData = version >= 0 ? getScriptData(elementKey, attribute) : null;
        }
        if (categories.length === 0) {
            const cats = getScriptCommandCategories();
            if (cats) {
                categories = cats.categories;
                if (cats.categories.length > 0 && cats.categories[0].commands.length > 0) {
                    addKeyword = cats.categories[0].commands[0].createString;
                }
            }
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
    }

    function mutate(fn: () => string): boolean {
        const result = fn();
        if (result === "ok") {
            refresh();
            return true;
        }
        return false;
    }

    function onAddScript() {
        if (!addKeyword) return;
        mutate(() => addScript(elementKey, attribute, containerPath, addKeyword));
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
    {#each scripts() as script, i (script.id)}
        <div class="group relative border border-surface-200-800 rounded mb-1 bg-surface-50-950">
            <!-- Script row actions -->
            <div class="absolute right-1 top-1 flex gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity z-10">
                <button
                    type="button"
                    class="btn btn-sm preset-outlined px-1 py-0 text-xs leading-none"
                    title="Move up"
                    disabled={i === 0}
                    onclick={() => onMoveUp(i)}
                >↑</button>
                <button
                    type="button"
                    class="btn btn-sm preset-outlined px-1 py-0 text-xs leading-none"
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
    {/each}

    <!-- Add script row -->
    <div class="flex items-center gap-1 mt-1">
        {#if categories.length > 0}
            <select
                class="select text-xs py-0.5 px-1 flex-1 min-w-0"
                bind:value={addKeyword}
            >
                {#each categories as cat, ci (ci)}
                    <optgroup label={cat.name}>
                        {#each cat.commands as cmd, cmi (cmi)}
                            <option value={cmd.createString}>{cmd.add}</option>
                        {/each}
                    </optgroup>
                {/each}
            </select>
            <button
                type="button"
                class="btn btn-sm preset-outlined text-xs py-0.5 whitespace-nowrap"
                onclick={onAddScript}
            >Add script</button>
        {:else if isRoot}
            <span class="text-xs text-surface-400-500 italic">Loading commands…</span>
        {/if}
    </div>
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
    {#if ctrl.controlType === "expression" || ctrl.controlType === "textbox" || ctrl.controlType === "richtext"}
        <input
            type="text"
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
    <div class="px-2 py-1 pr-16 text-xs">
        <!-- If condition -->
        <div class="flex items-center gap-1 flex-wrap">
            <span class="text-surface-600-400 font-medium select-none">if</span>
            <input
                type="text"
                class="input text-xs py-0 px-1 min-w-24 max-w-64 flex-1"
                value={script.expression ?? ""}
                onchange={(e) => onSetIfExpr(i, (e.target as HTMLInputElement).value)}
            />
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
            <div class="mt-1 flex items-center gap-1 flex-wrap">
                <span class="text-surface-600-400 font-medium select-none">else if</span>
                <input
                    type="text"
                    class="input text-xs py-0 px-1 min-w-24 max-w-64 flex-1"
                    value={elseIf.expression}
                    onchange={(e) => onSetElseIfExpr(i, ei, (e.target as HTMLInputElement).value)}
                />
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
                class="btn btn-sm preset-outlined text-xs py-0.5"
                onclick={() => onAddElseIf(i)}
            >+ else if</button>
            {#if script.elseScripts === null || script.elseScripts === undefined}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined text-xs py-0.5"
                    onclick={() => onAddElse(i)}
                >+ else</button>
            {/if}
        </div>
    </div>
{/snippet}
