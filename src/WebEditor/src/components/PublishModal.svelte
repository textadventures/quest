<script lang="ts">
    import { publishGame, canPublishToServer } from "$lib/editor-store";

    interface Props {
        oncancel: () => void;
    }
    const { oncancel }: Props = $props();

    let includeWalkthrough = $state(false);
    let publishing = $state(false);
    let error = $state("");

    async function handlePublish() {
        publishing = true;
        error = "";
        try {
            await publishGame(includeWalkthrough);
            oncancel();
        } catch (err) {
            error = String(err);
        } finally {
            publishing = false;
        }
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") oncancel();
    }
</script>

<div
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50"
    onclick={onBackdropClick}
    onkeydown={handleKeydown}
>
    <div class="card bg-white rounded-xl shadow-xl w-[28rem] p-6 flex flex-col gap-4">
        <div class="flex items-center justify-between">
            <h2 class="text-base font-semibold">Publish</h2>
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>Close</button>
        </div>

        <p class="text-sm text-surface-500-400">
            {#if $canPublishToServer}
                Builds a <code>.quest</code> package (game file plus assets) and submits it to
                textadventures.co.uk, where you'll fill in title, description, category and visibility.
            {:else}
                Builds a <code>.quest</code> package (game file plus assets) and downloads it. Use the
                site's manual "submit a game" page to publish it on textadventures.co.uk.
            {/if}
        </p>

        <label class="flex items-center gap-2 text-sm">
            <input type="checkbox" bind:checked={includeWalkthrough} />
            Include walkthrough
        </label>

        {#if error}<p class="text-xs text-error-500">{error}</p>{/if}

        <button
            type="button"
            class="btn btn-sm preset-filled-primary-500 self-start"
            onclick={handlePublish}
            disabled={publishing}
        >{publishing ? "Publishing…" : $canPublishToServer ? "Publish to textadventures.co.uk…" : "Publish…"}</button>
    </div>
</div>
