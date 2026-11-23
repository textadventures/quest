<script lang="ts">
    import { onMount } from "svelte";
    import { publishGame, buildPackageForUpload, rememberPublishTarget, getCurrentGameId, listPublishFiles, publishProgress, assetManagerOpen, checkGame, checkPanelOpen, type PublishFile, type PublishProgress } from "#lib/editor-store.js";
    import { loadPublishTarget, DEFAULT_PUBLISH_TARGET, type PublishTarget } from "#lib/publish-target.js";
    import { uploadGame, checkStatus, UploadError, type Upload } from "#lib/publish-online.js";
    import { formatFileSize } from "#lib/format-size.js";
    import { t, tPlural } from "#lib/i18n/index.js";
    import { trapFocus } from "#lib/actions/trapFocus.js";

    interface Props {
        oncancel: () => void;
    }
    const { oncancel }: Props = $props();

    // textadventures.co.uk's upload limit. HTML exports have no hard limit, but past this a
    // player waits a long time before the game can start, so the same threshold warns there too.
    const LARGE_PUBLISH_BYTES = 50e6;
    // Files at least this big are highlighted in the list — the likeliest ones not to belong.
    const LARGE_FILE_BYTES = 5e6;

    let target = $state<PublishTarget>(DEFAULT_PUBLISH_TARGET);
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
        { value: "online", title: t("publishModal.optionOnlineTitle"), detail: t("publishModal.optionOnlineDetail") },
        { value: "quest", title: t("publishModal.optionQuestTitle"), detail: t("publishModal.optionQuestDetail") },
        { value: "cdn", title: t("publishModal.optionCdnTitle"), detail: t("publishModal.optionCdnDetail") },
        { value: "zip", title: t("publishModal.optionZipTitle"), detail: t("publishModal.optionZipDetail") },
    ]);

    function progressLabel(progress: PublishProgress | null): string {
        switch (progress?.step) {
            case "reading": return t("publishModal.progressReading", { done: progress.done + 1, total: progress.total });
            case "building": return t("publishModal.progressBuilding");
            case "fetchingPlayer": return t("publishModal.progressFetchingPlayer");
            case "zipping": return t("publishModal.progressZipping");
            default: return t("publishModal.publishing");
        }
    }

    // Publish checks the game first, the last chance to catch its problems before players do. Any
    // it finds are shown here, with the choice to look at them or publish anyway; the check runs
    // once per opening of this dialog, so Publish anyway doesn't repeat it.
    let checking = $state(false);
    let problemCount = $state<number | null>(null);

    async function handlePublishClick() {
        if (problemCount === null) {
            checking = true;
            error = "";
            try {
                problemCount = (await checkGame()).length;
            } catch (err) {
                // A check that fails shouldn't stop the game being published.
                console.error("Check game failed", err);
                problemCount = 0;
            } finally {
                checking = false;
            }
            if (problemCount > 0) return;
        }
        await handlePublish();
    }

    function showProblems() {
        oncancel();
        checkPanelOpen.set(true);
    }

    // Publishing to textadventures.co.uk carries on in this dialog once the package is uploaded:
    // the author goes to a page there to finish (they're signed in there, not here), and this
    // says when it's done. See #lib/publish-online.ts.
    type OnlineStage =
        | { name: "uploading"; progress: number }
        | { name: "uploaded"; upload: Upload; opened: boolean }
        | { name: "published"; gameUrl?: string }
        | { name: "failed"; message: string };
    let online = $state<OnlineStage | null>(null);
    // How often to ask textadventures.co.uk whether the author has finished there
    const POLL_INTERVAL = 3000;

    function uploadErrorMessage(err: unknown): string {
        if (!(err instanceof UploadError)) return String(err);
        switch (err.status) {
            case 413: return t("publishModal.uploadErrorTooBig");
            case 429: return t("publishModal.uploadErrorRateLimited");
            case 0: return t("publishModal.uploadErrorOffline");
            default: return t("publishModal.uploadErrorRefused", { status: err.status });
        }
    }

    async function publishOnline(signal: AbortSignal) {
        const { bytes, filename } = await buildPackageForUpload(signal);
        signal.throwIfAborted();
        online = { name: "uploading", progress: 0 };
        try {
            const upload = await uploadGame(bytes, filename, {
                signal,
                onprogress: fraction => {
                    if (online?.name === "uploading") online = { name: "uploading", progress: fraction };
                },
            });
            rememberPublishTarget("online");
            // A browser only lets a page open a tab in answer to a click, and for a few seconds
            // after it, so after a long upload the author clicks the link instead.
            const opened = navigator.userActivation?.isActive ? !!window.open(upload.claimUrl, "_blank", "noopener") : false;
            online = { name: "uploaded", upload, opened };
        } catch (err) {
            if (signal.aborted) throw err;
            online = { name: "failed", message: uploadErrorMessage(err) };
        }
    }

    async function checkOnline() {
        if (online?.name !== "uploaded") return;
        const id = online.upload.id;
        const status = await checkStatus(id);
        if (online?.name !== "uploaded" || online.upload.id !== id) return;
        if (status?.status === "published") online = { name: "published", gameUrl: status.gameUrl };
        else if (status?.status === "gone") online = { name: "failed", message: t("publishModal.uploadErrorExpired") };
    }

    onMount(() => {
        const timer = setInterval(() => void checkOnline(), POLL_INTERVAL);
        // Coming back from the tab where they finished
        const onfocus = () => void checkOnline();
        window.addEventListener("focus", onfocus);
        return () => {
            clearInterval(timer);
            window.removeEventListener("focus", onfocus);
        };
    });

    async function handlePublish() {
        publishing = true;
        error = "";
        abort = new AbortController();
        try {
            if (target === "online") {
                await publishOnline(abort.signal);
            } else {
                await publishGame(target, abort.signal);
                oncancel();
            }
        } catch (err) {
            if (!abort.signal.aborted) {
                error = String(err);
                online = null;
            }
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
            <h2 class="text-base font-semibold">{online ? t("publishModal.onlineTitle") : t("publishModal.title")}</h2>
            <button class="btn btn-xs preset-tonal" onclick={close}>{t("common.close")}</button>
        </div>

        {#if online}
            <div class="publish-online flex flex-col gap-3 text-sm" aria-live="polite">
                {#if online.name === "uploading"}
                    <p>{t("publishModal.uploading")}</p>
                    <progress class="progress w-full" value={online.progress} max="1" aria-label={t("publishModal.uploading")}></progress>
                    <button type="button" class="btn btn-xs preset-tonal self-start" onclick={close}>{t("common.cancel")}</button>
                {:else if online.name === "uploaded"}
                    <p>{online.opened ? t("publishModal.uploadedOpened") : t("publishModal.uploadedContinue")}</p>
                    <a class="publish-continue btn preset-filled-primary-500 self-start" href={online.upload.claimUrl} target="_blank" rel="noopener">
                        {t("publishModal.continueButton")}
                    </a>
                    <p class="text-xs text-surface-600-400">{t("publishModal.uploadedNote")}</p>
                {:else if online.name === "published"}
                    <p class="publish-published">{t("publishModal.published")}</p>
                    {#if online.gameUrl}
                        <a class="publish-view-game btn preset-filled-primary-500 self-start" href={online.gameUrl} target="_blank" rel="noopener">
                            {t("publishModal.viewGame")}
                        </a>
                    {/if}
                {:else}
                    <p class="publish-upload-error text-error-500" role="alert">{online.message}</p>
                    <div class="flex items-center gap-3">
                        <button type="button" class="btn btn-xs preset-filled-primary-500" onclick={handlePublish}>{t("publishModal.tryAgain")}</button>
                        <button type="button" class="btn btn-xs preset-tonal" onclick={() => (online = null)}>{t("publishModal.back")}</button>
                    </div>
                {/if}
            </div>
        {:else}
            <p class="text-sm text-surface-600-400">{t("publishModal.description")}</p>

            <fieldset class="flex flex-col gap-3" disabled={publishing || checking}>
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

            {#if target === "cdn" || target === "zip"}
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
                    {target === "quest" || target === "online" ? t("publishModal.sizeWarningQuest") : t("publishModal.sizeWarningHtml")}
                    {#if folder}
                        {t("publishModal.sizeWarningFixFolder", { folder })}
                    {:else}
                        {fixAssetsParts[0]}<button type="button" class="publish-warning-assets-link anchor" disabled={publishing} onclick={manageAssets}>{t("assetManager.title")}</button>{fixAssetsParts[1] ?? ""}
                    {/if}
                </p>
            {/if}

            {#if problemCount}
                <p class="publish-problems-warning text-sm text-warning-700-300" role="alert">{tPlural("publishModal.problems", problemCount)}</p>
            {/if}

            {#if error}<p class="text-xs text-error-500">{error}</p>{/if}

            <div class="flex items-center gap-3">
                {#if publishing}
                    <button type="button" class="btn btn-xs preset-tonal" onclick={close}>{t("common.cancel")}</button>
                    <p class="publish-progress text-sm text-surface-600-400" role="status">{progressLabel($publishProgress)}</p>
                {:else if checking}
                    <p class="publish-checking text-sm text-surface-600-400" role="status">{t("publishModal.checking")}</p>
                {:else if problemCount}
                    <button type="button" class="publish-show-problems btn btn-xs preset-filled-primary-500" onclick={showProblems}>{t("publishModal.showProblems")}</button>
                    <button type="button" class="publish-anyway btn btn-xs preset-tonal" onclick={handlePublish}>{t("publishModal.publishAnyway")}</button>
                {:else}
                    <button
                        type="button"
                        class="btn btn-xs preset-filled-primary-500"
                        onclick={handlePublishClick}
                    >{t("publishModal.publishButton")}</button>
                {/if}
            </div>
        {/if}
    </div>
</div>
