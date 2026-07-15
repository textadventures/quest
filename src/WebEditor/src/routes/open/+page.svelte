<script lang="ts">
    import { goto } from "$app/navigation";
    import { page } from "$app/state";
    import { base } from "$app/paths";
    import { PUBLIC_HAS_SERVER } from "$env/static/public";
    import { openGame, loadingStatus } from "$lib/editor-store";
    import { hasFSA, openDirectory, loadFileFromDirectory, createLocalGame } from "$lib/filesystem/browser-adapter";
    import { openElectronDirectory, loadElectronFile, createElectronGame } from "$lib/filesystem/electron-adapter";
    import { isElectron } from "$lib/runtime";
    import { createNewGame, getGameTemplates } from "$lib/filesystem/server-adapter";
    import type { GameTemplate } from "$lib/filesystem/server-adapter";
    import { pickFile } from "$lib/filesystem/file-picker";
    import {
        listLocalDrafts, loadLocalDraft, deleteLocalDraft, createLocalDraft, createLocalDraftFromFile,
        createLocalDraftFromZipEntry, parseGameIdFromAslx,
    } from "$lib/filesystem/local-adapter";
    import type { LocalDraftSummary, ZipEntries } from "$lib/filesystem/local-adapter";
    import { loadWasm } from "$lib/wasm";

    const hasServer = PUBLIC_HAS_SERVER === "true";

    // Electron always uses window.electronApp (Node fs via IPC), never FSA —
    // its Chromium supports showDirectoryPicker, but docs/electron-desktop-app.md
    // flags known parity bugs there (missing persistent permissions, broken
    // directory iteration). Checked once; doesn't change during the session.
    const nativeFolder = isElectron() || hasFSA();

    let loading = $state(false);
    let error = $state<string | null>(null);

    // Set when a directory (FSA or Electron) or an imported zip (local drafts)
    // contained multiple .aslx files — pendingFiles holds the names to choose
    // from either way.
    let pendingDir = $state<FileSystemDirectoryHandle | null>(null);
    let pendingElectronDir = $state<string | null>(null);
    let pendingZip = $state<ZipEntries | null>(null);
    let pendingFiles = $state<string[]>([]);

    // Local drafts (OPFS, non-FSA/non-Electron browsers only)
    let drafts = $state<LocalDraftSummary[]>([]);
    const isSafari = typeof navigator !== "undefined" && /^((?!chrome|android).)*safari/i.test(navigator.userAgent);

    async function refreshDrafts() {
        if (nativeFolder) return;
        drafts = (await listLocalDrafts()).sort((a, b) => b.lastModified - a.lastModified);
    }
    void refreshDrafts();

    // Electron's File > Open Game Folder… menu item (see +layout.svelte's
    // menu.onAction) lands here with ?action=open&t=<nonce> so it pops the
    // native directory picker immediately, instead of making the user click
    // the "Open game folder" button after already choosing this route. A
    // reactive $effect (not onMount) because the menu can fire while this
    // page is already mounted — e.g. the user is already sitting at /open —
    // and SvelteKit doesn't remount a page for a same-route, query-only
    // navigation; the nonce is what makes goto() to "the same" URL actually
    // navigate at all, and this effect is what notices it once it does.
    let handledOpenNonce = "";
    $effect(() => {
        const params = page.url.searchParams;
        const nonce = params.get("t");
        if (nativeFolder && params.get("action") === "open" && nonce !== handledOpenNonce) {
            handledOpenNonce = nonce ?? "";
            void handleOpenFolder();
        }
    });

    function relativeTime(ms: number): string {
        const mins = Math.round((Date.now() - ms) / 60000);
        if (mins < 1) return "just now";
        if (mins < 60) return `${mins}m ago`;
        const hours = Math.round(mins / 60);
        if (hours < 24) return `${hours}h ago`;
        return `${Math.round(hours / 24)}d ago`;
    }

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
        if (isElectron()) return handleOpenFolderElectron();
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

    async function handleOpenFolderElectron() {
        error = null;
        try {
            const result = await openElectronDirectory();
            if (!result) return;
            if (result.files.length === 0) {
                error = "No .aslx files found in this folder.";
                return;
            }
            if (result.files.length === 1) {
                await loadFromElectron(result.dirPath, result.files[0]);
            } else {
                pendingElectronDir = result.dirPath;
                pendingFiles = result.files;
            }
        } catch (err) {
            error = String(err);
        }
    }

    async function handlePickFile(filename: string) {
        if (pendingDir) {
            const dir = pendingDir;
            pendingDir = null;
            pendingFiles = [];
            await loadFrom(dir, filename);
        } else if (pendingElectronDir) {
            const dirPath = pendingElectronDir;
            pendingElectronDir = null;
            pendingFiles = [];
            await loadFromElectron(dirPath, filename);
        } else if (pendingZip) {
            const entries = pendingZip;
            pendingZip = null;
            pendingFiles = [];
            loading = true;
            error = null;
            try {
                const { bytes, adapter } = await createLocalDraftFromZipEntry(entries, filename);
                const ok = await openGame(bytes, adapter.filename, adapter);
                if (ok) { goto(base || "/"); return; }
                error = "Failed to load game file.";
            } catch (err) {
                error = String(err);
            }
            loading = false;
        }
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

    async function loadFromElectron(dirPath: string, filename: string) {
        loading = true;
        error = null;
        try {
            const loaded = await loadElectronFile(dirPath, filename);
            const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
            if (ok) { goto(base || "/"); return; }
            error = "Failed to load game file.";
        } catch (err) {
            error = String(err);
        }
        loading = false;
    }

    async function handleImportFile() {
        error = null;
        try {
            const file = await pickFile(".aslx,.zip");
            if (!file) return;
            loading = true;
            const result = await createLocalDraftFromFile(file);
            loading = false;
            if (result.kind === "chooseEntry") {
                pendingZip = result.entries;
                pendingFiles = result.names;
                return;
            }
            const ok = await openGame(result.bytes, result.adapter.filename, result.adapter);
            if (ok) { goto(base || "/"); return; }
            error = "Failed to load game file.";
        } catch (err) {
            error = String(err);
        }
        loading = false;
    }

    async function handleOpenDraft(gameId: string) {
        error = null;
        loading = true;
        try {
            const loaded = await loadLocalDraft(gameId);
            if (!loaded) {
                error = "Could not open that draft.";
            } else {
                const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
                if (ok) { goto(base || "/"); return; }
                error = "Failed to load game file.";
            }
        } catch (err) {
            error = String(err);
        }
        loading = false;
    }

    async function handleDeleteDraft(gameId: string, filename: string) {
        if (!confirm(`Delete the local draft "${filename}"? This can't be undone.`)) return;
        await deleteLocalDraft(gameId);
        await refreshDrafts();
    }

    async function handleCreateLocal() {
        createLocalError = null;
        const trimmed = createName.trim();
        if (!trimmed) { createLocalError = "Please enter a game name."; return; }
        if (!selectedTemplateId) { createLocalError = "Please select a template."; return; }
        creatingLocal = true;
        try {
            const bridge = await loadWasm();
            const content = bridge.CreateGameFromTemplate(selectedTemplateId, trimmed);
            const filename = safeFilename(trimmed);
            if (isElectron()) {
                const result = await createElectronGame(filename, content);
                if (result) {
                    const ok = await openGame(result.bytes, result.adapter.filename, result.adapter);
                    if (ok) { goto(base || "/"); return; }
                    createLocalError = "Failed to load new game.";
                }
                // result === null: user cancelled the location picker — do nothing.
                // createElectronGame() can also throw (e.g. a folder with that
                // name already exists there) — caught by this function's outer
                // try/catch below, same as every other error path here.
            } else if (hasFSA()) {
                const result = await createLocalGame(filename, content);
                if (result) {
                    const ok = await openGame(result.loaded.bytes, result.loaded.adapter.filename, result.loaded.adapter);
                    if (ok) { goto(base || "/"); return; }
                    createLocalError = "Failed to load new game.";
                }
            // result === null: user cancelled the directory picker — do nothing
            } else {
                const gameId = parseGameIdFromAslx(content);
                if (!gameId) { createLocalError = "New game is missing a gameid."; creatingLocal = false; return; }
                const adapter = await createLocalDraft(gameId, filename, content);
                const ok = await openGame(new TextEncoder().encode(content), filename, adapter);
                if (ok) { goto(base || "/"); return; }
                createLocalError = "Failed to load new game.";
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
            goto(`${base || ""}/?game=${gameId}`);
        } catch (err) {
            createServerError = String(err);
            creatingServer = false;
        }
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
                >{file.slice(file.lastIndexOf("/") + 1)}</button>
            {/each}
            <button
                type="button"
                class="btn preset-outlined-surface-500 w-full"
                onclick={() => { pendingDir = null; pendingElectronDir = null; pendingZip = null; pendingFiles = []; }}
            >Cancel</button>
        </div>
    {:else}
        <div class="flex flex-col items-center gap-4 w-full max-w-sm">

            {#if !hasServer}
                <!-- Open existing game -->
                <p class="text-surface-500-400 text-sm font-medium self-start">Open existing game</p>

                {#if nativeFolder}
                    <button type="button" class="btn preset-filled-primary-500 w-full" onclick={handleOpenFolder}>
                        Open game folder
                    </button>
                {:else}
                    <button type="button" class="btn preset-filled-primary-500 w-full" onclick={handleImportFile}>
                        Import game file
                    </button>
                    <p class="text-sm text-surface-500-400 max-w-[40ch] text-center">
                        Accepts a .aslx file or a .zip backed up from here. Imported games are stored in this
                        browser as a local draft — use <strong>Backup</strong> in the editor to save a copy to disk.
                    </p>
                {/if}

                {#if error}
                    <p class="text-error-500 text-sm">{error}</p>
                {/if}

                {#if !nativeFolder && drafts.length > 0}
                    <hr class="w-full border-surface-300-700 my-2" />
                    <p class="text-surface-500-400 text-sm font-medium self-start">Your local drafts</p>
                    <div class="flex flex-col gap-2 w-full">
                        {#each drafts as draft (draft.gameId)}
                            <div class="flex items-center gap-2 w-full">
                                <button
                                    type="button"
                                    class="btn btn-sm preset-outlined-primary-500 flex-1 justify-between"
                                    onclick={() => handleOpenDraft(draft.gameId)}
                                >
                                    <span>{draft.filename}</span>
                                    <span class="text-surface-500-400">{relativeTime(draft.lastModified)}</span>
                                </button>
                                <button
                                    type="button"
                                    class="btn btn-sm preset-outlined-error-500"
                                    title="Delete draft"
                                    onclick={() => handleDeleteDraft(draft.gameId, draft.filename)}
                                >Delete</button>
                            </div>
                        {/each}
                    </div>
                    {#if isSafari}
                        <p class="text-xs text-surface-500-400 max-w-[40ch] text-center">
                            Safari may clear local drafts if you don't open this site for a week or more —
                            export a backup of anything important.
                        </p>
                    {/if}
                {/if}

                <hr class="w-full border-surface-300-700 my-2" />
            {/if}

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
                            {#if hasServer}
                                <button
                                    type="button"
                                    class="btn preset-filled-primary-500 flex-1"
                                    onclick={handleCreateServer}
                                    disabled={!createName.trim()}
                                    title="Save to textadventures.co.uk"
                                >
                                    Save to server
                                </button>
                            {:else}
                                <button
                                    type="button"
                                    class="btn preset-filled-primary-500 flex-1"
                                    onclick={handleCreateLocal}
                                    disabled={!createName.trim()}
                                    title={isElectron() ? "Choose where to create your game's folder" : nativeFolder ? "Choose a folder to save your game in" : "Save as a local draft in this browser"}
                                >
                                    {nativeFolder ? "Save to my computer" : "Create local draft"}
                                </button>
                            {/if}
                        </div>

                        {#if hasServer}
                            <p class="text-xs text-surface-500-400 max-w-[40ch] text-center self-center">
                                Want to keep this game only on your own device? Use the
                                <a class="anchor" href="https://play.questviva.com/editor/">play.questviva.com editor</a> instead.
                            </p>
                        {/if}
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
