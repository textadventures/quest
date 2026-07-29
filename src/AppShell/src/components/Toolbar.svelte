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
        treeNodes, selectedKey, isGamebook, openAddModal,
        createExit, createTurnScript, createCommand, createVerb,
        deleteElement,
        assetManagerOpen,
        codeViewPanelOpen,
        codeViewCloseRequested,
    } from "$lib/editor-store";
    import { hasActiveCmView, cmUndo, cmRedo } from "$lib/code-editor-registry";
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
    import Ellipsis from "@lucide/svelte/icons/ellipsis";
    import FileCode from "@lucide/svelte/icons/file-code";
    import DiscordIcon from "$components/DiscordIcon.svelte";
    import GithubIcon from "$components/GithubIcon.svelte";
    import DropdownMenu from "$components/DropdownMenu.svelte";
    import type { DropdownMenuItem } from "$components/DropdownMenu.svelte";
    import { isNarrow } from "$lib/layout.svelte";

    const DISCORD_URL = "https://textadventures.co.uk/community/discord";
    const GITHUB_URL = "https://github.com/textadventures/quest";
    function openLink(url: string) {
        window.open(url, "_blank", "noopener,noreferrer");
    }

    const wasmPlayerUrl = PUBLIC_WASM_PLAYER_URL || "/player/";
    const showHome = PUBLIC_SHOW_HOME === "true";

    let saving = $state(false);

    // Only relevant when showHome is true — otherwise root has no content of
    // its own (see routes/+page.svelte) and there's nowhere useful to go.
    // Edits autosave continuously, so this just flushes the current game
    // first, matching the File > New/Open flush pattern in +layout.svelte.
    async function handleHome() {
        if (get(isLoaded)) await saveGame();
        void goto(`${base}/open`);
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
        // Without this, a field the author is still mid-typing in (not yet blurred) never
        // commits into the bridge at all — Preview would open showing stale content, since text
        // inputs commit on blur/change, not on every keystroke. Same flush saveGame() already
        // does before navigating home/away; Preview just never called it.
        await saveGame();
        await previewInWasmPlayer(wasmPlayerUrl);
    }

    // While a CodeEditor (per-script Code view, or the raw XML panel) is on screen and editable,
    // Undo/Redo act on its own CodeMirror history instead of Quest's model — otherwise these
    // buttons would silently undo/redo the last applied model change in the background while the
    // author is still looking at unrelated, not-yet-committed text.
    function handleUndo() {
        if (get(hasActiveCmView)) { cmUndo(); return; }
        undo();
    }
    function handleRedo() {
        if (get(hasActiveCmView)) { cmRedo(); return; }
        redo();
    }

    // Acts as a toggle: opens the raw XML panel if closed, or — if it's already open — asks it to
    // attempt closing (CodeViewPanel prompts to Apply/Discard first when there are unsaved edits,
    // rather than this just forcing it shut).
    function handleToggleCodeView() {
        if (get(codeViewPanelOpen)) {
            codeViewCloseRequested.update(n => n + 1);
        } else {
            codeViewPanelOpen.set(true);
        }
    }

    // Derive the currently selected tree node
    let selectedNode = $derived<TreeNode | null>(
        $treeNodes.find(n => n.key === $selectedKey) ?? null
    );

    let nt = $derived(selectedNode?.nodeType ?? "");
    let canDelete = $derived(
        nt !== "" && nt !== "header" && nt !== "game" && nt !== "other" && !selectedNode?.isLibrary
    );

    // Context-sensitive add options. Function/Timer/Walkthrough/Library/Template/
    // Dynamic Template/Type/JavaScript live under the "Advanced" tree node instead
    // (see PropertyEditor's ADVANCED_ADDERS) — they're rare enough that they don't
    // earn a slot in this always-visible dropdown.
    type AddOption = { label: string; action: () => void };
    // Gamebook pages are always flat (no rooms/objects/exits/verbs/commands), so
    // the only add option is a single top-level "Add Page".
    let addOptions = $derived<AddOption[]>($isGamebook ? [
        { label: "Add Page", action: () => openAddModal("page", null) },
    ] : [
        // Always available
        { label: "Add Room", action: () => openAddModal("room", null) },
        // Context-sensitive: when a room or object is selected (but not a read-only
        // library-origin one — same restriction as the tree's own "…" menu).
        ...(!selectedNode?.isLibrary && (nt === "room" || nt === "object") ? [
            { label: `Add Object in "${selectedNode!.text}"`, action: () => openAddModal("object", selectedNode!.key) },
        ] : []),
        ...(!selectedNode?.isLibrary && nt === "room" ? [
            { label: `Add Room in "${selectedNode!.text}"`, action: () => openAddModal("room", selectedNode!.key) },
            { label: `Add Exit from "${selectedNode!.text}"`, action: () => createExit(selectedNode!.key) },
        ] : []),
        ...(!selectedNode?.isLibrary && (nt === "room" || nt === "object") ? [
            { label: `Add Command to "${selectedNode!.text}"`, action: () => createCommand(selectedNode!.key) },
            { label: `Add Verb to "${selectedNode!.text}"`, action: () => createVerb(selectedNode!.key) },
            { label: `Add Turn Script to "${selectedNode!.text}"`, action: () => createTurnScript(selectedNode!.key) },
        ] : []),
    ]);

    // File menu (desktop): Save As / Backup / Publish, each only present under
    // the same conditions the old individual buttons used.
    let fileMenuItems = $derived.by((): DropdownMenuItem[] => {
        const items: DropdownMenuItem[] = [];
        if ($canSaveAs) items.push({ label: "Save As…", action: handleSaveAs, icon: Save, disabled: saving });
        if ($canBackup) items.push({ label: "Backup…", action: handleBackup, icon: Download, disabled: saving });
        if ($gameFilename) items.push({ label: "Publish…", action: () => publishModalOpen.set(true), icon: Package });
        return items;
    });

    // Overflow (⋯) menu: on desktop just the community links (reserved future
    // home for anything that doesn't earn a top-level slot). On mobile it also
    // absorbs everything that doesn't fit the collapsed toolbar — Delete,
    // Assets/Undo/Redo, and the File menu's items — since Add and Preview stay
    // as visible icon-only buttons.
    let overflowItems = $derived.by((): DropdownMenuItem[] => {
        const links: DropdownMenuItem[] = [
            { label: "Discord", action: () => openLink(DISCORD_URL), icon: DiscordIcon },
            { label: "GitHub", action: () => openLink(GITHUB_URL), icon: GithubIcon },
        ];
        if (!isNarrow.current) return links;

        const fileItems = fileMenuItems;
        if (fileItems.length > 0) fileItems[0] = { ...fileItems[0], divider: true };
        links[0] = { ...links[0], divider: true };

        return [
            { label: "Delete", action: () => selectedNode && deleteElement(selectedNode.key), icon: Trash2, disabled: $codeViewPanelOpen || !canDelete },
            { label: "Manage assets", action: () => assetManagerOpen.set(true), icon: ImageIcon, divider: true },
            { label: $hasActiveCmView ? "Undo (in code editor)" : "Undo", action: handleUndo, icon: Undo2, disabled: $hasActiveCmView ? false : !$canUndo },
            { label: $hasActiveCmView ? "Redo (in code editor)" : "Redo", action: handleRedo, icon: Redo2, disabled: $hasActiveCmView ? false : !$canRedo },
            { label: "Raw XML code view", action: handleToggleCodeView, icon: FileCode },
            ...fileItems,
            ...links,
        ];
    });
</script>

<AppBar>
    <AppBar.Toolbar class="grid-cols-[auto_1fr_auto]">
        <AppBar.Lead class="flex-1 min-w-0">
            <div class="flex items-center min-w-0">
                {#if showHome}
                    <button
                        type="button"
                        class="toolbar-icon-btn mr-2 shrink-0"
                        onclick={handleHome}
                        title="Back to Home"
                    ><Home size={16} /></button>
                {/if}
                {#if $gameFilename}
                    <span class="font-mono text-sm font-medium truncate min-w-0">{$gameFilename}</span>
                {/if}
                {#if $saveError}
                    <button
                        type="button"
                        class="save-chip save-chip-error shrink-0"
                        onclick={() => retrySave()}
                        title={$saveError}
                    ><TriangleAlert size={13} /> <span class="hidden md:inline">Save failed — Retry</span></button>
                {:else if $isSaving}
                    <span class="save-chip save-chip-saving shrink-0"><LoaderCircle size={13} class="animate-spin" /> <span class="hidden md:inline">Saving…</span></span>
                {:else if $isDirty || $isEditingField}
                    <button
                        type="button"
                        class="save-chip save-chip-unsaved shrink-0"
                        onclick={handleSaveNow}
                        title="Save now"
                    ><Circle size={8} fill="currentColor" /> <span class="hidden md:inline">Unsaved</span></button>
                {:else if $gameFilename}
                    <span class="save-chip save-chip-saved shrink-0"><Check size={13} /> <span class="hidden md:inline">Saved</span></span>
                {/if}
            </div>
        </AppBar.Lead>
        <AppBar.Trail>
            <div class="flex gap-1.5 items-center">
                <!-- Add dropdown: icon-only below md. Disabled while the raw XML panel is open —
                     there's no tree selection context there, and the tree itself isn't even
                     mounted (see edit/+page.svelte), so these actions have nothing to act on. -->
                <DropdownMenu items={addOptions}>
                    {#snippet trigger(toggle)}
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500"
                            onclick={toggle}
                            disabled={$codeViewPanelOpen}
                            title="Add element"
                        ><Plus size={14} /> <span class="hidden md:inline">Add</span> <ChevronDown size={12} class="hidden md:inline" /></button>
                    {/snippet}
                </DropdownMenu>
                <!-- Delete button: always rendered, disabled when nothing deletable is
                     selected (or the raw XML panel is open — see the Add dropdown above), so
                     surrounding buttons don't shift as selection changes.
                     Desktop only — folded into the ⋯ menu on mobile. -->
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-error-500 hidden md:inline-flex"
                    onclick={() => selectedNode && deleteElement(selectedNode.key)}
                    disabled={$codeViewPanelOpen || !canDelete}
                    title={canDelete ? "Delete " + (selectedNode?.text ?? "") : "Delete"}
                ><Trash2 size={14} /> Delete</button>
                <div class="toolbar-divider hidden md:block"></div>
                <button type="button" class="toolbar-icon-btn !hidden md:!inline-flex" onclick={() => assetManagerOpen.set(true)} title="Manage assets"><ImageIcon size={16} /></button>
                <button type="button" class="toolbar-icon-btn !hidden md:!inline-flex" onclick={handleUndo} disabled={$hasActiveCmView ? false : !$canUndo} title={$hasActiveCmView ? "Undo (in code editor)" : "Undo"}><Undo2 size={16} /></button>
                <button type="button" class="toolbar-icon-btn !hidden md:!inline-flex" onclick={handleRedo} disabled={$hasActiveCmView ? false : !$canRedo} title={$hasActiveCmView ? "Redo (in code editor)" : "Redo"}><Redo2 size={16} /></button>
                <button type="button" class="toolbar-icon-btn !hidden md:!inline-flex" onclick={handleToggleCodeView} title="Raw XML code view"><FileCode size={16} /></button>
                <div class="toolbar-divider hidden md:block"></div>
                {#if fileMenuItems.length > 0}
                    <div class="hidden md:block">
                        <DropdownMenu items={fileMenuItems}>
                            {#snippet trigger(toggle)}
                                <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={toggle} disabled={saving} title="File"
                                >File <ChevronDown size={12} /></button>
                            {/snippet}
                        </DropdownMenu>
                    </div>
                {/if}
                {#if $gameFilename}
                    <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handlePreview} title="Preview game"><Play size={14} /> <span class="hidden md:inline">Preview</span></button>
                {/if}
                <!-- Overflow menu: community links on desktop; also Delete/Assets/
                     Undo/Redo/File-menu items on mobile (see overflowItems) -->
                <DropdownMenu items={overflowItems}>
                    {#snippet trigger(toggle)}
                        <button type="button" class="toolbar-icon-btn" onclick={toggle} title="More"><Ellipsis size={16} /></button>
                    {/snippet}
                </DropdownMenu>
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
        border: none;
        border-radius: 999px;
        font: inherit;
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
    @media (max-width: 767px) {
        .save-chip-saved,
        .save-chip-saving,
        .save-chip-unsaved {
            min-width: 0;
        }
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

