<script lang="ts">
    import { toasts, dismissToast } from "$lib/toast";
</script>

<div class="fixed bottom-4 right-4 z-[60] flex flex-col gap-2 items-end pointer-events-none">
    {#each $toasts as toast (toast.id)}
        <div
            role="alert"
            class={"pointer-events-auto shadow-lg rounded-lg px-4 py-2 text-sm max-w-sm flex items-start gap-2 " + (toast.kind === "error" ? "preset-filled-error-500" : "preset-filled-surface-500")}
        >
            <span class="flex-1">{toast.text}</span>
            <button
                type="button"
                class="opacity-80 hover:opacity-100 leading-none"
                aria-label="Dismiss"
                onclick={() => dismissToast(toast.id)}
            >✕</button>
        </div>
    {/each}
</div>
