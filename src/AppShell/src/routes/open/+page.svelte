<script lang="ts">
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { page } from "$app/state";
    import { base } from "$app/paths";
    import { PUBLIC_HAS_SERVER } from "$env/static/public";
    import { openGame, loadingStatus } from "$lib/editor-store";
    import { hasFSA, openDirectory, loadFileFromDirectory, createLocalGame } from "$lib/filesystem/browser-adapter";
    import {
        openElectronFile, loadElectronFile, createElectronGame, getDefaultGamesDir, pickGameLocation,
        listRecentGames, removeRecentGame,
    } from "$lib/filesystem/electron-adapter";
    import type { RecentGame } from "$lib/filesystem/electron-adapter";
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
    import PreviewBanner from "$components/PreviewBanner.svelte";

    const hasServer = PUBLIC_HAS_SERVER === "true";

    // Electron always uses window.electronApp (Node fs via IPC), never FSA —
    // its Chromium supports showDirectoryPicker, but docs/electron-desktop-app.md
    // flags known parity bugs there (missing persistent permissions, broken
    // directory iteration). Checked once; doesn't change during the session.
    const isElectronApp = isElectron();
    // FSA folder access (Open game folder / Save to folder) is offered as a
    // secondary option on capable browsers — OPFS local drafts are the default
    // everywhere else so trying the editor doesn't start with a "give this
    // website access to a folder" prompt.
    const canUseFSA = !isElectronApp && hasFSA();

    // Explanation for "Import game file" — kept out of the page as a
    // click-to-open popover (see importHelpOpen) rather than a permanent
    // paragraph so the open screen isn't dominated by fine print. A native
    // title tooltip doesn't work here: it never triggers on click/tap, so
    // touch users (and impatient mouse users) never see it. Scoped to just
    // this button — the "open a folder" case is covered by the button below
    // it, and the backup reminder lives in the editor itself instead (see
    // Toolbar.svelte), where it's actually actionable.
    const importHelpText = [
        "Accepts a .aslx file, or a .zip backed up from here.",
        "Imported games are stored as a local draft in this browser.",
    ];

    let importHelpOpen = $state(false);

    $effect(() => {
        if (!importHelpOpen) return;
        function onOutside(e: MouseEvent) {
            if (!(e.target as HTMLElement).closest(".import-help")) importHelpOpen = false;
        }
        document.addEventListener("mousedown", onOutside);
        return () => document.removeEventListener("mousedown", onOutside);
    });

    let loading = $state(false);
    let error = $state<string | null>(null);

    // Set when a directory (FSA) or an imported zip (local drafts) contained
    // multiple .aslx files — pendingFiles holds the names to choose from
    // either way. Electron has no equivalent case: openElectronFile() picks
    // one exact file directly, no folder-then-disambiguate step.
    let pendingDir = $state<FileSystemDirectoryHandle | null>(null);
    let pendingZip = $state<ZipEntries | null>(null);
    let pendingFiles = $state<string[]>([]);

    // Local drafts (OPFS) — every non-Electron browser, regardless of FSA support.
    let drafts = $state<LocalDraftSummary[]>([]);
    const isSafari = typeof navigator !== "undefined" && /^((?!chrome|android).)*safari/i.test(navigator.userAgent);

    async function refreshDrafts() {
        if (isElectronApp) return;
        drafts = (await listLocalDrafts()).sort((a, b) => b.lastModified - a.lastModified);
    }
    void refreshDrafts();

    // Recently opened/created/saved-as game folders (Electron only — tracked
    // by electron-adapter.ts on every successful load/create/saveAs).
    let recentGames = $state<RecentGame[]>([]);

    async function refreshRecentGames() {
        if (!isElectron()) return;
        recentGames = await listRecentGames();
    }
    void refreshRecentGames();

    // Recent-list changes that originate outside this page (the native
    // "Clear Recent" menu item) have no other way to reach an already-mounted
    // /open page — the page's own Remove button updates recentGames directly
    // and doesn't rely on this.
    onMount(() => {
        if (!isElectron()) return;
        return window.electronApp!.recent.onChanged(() => void refreshRecentGames());
    });

    // Electron's File > Open Game… menu item (see +layout.svelte's
    // menu.onAction) lands here with ?action=open&t=<nonce> so it pops the
    // native file picker immediately, instead of making the user click
    // the "Open game…" button after already choosing this route. A
    // reactive $effect (not onMount) because the menu can fire while this
    // page is already mounted — e.g. the user is already sitting at /open —
    // and SvelteKit doesn't remount a page for a same-route, query-only
    // navigation; the nonce is what makes goto() to "the same" URL actually
    // navigate at all, and this effect is what notices it once it does.
    // The native "Open Recent" submenu reuses the same nonce mechanism —
    // ?action=open-recent&dir=...&file=...&t=<nonce>, set by +layout.svelte's
    // onOpenRecent handler — instead of a second menu→renderer IPC round trip.
    let handledOpenNonce = "";
    $effect(() => {
        const params = page.url.searchParams;
        const nonce = params.get("t");
        if (!isElectronApp || nonce === handledOpenNonce) return;
        const action = params.get("action");
        if (action === "open") {
            handledOpenNonce = nonce ?? "";
            void handleOpenFolder();
        } else if (action === "open-recent") {
            const dirPath = params.get("dir");
            const filename = params.get("file");
            if (dirPath && filename) {
                handledOpenNonce = nonce ?? "";
                void loadFromElectron(dirPath, filename);
            }
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

    function folderName(dirPath: string): string {
        return dirPath.split(/[\\/]/).pop() || dirPath;
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
    let lastTextAdventureTemplateId = $state("");

    $effect(() => {
        if (selectedTemplate?.type === "textadventure") {
            lastTextAdventureTemplateId = selectedTemplate.id;
        }
    });

    // Local creation state
    let creatingLocal = $state(false);
    let createLocalError = $state<string | null>(null);

    // Electron's new-game location — defaults to Documents/Quest Games
    // (matches Quest 5's desktop editor) unless the user picks somewhere
    // else via "Change location…". electronParentDir stays null until they
    // do, so the preview below always reflects whichever is actually in play.
    let defaultGamesDir = $state("");
    let electronParentDir = $state<string | null>(null);
    if (isElectron()) void getDefaultGamesDir().then(dir => defaultGamesDir = dir);
    let electronGamesDir = $derived(electronParentDir ?? defaultGamesDir);

    async function handleChangeLocation() {
        const picked = await pickGameLocation(electronGamesDir || undefined);
        if (picked) electronParentDir = picked;
    }

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
        if (isElectron()) return handleOpenFileElectron();
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

    async function handleOpenFileElectron() {
        error = null;
        try {
            const result = await openElectronFile();
            if (!result) return;
            await loadFromElectron(result.dirPath, result.filename);
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
        } else if (pendingZip) {
            const entries = pendingZip;
            pendingZip = null;
            pendingFiles = [];
            loading = true;
            error = null;
            try {
                const { bytes, adapter } = await createLocalDraftFromZipEntry(entries, filename);
                const ok = await openGame(bytes, adapter.filename, adapter);
                if (ok) { goto(`${base}/edit`); return; }
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
            if (ok) { goto(`${base}/edit`); return; }
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
            if (ok) { goto(`${base}/edit`); return; }
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
            if (ok) { goto(`${base}/edit`); return; }
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
                if (ok) { goto(`${base}/edit`); return; }
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

    // Removes a Recent entry only — never touches the actual file on disk.
    async function handleRemoveRecent(game: RecentGame) {
        await removeRecentGame(game.dirPath, game.filename);
        await refreshRecentGames();
    }

    // Shared by both create paths below — validates the form and renders the
    // template through WASM. Returns null (with createLocalError already set)
    // if the form isn't ready yet.
    async function buildNewGameFile(): Promise<{ filename: string; content: string } | null> {
        const trimmed = createName.trim();
        if (!trimmed) { createLocalError = "Please enter a game name."; return null; }
        if (!selectedTemplateId) { createLocalError = "Please select a template."; return null; }
        const bridge = await loadWasm();
        const content = bridge.CreateGameFromTemplate(selectedTemplateId, trimmed);
        return { filename: safeFilename(trimmed), content };
    }

    // Default create path: Electron → native folder (its only storage mode);
    // every other browser → an OPFS local draft, regardless of FSA support,
    // so trying the editor never opens with a folder-permission prompt.
    async function handleCreateLocal() {
        createLocalError = null;
        creatingLocal = true;
        try {
            const file = await buildNewGameFile();
            if (!file) { creatingLocal = false; return; }
            if (isElectronApp) {
                // Falls back to a fresh fetch on the off chance the mount-time
                // getDefaultGamesDir() call (see electronGamesDir) hasn't
                // resolved yet — cheap IPC round trip, not worth blocking the
                // form on at mount.
                const parentDir = electronGamesDir || await getDefaultGamesDir();
                // Errors here (e.g. a folder with that name already exists at
                // parentDir) are caught by this function's outer try/catch below,
                // same as every other error path in this function.
                const result = await createElectronGame(parentDir, file.filename, file.content);
                const ok = await openGame(result.bytes, result.adapter.filename, result.adapter);
                if (ok) { goto(`${base}/edit`); return; }
                createLocalError = "Failed to load new game.";
            } else {
                const gameId = parseGameIdFromAslx(file.content);
                if (!gameId) { createLocalError = "New game is missing a gameid."; creatingLocal = false; return; }
                const adapter = await createLocalDraft(gameId, file.filename, file.content);
                const ok = await openGame(new TextEncoder().encode(file.content), file.filename, adapter);
                if (ok) { goto(`${base}/edit`); return; }
                createLocalError = "Failed to load new game.";
            }
        } catch (err) {
            createLocalError = String(err);
        }
        creatingLocal = false;
    }

    // Secondary create path, FSA-capable browsers only: saves straight to a
    // folder on disk instead of an OPFS draft — offered alongside, not instead
    // of, handleCreateLocal() (see canUseFSA).
    async function handleCreateLocalFolder() {
        createLocalError = null;
        creatingLocal = true;
        try {
            const file = await buildNewGameFile();
            if (!file) { creatingLocal = false; return; }
            const result = await createLocalGame(file.filename, file.content);
            // result === null: user cancelled the directory picker — do nothing
            if (result) {
                const ok = await openGame(result.loaded.bytes, result.loaded.adapter.filename, result.loaded.adapter);
                if (ok) { goto(`${base}/edit`); return; }
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
            goto(`${base}/edit?game=${gameId}`);
        } catch (err) {
            createServerError = String(err);
            creatingServer = false;
        }
    }

    const creating = $derived(creatingLocal || creatingServer);
</script>

<PreviewBanner />

<main class="flex flex-col items-center justify-center min-h-[calc(100svh-var(--home-bar-height,0px))] gap-6 p-8">
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
                onclick={() => { pendingDir = null; pendingZip = null; pendingFiles = []; }}
            >Cancel</button>
        </div>
    {:else}
        <div class="flex flex-col items-center gap-4 w-full max-w-sm">

            {#if !hasServer}
                <!-- Open existing game -->
                <p class="text-surface-500-400 text-sm font-medium self-start">Open existing game</p>

                {#if isElectronApp}
                    <button type="button" class="btn preset-filled-primary-500 w-full" onclick={handleOpenFolder}>
                        {isElectron() ? "Open game…" : "Open game folder"}
                    </button>
                {:else}
                    <div class="flex items-center gap-2 w-full">
                        <button type="button" class="btn preset-filled-primary-500 flex-1" onclick={handleImportFile}>
                            Import game file
                        </button>
                        <div class="import-help relative shrink-0">
                            <button
                                type="button"
                                class="btn-icon preset-outlined-surface-500"
                                onclick={() => importHelpOpen = !importHelpOpen}
                                aria-label="About importing games"
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="size-4">
                                    <circle cx="12" cy="12" r="10" />
                                    <path d="M12 16v-4" />
                                    <path d="M12 8h.01" />
                                </svg>
                            </button>
                            {#if importHelpOpen}
                                <div class="absolute right-0 top-full z-[999] mt-1 w-64 bg-surface-50-950 border border-surface-200-800 rounded shadow-lg p-3 flex flex-col gap-2 text-sm text-surface-700-300 text-left">
                                    {#each importHelpText as line (line)}
                                        <p>{line}</p>
                                    {/each}
                                </div>
                            {/if}
                        </div>
                    </div>
                    {#if canUseFSA}
                        <button type="button" class="btn preset-outlined-primary-500 w-full" onclick={handleOpenFolder}>
                            Open game folder
                        </button>
                    {/if}
                {/if}

                {#if error}
                    <p class="text-error-500 text-sm">{error}</p>
                {/if}

                {#if isElectronApp && recentGames.length > 0}
                    <hr class="w-full border-surface-300-700 my-2" />
                    <p class="text-surface-500-400 text-sm font-medium self-start">Recent</p>
                    <div class="flex flex-col gap-2 w-full">
                        {#each recentGames as game (game.dirPath + "/" + game.filename)}
                            <div class="flex items-center gap-2 w-full">
                                <button
                                    type="button"
                                    class="btn btn-sm preset-outlined-primary-500 flex-1 min-w-0 flex-col! items-start! h-auto! py-2 gap-0.5"
                                    onclick={() => loadFromElectron(game.dirPath, game.filename)}
                                >
                                    <span class="w-full truncate text-left">{game.filename}</span>
                                    <span class="w-full truncate text-left text-surface-500-400 text-xs">{folderName(game.dirPath)} · {relativeTime(game.lastOpened)}</span>
                                </button>
                                <button
                                    type="button"
                                    class="btn btn-sm preset-outlined-error-500"
                                    title="Remove from Recent"
                                    onclick={() => handleRemoveRecent(game)}
                                >Remove</button>
                            </div>
                        {/each}
                    </div>
                {/if}

                {#if !isElectronApp && drafts.length > 0}
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
                        autocapitalize="off"
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
                                                if (type.value === "gamebook") {
                                                    selectedTemplateId = gamebookTemplates[0]?.id ?? "";
                                                } else {
                                                    const preferred = textAdventureTemplates.find(t => t.id === lastTextAdventureTemplateId);
                                                    const english = textAdventureTemplates.find(t => t.label === "English");
                                                    selectedTemplateId = (preferred ?? english ?? textAdventureTemplates[0])?.id ?? "";
                                                }
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
                                <div class="flex flex-col gap-1 flex-1">
                                    <button
                                        type="button"
                                        class="btn preset-filled-primary-500 w-full"
                                        onclick={handleCreateServer}
                                        disabled={!createName.trim()}
                                        title="Save to textadventures.co.uk"
                                    >
                                        Save to server
                                    </button>
                                    <p class="text-xs text-surface-500-400 text-center">Saved to your textadventures.co.uk account</p>
                                </div>
                            {:else}
                                <div class="flex flex-col gap-1 flex-1">
                                    <button
                                        type="button"
                                        class="btn preset-filled-primary-500 w-full"
                                        onclick={handleCreateLocal}
                                        disabled={!createName.trim()}
                                        title={isElectronApp ? "Choose where to create your game's folder" : "Save as a local draft in this browser"}
                                    >
                                        {isElectronApp ? "Create" : "Create local draft"}
                                    </button>
                                    {#if !isElectronApp}
                                        <p class="text-xs text-surface-500-400 text-center">Stored in this browser only</p>
                                    {/if}
                                </div>
                                {#if canUseFSA}
                                    <div class="flex flex-col gap-1 flex-1">
                                        <button
                                            type="button"
                                            class="btn preset-outlined-primary-500 w-full"
                                            onclick={handleCreateLocalFolder}
                                            disabled={!createName.trim()}
                                            title="Choose a folder on your computer to save your game in"
                                        >
                                            Save to folder…
                                        </button>
                                        <p class="text-xs text-surface-500-400 text-center">You'll choose a folder on this device</p>
                                    </div>
                                {/if}
                            {/if}
                        </div>

                        {#if isElectron()}
                            <p class="text-xs text-surface-500-400 text-center self-center">
                                Will be created as a new folder in:<br />
                                <span class="font-mono">{electronGamesDir || "…"}</span>
                                <button type="button" class="anchor" onclick={handleChangeLocation}>Change location…</button>
                            </p>
                        {/if}

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
