<script lang="ts">
    import { onMount } from "svelte";
    import { tick } from "svelte";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { get } from "svelte/store";
    import { isLoaded, loadingStatus, addElementModal, openGame, createRoom, createObject, createFunction, createTimer, createWalkthrough, createTemplate, createDynamicTemplate, createObjectType } from "$lib/editor-store";
    import { loadFromServer } from "$lib/filesystem/server-adapter";
    import Toolbar from "$components/Toolbar.svelte";
    import TreePanel from "$components/TreePanel.svelte";
    import PropertyEditor from "$components/PropertyEditor.svelte";
    import AddElementModal from "$components/AddElementModal.svelte";

    let serverLoadError = $state<string | null>(null);

    onMount(() => {
        const gameId = new URLSearchParams(window.location.search).get("game");
        if (gameId) {
            loadGameFromServer(gameId);
        } else if (!get(isLoaded)) {
            goto(`${base}/open`);
        }
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
    <div class="flex flex-col h-svh overflow-hidden">
        <Toolbar />
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
{/if}
