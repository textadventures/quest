<script lang="ts">
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { openGame, loadingStatus } from "$lib/editor-store";
    import { hasFSA, openDirectory, loadFileFromDirectory, loadLocalFile } from "$lib/filesystem/browser-adapter";
    import { createNewGame, loadFromServer, LANGUAGES } from "$lib/filesystem/server-adapter";

    let loading = $state(false);
    let error = $state<string | null>(null);

    // Set when a directory was opened but contained multiple .aslx files
    let pendingDir = $state<FileSystemDirectoryHandle | null>(null);
    let pendingFiles = $state<string[]>([]);

    // Create new game form
    let createName = $state("");
    let createType = $state<"textadventure" | "gamebook">("textadventure");
    let createLanguage = $state("English");
    let creating = $state(false);
    let createError = $state<string | null>(null);

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

    async function handleCreateGame() {
        createError = null;
        const trimmed = createName.trim();
        if (!trimmed) { createError = "Please enter a game name."; return; }
        creating = true;
        try {
            const gameId = await createNewGame(trimmed, createType, createLanguage);
            const loaded = await loadFromServer(gameId);
            const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
            if (ok) { goto(base || "/"); return; }
            createError = "Failed to load new game.";
        } catch (err) {
            createError = String(err);
        }
        creating = false;
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
    {:else}
        <div class="flex flex-col items-center gap-4 w-full max-w-sm">
            <p class="text-surface-500-400 text-sm font-medium self-start">Open existing game</p>

            {#if hasFSA()}
                <button type="button" class="btn preset-filled-primary-500 w-full" onclick={handleOpenFolder}>
                    Open game folder
                </button>
            {:else}
                <button type="button" class="btn preset-filled-primary-500 w-full" onclick={handleOpenFile}>
                    Open game file
                </button>
                <p class="text-sm text-surface-500-400 max-w-[40ch] text-center">
                    For full support including image assets, use Chrome or Edge.
                </p>
            {/if}

            {#if error}
                <p class="text-error-500 text-sm">{error}</p>
            {/if}

            <hr class="w-full border-surface-300-700 my-2" />

            <p class="text-surface-500-400 text-sm font-medium self-start">Create new game</p>

            <div class="flex flex-col gap-3 w-full">
                <input
                    type="text"
                    class="input"
                    placeholder="Game name"
                    bind:value={createName}
                    onkeydown={(e) => e.key === "Enter" && handleCreateGame()}
                />

                <div class="flex gap-4">
                    <label class="flex items-center gap-2 cursor-pointer">
                        <input type="radio" class="radio" bind:group={createType} value="textadventure" />
                        <span class="text-sm">Text adventure</span>
                    </label>
                    <label class="flex items-center gap-2 cursor-pointer">
                        <input type="radio" class="radio" bind:group={createType} value="gamebook" />
                        <span class="text-sm">Gamebook</span>
                    </label>
                </div>

                {#if createType === "textadventure"}
                    <select class="select" bind:value={createLanguage}>
                        {#each LANGUAGES as lang (lang.id)}
                            <option value={lang.id}>{lang.label}</option>
                        {/each}
                    </select>
                {/if}

                {#if creating}
                    <div class="flex items-center gap-3">
                        <div class="size-5 rounded-full border-2 border-surface-300-700 border-t-primary-500 animate-spin"></div>
                        <span class="text-surface-500-400 text-sm">{$loadingStatus || "Creating..."}</span>
                    </div>
                {:else}
                    <button
                        type="button"
                        class="btn preset-filled-primary-500 w-full"
                        onclick={handleCreateGame}
                        disabled={!createName.trim()}
                    >
                        Create game
                    </button>
                {/if}

                {#if createError}
                    <p class="text-error-500 text-sm">{createError}</p>
                {/if}
            </div>
        </div>
    {/if}
</main>
