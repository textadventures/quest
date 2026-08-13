<script lang="ts">
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import {
        lastFailedGameBytes, lastFailedGameFilename, lastOpenGameError,
        retryFailedLoad, listSafeModeLibraryFiles, getAssetText, putAssetText,
    } from "$lib/editor-store";
    import { showToast } from "$lib/toast";
    import CodeEditor from "./CodeEditor.svelte";
    import { t } from "$lib/i18n";

    interface PanelFile {
        filename: string;
        isMain: boolean;
    }

    // The main file plus every candidate library file already sitting in the adapter's asset
    // storage (see listSafeModeLibraryFiles) — independent of whether the game has ever parsed,
    // unlike CodeViewPanel's treeNodes-derived list. Fetched once; a Safe Mode session's file set
    // doesn't change (Save only overwrites an existing file's content).
    let libraryFiles = $state<string[]>([]);
    const files: PanelFile[] = $derived([
        { filename: $lastFailedGameFilename ?? "game.aslx", isMain: true },
        ...libraryFiles.map(f => ({ filename: f, isMain: false })),
    ]);

    // Per-file buffers, keyed by filename, kept in memory for the life of this Safe Mode session —
    // unlike CodeViewPanel, switching files here never discards in-progress edits, since "Try
    // Loading Again" must be reachable (see its own comment) regardless of which file is on screen
    // when you click it, and that only works if the main file's buffer survives you wandering off
    // to fix a library first.
    let buffers = $state<Record<string, string>>({});
    let originalBuffers = $state<Record<string, string>>({});
    let loadedFiles = $state<Set<string>>(new Set());

    let selectedIndex = $state(0);
    let loading = $state(false);
    let loadError = $state<string | null>(null);
    let applying = $state(false);
    let saving = $state(false);

    const currentFile = $derived(files[selectedIndex]);
    const currentXml = $derived(buffers[currentFile?.filename ?? ""] ?? "");
    const hasChanges = $derived(buffers[currentFile?.filename ?? ""] !== originalBuffers[currentFile?.filename ?? ""]);

    // Fetches a file's content the first time it's selected; a no-op on every subsequent
    // selection so re-visiting a file never clobbers whatever's already in its buffer.
    async function ensureLoaded(index: number) {
        const file = files[index];
        if (!file || loadedFiles.has(file.filename)) return;
        loading = true;
        try {
            // The main file's content is exactly the bytes the failed load attempted — there's no
            // live model to read it back from (see retryFailedLoad's own comment).
            let content: string | null = file.isMain
                ? new TextDecoder().decode($lastFailedGameBytes ?? new Uint8Array())
                : await getAssetText(file.filename);
            if (content === null) {
                loadError = t("codeViewPanel.fileNotFound");
                content = "";
            }
            buffers[file.filename] = content;
            originalBuffers[file.filename] = content;
            loadedFiles.add(file.filename);
        } finally {
            loading = false;
        }
    }

    function handleChange(value: string) {
        const file = currentFile;
        if (!file) return;
        buffers[file.filename] = value;
    }

    function selectFile(index: number) {
        selectedIndex = index;
        loadError = null;
        void ensureLoaded(index);
    }

    function handleFileChange(e: Event) {
        selectFile(Number((e.target as HTMLSelectElement).value));
    }

    // Plain storage write, no validation possible without a live model to check against (see the
    // plan's own reasoning) — same as editing the file in an external editor. Shared by the
    // explicit Save button and handleTryAgain's pre-retry flush below.
    async function saveLibrary(filename: string): Promise<void> {
        const xml = buffers[filename];
        await putAssetText(filename, xml);
        originalBuffers[filename] = xml;
    }

    // Always re-attempts the load using the main file's buffer specifically — never whatever file
    // happens to be on screen — so this is reachable and correct whether you're looking at the
    // main file or a library you just fixed. Deliberately not gated on hasChanges: a fix that only
    // touched a library file can make an unedited main file load successfully, so this must stay
    // clickable either way. Jumps the view to the main file so the outcome (success, or a fresh
    // error) is shown next to the text it actually applies to.
    async function handleTryAgain() {
        const mainFilename = $lastFailedGameFilename;
        if (!mainFilename) return;
        selectFile(0);
        applying = true;
        try {
            // Flush every library with an edit still sitting in its buffer first — otherwise a fix
            // the author typed but never explicitly clicked Save for is invisible to the retry (it
            // re-stages every library fresh from the adapter, not from these in-memory buffers),
            // and they'd see the exact same failure again despite having "fixed" it.
            for (const file of files) {
                if (!file.isMain && buffers[file.filename] !== originalBuffers[file.filename]) {
                    await saveLibrary(file.filename);
                }
            }
            const mainXml = buffers[mainFilename] ?? "";
            const ok = await retryFailedLoad(mainXml);
            // On success isLoaded flips true and edit/+page.svelte swaps away from this
            // component entirely. On failure, lastOpenGameError already reflects the new
            // attempt — nothing else to do here, the banner below re-renders on its own.
            if (ok) return;
        } finally {
            applying = false;
        }
    }

    // The button below is deliberately not disabled on !hasChanges (only the no-op guard here
    // checks it): CodeEditor only reports edits on blur (see its own comments), and clicking Save
    // is itself what triggers that blur, so a disabled-on-!hasChanges button would look (and
    // actually be) unclickable for as long as the user is still typing — same reasoning as
    // CodeViewPanel's Apply button.
    async function handleSaveLibrary() {
        const file = currentFile;
        if (!file || file.isMain || !hasChanges) return;
        saving = true;
        try {
            await saveLibrary(file.filename);
            showToast(t("safeMode.librarySaved", { filename: file.filename }), "info");
        } finally {
            saving = false;
        }
    }

    onMount(async () => {
        libraryFiles = await listSafeModeLibraryFiles();
        void ensureLoaded(0);
    });
</script>

<div class="flex flex-col h-dvh overflow-hidden safe-area-inset">
    <div class="flex items-center justify-between gap-3 px-4 py-3 border-b border-warning-300-700 bg-warning-100-900">
        <div class="min-w-0">
            <p class="font-semibold text-sm">{t("safeMode.title")}</p>
            <p class="text-xs text-surface-600-400">{t("safeMode.description")}</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
            <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handleTryAgain} disabled={applying || loading}>
                {applying ? t("safeMode.tryingAgain") : t("safeMode.tryAgainLabel")}
            </button>
            <button type="button" class="btn btn-sm preset-tonal" onclick={() => goto(`${base}/open`)}>{t("editPage.backToHome")}</button>
        </div>
    </div>

    {#if $lastOpenGameError}
        <div class="px-4 py-2 bg-error-100-900 border-b border-error-300-700 text-sm text-error-600-400 whitespace-pre-wrap">{$lastOpenGameError}</div>
    {/if}

    <div class="flex flex-col flex-1 min-w-0 overflow-hidden">
        <div class="px-4 pt-3 pb-2 border-b border-surface-200-800 flex items-center gap-2">
            <label for="safe-mode-file-select" class="text-xs text-surface-600-400 shrink-0">{t("codeViewPanel.fileLabel")}</label>
            <select
                id="safe-mode-file-select"
                class="select text-xs py-1 px-2 bg-surface-50-950 flex-1 min-w-0"
                value={selectedIndex}
                onchange={handleFileChange}
                disabled={applying || saving}
            >
                <optgroup label={t("codeViewPanel.gameFileGroup")}>
                    <option value={0}>{files[0]?.filename ?? "game.aslx"}</option>
                </optgroup>
                {#if files.length > 1}
                    <optgroup label={t("codeViewPanel.librariesGroup")}>
                        {#each files.slice(1) as f, i (f.filename)}
                            <option value={i + 1}>{f.filename}</option>
                        {/each}
                    </optgroup>
                {/if}
            </select>
        </div>

        {#if loadError}
            <div class="px-4 py-2 bg-warning-100-900 border-b border-warning-300-700 text-sm text-warning-600-400 whitespace-pre-wrap">{loadError}</div>
        {/if}

        {#if loading || applying}
            <main class="flex-1 flex flex-col items-center justify-center gap-4 p-8">
                <div class="size-8 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
                <p class="text-surface-600-400 text-sm">{applying ? t("safeMode.tryingAgain") : t("codeViewPanel.loadingFile")}</p>
            </main>
        {:else}
            <div class="flex-1 min-h-0 p-2">
                <CodeEditor value={currentXml} language="xml" onChange={handleChange} class="h-full" />
            </div>
        {/if}

        {#if currentFile && !currentFile.isMain}
            <div class="flex items-center justify-end gap-2 px-4 py-2 border-t border-surface-200-800">
                <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handleSaveLibrary} disabled={saving || loading}>
                    {saving ? t("safeMode.saving") : t("safeMode.saveLabel")}
                </button>
            </div>
        {/if}
    </div>
</div>
