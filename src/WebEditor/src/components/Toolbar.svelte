<script lang="ts">
    import { AppBar } from "@skeletonlabs/skeleton-svelte";
    import { PUBLIC_WEBEDITOR_VERSION, PUBLIC_WASM_PLAYER_URL } from "$env/static/public";
    import {
        gameFilename, isDirty, saveGame, saveGameAs, canSaveAs,
        previewInWasmPlayer,
        undo, redo, canUndo, canRedo,
        treeNodes, selectedKey, openAddModal,
        createExit, createTurnScript, createCommand, createVerb,
        createIncludedLibrary, createJavascript,
        deleteElement,
        assetManagerOpen,
    } from "$lib/editor-store";
    import type { TreeNode } from "$lib/types";

    const wasmPlayerUrl = PUBLIC_WASM_PLAYER_URL || '/player/';

    let saving = $state(false);

    async function handleSave() {
        saving = true;
        try { await saveGame(); } finally { saving = false; }
    }

    async function handleSaveAs() {
        saving = true;
        try { await saveGameAs(); } finally { saving = false; }
    }

    async function handlePreview() {
        await previewInWasmPlayer(wasmPlayerUrl);
    }

    // Derive the currently selected tree node
    let selectedNode = $derived<TreeNode | null>(
        $treeNodes.find(n => n.key === $selectedKey) ?? null
    );

    let nt = $derived(selectedNode?.nodeType ?? "");
    let canDelete = $derived(
        nt !== "" && nt !== "header" && nt !== "game" && nt !== "other"
    );

    // Context-sensitive add options
    type AddOption = { label: string; action: () => void };
    let addOptions = $derived<AddOption[]>([
        // Always available
        { label: "Add Room", action: () => openAddModal("room", null) },
        { label: "Add Function", action: () => openAddModal("function", null) },
        { label: "Add Timer", action: () => openAddModal("timer", null) },
        { label: "Add Walkthrough", action: () => openAddModal("walkthrough", null) },
        { label: "Add Template", action: () => openAddModal("template", null) },
        { label: "Add Dynamic Template", action: () => openAddModal("dynamictemplate", null) },
        { label: "Add Type", action: () => openAddModal("type", null) },
        { label: "Add Library", action: () => createIncludedLibrary() },
        { label: "Add JavaScript", action: () => createJavascript() },
        // Context-sensitive: when a room or object is selected
        ...(nt === "room" || nt === "object" ? [
            { label: `Add Object in "${selectedNode!.text}"`, action: () => openAddModal("object", selectedNode!.key) },
        ] : []),
        ...(nt === "room" ? [
            { label: `Add Room in "${selectedNode!.text}"`, action: () => openAddModal("room", selectedNode!.key) },
            { label: `Add Exit from "${selectedNode!.text}"`, action: () => createExit(selectedNode!.key) },
        ] : []),
        ...(nt === "room" || nt === "object" ? [
            { label: `Add Command to "${selectedNode!.text}"`, action: () => createCommand(selectedNode!.key) },
            { label: `Add Verb to "${selectedNode!.text}"`, action: () => createVerb(selectedNode!.key) },
            { label: `Add Turn Script to "${selectedNode!.text}"`, action: () => createTurnScript(selectedNode!.key) },
        ] : []),
    ]);

    let addOpen = $state(false);

    function closeAdd() { addOpen = false; }
    function toggleAdd(e: MouseEvent) { e.stopPropagation(); addOpen = !addOpen; }
    function doAdd(action: () => void) { action(); addOpen = false; }

    $effect(() => {
        if (!addOpen) return;
        function onOutside(e: MouseEvent) {
            if (!(e.target as HTMLElement).closest(".add-dropdown")) closeAdd();
        }
        document.addEventListener("mousedown", onOutside);
        return () => document.removeEventListener("mousedown", onOutside);
    });
</script>

<AppBar>
        <AppBar.Toolbar class="grid-cols-[auto_1fr_auto]">
            <AppBar.Lead>
                <span class="font-semibold">Quest Viva Editor</span>
                {#if PUBLIC_WEBEDITOR_VERSION}
                    <span class="ml-2 text-xs text-surface-500-400">{PUBLIC_WEBEDITOR_VERSION}</span>
                {/if}
                {#if $gameFilename}
                    <span class="ml-3 text-sm text-surface-500-400">{$gameFilename}{#if $isDirty} *{/if}</span>
                {/if}
            </AppBar.Lead>
            <AppBar.Trail>
                <div class="flex gap-2 items-center">
                    <!-- Add dropdown -->
                    <div class="add-dropdown relative">
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500"
                            onclick={toggleAdd}
                            title="Add element"
                        >+ Add ▾</button>
                        {#if addOpen}
                            <div class="absolute right-0 top-full z-[999] mt-1 w-56 bg-surface-50-950 border border-surface-200-800 rounded shadow-lg py-1">
                                {#each addOptions as opt (opt.label)}
                                    <button
                                        class="w-full text-left px-3 py-1.5 text-xs hover:bg-surface-200-800"
                                        onclick={() => doAdd(opt.action)}
                                    >{opt.label}</button>
                                {/each}
                            </div>
                        {/if}
                    </div>
                    <!-- Delete button -->
                    {#if canDelete}
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-error-500"
                            onclick={() => deleteElement(selectedNode!.key)}
                            title={"Delete " + (selectedNode?.text ?? "")}
                        >Delete</button>
                    {/if}
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={() => assetManagerOpen.set(true)} title="Manage assets">🖼 Assets</button>
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={undo} disabled={!$canUndo} title="Undo">↩ Undo</button>
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={redo} disabled={!$canRedo} title="Redo">↪ Redo</button>
                    <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handleSave} disabled={saving} title="Save">💾 Save</button>
                    {#if $canSaveAs}
                        <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={handleSaveAs} disabled={saving} title="Save As">Save As…</button>
                    {/if}
                    {#if $gameFilename}
                        <button type="button" class="btn btn-sm preset-outlined-secondary-500" onclick={handlePreview} title="Preview game">▶ Preview</button>
                    {/if}
                </div>
            </AppBar.Trail>
        </AppBar.Toolbar>
    </AppBar>

