<script lang="ts">
    import { onMount, untrack } from "svelte";
    import { getGameXml, setGameXml, setLibraryXml, getLibraryXml, isBuiltInLibrary, codeViewCloseRequested, treeNodes, gameFilename, getAssetText } from "$lib/editor-store";
    import { chooseDialog, confirmDialog } from "$lib/confirm";
    import CodeEditor from "./CodeEditor.svelte";
    import { t } from "$lib/i18n";

    interface Props {
        onclose: () => void;
    }

    let { onclose }: Props = $props();

    interface PanelFile {
        filename: string;
        isMain: boolean;
        isBuiltIn: boolean;
    }

    // EditorController.GetDisplayName's fallback for an Included Library with no filename set.
    const NO_FILENAME_PLACEHOLDER = "(filename not set)";

    // Files the panel can show: the game's own file first, then every Included Library in tree
    // order. Built-in (engine-shipped) libraries are read-only — they don't belong to the game,
    // so they can't be edited here (see isBuiltInLibrary / WorldModel.IsBuiltInLibrary).
    // treeNodes/gameFilename don't change while the panel is open (applying a change closes it,
    // and the panel replaces the tree), so this is effectively constant for the panel's lifetime.
    const files: PanelFile[] = $derived([
        { filename: $gameFilename ?? "game.aslx", isMain: true, isBuiltIn: false },
        ...$treeNodes
            .filter(n => n.nodeType === "include" && n.text && n.text !== NO_FILENAME_PLACEHOLDER)
            .map(n => ({ filename: n.text!, isMain: false, isBuiltIn: isBuiltInLibrary(n.text!) })),
    ]);

    // Content of the currently selected file. Loaded once per selection, not kept in sync with
    // scriptVersion/undo-redo — re-syncing while the panel is open would fight whatever raw XML
    // the user is mid-edit on.
    let selectedIndex = $state(0);
    let xml = $state("");
    let originalXml = $state("");
    let loading = $state(false);
    let loadError = $state<string | null>(null);
    let applying = $state(false);
    let error = $state<string | null>(null);

    const currentFile = $derived(files[selectedIndex]);
    const hasChanges = $derived(xml !== originalXml);
    const isReadOnly = $derived(currentFile?.isBuiltIn ?? false);

    // Guards against a fast file-switch racing two loads: only the most recent selection's result
    // is allowed to land (a slow older load otherwise wins after the newer one has already shown).
    let loadToken = 0;

    async function loadFile(index: number) {
        const file = files[index];
        if (!file) return;
        const token = ++loadToken;
        loading = true;
        loadError = null;
        error = null;
        try {
            let content: string | null;
            if (file.isMain) {
                content = getGameXml();
            } else if (file.isBuiltIn) {
                content = getLibraryXml(file.filename);
            } else {
                content = await getAssetText(file.filename);
            }
            if (token !== loadToken) return;
            if (content === null) {
                loadError = t("codeViewPanel.fileNotFound");
                content = "";
            }
            originalXml = content;
            xml = content;
            selectedIndex = index;
        } finally {
            if (token === loadToken) loading = false;
        }
    }

    function handleChange(value: string) {
        xml = value;
        error = null;
    }

    // Switching files with unapplied edits asks first — same principle as attemptClose, minus the
    // "Apply changes" choice: Apply always targets the file currently on screen, and a switch
    // abandons it, so Apply-then-switch is the explicit two-step.
    async function handleFileChange(e: Event) {
        const next = Number((e.target as HTMLSelectElement).value);
        if (next === selectedIndex) return;
        if (!hasChanges) {
            await loadFile(next);
            return;
        }
        const choice = await chooseDialog(
            t("codeViewPanel.unsavedChangesMessage"),
            [
                { label: t("codeViewPanel.keepEditing"), value: "keep" as const },
                { label: t("codeViewPanel.discardChanges"), value: "discard" as const },
            ]
        );
        if (choice === "discard") await loadFile(next);
        // Any other outcome ("keep", or Escape/backdrop which resolves null) — stay put; the
        // <select> snaps back to selectedIndex on the next render.
    }

    // Applies without its own confirmation — callers (the Apply button, and the "Apply" choice
    // from the unsaved-changes prompt below) are each responsible for confirming first, so a user
    // routed through the prompt doesn't see a second "are you sure" right after choosing Apply.
    async function applyChanges(): Promise<boolean> {
        // No-op apply: the buffer matches what was loaded, so there's nothing to commit. Skip the
        // reload/validation entirely — running it anyway would raise the "reload needed" banner
        // for a change the user never actually made (apply stays clickable even when the buffer is
        // untouched, because CodeEditor reports edits on blur and the click itself is what blurs).
        if (!hasChanges) return true;
        const file = currentFile;
        if (!file || file.isBuiltIn) return false;
        applying = true;
        error = null;
        try {
            const result = file.isMain
                ? await setGameXml(xml)
                : await setLibraryXml(file.filename, xml);
            if (result === "ok") return true;
            // Keep the panel open with the user's text intact so they can fix and retry —
            // SetGameXml()/SetLibraryXml() never touched the live game on a failure like this.
            error = result.startsWith("error:") ? result.slice("error:".length) : result;
            return false;
        } finally {
            applying = false;
        }
    }

    async function handleApplyButton() {
        const confirmed = await confirmDialog(
            t("codeViewPanel.applyConfirmMessage"),
            { confirmLabel: t("codeViewPanel.applyLabel"), danger: true }
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
            t("codeViewPanel.unsavedChangesMessage"),
            [
                { label: t("codeViewPanel.keepEditing"), value: "keep" as const },
                { label: t("codeViewPanel.discardChanges"), value: "discard" as const },
                { label: t("codeViewPanel.applyChangesChoice"), value: "apply" as const },
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

    // Initial load: the game's own file.
    onMount(() => {
        void loadFile(0);
    });
</script>

{#if applying}
    <!-- Matches the app's main "Loading game…" treatment (edit/+page.svelte) rather than a small
         in-button spinner — reloading the whole model from raw XML is exactly that kind of
         operation, and deserves to be at least as obvious as the initial load. -->
    <main class="flex flex-col items-center justify-center flex-1 gap-6 p-8">
        <div class="size-10 rounded-full border-4 border-surface-300-700 border-t-primary-500 animate-spin"></div>
        <p class="text-surface-600-400 text-sm">{t("codeViewPanel.applyingChanges")}</p>
    </main>
{:else}
    <div class="flex flex-col flex-1 min-w-0 overflow-hidden">
        <div class="px-4 pt-3 pb-2 border-b border-surface-200-800 flex items-center gap-2">
            <label for="code-view-file-select" class="text-xs text-surface-600-400 shrink-0">{t("codeViewPanel.fileLabel")}</label>
            <select
                id="code-view-file-select"
                class="select text-xs py-1 px-2 bg-surface-50-950 flex-1 min-w-0"
                value={selectedIndex}
                onchange={handleFileChange}
                disabled={applying}
            >
                <optgroup label={t("codeViewPanel.gameFileGroup")}>
                    <option value={0}>{files[0]?.filename ?? "game.aslx"}</option>
                </optgroup>
                {#if files.length > 1}
                    <optgroup label={t("codeViewPanel.librariesGroup")}>
                        {#each files.slice(1) as f, i (f.filename)}
                            <option value={i + 1}>{f.filename}{f.isBuiltIn ? ` (${t("codeViewPanel.readOnly")})` : ""}</option>
                        {/each}
                    </optgroup>
                {/if}
            </select>
        </div>

        {#if error}
            <div class="px-4 py-2 bg-error-100-900 border-b border-error-300-700 text-sm text-error-600-400 whitespace-pre-wrap">{error}</div>
        {/if}
        {#if loadError}
            <div class="px-4 py-2 bg-warning-100-900 border-b border-warning-300-700 text-sm text-warning-600-400 whitespace-pre-wrap">{loadError}</div>
        {/if}
        {#if isReadOnly}
            <div class="px-4 py-2 bg-surface-100-900 border-b border-surface-200-800 text-xs text-surface-600-400">{t("codeViewPanel.builtInReadOnlyMessage")}</div>
        {/if}

        {#if loading}
            <div class="flex-1 flex items-center justify-center gap-2 text-sm text-surface-600-400">
                <div class="size-4 rounded-full border-2 border-surface-300-700 border-t-primary-500 animate-spin"></div>
                <p>{t("codeViewPanel.loadingFile")}</p>
            </div>
        {:else}
            <!-- data-staging: edits here are a local scratch buffer until Apply commits them — opt out
                 of the page-level isEditingField/"Unsaved" tracking that assumes input events mean an
                 edit already landed in the bridge, same as ListEditor/DictionaryEditor. -->
            <div class="flex-1 min-h-0 p-2" data-staging>
                <CodeEditor value={xml} language="xml" readonly={isReadOnly} onChange={handleChange} class="h-full" />
            </div>
        {/if}

        <div class="flex items-center justify-end gap-2 px-4 py-2 border-t border-surface-200-800">
            <button type="button" class="btn btn-sm preset-tonal" onclick={attemptClose}>{t("common.cancel")}</button>
            <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handleApplyButton} disabled={applying || loading || isReadOnly}>{t("codeViewPanel.applyLabel")}</button>
        </div>
    </div>
{/if}
