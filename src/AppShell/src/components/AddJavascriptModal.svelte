<script lang="ts">
    import { assets, treeNodes, uniqueAssetName, putAssetText } from "$lib/editor-store";
    import { t } from "$lib/i18n";
    import AssetPicker from "./AssetPicker.svelte";

    interface Props {
        onconfirm: (src: string) => void;
        oncancel: () => void;
    }

    const { onconfirm, oncancel }: Props = $props();

    // Pre-filled with a free filename so accepting the default just works — typing over it
    // targets a different new file, and picking from the dropdown targets an existing one.
    let src = $state(uniqueAssetName("javascript.js"));
    let confirming = $state(false);

    // .js files already pointed to by another Javascript element — no point offering those in
    // the "pick an existing file" dropdown, since each element needs its own file (src is locked
    // once set; see PropertyEditor's "lockedAfterCreate"). A Javascript node's tree label is
    // exactly its src (EditorController.GetDisplayName), so this needs no extra bridge call.
    let alreadyUsed = $derived($treeNodes.filter(n => n.nodeType === "javascript").map(n => n.text));

    let dialogEl = $state<HTMLDivElement>();
    $effect(() => { dialogEl?.focus(); });

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") oncancel();
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    async function confirm() {
        if (!src || confirming) return;
        confirming = true;
        try {
            // AssetPicker only creates the blank asset when its own onchange fires (typing +
            // blur/select) — accepting the pre-filled default untouched never triggers that, so
            // guarantee the asset exists here regardless of how `src` got its value.
            if (!$assets.some(a => a.key === src)) {
                await putAssetText(src, "");
            }
            onconfirm(src);
        } finally {
            confirming = false;
        }
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
        <h2 class="text-base font-semibold">{t("elementAdders.javascript")}</h2>

        <div class="flex flex-col gap-1">
            <span class="text-xs text-surface-600-400">
                {t("addJavascriptModal.helpText")}
            </span>
            <AssetPicker
                value={src}
                source="*.js"
                creatable
                exclude={alreadyUsed}
                onchange={(v) => src = v}
                onEnter={confirm}
                containerClass="w-full"
            />
        </div>

        <div class="flex justify-end gap-2">
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>{t("common.cancel")}</button>
            <button
                class="btn btn-sm preset-filled-primary-500"
                onclick={confirm}
                disabled={!src || confirming}
            >
                {t("elementAdders.javascript")}
            </button>
        </div>
    </div>
</div>
