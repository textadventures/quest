<script lang="ts">
    import { treeNodes, uploadAsset } from "$lib/editor-store";
    import { t } from "$lib/i18n";
    import FileIcon from "@lucide/svelte/icons/file";

    interface Props {
        onconfirm: (filename: string) => void;
        oncancel: () => void;
    }

    const { onconfirm, oncancel }: Props = $props();

    let filename = $state("");
    let uploading = $state(false);
    let error = $state("");
    let inputEl: HTMLInputElement;

    // .aslx files already pointed to by another Included Library — each library element needs
    // its own file (filename is locked once set; see CoreEditorIncludedLibrary's
    // <lockedaftercreate/>), so uploading the same name here would silently overwrite that
    // element's content out from under it. An Included Library node's tree label is exactly its
    // filename (EditorController.GetDisplayName), so this needs no extra bridge call.
    let alreadyUsed = $derived(new Set($treeNodes.filter(n => n.nodeType === "include").map(n => n.text)));

    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { dialogEl?.focus(); });

    async function handleUpload(e: Event) {
        const target = e.target as HTMLInputElement;
        const file = target.files?.[0];
        target.value = "";
        if (!file) return;
        if (alreadyUsed.has(file.name)) {
            error = t("addLibraryModal.alreadyUsedError", { name: file.name });
            return;
        }
        uploading = true;
        error = "";
        try {
            await uploadAsset(file);
            filename = file.name;
        } catch (err) {
            error = String(err);
        } finally {
            uploading = false;
        }
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") oncancel();
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    function confirm() {
        if (!filename || uploading) return;
        onconfirm(filename);
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
>
    <div class="card bg-surface-50-950 rounded-xl shadow-xl w-full max-w-96 p-6 flex flex-col gap-4">
        <h2 class="text-base font-semibold">{t("addLibraryModal.title")}</h2>

        <div class="flex flex-col gap-1">
            <span class="text-xs text-surface-600-400">
                {t("addLibraryModal.helpText")}
            </span>
            <div class="flex items-center gap-1.5 min-w-0">
                {#if filename}
                    <span class="shrink-0 opacity-70"><FileIcon size={14} aria-hidden="true" /></span>
                    <span class="text-xs truncate flex-1 min-w-0" title={filename}>{filename}</span>
                {:else}
                    <span class="text-xs text-surface-600-400 flex-1 min-w-0">{t("addLibraryModal.noFileChosen")}</span>
                {/if}
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs px-1.5 py-0.5 whitespace-nowrap shrink-0"
                    onclick={() => inputEl.click()}
                    disabled={uploading}
                >{uploading ? t("assetManager.uploading") : filename ? t("addLibraryModal.change") : t("assetManager.upload")}</button>
                <input bind:this={inputEl} type="file" accept=".aslx,.xml" class="hidden" onchange={handleUpload} />
            </div>
            {#if error}<p class="text-xs text-error-500">{error}</p>{/if}
        </div>

        <div class="flex justify-end gap-2">
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>{t("common.cancel")}</button>
            <button
                class="btn btn-sm preset-filled-primary-500"
                onclick={confirm}
                disabled={!filename || uploading}
            >
                {t("elementAdders.library")}
            </button>
        </div>
    </div>
</div>
