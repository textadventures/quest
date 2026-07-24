<script lang="ts">
    import { onMount } from "svelte";
    import { tick } from "svelte";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { get } from "svelte/store";
    import { isLoaded, isDirty, isEditingField, markFieldEditing, clearFieldEditing, saveGame, loadingStatus, addElementModal, assetManagerOpen, publishModalOpen, openGame, createRoom, createObject, createFunction, createTimer, createWalkthrough, createTemplate, createDynamicTemplate, createObjectType } from "$lib/editor-store";
    import { loadFromServer } from "$lib/filesystem/server-adapter";
    import Toolbar from "$components/Toolbar.svelte";
    import BackupBanner from "$components/BackupBanner.svelte";
    import TreePanel from "$components/TreePanel.svelte";
    import PropertyEditor from "$components/PropertyEditor.svelte";
    import AddElementModal from "$components/AddElementModal.svelte";
    import AssetManagerModal from "$components/AssetManagerModal.svelte";
    import PublishModal from "$components/PublishModal.svelte";

    let serverLoadError = $state<string | null>(null);

    // Session-only (not persisted across reloads), same tradeoff as
    // WasmPlayer's debugger splitter: a panel width isn't worth the
    // complexity of persisting, but should survive for the life of the tab.
    let treeWidth = $state(240); // matches TreePanel's old fixed w-60
    const TREE_MIN_WIDTH = 180;
    const TREE_MAX_WIDTH = 600;

    function handleSplitterPointerDown(e: PointerEvent) {
        e.preventDefault();
        const startX = e.clientX;
        const startWidth = treeWidth;
        document.body.style.cursor = "col-resize";
        document.body.style.userSelect = "none";

        function onMove(moveEvent: PointerEvent) {
            const next = startWidth + (moveEvent.clientX - startX);
            treeWidth = Math.min(TREE_MAX_WIDTH, Math.max(TREE_MIN_WIDTH, next));
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

    // Delegated at the panel level so every field-editing component (property
    // panel, script editor, nested dictionary/list editors, ...) is covered
    // without each one having to wire this up itself. "input"/"focusout" both
    // bubble, unlike "change"/"blur". Elements the user is still just typing
    // into to filter/stage something — not committing a value yet, e.g. the
    // "add new item" boxes in ListEditor/DictionaryEditor — opt out via
    // data-staging so the pill/unload guard don't treat searching as editing.
    function handleFieldInput(e: Event) {
        const target = e.target as HTMLElement;
        if (target.closest("[data-staging]")) return;
        markFieldEditing(target);
    }

    // Best-effort: force any focused field to commit and flush it to storage
    // before the tab actually goes away. saveGame() itself blurs the focused
    // field first (flushing a pending edit into the bridge via its change
    // event) before checking isDirty, so isEditingField only needs to get the
    // listener attached — the actual flush happens inside saveGame(). Can't be
    // awaited here — the page may already be gone by the time an async write
    // settles — so the beforeunload warning below stays as the real safety net
    // for that narrow window.
    $effect(() => {
        if (!$isDirty && !$isEditingField) return;
        function handleBeforeUnload(e: BeforeUnloadEvent) {
            void saveGame();
            e.preventDefault();
        }
        window.addEventListener("beforeunload", handleBeforeUnload);
        return () => window.removeEventListener("beforeunload", handleBeforeUnload);
    });

    onMount(() => {
        // Covers tab switches, backgrounding, and the mobile-Safari-style tab
        // closes that don't reliably fire beforeunload.
        function handleVisibilityChange() {
            if (document.hidden && (get(isDirty) || get(isEditingField))) void saveGame();
        }
        document.addEventListener("visibilitychange", handleVisibilityChange);

        const gameId = new URLSearchParams(window.location.search).get("game");
        if (gameId) {
            loadGameFromServer(gameId);
        } else if (!get(isLoaded)) {
            // Nothing to edit — /edit is only ever reached with something
            // already loaded (via /open) or a ?game= to load here.
            goto(`${base}/open`);
        }

        return () => document.removeEventListener("visibilitychange", handleVisibilityChange);
    });

    async function loadGameFromServer(gameId: string) {
        try {
            const loaded = await loadFromServer(gameId);
            const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
            if (!ok) serverLoadError = "Failed to load game.";
        } catch (err) {
            serverLoadError = String(err);
        }
    }

    async function handleAddConfirm(name: string) {
        const mode = get(addElementModal);
        addElementModal.set(null);
        await tick();
        if (!mode) return;
        if (mode.type === "room") createRoom(name, mode.parent);
        else if (mode.type === "object") createObject(name, mode.parent);
        else if (mode.type === "function") createFunction(name);
        else if (mode.type === "timer") createTimer(name);
        else if (mode.type === "walkthrough") createWalkthrough(name);
        else if (mode.type === "template") createTemplate(name);
        else if (mode.type === "dynamictemplate") createDynamicTemplate(name);
        else if (mode.type === "type") createObjectType(name);
    }
</script>

{#if serverLoadError}
    <main class="flex flex-col items-center justify-center min-h-svh gap-6 p-8">
        <p class="text-error-500 max-w-[40ch] text-center">{serverLoadError}</p>
    </main>
{:else if $loadingStatus}
    <main class="flex flex-col items-center justify-center min-h-svh gap-6 p-8">
        <div class="size-10 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
        <p class="text-surface-500-400 text-sm">{$loadingStatus}</p>
    </main>
{:else if $isLoaded}
    <div class="flex flex-col h-dvh overflow-hidden">
        <Toolbar />
        <BackupBanner />
        <div class="flex flex-1 overflow-hidden" oninput={handleFieldInput} onfocusout={clearFieldEditing}>
            <TreePanel width={treeWidth} />
            <div
                role="separator"
                aria-orientation="vertical"
                class="w-1 shrink-0 cursor-col-resize hover:bg-primary-500/50 active:bg-primary-500/70 transition-colors"
                onpointerdown={handleSplitterPointerDown}
            ></div>
            <PropertyEditor />
        </div>
    </div>

    {#if $addElementModal}
        <AddElementModal
            elementType={$addElementModal.type}
            parent={$addElementModal.parent}
            onconfirm={handleAddConfirm}
            oncancel={() => addElementModal.set(null)}
        />
    {/if}

    {#if $assetManagerOpen}
        <AssetManagerModal oncancel={() => assetManagerOpen.set(false)} />
    {/if}

    {#if $publishModalOpen}
        <PublishModal oncancel={() => publishModalOpen.set(false)} />
    {/if}
{/if}
