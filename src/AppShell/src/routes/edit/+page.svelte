<script lang="ts">
    import { onMount } from "svelte";
    import { tick } from "svelte";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { get } from "svelte/store";
    import { isLoaded, isDirty, isEditingField, markFieldEditing, clearFieldEditing, saveGame, loadingStatus, addElementModal, addJavascriptModalOpen, addLibraryModalOpen, assetManagerOpen, publishModalOpen, codeViewPanelOpen, openGame, lastOpenGameError, lastFailedGameBytes, lastFailedGameFilename, createRoom, createObject, createPage, createFunction, createTimer, createWalkthrough, createTemplate, createDynamicTemplate, createObjectType, createJavascript, createIncludedLibrary, moveElementModal, moveElement, moveToFolderModal, setFunctionFolder } from "$lib/editor-store";
    import { loadFromServer } from "$lib/filesystem/server-adapter";
    import Toolbar from "$components/Toolbar.svelte";
    import BackupBanner from "$components/BackupBanner.svelte";
    import LibraryReloadBanner from "$components/LibraryReloadBanner.svelte";
    import FileChangedExternallyBanner from "$components/FileChangedExternallyBanner.svelte";
    import TreePanel from "$components/TreePanel.svelte";
    import PropertyEditor from "$components/PropertyEditor.svelte";
    import CodeViewPanel from "$components/CodeViewPanel.svelte";
    import SafeModeEditor from "$components/SafeModeEditor.svelte";
    import { isNarrow } from "$lib/layout.svelte";
    import AddElementModal from "$components/AddElementModal.svelte";
    import AddJavascriptModal from "$components/AddJavascriptModal.svelte";
    import AddLibraryModal from "$components/AddLibraryModal.svelte";
    import MoveElementModal from "$components/MoveElementModal.svelte";
    import MoveToFolderModal from "$components/MoveToFolderModal.svelte";
    import AssetManagerModal from "$components/AssetManagerModal.svelte";
    import PublishModal from "$components/PublishModal.svelte";
    import { t } from "$lib/i18n";

    let serverLoadError = $state<string | null>(null);

    // Tracks whether this page instance has ever reached a loaded game, so the
    // final fallback branch below can tell "mid-session reload/switch failed"
    // (isLoaded flips back to false — a real error, see that branch's comment)
    // apart from "haven't started loading yet" (isLoaded is still its initial
    // false and $loadingStatus hasn't been set by openGame() yet because
    // onMount/loadGameFromServer haven't run/resolved). Without this, that
    // initial tick rendered the same error UI as a genuine failure — a
    // half-second flash of "This game could not be loaded" on every open
    // while loadFromServer's network fetch was still in flight.
    let hasLoadedOnce = $state(false);
    $effect(() => {
        if ($isLoaded) hasLoadedOnce = true;
    });

    // Session-only (not persisted across reloads), same tradeoff as
    // WasmPlayer's debugger splitter: a panel width isn't worth the
    // complexity of persisting, but should survive for the life of the tab.
    let treeWidth = $state(240); // matches TreePanel's old fixed w-60
    const TREE_MIN_WIDTH = 180;
    const TREE_MAX_WIDTH = 600;

    // Mobile master-detail: tree is the full-screen home, selecting an element
    // pushes a full-screen properties view with a back button. Not reset when
    // isNarrow flips (e.g. rotating a tablet past the breakpoint) — resuming
    // on whichever pane was active is less surprising than snapping back to
    // the tree.
    let mobilePane = $state<"tree" | "props">("tree");

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
        } else if (!get(isLoaded) && !get(lastFailedGameBytes)) {
            // Nothing to edit and nothing to recover — /edit is only ever
            // reached with something already loaded (via /open), a ?game=
            // to load here, or a failed load's bytes to fix in Safe Mode
            // (see the template branch below).
            goto(`${base}/open`);
        }

        return () => document.removeEventListener("visibilitychange", handleVisibilityChange);
    });

    async function loadGameFromServer(gameId: string) {
        try {
            const loaded = await loadFromServer(gameId);
            const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
            if (!ok) serverLoadError = get(lastOpenGameError) ?? t("editPage.failedToLoadGame");
        } catch (err) {
            // Thrown before openGame() ever ran (e.g. an unknown game id) — no bytes exist to
            // recover into Safe Mode. Clear defensively in case a stale value is still sitting
            // in the store from an earlier failed attempt this session (see the template's
            // lastFailedGameBytes branch, which would otherwise take priority over this error).
            lastFailedGameBytes.set(null);
            lastFailedGameFilename.set(null);
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
        else if (mode.type === "page") createPage(name, mode.parent);
        else if (mode.type === "function") createFunction(name);
        else if (mode.type === "timer") createTimer(name);
        else if (mode.type === "walkthrough") createWalkthrough(name, mode.parent);
        else if (mode.type === "template") createTemplate(name);
        else if (mode.type === "dynamictemplate") createDynamicTemplate(name);
        else if (mode.type === "type") createObjectType(name);
    }
</script>

{#if $lastFailedGameBytes}
    <!-- Checked before $loadingStatus so a retry attempt from within Safe Mode (success or
         failure) never unmounts SafeModeEditor while it's running — openGame() sets loadingStatus
         for the duration of Initialise(), and this store isn't cleared until that call resolves,
         so it stays truthy (and this branch keeps matching) throughout. An unmount here would
         reset SafeModeEditor's per-file buffers, silently discarding any library fix the author
         hadn't clicked Save on yet. SafeModeEditor shows its own "applying" spinner internally for
         this same window instead. -->
    <SafeModeEditor />
{:else if $loadingStatus}
    <main class="flex flex-col items-center justify-center min-h-svh gap-6 p-8">
        <div class="size-10 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
        <p class="text-surface-600-400 text-sm">{$loadingStatus}</p>
    </main>
{:else if $isLoaded}
    <div class="flex flex-col h-dvh overflow-hidden safe-area-inset">
        <Toolbar />
        <BackupBanner />
        <LibraryReloadBanner />
        <FileChangedExternallyBanner />
        <div class="flex flex-1 overflow-hidden" oninput={handleFieldInput} onfocusout={clearFieldEditing}>
            {#if $codeViewPanelOpen}
                <CodeViewPanel onclose={() => codeViewPanelOpen.set(false)} />
            {:else}
                <!-- Both panes stay mounted (display toggled, not {#if}'d away) so tree
                     expansion state and property tab state survive switching panes on
                     mobile. -->
                <div class={isNarrow.current && mobilePane !== "tree" ? "hidden" : "contents"}>
                    <TreePanel
                        width={isNarrow.current ? undefined : treeWidth}
                        onactivate={() => { mobilePane = "props"; }}
                    />
                </div>
                {#if !isNarrow.current}
                    <div
                        role="separator"
                        aria-orientation="vertical"
                        class="w-1 shrink-0 cursor-col-resize hover:bg-primary-500/50 active:bg-primary-500/70 transition-colors"
                        onpointerdown={handleSplitterPointerDown}
                    ></div>
                {/if}
                <div class={isNarrow.current && mobilePane !== "props" ? "hidden" : "contents"}>
                    <PropertyEditor onback={isNarrow.current ? () => { mobilePane = "tree"; } : undefined} />
                </div>
            {/if}
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

    {#if $addJavascriptModalOpen}
        <AddJavascriptModal
            onconfirm={(src) => { addJavascriptModalOpen.set(false); createJavascript(src); }}
            oncancel={() => addJavascriptModalOpen.set(false)}
        />
    {/if}

    {#if $addLibraryModalOpen}
        <AddLibraryModal
            onconfirm={(filename) => { addLibraryModalOpen.set(false); createIncludedLibrary(filename); }}
            oncancel={() => addLibraryModalOpen.set(false)}
        />
    {/if}

    {#if $moveElementModal}
        <MoveElementModal
            elementKey={$moveElementModal}
            onconfirm={(parent) => { moveElement($moveElementModal!, parent); moveElementModal.set(null); }}
            oncancel={() => moveElementModal.set(null)}
        />
    {/if}

    {#if $moveToFolderModal}
        <MoveToFolderModal
            elementKey={$moveToFolderModal}
            onconfirm={(folder) => { setFunctionFolder($moveToFolderModal!, folder); moveToFolderModal.set(null); }}
            oncancel={() => moveToFolderModal.set(null)}
        />
    {/if}

    {#if $assetManagerOpen}
        <AssetManagerModal oncancel={() => assetManagerOpen.set(false)} />
    {/if}

    {#if $publishModalOpen}
        <PublishModal oncancel={() => publishModalOpen.set(false)} />
    {/if}
{:else if serverLoadError}
    <!-- loadGameFromServer's own fetch/parse threw before any bytes ever reached openGame() (e.g.
         an unknown game id) — nothing to recover into Safe Mode, unlike the branch above. -->
    <main class="flex flex-col items-center justify-center min-h-svh gap-6 p-8">
        <p class="text-error-500 max-w-[40ch] text-center whitespace-pre-wrap">{serverLoadError}</p>
    </main>
{:else if hasLoadedOnce}
    <!-- Defensive fallback: shouldn't normally be reached now that a genuine openGame() failure
         is caught by the lastFailedGameBytes branch above, but kept in case isLoaded somehow
         flips false without that store being set. -->
    <main class="flex flex-col items-center justify-center min-h-svh gap-6 p-8">
        <p class="text-error-500 max-w-[40ch] text-center whitespace-pre-wrap">{$lastOpenGameError ?? t("editPage.loadError")}</p>
        <button type="button" class="btn preset-filled-primary-500" onclick={() => goto(`${base}/open`)}>{t("editPage.backToHome")}</button>
    </main>
{:else}
    <!-- The page hasn't reached any terminal state yet: onMount hasn't run/resolved far enough
         to set $loadingStatus (e.g. loadFromServer's network fetch is still in flight) or to
         redirect away. Show the same spinner shell as the $loadingStatus branch rather than
         falling through to the error UI above. -->
    <main class="flex flex-col items-center justify-center min-h-svh gap-6 p-8">
        <div class="size-10 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
        <p class="text-surface-600-400 text-sm">{t("editorStore.startingEditor")}</p>
    </main>
{/if}
