<script lang="ts">
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { openGame, loadingStatus } from "$lib/editor-store";
    import { loadLocalFile } from "$lib/filesystem/browser-adapter";

    let loading = $state(false);
    let error = $state<string | null>(null);

    async function handleOpen() {
        error = null;
        try {
            const loaded = await loadLocalFile();
            if (!loaded) return;
            loading = true;
            const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
            if (ok) {
                goto(base || "/");
                return;
            }
            error = "Failed to load game file.";
        } catch (err) {
            error = String(err);
        }
        loading = false;
    }
</script>

<main class="flex flex-col items-center justify-center min-h-svh gap-6 p-8">
    <h1 class="text-3xl font-semibold">Quest Viva Editor</h1>
    <p class="text-surface-500-400">Open an <code>.aslx</code> game file to begin editing.</p>

    {#if loading}
        <div class="flex flex-col items-center gap-3">
            <div class="size-10 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
            <p class="text-surface-500-400 text-sm">{$loadingStatus}</p>
        </div>
    {:else}
        <button type="button" class="btn preset-filled-primary-500" onclick={handleOpen}>
            Open game file
        </button>
    {/if}

    {#if error}
        <p class="text-error-500 max-w-[40ch] text-center">{error}</p>
    {/if}
</main>
