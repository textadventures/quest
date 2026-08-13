<script lang="ts">
    import { onMount } from "svelte";
    import { assets, treeNodes, refreshAssets, uploadAsset, deleteAssetAndOwner, resolveAssetUrl, isImageAsset } from "$lib/editor-store";
    import { confirmDialog } from "$lib/confirm";
    import { t } from "$lib/i18n";
    import FileIcon from "@lucide/svelte/icons/file";

    interface Props {
        oncancel: () => void;
    }
    const { oncancel }: Props = $props();

    // The asset list is otherwise only refreshed on upload/delete — files
    // dropped into the game folder from outside the app (Finder, etc.) would
    // never show up until the next edit. Re-scan whenever this modal opens.
    onMount(() => { void refreshAssets(); });

    let thumbUrls = $state<Record<string, string>>({});

    $effect(() => {
        for (const asset of $assets) {
            if (isImageAsset(asset.key) && !(asset.key in thumbUrls)) {
                resolveAssetUrl(asset.key).then((url) => {
                    if (url) thumbUrls = { ...thumbUrls, [asset.key]: url };
                });
            }
        }
    });

    let inputEl: HTMLInputElement;
    let uploading = $state(false);
    let error = $state("");

    async function handleUpload(e: Event) {
        const target = e.target as HTMLInputElement;
        const file = target.files?.[0];
        target.value = "";
        if (!file) return;
        uploading = true;
        error = "";
        try {
            await uploadAsset(file);
        } catch (err) {
            error = String(err);
        } finally {
            uploading = false;
        }
    }

    async function handleDelete(key: string) {
        // Unlike model edits (covered by Quest's own Undo), a deleted asset is gone for good —
        // always confirm, and call out a Javascript/Included Library element that owns this file
        // specifically, since deleting it here takes that element down with it too (see
        // deleteAssetAndOwner) rather than leaving it pointing at a missing file.
        const owner = $treeNodes.find(n => (n.nodeType === "javascript" || n.nodeType === "include") && n.text === key);
        const messageKey = owner
            ? (owner.nodeType === "javascript" ? "assetManager.deleteConfirm.usedByJavascript" : "assetManager.deleteConfirm.usedByLibrary")
            : "assetManager.deleteConfirm.plain";
        const ok = await confirmDialog(t(messageKey, { name: key }), { confirmLabel: t("common.delete"), danger: true });
        if (!ok) return;
        await deleteAssetAndOwner(key);
        if (key in thumbUrls) {
            const rest = { ...thumbUrls };
            delete rest[key];
            thumbUrls = rest;
        }
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") oncancel();
    }
</script>

<div
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50 p-4"
    onclick={onBackdropClick}
    onkeydown={handleKeydown}
>
    <div class="card bg-surface-50-950 rounded-xl shadow-xl w-full max-w-[32rem] max-h-[85dvh] p-6 flex flex-col gap-4">
        <div class="flex items-center justify-between">
            <h2 class="text-base font-semibold">{t("assetManager.title")}</h2>
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>{t("common.close")}</button>
        </div>

        <div class="flex items-center gap-2">
            <button
                type="button"
                class="btn btn-sm preset-filled-primary-500"
                onclick={() => inputEl.click()}
                disabled={uploading}
            >{uploading ? t("assetManager.uploading") : t("assetManager.upload")}</button>
            <input bind:this={inputEl} type="file" class="hidden" onchange={handleUpload} />
            {#if error}<p class="text-xs text-error-500">{error}</p>{/if}
        </div>

        <div class="flex-1 overflow-y-auto flex flex-col gap-1">
            {#if $assets.length === 0}
                <p class="text-xs text-surface-600-400">{t("assetManager.noAssets")}</p>
            {/if}
            {#each $assets as asset (asset.key)}
                <div class="flex items-center gap-2 px-1.5 py-1 rounded hover:bg-surface-100-900">
                    {#if thumbUrls[asset.key]}
                        <img src={thumbUrls[asset.key]} alt="" class="h-8 w-8 object-cover rounded border border-surface-200-800 shrink-0" />
                    {:else}
                        <span class="h-8 w-8 rounded border border-surface-200-800 shrink-0 flex items-center justify-center text-surface-500-400"><FileIcon size={16} /></span>
                    {/if}
                    <span class="text-xs flex-1 truncate" title={asset.key}>{asset.key}</span>
                    <button
                        type="button"
                        class="btn btn-sm preset-outlined-error-500 text-xs px-2 py-0.5"
                        onclick={() => handleDelete(asset.key)}
                    >{t("common.delete")}</button>
                </div>
            {/each}
        </div>
    </div>
</div>
