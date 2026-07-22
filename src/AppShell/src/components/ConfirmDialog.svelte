<script lang="ts">
    import { confirmState } from "$lib/confirm";

    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { if ($confirmState) dialogEl?.focus(); });

    function respond(result: boolean) {
        $confirmState?.resolve(result);
        confirmState.set(null);
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") respond(false);
        if (e.key === "Enter") respond(true);
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) respond(false);
    }
</script>

{#if $confirmState}
    {@const state = $confirmState}
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
                <button class="btn btn-sm preset-tonal" onclick={() => respond(false)}>{state.cancelLabel}</button>
                <button
                    class={`btn btn-sm ${state.danger ? "preset-filled-error-500" : "preset-filled-primary-500"}`}
                    onclick={() => respond(true)}
                >{state.confirmLabel}</button>
            </div>
        </div>
    </div>
{/if}
