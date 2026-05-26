<script lang="ts">
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { openGame, loadingStatus } from "$lib/editor-store";
    import { hasFSA, openDirectory, loadFileFromDirectory, loadLocalFile } from "$lib/filesystem/browser-adapter";

    let loading = $state(false);
    let error = $state<string | null>(null);

    // Set when a directory was opened but contained multiple .aslx files
    let pendingDir = $state<FileSystemDirectoryHandle | null>(null);
    let pendingFiles = $state<string[]>([]);

    async function handleOpenFolder() {
        error = null;
        try {
            const result = await openDirectory();
            if (!result) return;
            if (result.files.length === 0) {
                error = "No .aslx files found in this folder.";
                return;
            }
            if (result.files.length === 1) {
                await loadFrom(result.dir, result.files[0]);
            } else {
                pendingDir = result.dir;
                pendingFiles = result.files.sort();
            }
        } catch (err) {
            error = String(err);
        }
    }

    async function handlePickFile(filename: string) {
        if (!pendingDir) return;
        const dir = pendingDir;
        pendingDir = null;
        pendingFiles = [];
        await loadFrom(dir, filename);
    }

    async function loadFrom(dir: FileSystemDirectoryHandle, filename: string) {
        loading = true;
        error = null;
        try {
            const loaded = await loadFileFromDirectory(dir, filename);
            const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
            if (ok) { goto(base || "/"); return; }
            error = "Failed to load game file.";
        } catch (err) {
            error = String(err);
        }
        loading = false;
    }

    async function handleOpenFile() {
        error = null;
        try {
            const loaded = await loadLocalFile();
            if (!loaded) return;
            loading = true;
            const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
            if (ok) { goto(base || "/"); return; }
            error = "Failed to load game file.";
        } catch (err) {
            error = String(err);
        }
        loading = false;
    }
</script>

<main class="flex flex-col items-center justify-center min-h-svh gap-6 p-8">
    <h1 class="text-3xl font-semibold">Quest Viva Editor</h1>

    {#if loading}
        <div class="flex flex-col items-center gap-3">
            <div class="size-10 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
            <p class="text-surface-500-400 text-sm">{$loadingStatus}</p>
        </div>
    {:else if pendingFiles.length > 0}
        <p class="text-surface-500-400">Multiple game files found — choose one to open:</p>
        <div class="flex flex-col gap-2 w-full max-w-sm">
            {#each pendingFiles as file (file)}
                <button
                    type="button"
                    class="btn preset-outlined-primary-500 w-full"
                    onclick={() => handlePickFile(file)}
                >{file}</button>
            {/each}
            <button
                type="button"
                class="btn preset-outlined-surface-500 w-full"
                onclick={() => { pendingDir = null; pendingFiles = []; }}
            >Cancel</button>
        </div>
    {:else if hasFSA()}
        <p class="text-surface-500-400">Open a game folder to begin editing.</p>
        <button type="button" class="btn preset-filled-primary-500" onclick={handleOpenFolder}>
            Open game folder
        </button>
    {:else}
        <p class="text-surface-500-400">Open an <code>.aslx</code> game file to begin editing.</p>
        <button type="button" class="btn preset-filled-primary-500" onclick={handleOpenFile}>
            Open game file
        </button>
        <p class="text-sm text-surface-500-400 max-w-[40ch] text-center">
            For full support including image assets, use Chrome, Edge, or Safari 15.2+.
        </p>
    {/if}

    {#if error}
        <p class="text-error-500 max-w-[40ch] text-center">{error}</p>
    {/if}
</main>
