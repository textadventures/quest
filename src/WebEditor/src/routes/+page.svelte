<script lang="ts">
    import { onMount } from "svelte";
    import { tick } from "svelte";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { get } from "svelte/store";
    import { isLoaded, addElementModal, createRoom, createObject, createFunction, createTimer, createWalkthrough, createTemplate, createDynamicTemplate, createObjectType } from "$lib/editor-store";
    import Toolbar from "$components/Toolbar.svelte";
    import TreePanel from "$components/TreePanel.svelte";
    import PropertyEditor from "$components/PropertyEditor.svelte";
    import AddElementModal from "$components/AddElementModal.svelte";

    onMount(() => {
        if (!get(isLoaded)) {
            goto(`${base}/open`);
        }
    });

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

{#if $isLoaded}
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
