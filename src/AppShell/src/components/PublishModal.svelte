<script lang="ts">
    import { publishGame, canPublishToServer, getCurrentGameId, listPublishFiles, publishProgress, assetManagerOpen, type PublishFile, type PublishProgress } from "$lib/editor-store";
    import { loadPublishTarget, type PublishTarget } from "$lib/publish-target";
    import { formatFileSize } from "$lib/format-size";
    import { t, tPlural } from "$lib/i18n";
    import { trapFocus } from "$lib/actions/trapFocus";

    interface Props {
        oncancel: () => void;
    }
    const { oncancel }: Props = $props();

    // textadventures.co.uk's upload limit. HTML exports have no hard limit, but past this a
    // player waits a long time before the game can start, so the same threshold warns there too.
    const LARGE_PUBLISH_BYTES = 50e6;
    // Files at least this big are highlighted in the list — the likeliest ones not to belong.
    const LARGE_FILE_BYTES = 5e6;

    let target = $state<PublishTarget>("quest");
    // Start on whichever target this game was last published to. The load is async
    // (IPC in Electron), so don't override a choice the user has already made meanwhile.
    let targetTouched = false;
    const gameId = getCurrentGameId();
    if (gameId) {
        void loadPublishTarget(gameId).then((stored) => {
            if (stored && !targetTouched) target = stored;
        });
    }

    let files = $state<PublishFile[] | null>(null);
    let folder = $state<string | null>(null);
    let filesError = $state("");
    let showFiles = $state(false);
    void listPublishFiles().then(
        (result) => {
            files = result.files.toSorted((a, b) => (b.size ?? 0) - (a.size ?? 0));
            folder = result.folder;
        },
        (err: unknown) => { filesError = String(err); },
    );
    const sizesKnown = $derived(files !== null && files.every(f => f.size !== null));
    const totalSize = $derived(files?.reduce((sum, f) => sum + (f.size ?? 0), 0) ?? 0);
    const tooLarge = $derived(sizesKnown && totalSize > LARGE_PUBLISH_BYTES);
    // Split around {link} so "Assets" can be rendered as a link that opens that dialog.
    const fixAssetsParts = $derived(t("publishModal.sizeWarningFixAssets").split("{link}"));

    let abort: AbortController | null = null;
    let publishing = $state(false);
    let error = $state("");
    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { dialogEl?.focus(); });

    const options = $derived<{ value: PublishTarget; title: string; detail: string }[]>([
        $canPublishToServer
            ? { value: "quest", title: t("publishModal.optionServerTitle"), detail: t("publishModal.optionServerDetail") }
            : { value: "quest", title: t("publishModal.optionQuestTitle"), detail: t("publishModal.optionQuestDetail") },
        { value: "cdn", title: t("publishModal.optionCdnTitle"), detail: t("publishModal.optionCdnDetail") },
        { value: "zip", title: t("publishModal.optionZipTitle"), detail: t("publishModal.optionZipDetail") },
    ]);

    function progressLabel(progress: PublishProgress | null): string {
        switch (progress?.step) {
            case "reading": return t("publishModal.progressReading", { done: progress.done + 1, total: progress.total });
            case "building": return t("publishModal.progressBuilding");
            case "uploading": return t("publishModal.progressUploading");
            case "fetchingPlayer": return t("publishModal.progressFetchingPlayer");
            case "zipping": return t("publishModal.progressZipping");
            default: return t("publishModal.publishing");
        }
    }

    async function handlePublish() {
        publishing = true;
        error = "";
        abort = new AbortController();
        try {
            await publishGame(target, abort.signal);
            oncancel();
        } catch (err) {
            if (!abort.signal.aborted) error = String(err);
        } finally {
            publishing = false;
            abort = null;
        }
    }

    // Closing mid-publish cancels it (between files; the synchronous build step can't be
    // interrupted, but the page can't take input during it anyway).
    function close() {
        abort?.abort();
        oncancel();
    }

    function manageAssets() {
        oncancel();
        assetManagerOpen.set(true);
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) close();
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") close();
    }
</script>

<div
    bind:this={dialogEl}
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50 p-4"
    onclick={onBackdropClick}
    onkeydown={handleKeydown}
    use:trapFocus
>
    <div class="card bg-surface-50-950 rounded-xl shadow-xl w-full max-w-[28rem] p-6 flex flex-col gap-4">
        <div class="flex items-center justify-between">
            <h2 class="text-base font-semibold">{t("publishModal.title")}</h2>
            <button class="btn btn-sm preset-tonal" onclick={close}>{t("common.close")}</button>
        </div>

        <p class="text-sm text-surface-600-400">{t("publishModal.description")}</p>

        <fieldset class="flex flex-col gap-3" disabled={publishing}>
            <legend class="sr-only">{t("publishModal.title")}</legend>
            {#each options as option (option.value)}
                <label class="flex items-start gap-2 text-sm cursor-pointer">
                    <!-- One line tall (1lh, not a fixed h-5: the modal's text-sm doesn't resolve to a
                         20px line), so the radio centres on the caption's first line. -->
                    <span class="flex h-[1lh] shrink-0 items-center">
                        <input type="radio" name="publish-target" value={option.value} bind:group={target} onchange={() => (targetTouched = true)} />
                    </span>
                    <span>
                        <span class="font-medium">{option.title}</span>
                        <span class="text-surface-600-400"> — {option.detail}</span>
                    </span>
                </label>
            {/each}
        </fieldset>

        {#if target !== "quest"}
            <p class="publish-footnote text-xs text-surface-500">{t("publishModal.htmlFootnote")}</p>
        {/if}

        {#if filesError}
            <p class="text-sm text-error-500">{t("publishModal.filesError", { error: filesError })}</p>
        {:else if files && files.length > 0}
            <section class="publish-files rounded-lg bg-surface-100-900 p-3 text-sm flex flex-col gap-1">
                <div class="flex items-baseline justify-between gap-2">
                    <p class="publish-files-summary">
                        {sizesKnown
                            ? tPlural("publishModal.filesSummary", files.length, { size: formatFileSize(totalSize) })
                            : tPlural("publishModal.filesSummaryNoSize", files.length)}
                    </p>
                    <button type="button" class="anchor shrink-0 text-xs" aria-expanded={showFiles} onclick={() => (showFiles = !showFiles)}>
                        {showFiles ? t("publishModal.hideFiles") : t("publishModal.showFiles")}
                    </button>
                </div>
                {#if folder}
                    <p class="text-xs text-surface-600-400 break-all">{t("publishModal.filesFolder", { folder })}</p>
                {/if}
                {#if showFiles}
                    <ul class="publish-files-list mt-1 max-h-40 overflow-y-auto text-xs">
                        {#each files as file (file.name)}
                            <li class="flex justify-between gap-3 py-0.5" class:text-warning-700-300={file.size !== null && file.size >= LARGE_FILE_BYTES} class:font-medium={file.size !== null && file.size >= LARGE_FILE_BYTES}>
                                <span class="truncate" title={file.name}>{file.name}</span>
                                {#if file.size !== null}<span class="shrink-0 tabular-nums">{formatFileSize(file.size)}</span>{/if}
                            </li>
                        {/each}
                    </ul>
                {/if}
                <!-- Deleting in the Assets dialog removes the file from disk for real folders — fine for a
                     game's own files, not for unrelated ones in a shared folder, hence the size warning's
                     folder-specific advice below. -->
                <button type="button" class="publish-manage-assets anchor self-start text-xs" disabled={publishing} onclick={manageAssets}>{t("publishModal.manageAssets")}</button>
            </section>
        {/if}

        {#if tooLarge}
            <p class="publish-size-warning text-sm text-warning-700-300" role="alert">
                {target === "quest" ? t("publishModal.sizeWarningQuest") : t("publishModal.sizeWarningHtml")}
                {#if folder}
                    {t("publishModal.sizeWarningFixFolder", { folder })}
                {:else}
                    {fixAssetsParts[0]}<button type="button" class="publish-warning-assets-link anchor" disabled={publishing} onclick={manageAssets}>{t("assetManager.title")}</button>{fixAssetsParts[1] ?? ""}
                {/if}
            </p>
        {/if}

        {#if error}<p class="text-xs text-error-500">{error}</p>{/if}

        <div class="flex items-center gap-3">
            {#if publishing}
                <button type="button" class="btn btn-sm preset-tonal" onclick={close}>{t("common.cancel")}</button>
                <p class="publish-progress text-sm text-surface-600-400" role="status">{progressLabel($publishProgress)}</p>
            {:else}
                <button
                    type="button"
                    class="btn btn-sm preset-filled-primary-500"
                    onclick={handlePublish}
                >{target === "quest" && $canPublishToServer ? t("publishModal.publishToServer") : t("publishModal.publishButton")}</button>
            {/if}
        </div>
    </div>
</div>
