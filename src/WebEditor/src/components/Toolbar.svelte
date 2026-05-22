<script lang="ts">
    import { AppBar } from "@skeletonlabs/skeleton-svelte";
    import { PUBLIC_WEBEDITOR_VERSION } from "$env/static/public";
    import {
        gameFilename, saveGame, undo, redo, canUndo, canRedo,
        treeNodes, selectedKey, openAddModal,
        createExit, createTurnScript, createCommand, createVerb,
        deleteElement,
    } from "$lib/editor-store";
    import type { TreeNode } from "$lib/types";

    function handleSave() {
        const xml = saveGame();
        const blob = new Blob([xml], { type: "application/xml" });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = $gameFilename ?? "game.aslx";
        a.click();
        URL.revokeObjectURL(url);
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
                    <span class="ml-2 text-xs text-surface-500-400">{PUBLIC_WEBEDITOR_VERSION.replace('webeditor-', '')}</span>
                {/if}
                {#if $gameFilename}
                    <span class="ml-3 text-sm text-surface-500-400">{$gameFilename}</span>
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
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={undo} disabled={!$canUndo} title="Undo">↩ Undo</button>
                    <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={redo} disabled={!$canRedo} title="Redo">↪ Redo</button>
                    <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handleSave} title="Save">💾 Save</button>
                </div>
            </AppBar.Trail>
        </AppBar.Toolbar>
    </AppBar>
