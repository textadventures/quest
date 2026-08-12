<script lang="ts">
    import { AppBar } from "@skeletonlabs/skeleton-svelte";
    import { get } from "svelte/store";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { PUBLIC_WASM_PLAYER_URL, PUBLIC_SHOW_HOME } from "$env/static/public";
    import {
        gameFilename, isLoaded, isDirty, isSaving, isEditingField, getLastEditedElement, saveError, retrySave, saveGame, saveGameAs, canSaveAs, backupGame, canBackup,
        publishModalOpen, exportSingleFile,
        previewInWasmPlayer,
        undo, redo, canUndo, canRedo,
        navigateBack, navigateForward, canGoBack, canGoForward,
        treeNodes, selectedKey, isGamebook, openAddModal,
        createExit, createTurnScript, createCommand, createVerb,
        deleteElement,
        assetManagerOpen,
        codeViewPanelOpen,
        codeViewCloseRequested,
    } from "$lib/editor-store";
    import { hasActiveCmView, cmUndo, cmRedo } from "$lib/code-editor-registry";
    import { showToast } from "$lib/toast";
    import { settingsModalOpen } from "$lib/settings-store";
    import { t } from "$lib/i18n";
    import type { TreeNode } from "$lib/types";
    import Home from "@lucide/svelte/icons/home";
    import ArrowLeft from "@lucide/svelte/icons/arrow-left";
    import ArrowRight from "@lucide/svelte/icons/arrow-right";
    import ImageIcon from "@lucide/svelte/icons/image";
    import Undo2 from "@lucide/svelte/icons/undo-2";
    import Redo2 from "@lucide/svelte/icons/redo-2";
    import Plus from "@lucide/svelte/icons/plus";
    import ChevronDown from "@lucide/svelte/icons/chevron-down";
    import Trash2 from "@lucide/svelte/icons/trash-2";
    import Save from "@lucide/svelte/icons/save";
    import Download from "@lucide/svelte/icons/download";
    import Package from "@lucide/svelte/icons/package";
    import Globe from "@lucide/svelte/icons/globe";
    import Play from "@lucide/svelte/icons/play";
    import Check from "@lucide/svelte/icons/check";
    import LoaderCircle from "@lucide/svelte/icons/loader-circle";
    import TriangleAlert from "@lucide/svelte/icons/triangle-alert";
    import Circle from "@lucide/svelte/icons/circle";
    import Ellipsis from "@lucide/svelte/icons/ellipsis";
    import FileCode from "@lucide/svelte/icons/file-code";
    import SettingsIcon from "@lucide/svelte/icons/settings";
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

    async function handleExportSingleFile() {
        saving = true;
        try { await exportSingleFile(); } catch (err) { showToast(String(err)); } finally { saving = false; }
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
    // selectedNode.canDelete is authoritative (mirrors EditorController.CanDelete exactly —
    // "game", the gamebook player, and any built-in library are all already false there).
    // "header"/"other" and library-origin content are kept as extra client-side guards since
    // CanDelete doesn't know about tree presentation concerns.
    let canDelete = $derived(
        nt !== "" && nt !== "header" && nt !== "other" && !selectedNode?.isLibrary && !!selectedNode?.canDelete
    );

    // Context-sensitive add options. Function/Timer/Walkthrough/Library/Template/
    // Dynamic Template/Type/JavaScript live under the "Advanced" tree node instead
    // (see PropertyEditor's ADVANCED_ADDERS) — they're rare enough that they don't
    // earn a slot in this always-visible dropdown.
    type AddOption = { label: string; action: () => void };
    // Gamebook pages are always flat (no rooms/objects/exits/verbs/commands), so
    // the only add option is a single top-level "Add Page".
    let addOptions = $derived<AddOption[]>($isGamebook ? [
        { label: t("toolbar.addPage"), action: () => openAddModal("page", null) },
    ] : [
        // Always available
        { label: t("toolbar.addRoom"), action: () => openAddModal("room", null) },
        // Context-sensitive: when a room or object is selected (but not a read-only
        // library-origin one — same restriction as the tree's own "…" menu).
        ...(!selectedNode?.isLibrary && (nt === "room" || nt === "object") ? [
            { label: t("toolbar.addObjectIn", { name: selectedNode!.text }), action: () => openAddModal("object", selectedNode!.key) },
        ] : []),
        ...(!selectedNode?.isLibrary && nt === "room" ? [
            { label: t("toolbar.addRoomIn", { name: selectedNode!.text }), action: () => openAddModal("room", selectedNode!.key) },
            { label: t("toolbar.addExitFrom", { name: selectedNode!.text }), action: () => createExit(selectedNode!.key) },
        ] : []),
        ...(!selectedNode?.isLibrary && (nt === "room" || nt === "object") ? [
            { label: t("toolbar.addCommandTo", { name: selectedNode!.text }), action: () => createCommand(selectedNode!.key) },
            { label: t("toolbar.addVerbTo", { name: selectedNode!.text }), action: () => createVerb(selectedNode!.key) },
            { label: t("toolbar.addTurnScriptTo", { name: selectedNode!.text }), action: () => createTurnScript(selectedNode!.key) },
        ] : []),
        // Text Adventure dialogue pages. Created under the selected room/object/page
        // when there is one — the natural authoring structure for dialogue — and at
        // the top level otherwise.
        !selectedNode?.isLibrary && (nt === "room" || nt === "object" || nt === "page")
            ? { label: t("toolbar.addPageIn", { name: selectedNode!.text }), action: () => openAddModal("page", selectedNode!.key) }
            : { label: t("toolbar.addPage"), action: () => openAddModal("page", null) },
    ]);

    // File menu (desktop): Save As / Backup / Publish, each only present under
    // the same conditions the old individual buttons used.
    let fileMenuItems = $derived.by((): DropdownMenuItem[] => {
        const items: DropdownMenuItem[] = [];
        if ($canSaveAs) items.push({ label: t("toolbar.saveAs"), action: handleSaveAs, icon: Save, disabled: saving });
        if ($canBackup) items.push({ label: t("toolbar.backup"), action: handleBackup, icon: Download, disabled: saving });
        if ($gameFilename) items.push({ label: t("toolbar.publish"), action: () => publishModalOpen.set(true), icon: Package });
        if ($gameFilename) items.push({ label: t("toolbar.exportSingleFile"), action: handleExportSingleFile, icon: Globe, disabled: saving });
        return items;
    });

    // Overflow (⋯) menu: on desktop just the community links (reserved future
    // home for anything that doesn't earn a top-level slot). On mobile it also
    // absorbs everything that doesn't fit the collapsed toolbar — Delete,
    // Assets/Undo/Redo, and the File menu's items — since Add and Preview stay
    // as visible icon-only buttons.
    let overflowItems = $derived.by((): DropdownMenuItem[] => {
        const links: DropdownMenuItem[] = [
            { label: t("toolbar.discord"), action: () => openLink(DISCORD_URL), icon: DiscordIcon },
            { label: t("toolbar.github"), action: () => openLink(GITHUB_URL), icon: GithubIcon },
            { label: t("common.settings"), action: () => settingsModalOpen.set(true), icon: SettingsIcon },
        ];
        if (!isNarrow.current) return links;

        const fileItems = fileMenuItems;
        if (fileItems.length > 0) fileItems[0] = { ...fileItems[0], divider: true };
        links[0] = { ...links[0], divider: true };

        return [
            { label: t("common.delete"), action: () => selectedNode && deleteElement(selectedNode.key), icon: Trash2, disabled: $codeViewPanelOpen || !canDelete },
            { label: t("toolbar.manageAssets"), action: () => assetManagerOpen.set(true), icon: ImageIcon, divider: true },
            { label: $hasActiveCmView ? t("toolbar.undoInCodeEditor") : t("toolbar.undo"), action: handleUndo, icon: Undo2, disabled: $hasActiveCmView ? false : !$canUndo },
            { label: $hasActiveCmView ? t("toolbar.redoInCodeEditor") : t("toolbar.redo"), action: handleRedo, icon: Redo2, disabled: $hasActiveCmView ? false : !$canRedo },
            { label: t("toolbar.rawXmlCodeView"), action: handleToggleCodeView, icon: FileCode },
            ...fileItems,
            ...links,
        ];
    });
</script>

<AppBar>
    <AppBar.Toolbar class="grid-cols-[auto_1fr_auto]">
        <AppBar.Lead class="flex-1 min-w-0">
            <div class="flex items-center min-w-0">
                {#if $isLoaded}
                    <button
                        type="button"
                        class="toolbar-icon-btn shrink-0"
                        onclick={() => navigateBack()}
                        disabled={!$canGoBack}
                        title={t("toolbar.back")}
                    ><ArrowLeft size={16} /></button>
                    <button
                        type="button"
                        class="toolbar-icon-btn mr-2 shrink-0"
                        onclick={() => navigateForward()}
                        disabled={!$canGoForward}
                        title={t("toolbar.forward")}
                    ><ArrowRight size={16} /></button>
                {/if}
                {#if showHome}
                    <button
                        type="button"
                        class="toolbar-icon-btn mr-2 shrink-0"
                        onclick={handleHome}
                        title={t("toolbar.backToHome")}
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
                    ><TriangleAlert size={13} /> <span class="hidden md:inline">{t("toolbar.saveFailedRetry")}</span></button>
                {:else if $isSaving}
                    <span class="save-chip save-chip-saving shrink-0"><LoaderCircle size={13} class="animate-spin" /> <span class="hidden md:inline">{t("toolbar.saving")}</span></span>
                {:else if $isDirty || $isEditingField}
                    <button
                        type="button"
                        class="save-chip save-chip-unsaved shrink-0"
                        onclick={handleSaveNow}
                        title={t("toolbar.saveNow")}
                    ><Circle size={8} fill="currentColor" /> <span class="hidden md:inline">{t("toolbar.unsaved")}</span></button>
                {:else if $gameFilename}
                    <span class="save-chip save-chip-saved shrink-0"><Check size={13} /> <span class="hidden md:inline">{t("toolbar.saved")}</span></span>
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
                            title={t("toolbar.addElement")}
                        ><Plus size={14} /> <span class="hidden md:inline">{t("toolbar.add")}</span> <ChevronDown size={12} class="hidden md:inline" /></button>
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
                    title={canDelete ? t("toolbar.deleteTitleNamed", { name: selectedNode?.text ?? "" }) : t("toolbar.deleteTitle")}
                ><Trash2 size={14} /> {t("common.delete")}</button>
                <div class="toolbar-divider hidden md:block"></div>
                <button type="button" class="toolbar-icon-btn !hidden md:!inline-flex" onclick={() => assetManagerOpen.set(true)} title={t("toolbar.manageAssets")}><ImageIcon size={16} /></button>
                <button type="button" class="toolbar-icon-btn !hidden md:!inline-flex" onclick={handleUndo} disabled={$hasActiveCmView ? false : !$canUndo} title={$hasActiveCmView ? t("toolbar.undoInCodeEditor") : t("toolbar.undo")}><Undo2 size={16} /></button>
                <button type="button" class="toolbar-icon-btn !hidden md:!inline-flex" onclick={handleRedo} disabled={$hasActiveCmView ? false : !$canRedo} title={$hasActiveCmView ? t("toolbar.redoInCodeEditor") : t("toolbar.redo")}><Redo2 size={16} /></button>
                <button type="button" class="toolbar-icon-btn !hidden md:!inline-flex" onclick={handleToggleCodeView} title={t("toolbar.rawXmlCodeView")}><FileCode size={16} /></button>
                <div class="toolbar-divider hidden md:block"></div>
                {#if fileMenuItems.length > 0}
                    <div class="hidden md:block">
                        <DropdownMenu items={fileMenuItems}>
                            {#snippet trigger(toggle)}
                                <button type="button" class="btn btn-sm preset-outlined-primary-500" onclick={toggle} disabled={saving} title={t("toolbar.file")}
                                >{t("toolbar.file")} <ChevronDown size={12} /></button>
                            {/snippet}
                        </DropdownMenu>
                    </div>
                {/if}
                {#if $gameFilename}
                    <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handlePreview} title={t("toolbar.previewGame")}><Play size={14} /> <span class="hidden md:inline">{t("toolbar.preview")}</span></button>
                {/if}
                <!-- Overflow menu: community links + Settings on desktop; also Delete/Assets/
                     Undo/Redo/File-menu items on mobile (see overflowItems) -->
                <DropdownMenu items={overflowItems}>
                    {#snippet trigger(toggle)}
                        <button type="button" class="toolbar-icon-btn" onclick={toggle} title={t("toolbar.more")}><Ellipsis size={16} /></button>
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
        color: var(--color-surface-600-400);
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
        color: var(--color-surface-600-400);
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

