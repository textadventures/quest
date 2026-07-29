<script lang="ts">
    import { untrack } from "svelte";
    import { getGameXml, setGameXml, codeViewCloseRequested } from "$lib/editor-store";
    import { chooseDialog, confirmDialog } from "$lib/confirm";
    import CodeEditor from "./CodeEditor.svelte";

    interface Props {
        onclose: () => void;
    }

    let { onclose }: Props = $props();

    // Loaded once on open, not kept in sync with scriptVersion/undo-redo — re-syncing while the
    // panel is open would fight whatever raw XML the user is mid-edit on.
    const originalXml = getGameXml();
    let xml = $state(originalXml);
    let applying = $state(false);
    let error = $state<string | null>(null);
    let hasChanges = $derived(xml !== originalXml);

    function handleChange(value: string) {
        xml = value;
        error = null;
    }

    // Applies without its own confirmation — callers (the Apply button, and the "Apply" choice
    // from the unsaved-changes prompt below) are each responsible for confirming first, so a user
    // routed through the prompt doesn't see a second "are you sure" right after choosing Apply.
    async function applyChanges(): Promise<boolean> {
        applying = true;
        error = null;
        try {
            const result = await setGameXml(xml);
            if (result === "ok") return true;
            // Keep the panel open with the user's text intact so they can fix and retry —
            // SetGameXml() never touched the live game on a failure like this.
            error = result.startsWith("error:") ? result.slice("error:".length) : result;
            return false;
        } finally {
            applying = false;
        }
    }

    async function handleApplyButton() {
        const confirmed = await confirmDialog(
            "Applying reloads the whole game from this text and discards undo history. Continue?",
            { confirmLabel: "Apply", danger: true }
        );
        if (!confirmed) return;
        if (await applyChanges()) onclose();
    }

    // Shared by the Cancel button and the toolbar's toggle-to-close button (see the
    // codeViewCloseRequested effect below) — asks before throwing away unapplied edits either way,
    // rather than only guarding one of the two ways to leave this panel.
    async function attemptClose() {
        if (!hasChanges) {
            onclose();
            return;
        }
        const choice = await chooseDialog(
            "You have unsaved raw XML changes. Applying reloads the game and discards undo history.",
            [
                { label: "Keep editing", value: "keep" as const },
                { label: "Discard changes", value: "discard" as const },
                { label: "Apply changes", value: "apply" as const },
            ]
        );
        if (choice === "discard") {
            onclose();
        } else if (choice === "apply") {
            if (await applyChanges()) onclose();
        }
    // Any other outcome ("keep", or Escape/backdrop which resolves null) — stay open.
    }

    // Reacts to the toolbar's toggle button being clicked while this panel is already open. Only
    // treated as a genuine request when the counter changes *after* this component mounted —
    // otherwise every fresh mount would immediately see a nonzero leftover count from some earlier
    // session and try to close itself right away.
    let lastCloseRequest = untrack(() => $codeViewCloseRequested);
    $effect(() => {
        if ($codeViewCloseRequested !== lastCloseRequest) {
            lastCloseRequest = $codeViewCloseRequested;
            void attemptClose();
        }
    });
</script>

{#if applying}
    <!-- Matches the app's main "Loading game…" treatment (edit/+page.svelte) rather than a small
         in-button spinner — reloading the whole model from raw XML is exactly that kind of
         operation, and deserves to be at least as obvious as the initial load. -->
    <main class="flex flex-col items-center justify-center flex-1 gap-6 p-8">
        <div class="size-10 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
        <p class="text-surface-600-400 text-sm">Applying changes…</p>
    </main>
{:else}
    <div class="flex flex-col flex-1 min-w-0 overflow-hidden">
        {#if error}
            <div class="px-4 py-2 bg-error-100-900 border-b border-error-300-700 text-sm text-error-600-400 whitespace-pre-wrap">{error}</div>
        {/if}

        <!-- data-staging: edits here are a local scratch buffer until Apply commits them — opt out
             of the page-level isEditingField/"Unsaved" tracking that assumes input events mean an
             edit already landed in the bridge, same as ListEditor/DictionaryEditor. -->
        <div class="flex-1 min-h-0 p-2" data-staging>
            <CodeEditor value={xml} language="xml" onChange={handleChange} class="h-full" />
        </div>

        <div class="flex items-center justify-end gap-2 px-4 py-2 border-t border-surface-200-800">
            <button type="button" class="btn btn-sm preset-tonal" onclick={attemptClose}>Cancel</button>
            <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handleApplyButton}>Apply</button>
        </div>
    </div>
{/if}
