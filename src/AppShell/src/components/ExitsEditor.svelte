<script lang="ts">
    import { selectNode, deleteChildElement, deleteChildElements, swapElements, createExit, getExitsData, createExitInDirection, createLookExitInDirection, scriptVersion } from "$lib/editor-store";
    import { chooseDialog } from "$lib/confirm";
    import type { ExitsData, CompassDirectionInfo } from "$lib/types";
    import Combobox from "./Combobox.svelte";
    import Pencil from "@lucide/svelte/icons/pencil";

    interface Props {
        elementKey: string;
    }

    let { elementKey }: Props = $props();

    let data = $state<ExitsData | null>(null);
    let openDirection = $state<string | null>(null);
    let createTo = $state("");
    let createInverse = $state(true);
    let warning = $state<string | null>(null);

    // scriptVersion bumps on undo/redo (see editor-store.ts) — this control's exits data is
    // fetched separately from $selectedData, which refreshSelectedData() already handles on
    // undo/redo, so without tracking scriptVersion here an undo that recreates/removes an exit
    // wouldn't be reflected until switching tabs away and back (same pattern ScriptEditor.svelte
    // uses for its own separately-fetched state).
    $effect(() => {
        void $scriptVersion;
        refresh(elementKey);
    });

    function refresh(key: string) {
        data = getExitsData(key);
    }

    // Exits aren't stored as linked pairs — "also create the return exit" is a one-time
    // convenience at creation time, not a persisted relationship — so the best a "return exit"
    // can do is match by shape: the destination room having its own exit aliased to the inverse
    // direction that points back to this room. Returns null for non-directional aliases (no
    // inverse to look for) and for look exits (one-way by definition, no return exit possible).
    function findReciprocalExit(alias: string | null, to: string | null, lookOnly: boolean): string | null {
        if (!data || lookOnly || !alias || !to) return null;
        const inverseDirection = data.compass.find(d => d.direction === alias)?.inverseDirection;
        if (!inverseDirection) return null;
        const destData = getExitsData(to);
        const reciprocal = destData?.allExits.find(e => e.alias === inverseDirection && e.to === elementKey && !e.lookOnly);
        return reciprocal?.key ?? null;
    }

    // deleteChildElement()/deleteChildElements() leave the current selection alone (unlike
    // deleteElement(), which always clears it) — the exit being deleted here is always a child of
    // elementKey (the room), never the room itself, so there's nothing to reselect, and the room's
    // own tabs/attributes don't depend on which exits it has.
    async function deleteExit(exitKey: string, alias: string | null, to: string | null, lookOnly: boolean) {
        const reciprocalKey = findReciprocalExit(alias, to, lookOnly);
        if (reciprocalKey) {
            const choice = await chooseDialog(`"${to}" has a matching return exit back to this room. Delete that one too?`, [
                { label: "Cancel", value: "cancel" as const },
                { label: "Just this one", value: "this" as const },
                { label: "Delete both", value: "both" as const, danger: true },
            ]);
            if (choice === null || choice === "cancel") return;
            // One call, one transaction — deleting each separately would open/close its own
            // transaction, so a single Undo would only bring back the last of the two.
            if (choice === "both") {
                deleteChildElements([reciprocalKey, exitKey]);
                refresh(elementKey);
                return;
            }
        }
        deleteChildElement(exitKey);
        refresh(elementKey);
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
        <div class="border border-surface-200-800 rounded p-1.5 bg-surface-50-950 min-h-14 min-w-0 flex flex-col gap-0.5">
            <div class="flex items-center justify-between gap-1 min-w-0">
                <span class="text-[10px] uppercase text-surface-400-500 truncate">{dir.direction}</span>
                <button
                    type="button"
                    class="text-surface-400-500 hover:text-primary-600-400 flex-shrink-0"
                    title="Edit exit"
                    onclick={() => selectNode(dir.exitKey!)}
                ><Pencil size={11} /></button>
            </div>
            <div class="flex items-center gap-1 min-w-0">
                {#if dir.lookOnly}
                    <span class="flex-1 min-w-0 text-xs text-surface-500-400 truncate">(look)</span>
                {:else}
                    <button
                        type="button"
                        class="flex-1 min-w-0 text-left text-xs text-primary-600-400 hover:underline truncate"
                        title={dir.to ?? undefined}
                        onclick={() => selectNode(dir.to!)}
                    >→ {dir.to}</button>
                {/if}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-error-500 px-1 py-0 text-xs leading-none flex-shrink-0"
                    title="Delete"
                    onclick={() => void deleteExit(dir.exitKey!, dir.direction, dir.to, dir.lookOnly)}
                >×</button>
            </div>
        </div>
    {:else}
        {@const isOpen = openDirection === dir.direction}
        <button
            type="button"
            class="border rounded p-1.5 min-h-14 min-w-0 flex items-center text-left text-xs transition-colors
                {isOpen
                    ? "border-primary-500 bg-primary-50-950 text-primary-600-400"
                    : "border-dashed border-surface-300-700 text-surface-400-500 hover:border-primary-400 hover:text-primary-600-400"}"
            onclick={() => toggleDirection(dir.direction)}
        ><span class="truncate">{dir.direction}</span></button>
    {/if}
{/snippet}

<div class="flex flex-col w-full px-3 py-2 gap-3">
    {#if data}
        <div class="flex gap-4 w-full max-w-xl">
            <div class="grid grid-cols-3 gap-1.5 flex-[3] min-w-0">
                {#each compassGrid as idx (idx)}
                    {@render directionCell(idx === -1 ? null : data.compass[idx])}
                {/each}
            </div>
            <div class="grid grid-cols-2 gap-1.5 flex-[2] min-w-0">
                {#each directionGrid as idx (idx)}
                    {@render directionCell(data.compass[idx])}
                {/each}
            </div>
        </div>

        <!-- Rendered as a separate block below the grids (rather than expanding the clicked cell
             in place) so opening/closing it never changes the grid's own layout. -->
        {#if openDirection}
            <div class="border border-primary-300-700 rounded p-2.5 flex flex-col gap-1.5 bg-surface-50-950 w-full max-w-xl">
                <div class="flex items-center justify-between">
                    <span class="text-xs font-medium text-surface-600-400">Create exit: {openDirection}</span>
                    <button
                        type="button"
                        class="text-xs text-surface-400-500 hover:text-surface-600-400"
                        onclick={() => { openDirection = null; }}
                    >✕</button>
                </div>
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
                <div class="flex items-center gap-3">
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                        disabled={!createTo}
                        onclick={() => doCreate(openDirection!)}
                    >Create exit</button>
                    {#if data.allowLookExits}
                        <button
                            type="button"
                            class="text-xs text-surface-400-500 hover:text-primary-600-400 underline text-left"
                            onclick={() => doCreateLook(openDirection!)}
                        >Create a look exit instead</button>
                    {/if}
                </div>
                {#if warning}
                    <p class="text-xs text-warning-600-400">{warning}</p>
                {/if}
            </div>
        {/if}

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
                        {#if !exit.lookOnly && exit.to}
                            <button
                                type="button"
                                class="flex-1 text-left text-xs px-1.5 py-1 pr-28 text-primary-600-400 hover:underline truncate"
                                onclick={() => selectNode(exit.to!)}
                            >{exit.alias ?? "(none)"} → {exit.to}</button>
                        {:else}
                            <span class="flex-1 text-xs px-1.5 py-1 pr-28 text-surface-500-400 truncate">{exit.alias ?? "(none)"} → {exit.lookOnly ? "(look)" : "(nowhere)"}</span>
                        {/if}
                        <div class="absolute right-1 top-1/2 -translate-y-1/2 flex gap-0.5 opacity-0 pointer-events-none group-hover:opacity-100 group-hover:pointer-events-auto transition-opacity z-10">
                            <button
                                type="button"
                                class="btn btn-sm preset-outlined-primary-500 px-1 py-0 text-xs leading-none flex items-center"
                                title="Edit exit"
                                onclick={() => selectNode(exit.key)}
                            ><Pencil size={12} /></button>
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
                                onclick={() => void deleteExit(exit.key, exit.alias, exit.to, exit.lookOnly)}
                            >×</button>
                        </div>
                    </div>
                {/each}
            {/if}
        </div>
    {/if}
</div>
