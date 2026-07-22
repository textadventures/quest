<script lang="ts">
    import { dialogState } from "$lib/confirm";

    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { if ($dialogState) dialogEl?.focus(); });

    function respond(result: unknown) {
        $dialogState?.resolve(result);
        dialogState.set(null);
    }

    // The last choice is treated as the primary/default action (filled button, rightmost) —
    // matches how the 2-choice confirmDialog() wrapper orders [cancel, confirm].
    function handleKeydown(e: KeyboardEvent) {
        if (!$dialogState) return;
        if (e.key === "Escape") respond(null);
        if (e.key === "Enter") respond($dialogState.choices.at(-1)?.value);
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) respond(null);
    }
</script>

{#if $dialogState}
    {@const state = $dialogState}
    <div
        bind:this={dialogEl}
        role="dialog"
        aria-modal="true"
        tabindex="-1"
        class="fixed inset-0 bg-black/30 flex items-center justify-center z-50"
        onclick={onBackdropClick}
        onkeydown={handleKeydown}
    >
        <div class="card bg-white rounded-xl shadow-xl w-80 p-6 flex flex-col gap-4">
            <p class="text-sm">{state.message}</p>

            <div class="flex justify-end gap-2">
                {#each state.choices as choice, i (choice.label)}
                    {@const isPrimary = i === state.choices.length - 1}
                    <button
                        class={`btn btn-sm ${isPrimary ? (choice.danger ? "preset-filled-error-500" : "preset-filled-primary-500") : "preset-tonal"}`}
                        onclick={() => respond(choice.value)}
                    >{choice.label}</button>
                {/each}
            </div>
        </div>
    </div>
{/if}
