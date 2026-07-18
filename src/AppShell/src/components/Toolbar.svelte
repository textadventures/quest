<script lang="ts">
    import { AppBar } from "@skeletonlabs/skeleton-svelte";
    import { get } from "svelte/store";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { PUBLIC_WASM_PLAYER_URL, PUBLIC_SHOW_HOME } from "$env/static/public";
    import {
        gameFilename, isLoaded, isDirty, isSaving, saveError, retrySave, saveGame, saveGameAs, canSaveAs, backupGame, canBackup,
        publishModalOpen,
        previewInWasmPlayer,
        undo, redo, canUndo, canRedo,
        treeNodes, selectedKey, openAddModal,
        createExit, createTurnScript, createCommand, createVerb,
        createIncludedLibrary, createJavascript,
        deleteElement,
        assetManagerOpen,
    } from "$lib/editor-store";
    import type { TreeNode } from "$lib/types";

    const wasmPlayerUrl = PUBLIC_WASM_PLAYER_URL || "/player/";
    const showHome = PUBLIC_SHOW_HOME === "true";

    let saving = $state(false);

    // Only relevant when showHome is true — otherwise root has no content of
    // its own (see routes/+page.svelte) and there's nowhere useful to go.
    // Edits autosave continuously, so this just flushes the current game
    // first, matching the File > New/Open flush pattern in +layout.svelte.
    async function handleHome() {
        if (get(isLoaded)) await saveGame();
        void goto(`${base}/`);
    }

    async function handleSaveAs() {
        saving = true;
        try { await saveGameAs(); } finally { saving = false; }
    }

    async function handleBackup() {
        saving = true;
        try { await backupGame(); } finally { saving = false; }
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
            {#if showHome}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 mr-3"
                    onclick={handleHome}
                    title="Back to Home"
                >🏠 Home</button>
            {/if}
            <span class="font-semibold">Quest Viva Editor</span>
            {#if $gameFilename}
                <span class="ml-3 text-sm text-surface-500-400">{$gameFilename}</span>
            {/if}
            {#if $saveError}
                <button
                    type="button"
                    class="ml-3 text-sm text-error-500 underline"
                    onclick={() => retrySave()}
                    title={$saveError}
                >⚠ Save failed — Retry</button>
            {:else if $isSaving || $isDirty}
                <span class="ml-3 text-sm text-surface-500-400">Saving…</span>
            {:else if $gameFilename}
                <span class="ml-3 text-sm text-surface-500-400">Saved</span>
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
                {#if $canSaveAs}
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={handleSaveAs} disabled={saving} title="Save As">Save As…</button>
                {/if}
                {#if $canBackup}
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={handleBackup} disabled={saving} title="Download a .zip copy of this game and its assets">Backup…</button>
                {/if}
                {#if $gameFilename}
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={() => publishModalOpen.set(true)} title="Build a .quest package for distribution">Publish…</button>
                    <button type="button" class="btn btn-sm preset-outlined-secondary-500" onclick={handlePreview} title="Preview game">▶ Preview</button>
                {/if}
            </div>
        </AppBar.Trail>
    </AppBar.Toolbar>
</AppBar>

