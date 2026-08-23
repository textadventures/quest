<script lang="ts">
    import { untrack } from "svelte";
    import { validateName, getFunctionFolders, getPossibleNewObjectParents } from "$lib/editor-store";
    import { t } from "$lib/i18n";
    import Combobox from "$components/Combobox.svelte";
    import type { ControlOption } from "$lib/types";

    interface Props {
        elementType: "room" | "object" | "page" | "function" | "timer" | "walkthrough" | "template" | "dynamictemplate" | "type";
        parent?: string | null;
        folder?: string | null;
        onconfirm: (name: string, target: string | null) => void;
        oncancel: () => void;
    }

    const { elementType, parent = null, folder = null, onconfirm, oncancel }: Props = $props();

    let label = $derived(t(`addElementModal.labels.${elementType}`));
    let heading = $derived(parent ? t("addElementModal.addHeadingIn", { label, parent }) : t("addElementModal.addHeading", { label }));

    // Folder picker (Functions only) — lets a function be created straight into an existing or
    // brand-new folder, defaulting to the folder whose "..." menu was used (if any). Mirrors
    // MoveToFolderModal's own options/Combobox.
    let folderOptions: ControlOption[] = $derived([
        { value: "", label: t("moveToFolderModal.topLevel") },
        ...getFunctionFolders().map(name => ({ value: name, label: name })),
    ]);
    // Deliberately only the initial value — this modal is freshly mounted per open, so there's
    // nothing later to stay in sync with (mirrors CodeEditor's own initial-value-only props).
    let folderTarget = $state(untrack(() => folder ?? ""));

    // Parent picker (Objects only, and only when created under another Object rather than a Room
    // or at the top level) — scoped to just the clicked object's own ancestor chain, not every
    // object in the game (that's MoveElementModal's job for an existing object). Mirrors
    // MoveElementModal's own options/Combobox.
    let objectParentOptions: ControlOption[] = $derived(
        elementType === "object" && parent
            ? getPossibleNewObjectParents(parent).map(name => ({ value: name, label: name }))
            : []
    );
    let objectParentTarget = $state(untrack(() => parent ?? ""));

    let dialogEl: HTMLDivElement;
    let inputEl: HTMLInputElement;

    $effect(() => { inputEl?.focus(); });

    let name = $state("");
    let error = $state("");

    $effect(() => {
        const result = name ? validateName(name) : "";
        error = result === "ok" ? "" : result;
    });

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "Enter" && name && !error) confirm();
        if (e.key === "Escape") oncancel();
    }

    function onBackdropClick(e: MouseEvent) {
        if (e.target === e.currentTarget) oncancel();
    }

    function confirm() {
        if (!name || error) return;
        const result = validateName(name);
        if (result !== "ok") { error = result; return; }
        const target = elementType === "function" ? folderTarget
            : elementType === "object" && objectParentOptions.length > 0 ? objectParentTarget
            : null;
        onconfirm(name, target);
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
    <div class="card bg-surface-50-950 rounded-xl shadow-xl w-full max-w-80 p-6 flex flex-col gap-4">
        <h2 class="text-base font-semibold">
            {heading}
        </h2>

        <div class="flex flex-col gap-1">
            <label for="element-name" class="text-xs text-surface-600-400">{t("addElementModal.nameLabel")}</label>
            <input
                id="element-name"
                type="text"
                autocapitalize="off"
                class={"input bg-surface-50-950 px-2 py-1 text-sm" + (error ? " !border-error-500" : "")}
                bind:this={inputEl}
                bind:value={name}
                placeholder={t("addElementModal.namePlaceholder")}
            />
            {#if error}
                <p class="text-xs text-error-500">{error}</p>
            {/if}
        </div>

        {#if elementType === "function"}
            <div class="flex flex-col gap-1">
                <span class="text-xs text-surface-600-400">{t("moveToFolderModal.folderLabel")}</span>
                <Combobox
                    options={folderOptions}
                    value={folderTarget}
                    onchange={(v) => { folderTarget = v; }}
                    class="input bg-surface-50-950 px-2 py-1 text-sm w-full"
                />
            </div>
        {:else if elementType === "object" && objectParentOptions.length > 0}
            <div class="flex flex-col gap-1">
                <span class="text-xs text-surface-600-400">{t("addElementModal.parentLabel")}</span>
                <Combobox
                    options={objectParentOptions}
                    value={objectParentTarget}
                    onchange={(v) => { objectParentTarget = v; }}
                    class="input bg-surface-50-950 px-2 py-1 text-sm w-full"
                />
            </div>
        {/if}

        <div class="flex justify-end gap-2">
            <button class="btn btn-sm preset-tonal" onclick={oncancel}>{t("common.cancel")}</button>
            <button
                class="btn btn-sm preset-filled-primary-500"
                onclick={confirm}
                disabled={!name || !!error}
            >
                {t("addElementModal.addHeading", { label })}
            </button>
        </div>
    </div>
</div>
