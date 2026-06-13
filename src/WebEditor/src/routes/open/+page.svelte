<script lang="ts">
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { openGame, loadingStatus } from "$lib/editor-store";
    import { hasFSA, openDirectory, loadFileFromDirectory, loadLocalFile, createLocalGame } from "$lib/filesystem/browser-adapter";
    import { createNewGame, getGameTemplates, loadFromServer } from "$lib/filesystem/server-adapter";
    import type { GameTemplate } from "$lib/filesystem/server-adapter";
    import { loadWasm } from "$lib/wasm";

    let loading = $state(false);
    let error = $state<string | null>(null);

    // Set when a directory was opened but contained multiple .aslx files
    let pendingDir = $state<FileSystemDirectoryHandle | null>(null);
    let pendingFiles = $state<string[]>([]);

    // Create new game form (shared)
    let createName = $state("");
    let templates = $state<GameTemplate[]>([]);
    let selectedTemplateId = $state("");
    let templatesLoading = $state(false);
    let templatesError = $state<string | null>(null);

    let selectedTemplate = $derived(templates.find(t => t.id === selectedTemplateId) ?? null);
    let textAdventureTemplates = $derived(templates.filter(t => t.type === "textadventure"));
    let gamebookTemplates = $derived(templates.filter(t => t.type === "gamebook"));

    // Local creation state
    let creatingLocal = $state(false);
    let createLocalError = $state<string | null>(null);
    let localDownloaded = $state(false);

    // Server creation state
    let creatingServer = $state(false);
    let createServerError = $state<string | null>(null);

    async function ensureTemplates() {
        if (templates.length > 0 || templatesLoading) return;
        templatesLoading = true;
        try {
            templates = await getGameTemplates();
            const defaultTemplate = templates.find(t => t.label === "English") ?? templates[0];
            if (defaultTemplate) selectedTemplateId = defaultTemplate.id;
        } catch (err) {
            templatesError = String(err);
        }
        templatesLoading = false;
    }

    function safeFilename(name: string): string {
        return (name.replace(/[^a-zA-Z0-9 _-]/g, "").trim() || "game") + ".aslx";
    }

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

    async function handleCreateLocal() {
        createLocalError = null;
        localDownloaded = false;
        const trimmed = createName.trim();
        if (!trimmed) { createLocalError = "Please enter a game name."; return; }
        if (!selectedTemplateId) { createLocalError = "Please select a template."; return; }
        creatingLocal = true;
        try {
            const bridge = await loadWasm();
            const content = bridge.CreateGameFromTemplate(selectedTemplateId, trimmed);
            const result = await createLocalGame(safeFilename(trimmed), content);
            if (result === null) {
                // user cancelled directory picker — do nothing
            } else if (result.kind === "opened") {
                const ok = await openGame(result.loaded.bytes, result.loaded.adapter.filename, result.loaded.adapter);
                if (ok) { goto(base || "/"); return; }
                createLocalError = "Failed to load new game.";
            } else {
                localDownloaded = true;
            }
        } catch (err) {
            createLocalError = String(err);
        }
        creatingLocal = false;
    }

    async function handleCreateServer() {
        createServerError = null;
        const trimmed = createName.trim();
        if (!trimmed) { createServerError = "Please enter a game name."; return; }
        if (!selectedTemplateId) { createServerError = "Please select a template."; return; }
        creatingServer = true;
        try {
            const gameId = await createNewGame(trimmed, selectedTemplateId);
            const loaded = await loadFromServer(gameId);
            const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
            if (ok) { goto(`${base || ""}/?game=${gameId}`); return; }
            createServerError = "Failed to load new game.";
        } catch (err) {
            createServerError = String(err);
        }
        creatingServer = false;
    }

    const creating = $derived(creatingLocal || creatingServer);
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

            <!-- Open existing game -->
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

            <!-- Create new game -->
            <p class="text-surface-500-400 text-sm font-medium self-start">Create new game</p>

            {#if templatesError}
                <p class="text-error-500 text-sm">{templatesError}</p>
            {:else}
                <div class="flex flex-col gap-3 w-full">
                    <input
                        type="text"
                        class="input"
                        placeholder="Game name"
                        bind:value={createName}
                        onfocus={ensureTemplates}
                        onkeydown={(e) => { if (e.key === "Enter") handleCreateLocal(); }}
                        disabled={creating}
                    />

                    {#if templatesLoading}
                        <div class="flex items-center gap-2 text-surface-500-400 text-sm">
                            <div class="size-4 rounded-full border-2 border-surface-300-700 border-t-primary-500 animate-spin"></div>
                            Loading templates...
                        </div>
                    {:else if templates.length > 0}
                        <div class="flex flex-col gap-1">
                            <p class="text-sm text-surface-500-400">Game type</p>
                            <div class="flex gap-4">
                                {#each [{ value: "textadventure", label: "Text adventure" }, { value: "gamebook", label: "Gamebook" }] as type (type.value)}
                                    <label class="flex items-center gap-2 cursor-pointer">
                                        <input
                                            type="radio"
                                            class="radio"
                                            name="gametype"
                                            checked={selectedTemplate?.type === type.value}
                                            disabled={creating}
                                            onchange={() => {
                                                const list = type.value === "gamebook" ? gamebookTemplates : textAdventureTemplates;
                                                selectedTemplateId = list[0]?.id ?? "";
                                            }}
                                        />
                                        <span class="text-sm">{type.label}</span>
                                    </label>
                                {/each}
                            </div>
                        </div>

                        {#if selectedTemplate?.type === "textadventure" && textAdventureTemplates.length > 1}
                            <select class="select" bind:value={selectedTemplateId} disabled={creating}>
                                {#each textAdventureTemplates as t (t.id)}
                                    <option value={t.id}>{t.label}</option>
                                {/each}
                            </select>
                        {/if}
                    {/if}

                    {#if creating}
                        <div class="flex items-center gap-3">
                            <div class="size-5 rounded-full border-2 border-surface-300-700 border-t-primary-500 animate-spin"></div>
                            <span class="text-surface-500-400 text-sm">{$loadingStatus || "Creating..."}</span>
                        </div>
                    {:else}
                        <div class="flex gap-2">
                            <button
                                type="button"
                                class="btn preset-filled-primary-500 flex-1"
                                onclick={handleCreateLocal}
                                disabled={!createName.trim()}
                                title={hasFSA() ? "Choose a folder to save your game in" : "Download the game file"}
                            >
                                {hasFSA() ? "Save to my computer" : "Download"}
                            </button>
                            <button
                                type="button"
                                class="btn preset-outlined-primary-500 flex-1"
                                onclick={handleCreateServer}
                                disabled={!createName.trim()}
                                title="Save to textadventures.co.uk"
                            >
                                Save to server
                            </button>
                        </div>
                    {/if}

                    {#if localDownloaded}
                        <p class="text-sm text-surface-500-400">
                            Game file downloaded. Use <strong>Open game file</strong> above to open it.
                        </p>
                    {/if}

                    {#if createLocalError}
                        <p class="text-error-500 text-sm">{createLocalError}</p>
                    {/if}
                    {#if createServerError}
                        <p class="text-error-500 text-sm">{createServerError}</p>
                    {/if}
                </div>
            {/if}

        </div>
    {/if}
</main>
