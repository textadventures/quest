<script lang="ts">
    import { onMount } from "svelte";
    import { tick } from "svelte";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { page } from "$app/state";
    import { get } from "svelte/store";
    import { PUBLIC_SHOW_HOME } from "$env/static/public";
    import { isLoaded, isDirty, saveGame, loadingStatus, addElementModal, assetManagerOpen, publishModalOpen, openGame, createRoom, createObject, createFunction, createTimer, createWalkthrough, createTemplate, createDynamicTemplate, createObjectType } from "$lib/editor-store";
    import { loadFromServer } from "$lib/filesystem/server-adapter";
    import Toolbar from "$components/Toolbar.svelte";
    import BackupBanner from "$components/BackupBanner.svelte";
    import TreePanel from "$components/TreePanel.svelte";
    import PropertyEditor from "$components/PropertyEditor.svelte";
    import AddElementModal from "$components/AddElementModal.svelte";
    import AssetManagerModal from "$components/AssetManagerModal.svelte";
    import PublishModal from "$components/PublishModal.svelte";
    import PlayCatalog from "$components/PlayCatalog.svelte";

    const showHome = PUBLIC_SHOW_HOME === "true";

    // isLoaded deliberately never resets just because the user navigates away
    // from the editor (a refresh should resume your session) — but that means
    // it can't tell "arrived here with something loaded" apart from "clicked
    // Play/Back to Play while something happens to still be loaded in the
    // background". HomeTabs' Play link and /play/[id]'s "Back to Play" both
    // carry ?view=play specifically to disambiguate the latter.
    const forcePlayView = $derived(page.url.searchParams.get("view") === "play");

    let serverLoadError = $state<string | null>(null);

    // Best-effort: force any focused field to commit and flush it to storage
    // before the tab actually goes away. Can't be awaited here — the page may
    // already be gone by the time an async write settles — so the beforeunload
    // warning below stays as the real safety net for that narrow window.
    $effect(() => {
        if (!$isDirty) return;
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
            if (document.hidden && get(isDirty)) void saveGame();
        }
        document.addEventListener("visibilitychange", handleVisibilityChange);

        const gameId = new URLSearchParams(window.location.search).get("game");
        if (gameId) {
            loadGameFromServer(gameId);
        } else if (!get(isLoaded) && !showHome) {
            // When showHome is true, this state (nothing loaded, no ?game=) is
            // the Play tab, rendered below instead of redirected away from.
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
{:else if $isLoaded && !forcePlayView}
    <div class="flex flex-col h-svh overflow-hidden">
        <Toolbar />
        <BackupBanner />
        <div class="flex flex-1 overflow-hidden">
            <TreePanel />
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
{:else if showHome}
    <PlayCatalog />
{/if}
