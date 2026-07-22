<script lang="ts">
    import { selectNode, deleteElement, swapElements, createExit, getExitsData, createExitInDirection, createLookExitInDirection } from "$lib/editor-store";
    import type { ExitsData, CompassDirectionInfo } from "$lib/types";
    import Combobox from "./Combobox.svelte";

    interface Props {
        elementKey: string;
    }

    let { elementKey }: Props = $props();

    let data = $state<ExitsData | null>(null);
    let openDirection = $state<string | null>(null);
    let createTo = $state("");
    let createInverse = $state(true);
    let warning = $state<string | null>(null);

    $effect(() => {
        refresh(elementKey);
    });

    // Takes an explicit key rather than always reading the elementKey prop — deleteExit() below
    // needs to keep operating on the room's key even after deleteElement() has (briefly) unmounted
    // this component, at which point the live elementKey prop is no longer reliable.
    function refresh(key: string) {
        data = getExitsData(key);
    }

    // The deleted exit is always a child of elementKey (the currently-selected room), never the
    // room itself — but deleteElement() unconditionally clears the current selection, which
    // unmounts this component (PropertyEditor only renders it while $selectedKey is truthy). Capture
    // the room key first so selectNode()/refresh() below don't run against a torn-down prop.
    function deleteExit(exitKey: string) {
        const roomKey = elementKey;
        deleteElement(exitKey);
        void selectNode(roomKey);
        refresh(roomKey);
    }

    function moveUp(index: number) {
        if (!data || index <= 0) return;
        swapElements(data.allExits[index].key, data.allExits[index - 1].key);
        refresh(elementKey);
    }

    function moveDown(index: number) {
        if (!data || index >= data.allExits.length - 1) return;
        swapElements(data.allExits[index].key, data.allExits[index + 1].key);
        refresh(elementKey);
    }

    function toggleDirection(direction: string) {
        if (openDirection === direction) {
            openDirection = null;
        } else {
            openDirection = direction;
            createTo = "";
            createInverse = true;
            warning = null;
        }
    }

    function doCreate(direction: string) {
        if (!createTo) return;
        const result = createExitInDirection(elementKey, direction, createTo, createInverse);
        if (result.startsWith("error:")) {
            warning = result.slice("error:".length);
            return;
        }
        if (result.startsWith("warning:")) {
            warning = result.slice("warning:".length).split("|")[0];
        } else {
            openDirection = null;
        }
        refresh(elementKey);
    }

    function doCreateLook(direction: string) {
        const result = createLookExitInDirection(elementKey, direction);
        if (!result.startsWith("error:")) {
            openDirection = null;
        }
        refresh(elementKey);
    }

    function addExit() {
        createExit(elementKey);
    }

    const compassGrid = [0, 1, 2, 3, -1, 4, 5, 6, 7];
    const directionGrid = [8, 10, 9, 11];
</script>

{#snippet directionCell(dir: CompassDirectionInfo | null | undefined)}
    {#if !dir}
        <div></div>
    {:else if dir.exitKey !== null}
        <div class="border border-surface-200-800 rounded p-1.5 bg-surface-50-950 min-h-14 flex flex-col gap-0.5">
            <span class="text-[10px] uppercase text-surface-400-500">{dir.direction}</span>
            <div class="flex items-center gap-1">
                <button
                    type="button"
                    class="flex-1 text-left text-xs text-primary-600-400 hover:underline truncate"
                    onclick={() => selectNode(dir.exitKey!)}
                >{dir.lookOnly ? "(look)" : `→ ${dir.to}`}</button>
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-error-500 px-1 py-0 text-xs leading-none flex-shrink-0"
                    title="Delete"
                    onclick={() => deleteExit(dir.exitKey!)}
                >×</button>
            </div>
        </div>
    {:else}
        <div class="border border-dashed border-surface-300-700 rounded p-1.5 min-h-14 flex flex-col gap-1">
            <button
                type="button"
                class="text-xs text-surface-400-500 hover:text-primary-600-400 text-left"
                onclick={() => toggleDirection(dir.direction)}
            >+ {dir.direction}</button>
            {#if openDirection === dir.direction && data}
                <div class="flex flex-col gap-1 mt-1">
                    <Combobox
                        value={createTo}
                        options={data.objects}
                        onchange={(v) => { createTo = v; }}
                        class="input text-xs py-0.5 px-1.5 w-full"
                    />
                    <label class="flex items-center gap-1.5 text-xs cursor-pointer">
                        <input type="checkbox" class="checkbox size-3.5" bind:checked={createInverse} />
                        Also create the return exit
                    </label>
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                        disabled={!createTo}
                        onclick={() => doCreate(dir.direction)}
                    >Create exit</button>
                    {#if data.allowLookExits}
                        <button
                            type="button"
                            class="text-xs text-surface-400-500 hover:text-primary-600-400 underline text-left"
                            onclick={() => doCreateLook(dir.direction)}
                        >Create a look exit instead</button>
                    {/if}
                    {#if warning}
                        <p class="text-xs text-warning-600-400">{warning}</p>
                    {/if}
                </div>
            {/if}
        </div>
    {/if}
{/snippet}

<div class="flex flex-col w-full px-3 py-2 gap-3">
    {#if data}
        <div class="flex gap-4">
            <div class="grid grid-cols-3 gap-1 w-48">
                {#each compassGrid as idx (idx)}
                    {@render directionCell(idx === -1 ? null : data.compass[idx])}
                {/each}
            </div>
            <div class="grid grid-cols-2 gap-1 w-32">
                {#each directionGrid as idx (idx)}
                    {@render directionCell(data.compass[idx])}
                {/each}
            </div>
        </div>

        <div class="flex flex-col">
            <div class="flex items-center gap-1 mb-2">
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    onclick={addExit}
                >+ Add Exit</button>
            </div>

            {#if data.allExits.length === 0}
                <p class="text-xs text-surface-400-500 italic">No exits.</p>
            {:else}
                {#each data.allExits as exit, i (exit.key)}
                    <div class="group relative border border-surface-200-800 rounded mb-1 bg-surface-50-950 flex items-center">
                        <button
                            type="button"
                            class="flex-1 text-left text-xs px-1.5 py-1 pr-20 text-primary-600-400 hover:underline truncate"
                            onclick={() => selectNode(exit.key)}
                        >{exit.alias ?? "(none)"} → {exit.lookOnly ? "(look)" : (exit.to ?? "(nowhere)")}</button>
                        <div class="absolute right-1 top-1/2 -translate-y-1/2 flex gap-0.5 opacity-0 pointer-events-none group-hover:opacity-100 group-hover:pointer-events-auto transition-opacity z-10">
                            <button
                                type="button"
                                class="btn btn-sm preset-outlined-primary-500 px-1 py-0 text-xs leading-none"
                                title="Move up"
                                disabled={i === 0}
                                onclick={() => moveUp(i)}
                            >↑</button>
                            <button
                                type="button"
                                class="btn btn-sm preset-outlined-primary-500 px-1 py-0 text-xs leading-none"
                                title="Move down"
                                disabled={i === data.allExits.length - 1}
                                onclick={() => moveDown(i)}
                            >↓</button>
                            <button
                                type="button"
                                class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none"
                                title="Delete"
                                onclick={() => deleteExit(exit.key)}
                            >×</button>
                        </div>
                    </div>
                {/each}
            {/if}
        </div>
    {/if}
</div>
