<script lang="ts">
    import Combobox from "./Combobox.svelte";
    import AssetPicker from "./AssetPicker.svelte";
    import { t } from "$lib/i18n";
    import { trapFocus } from "$lib/actions/trapFocus";
    import type { ControlOption } from "$lib/types";

    interface Props {
        title: string;
        targetLabel: string;
        targetOptions: ControlOption[];
        // When set, the target is picked via AssetPicker (existing assets + upload) instead of a Combobox.
        assetSource?: string | null;
        textMode: "none" | "optional" | "required";
        textLabel?: string;
        initialText?: string;
        onconfirm: (target: string, text: string) => void;
        oncancel: () => void;
    }

    const {
        title, targetLabel, targetOptions, assetSource = null,
        textMode, textLabel = t("linkPickerModal.defaultTextLabel"), initialText = "",
        onconfirm, oncancel,
    }: Props = $props();

    let target = $state("");
    // This component is always freshly mounted per link-edit invocation (see
    // PropertyEditor's `{#if activeLinkCommand}`), so initialText never changes
    // on an existing instance: intentional one-time snapshot.
    // svelte-ignore state_referenced_locally
    let text = $state(initialText);
    let textInputEl: HTMLInputElement | undefined = $state();

    $effect(() => { textInputEl?.focus(); });

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Escape") oncancel();
    }

    function handleTextKeydown(e: KeyboardEvent) {
        if (e.key === "Enter") {
            e.preventDefault();
            confirm();
        }
    }

    let canConfirm = $derived(!!target && (textMode !== "required" || !!text.trim()));

    function confirm() {
        if (!canConfirm) return;
        onconfirm(target, text.trim());
    }
</script>

<div
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    class="fixed inset-0 bg-black/30 flex items-center justify-center z-50 p-4"
    onclick={onBackdropClick}
    onkeydown={handleKeydown}
    use:trapFocus
>
    <div class="card bg-surface-50-950 rounded-xl shadow-xl w-full max-w-96 p-6 flex flex-col gap-4">
        <h2 class="text-base font-semibold">{title}</h2>

        <div class="flex flex-col gap-1">
            <label for="link-picker-target" class="text-xs text-surface-600-400">{targetLabel}</label>
            {#if assetSource !== null}
                <AssetPicker
                    value={target}
                    source={assetSource}
                    onchange={(v) => { target = v; }}
                    class="input bg-surface-50-950 px-2 py-1 text-sm w-full min-w-0"
                    containerClass="w-full"
                />
            {:else}
                <Combobox
                    value={target}
                    options={targetOptions}
                    onchange={(v) => { target = v; }}
                    class="input bg-surface-50-950 px-2 py-1 text-sm w-full"
                    wrapperClass="w-full"
                />
                {#if targetOptions.length === 0}
                    <p class="text-xs text-surface-600-400">{t("linkPickerModal.noOptionsFound", { label: targetLabel.toLowerCase() })}</p>
                {/if}
            {/if}
        </div>

        {#if textMode !== "none"}
            <div class="flex flex-col gap-1">
                <label for="link-picker-text" class="text-xs text-surface-600-400">
                    {textLabel}{textMode === "optional" ? t("linkPickerModal.optionalSuffix") : ""}
                </label>
                <input
                    id="link-picker-text"
                    type="text"
                    autocapitalize="off"
                    class="input bg-surface-50-950 px-2 py-1 text-sm"
                    bind:this={textInputEl}
                    bind:value={text}
                    onkeydown={handleTextKeydown}
                />
            </div>
        {/if}

        <div class="flex justify-end gap-2">
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>{t("common.cancel")}</button>
            <button
                class="btn btn-sm preset-filled-primary-500"
                onclick={confirm}
                disabled={!canConfirm}
            >{t("common.insert")}</button>
        </div>
    </div>
</div>
