<script lang="ts">
    import { AppBar } from "@skeletonlabs/skeleton-svelte";
    import { get } from "svelte/store";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { PUBLIC_WASM_PLAYER_URL, PUBLIC_SHOW_HOME } from "$env/static/public";
    import {
        gameFilename, isLoaded, isDirty, isSaving, isEditingField, getLastEditedElement, saveError, retrySave, saveGame, saveGameAs, canSaveAs, backupGame, canBackup,
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
    import Home from "@lucide/svelte/icons/home";
    import ImageIcon from "@lucide/svelte/icons/image";
    import Undo2 from "@lucide/svelte/icons/undo-2";
    import Redo2 from "@lucide/svelte/icons/redo-2";
    import Plus from "@lucide/svelte/icons/plus";
    import ChevronDown from "@lucide/svelte/icons/chevron-down";
    import Trash2 from "@lucide/svelte/icons/trash-2";
    import Save from "@lucide/svelte/icons/save";
    import Download from "@lucide/svelte/icons/download";
    import Package from "@lucide/svelte/icons/package";
    import Play from "@lucide/svelte/icons/play";
    import Check from "@lucide/svelte/icons/check";
    import LoaderCircle from "@lucide/svelte/icons/loader-circle";
    import TriangleAlert from "@lucide/svelte/icons/triangle-alert";
    import Circle from "@lucide/svelte/icons/circle";

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

    // Clicking the chip itself steals focus away from whatever field the
    // author was mid-edit in — put it back once the save completes.
    async function handleSaveNow() {
        const field = getLastEditedElement();
        await saveGame();
        field?.focus();
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
                    class="toolbar-icon-btn mr-2"
                    onclick={handleHome}
                    title="Back to Home"
                ><Home size={16} /></button>
            {/if}
            {#if $gameFilename}
                <span class="font-mono text-sm font-medium">{$gameFilename}</span>
            {/if}
            {#if $saveError}
                <button
                    type="button"
                    class="save-chip save-chip-error"
                    onclick={() => retrySave()}
                    title={$saveError}
                ><TriangleAlert size={13} /> Save failed — Retry</button>
            {:else if $isSaving}
                <span class="save-chip save-chip-saving"><LoaderCircle size={13} class="animate-spin" /> Saving…</span>
            {:else if $isDirty || $isEditingField}
                <button
                    type="button"
                    class="save-chip save-chip-unsaved"
                    onclick={handleSaveNow}
                    title="Save now"
                ><Circle size={8} fill="currentColor" /> Unsaved</button>
            {:else if $gameFilename}
                <span class="save-chip save-chip-saved"><Check size={13} /> Saved</span>
            {/if}
        </AppBar.Lead>
        <AppBar.Trail>
            <div class="flex gap-1.5 items-center">
                <!-- Add dropdown -->
                <div class="add-dropdown relative">
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-primary-500"
                        onclick={toggleAdd}
                        title="Add element"
                    ><Plus size={14} /> Add <ChevronDown size={12} /></button>
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
                    ><Trash2 size={14} /> Delete</button>
                {/if}
                <div class="toolbar-divider"></div>
                <button type="button" class="toolbar-icon-btn" onclick={() => assetManagerOpen.set(true)} title="Manage assets"><ImageIcon size={16} /></button>
                <button type="button" class="toolbar-icon-btn" onclick={undo} disabled={!$canUndo} title="Undo"><Undo2 size={16} /></button>
                <button type="button" class="toolbar-icon-btn" onclick={redo} disabled={!$canRedo} title="Redo"><Redo2 size={16} /></button>
                <div class="toolbar-divider"></div>
                {#if $canSaveAs}
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={handleSaveAs} disabled={saving} title="Save As"><Save size={14} /> Save As…</button>
                {/if}
                {#if $canBackup}
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={handleBackup} disabled={saving} title="Download a .zip copy of this game and its assets"><Download size={14} /> Backup…</button>
                {/if}
                {#if $gameFilename}
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={() => publishModalOpen.set(true)} title="Build a .quest package for distribution"><Package size={14} /> Publish…</button>
                    <div class="toolbar-divider"></div>
                    <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handlePreview} title="Preview game"><Play size={14} /> Preview</button>
                {/if}
            </div>
        </AppBar.Trail>
    </AppBar.Toolbar>
</AppBar>

<style>
    .toolbar-icon-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 2rem;
        height: 2rem;
        border-radius: var(--radius-container);
        color: var(--color-surface-500-400);
    }
    .toolbar-icon-btn:hover:not(:disabled) {
        background-color: var(--color-surface-200-800);
        color: var(--color-primary-500);
    }
    .toolbar-icon-btn:disabled {
        opacity: 0.4;
    }
    .toolbar-divider {
        width: 1px;
        height: 1.25rem;
        background-color: var(--color-surface-200-800);
        margin: 0 0.25rem;
    }
    .save-chip {
        display: inline-flex;
        align-items: center;
        gap: 0.3rem;
        margin-left: 0.75rem;
        padding: 0.15rem 0.55rem 0.15rem 0.45rem;
        border-radius: 999px;
        font-size: 0.75rem;
        font-weight: 600;
        line-height: 1.4;
        white-space: nowrap;
    }
    .save-chip-saved,
    .save-chip-saving,
    .save-chip-unsaved {
        min-width: 6.25rem;
        justify-content: center;
    }
    .save-chip-saved {
        color: var(--color-success-600-400);
        background-color: color-mix(in srgb, var(--color-success-500) 12%, transparent);
    }
    .save-chip-saving {
        color: var(--color-surface-500-400);
    }
    .save-chip-unsaved {
        color: var(--color-warning-600-400);
        background-color: color-mix(in srgb, var(--color-warning-500) 12%, transparent);
        cursor: pointer;
    }
    .save-chip-unsaved:hover {
        text-decoration: underline;
    }
    .save-chip-error {
        color: var(--color-error-500);
        background-color: color-mix(in srgb, var(--color-error-500) 12%, transparent);
        cursor: pointer;
    }
    .save-chip-error:hover {
        text-decoration: underline;
    }
</style>

