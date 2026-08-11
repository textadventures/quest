<script lang="ts">
    import { onMount } from "svelte";
    import { get } from "svelte/store";
    import { goto } from "$app/navigation";
    import { page } from "$app/state";
    import { base } from "$app/paths";
    import { PUBLIC_HAS_SERVER } from "$env/static/public";
    import { openGame, loadingStatus, lastOpenGameError } from "$lib/editor-store";
    import { confirmDialog } from "$lib/confirm";
    import { hasFSA, openDirectory, loadFileFromDirectory, createLocalGame } from "$lib/filesystem/browser-adapter";
    import {
        openElectronFile, loadElectronFile, createElectronGame, getDefaultGamesDir, pickGameLocation,
        listRecentGames, removeRecentGame, showItemInFolder,
    } from "$lib/filesystem/electron-adapter";
    import type { RecentGame } from "$lib/filesystem/electron-adapter";
    import { isElectron } from "$lib/runtime";
    import { getGameTemplates } from "$lib/filesystem/server-adapter";
    import type { GameTemplate } from "$lib/filesystem/server-adapter";
    import { pickFile } from "$lib/filesystem/file-picker";
    import {
        listLocalDrafts, loadLocalDraft, deleteLocalDraft, createLocalDraft, createLocalDraftFromFile,
        createLocalDraftFromZipEntry, parseGameIdFromAslx,
    } from "$lib/filesystem/local-adapter";
    import type { LocalDraftSummary, ZipEntries } from "$lib/filesystem/local-adapter";
    import { loadWasm } from "$lib/wasm";
    import { triggerDownload } from "$lib/filesystem/download";
    import { zipSync } from "fflate";
    import FolderOpen from "@lucide/svelte/icons/folder-open";
    import Trash2 from "@lucide/svelte/icons/trash-2";
    import Download from "@lucide/svelte/icons/download";
    import { t, locale } from "$lib/i18n";

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
        t("openPage.importHelp1"),
        t("openPage.importHelp2"),
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
        if (mins < 1) return t("common.relativeTime.justNow");
        if (mins < 60) return t("common.relativeTime.minutesAgo", { mins });
        const hours = Math.round(mins / 60);
        if (hours < 24) return t("common.relativeTime.hoursAgo", { hours });
        return t("common.relativeTime.daysAgo", { days: Math.round(hours / 24) });
    }

    function folderName(dirPath: string): string {
        return dirPath.split(/[\\/]/).pop() || dirPath;
    }

    // Matches each platform's own term for its file manager, same as any
    // native app's "Show in Finder"/"Show in Explorer" context-menu item.
    const electronPlatform = typeof window !== "undefined" ? window.electronApp?.platform : undefined;
    const showInFolderLabel = electronPlatform === "Mac" ? t("openPage.showInFinder")
        : electronPlatform === "Windows" ? t("openPage.showInExplorer")
        : t("openPage.showInFolder");

    // Create new game form (shared)
    let createName = $state("");
    let templates = $state<GameTemplate[]>([]);
    let selectedTemplateId = $state("");
    let templatesLoading = $state(false);
    let templatesError = $state<string | null>(null);

    let selectedTemplate = $derived(templates.find(tpl => tpl.id === selectedTemplateId) ?? null);
    let textAdventureTemplates = $derived(templates.filter(tpl => tpl.type === "textadventure"));
    let gamebookTemplates = $derived(templates.filter(tpl => tpl.type === "gamebook"));
    let lastTextAdventureTemplateId = $state("");
    let lastGamebookTemplateId = $state("");

    $effect(() => {
        if (selectedTemplate?.type === "textadventure") {
            lastTextAdventureTemplateId = selectedTemplate.id;
        } else if (selectedTemplate?.type === "gamebook") {
            lastGamebookTemplateId = selectedTemplate.id;
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

    // Game templates are labelled by their own language's name (e.g. "Deutsch", not "German" —
    // see EditorController.AddTemplateData's stem/`template=` attribute fallback), so the UI
    // locale needs mapping to the matching template label. Only covers locales this UI actually
    // offers (see SUPPORTED_LOCALES); anything else falls back to English.
    const TEMPLATE_LABEL_BY_LOCALE: Record<string, string> = { en: "English", de: "Deutsch", es: "Español" };

    function defaultTemplateLabel(): string {
        return TEMPLATE_LABEL_BY_LOCALE[get(locale)] ?? "English";
    }

    // Same idea as TEMPLATE_LABEL_BY_LOCALE above, but Gamebook templates keep the
    // "Gamebook" stem as their base label rather than being named after the language —
    // see Gamebook-Deutsch.template/Gamebook-Espanol.template.
    const GAMEBOOK_TEMPLATE_LABEL_BY_LOCALE: Record<string, string> = { en: "Gamebook", de: "Gamebook (Deutsch)", es: "Gamebook (Español)" };

    function defaultGamebookTemplateLabel(): string {
        return GAMEBOOK_TEMPLATE_LABEL_BY_LOCALE[get(locale)] ?? "Gamebook";
    }

    // The "Gamebook" stem in a template's underlying label (see comment above) is redundant once
    // it's showing in a list that's already scoped to the Gamebook radio option — display just the
    // language part, matching how the Text Adventure list shows "English"/"Deutsch"/"Español" alone.
    function gamebookTemplateDisplayLabel(label: string): string {
        const match = /^Gamebook(?:\s*\(([^)]+)\))?$/.exec(label);
        if (!match) return label;
        return match[1] ?? "English";
    }

    async function ensureTemplates() {
        if (templates.length > 0 || templatesLoading) return;
        templatesLoading = true;
        try {
            templates = await getGameTemplates();
            const defaultTemplate = templates.find(tpl => tpl.label === defaultTemplateLabel())
                ?? templates.find(tpl => tpl.label === "English") ?? templates[0];
            if (defaultTemplate) selectedTemplateId = defaultTemplate.id;
        } catch (err) {
            templatesError = String(err);
        }
        templatesLoading = false;
    }

    function safeFilename(name: string): string {
        return (name.replace(/[^a-zA-Z0-9 _-]/g, "").trim() || "game") + ".aslx";
    }

    // openGame() sets lastOpenGameError with the actual reason (a missing library, invalid XML,
    // ...) whenever it returns false — prefer that over a generic message so the user isn't just
    // told "it failed" with no way to act on it.
    function openGameErrorMessage(fallback: string): string {
        return get(lastOpenGameError) ?? fallback;
    }

    async function handleOpenFolder() {
        if (isElectron()) return handleOpenFileElectron();
        error = null;
        try {
            const result = await openDirectory();
            if (!result) return;
            if (result.files.length === 0) {
                error = t("openPage.noAslxFilesFound");
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
                error = openGameErrorMessage(t("openPage.failedToLoadGameFile"));
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
            error = openGameErrorMessage(t("openPage.failedToLoadGameFile"));
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
            error = openGameErrorMessage(t("openPage.failedToLoadGameFile"));
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
            error = openGameErrorMessage(t("openPage.failedToLoadGameFile"));
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
                error = t("openPage.couldNotOpenDraft");
            } else {
                const ok = await openGame(loaded.bytes, loaded.adapter.filename, loaded.adapter);
                if (ok) { goto(`${base}/edit`); return; }
                error = openGameErrorMessage(t("openPage.failedToLoadGameFile"));
            }
        } catch (err) {
            error = String(err);
        }
        loading = false;
    }

    async function handleDeleteDraft(gameId: string, filename: string) {
        const confirmed = await confirmDialog(t("openPage.deleteDraftConfirm", { name: filename }), { confirmLabel: t("common.delete"), danger: true });
        if (!confirmed) return;
        await deleteLocalDraft(gameId);
        await refreshDrafts();
    }

    // Bundles the draft's raw stored .aslx plus its assets into a .zip, same shape as
    // editor-store's backupGame() — but reads straight from OPFS via loadLocalDraft()/listAssets()
    // rather than _bridge.Save(), so it's deliberately independent of openGame()/the WASM editor.
    // That's the whole point: a draft that can no longer be *loaded* (e.g. hand-edited into an
    // invalid state via Code View) can still be gotten out of the browser complete with its
    // assets, to fix elsewhere or send to someone for help, rather than being permanently stuck.
    async function handleDownloadDraft(gameId: string, filename: string) {
        const loaded = await loadLocalDraft(gameId);
        if (!loaded) {
            error = t("openPage.couldNotReadDraft");
            return;
        }
        const zipEntries: Record<string, Uint8Array> = { [filename]: loaded.bytes };
        for (const asset of await loaded.adapter.listAssets()) {
            const blob = await loaded.adapter.getAsset(asset.key);
            if (blob) zipEntries[asset.key] = new Uint8Array(await blob.arrayBuffer());
        }
        const zipBytes = zipSync(zipEntries);
        triggerDownload(zipBytes, filename.replace(/\.aslx$/i, "") + ".zip");
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
        if (!trimmed) { createLocalError = t("openPage.enterGameName"); return null; }
        if (!selectedTemplateId) { createLocalError = t("openPage.selectTemplate"); return null; }
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
                createLocalError = openGameErrorMessage(t("openPage.failedToLoadNewGame"));
            } else {
                const gameId = parseGameIdFromAslx(file.content);
                if (!gameId) { createLocalError = t("openPage.missingGameId"); creatingLocal = false; return; }
                const adapter = await createLocalDraft(gameId, file.filename, file.content);
                const ok = await openGame(new TextEncoder().encode(file.content), file.filename, adapter);
                if (ok) { goto(`${base}/edit`); return; }
                createLocalError = openGameErrorMessage(t("openPage.failedToLoadNewGame"));
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
                createLocalError = openGameErrorMessage(t("openPage.failedToLoadNewGame"));
            }
        } catch (err) {
            createLocalError = String(err);
        }
        creatingLocal = false;
    }

    const creating = $derived(creatingLocal);
</script>

<main class="flex flex-col items-center justify-center min-h-[calc(100svh-var(--home-bar-height,0px))] gap-6 p-8">
    <h1 class="text-3xl font-semibold">{t("openPage.title")}</h1>

    {#if loading}
        <div class="flex flex-col items-center gap-3">
            <div class="size-10 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
            <p class="text-surface-600-400 text-sm">{$loadingStatus}</p>
        </div>
    {:else if pendingFiles.length > 0}
        <p class="text-surface-600-400">{t("openPage.multipleFilesFound")}</p>
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
            >{t("common.cancel")}</button>
        </div>
    {:else if hasServer}
        <!-- textadventures.co.uk only opens games you already have — no local
             storage, no server-side create. New games are always seeded via
             play.questviva.com, which needs no account. -->
        <div class="flex flex-col items-center gap-4 w-full max-w-sm text-center">
            <p class="text-surface-600-400">
                {t("openPage.serverOnlyMessage")}
            </p>
            <a class="btn preset-filled-primary-500" href="https://play.questviva.com/open">
                {t("openPage.createAtPlayQuestviva")}
            </a>
        </div>
    {:else}
        <div class="flex flex-col items-center gap-4 w-full max-w-sm">

            <!-- Open existing game -->
            <p class="text-surface-600-400 text-sm font-medium self-start">{t("openPage.openExistingGame")}</p>

            {#if isElectronApp}
                <button type="button" class="btn preset-filled-primary-500 w-full" onclick={handleOpenFolder}>
                    {isElectron() ? t("openPage.openGameEllipsis") : t("openPage.openGameFolder")}
                </button>
            {:else}
                <div class="flex items-center gap-2 w-full">
                    <button type="button" class="btn preset-filled-primary-500 flex-1" onclick={handleImportFile}>
                        {t("openPage.importGameFile")}
                    </button>
                    <div class="import-help relative shrink-0">
                        <button
                            type="button"
                            class="btn-icon preset-outlined-surface-500"
                            onclick={() => importHelpOpen = !importHelpOpen}
                            aria-label={t("openPage.aboutImporting")}
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
                        {t("openPage.openGameFolder")}
                    </button>
                {/if}
            {/if}

            {#if error}
                <p class="text-error-500 text-sm whitespace-pre-wrap">{error}</p>
            {/if}

            {#if isElectronApp && recentGames.length > 0}
                <hr class="w-full border-surface-300-700 my-2" />
                <p class="text-surface-600-400 text-sm font-medium self-start">{t("openPage.recent")}</p>
                <div class="flex flex-col gap-2 w-full">
                    {#each recentGames as game (game.dirPath + "/" + game.filename)}
                        <div class="flex items-center gap-2 w-full">
                            <button
                                type="button"
                                class="btn btn-sm preset-outlined-primary-500 flex-1 min-w-0 flex-col! items-start! h-auto! py-2 gap-0.5"
                                onclick={() => loadFromElectron(game.dirPath, game.filename)}
                            >
                                <span class="w-full truncate text-left">{game.filename}</span>
                                <span class="w-full truncate text-left text-surface-600-400 text-xs">{folderName(game.dirPath)} · {relativeTime(game.lastOpened)}</span>
                            </button>
                            <button
                                type="button"
                                class="btn-icon btn-icon-sm preset-outlined-surface-500 shrink-0"
                                title={showInFolderLabel}
                                aria-label={showInFolderLabel}
                                onclick={() => showItemInFolder(game.dirPath, game.filename)}
                            ><FolderOpen size={14} /></button>
                            <button
                                type="button"
                                class="btn-icon btn-icon-sm preset-outlined-error-500 shrink-0"
                                title={t("openPage.removeFromRecent")}
                                aria-label={t("openPage.removeFromRecent")}
                                onclick={() => handleRemoveRecent(game)}
                            ><Trash2 size={14} /></button>
                        </div>
                    {/each}
                </div>
            {/if}

            {#if !isElectronApp && drafts.length > 0}
                <hr class="w-full border-surface-300-700 my-2" />
                <p class="text-surface-600-400 text-sm font-medium self-start">{t("openPage.yourLocalDrafts")}</p>
                <div class="flex flex-col gap-2 w-full">
                    {#each drafts as draft (draft.gameId)}
                        <div class="flex items-center gap-2 w-full">
                            <button
                                type="button"
                                class="btn btn-sm preset-outlined-primary-500 flex-1 min-w-0 justify-between"
                                onclick={() => handleOpenDraft(draft.gameId)}
                            >
                                <span class="truncate min-w-0">{draft.filename}</span>
                                <span class="text-surface-600-400 shrink-0">{relativeTime(draft.lastModified)}</span>
                            </button>
                            <button
                                type="button"
                                class="btn-icon btn-icon-sm preset-outlined-surface-500 shrink-0"
                                title={t("openPage.downloadDraftTitle")}
                                aria-label={t("openPage.downloadDraftAriaLabel")}
                                onclick={() => handleDownloadDraft(draft.gameId, draft.filename)}
                            ><Download size={14} /></button>
                            <button
                                type="button"
                                class="btn-icon btn-icon-sm preset-outlined-error-500 shrink-0"
                                title={t("openPage.deleteDraft")}
                                aria-label={t("openPage.deleteDraft")}
                                onclick={() => handleDeleteDraft(draft.gameId, draft.filename)}
                            ><Trash2 size={14} /></button>
                        </div>
                    {/each}
                </div>
                {#if isSafari}
                    <p class="text-xs text-surface-600-400 max-w-[40ch] text-center">
                        {t("openPage.safariWarning")}
                    </p>
                {/if}
            {/if}

            <hr class="w-full border-surface-300-700 my-2" />

            <!-- Create new game -->
            <p class="text-surface-600-400 text-sm font-medium self-start">{t("openPage.createNewGame")}</p>

            {#if templatesError}
                <p class="text-error-500 text-sm">{templatesError}</p>
            {:else}
                <div class="flex flex-col gap-3 w-full">
                    <input
                        type="text"
                        autocapitalize="off"
                        class="input"
                        placeholder={t("openPage.gameNamePlaceholder")}
                        bind:value={createName}
                        onfocus={ensureTemplates}
                        onkeydown={(e) => { if (e.key === "Enter") handleCreateLocal(); }}
                        disabled={creating}
                    />

                    {#if templatesLoading}
                        <div class="flex items-center gap-2 text-surface-600-400 text-sm">
                            <div class="size-4 rounded-full border-2 border-surface-300-700 border-t-primary-500 animate-spin"></div>
                            {t("openPage.loadingTemplates")}
                        </div>
                    {:else if templates.length > 0}
                        <div class="flex flex-col gap-1">
                            <p class="text-sm text-surface-600-400">{t("openPage.gameType")}</p>
                            <div class="flex gap-4">
                                {#each [{ value: "textadventure", label: t("common.textAdventure") }, { value: "gamebook", label: t("common.gamebook") }] as type (type.value)}
                                    <label class="flex items-center gap-2 cursor-pointer">
                                        <input
                                            type="radio"
                                            class="radio"
                                            name="gametype"
                                            checked={selectedTemplate?.type === type.value}
                                            disabled={creating}
                                            onchange={() => {
                                                if (type.value === "gamebook") {
                                                    const preferred = gamebookTemplates.find(tpl => tpl.id === lastGamebookTemplateId);
                                                    const localeDefault = gamebookTemplates.find(tpl => tpl.label === defaultGamebookTemplateLabel())
                                                        ?? gamebookTemplates.find(tpl => tpl.label === "Gamebook");
                                                    selectedTemplateId = (preferred ?? localeDefault ?? gamebookTemplates[0])?.id ?? "";
                                                } else {
                                                    const preferred = textAdventureTemplates.find(tpl => tpl.id === lastTextAdventureTemplateId);
                                                    const localeDefault = textAdventureTemplates.find(tpl => tpl.label === defaultTemplateLabel())
                                                        ?? textAdventureTemplates.find(tpl => tpl.label === "English");
                                                    selectedTemplateId = (preferred ?? localeDefault ?? textAdventureTemplates[0])?.id ?? "";
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
                                {#each textAdventureTemplates as tpl (tpl.id)}
                                    <option value={tpl.id}>{tpl.label}</option>
                                {/each}
                            </select>
                        {:else if selectedTemplate?.type === "gamebook" && gamebookTemplates.length > 1}
                            <select class="select" bind:value={selectedTemplateId} disabled={creating}>
                                {#each gamebookTemplates as tpl (tpl.id)}
                                    <option value={tpl.id}>{gamebookTemplateDisplayLabel(tpl.label)}</option>
                                {/each}
                            </select>
                        {/if}
                    {/if}

                    {#if creating}
                        <div class="flex items-center gap-3">
                            <div class="size-5 rounded-full border-2 border-surface-300-700 border-t-primary-500 animate-spin"></div>
                            <span class="text-surface-600-400 text-sm">{$loadingStatus || t("openPage.creating")}</span>
                        </div>
                    {:else}
                        <div class="flex gap-2">
                            <div class="flex flex-col gap-1 flex-1 min-w-0">
                                <button
                                    type="button"
                                    class="btn preset-filled-primary-500 w-full whitespace-normal h-auto"
                                    onclick={handleCreateLocal}
                                    disabled={!createName.trim()}
                                    title={isElectronApp ? t("openPage.createFolderTitle") : t("openPage.createLocalDraftTitle")}
                                >
                                    {isElectronApp ? t("openPage.createButton") : t("openPage.createLocalDraftButton")}
                                </button>
                                {#if !isElectronApp}
                                    <p class="text-xs text-surface-600-400 text-center">{t("openPage.storedInBrowserOnly")}</p>
                                {/if}
                            </div>
                            {#if canUseFSA}
                                <div class="flex flex-col gap-1 flex-1 min-w-0">
                                    <button
                                        type="button"
                                        class="btn preset-outlined-primary-500 w-full whitespace-normal h-auto"
                                        onclick={handleCreateLocalFolder}
                                        disabled={!createName.trim()}
                                        title={t("openPage.saveToFolderTitle")}
                                    >
                                        {t("openPage.saveToFolderButton")}
                                    </button>
                                    <p class="text-xs text-surface-600-400 text-center">{t("openPage.chooseFolderOnDevice")}</p>
                                </div>
                            {/if}
                        </div>

                        {#if isElectron()}
                            <p class="text-xs text-surface-600-400 text-center self-center">
                                {t("openPage.willBeCreatedIn")}<br />
                                <span class="font-mono">{electronGamesDir || "…"}</span>
                                <button type="button" class="anchor" onclick={handleChangeLocation}>{t("openPage.changeLocation")}</button>
                            </p>
                        {/if}
                    {/if}

                    {#if createLocalError}
                        <p class="text-error-500 text-sm whitespace-pre-wrap">{createLocalError}</p>
                    {/if}
                </div>
            {/if}

        </div>
    {/if}
</main>
