<script lang="ts">
    import { SvelteSet } from "svelte/reactivity";
    import { addScriptDictItem, removeScriptDictItem, renameScriptDictItem, makeScriptDictEditable, getObjectNames } from "$lib/editor-store";
    import ScriptEditor from "./ScriptEditor.svelte";
    import { t } from "$lib/i18n";

    interface Props {
        elementKey: string;
        attribute: string;
        value: string | null;
        isLocked?: boolean;
        // "object" swaps the free-text "Add entry key" field for a dropdown of the game's
        // objects — used by VerbsEditor's "Require another object" behaviour, where the key
        // must be a real object name for the interpreter to match against. Generic attribute
        // dictionaries (AttributesEditor) keep free-text keys, since those aren't necessarily
        // object names.
        keySource?: "text" | "object";
        // <keyname> - overrides the generic "entry key" wording in the add-row placeholder/
        // labels with a more specific noun (e.g. "Object" for useon/selfuseon/give/giveto).
        keyLabel?: string | null;
        // <keyprompt> - a longer descriptive sentence (e.g. "Please enter the name of the
        // object..."), shown as a tooltip on the add-row input alongside keyLabel's shorter
        // placeholder/aria-label wording.
        keyPrompt?: string | null;
    }

    let { elementKey, attribute, value, isLocked = false, keySource = "text", keyLabel = null, keyPrompt = null }: Props = $props();

    let items = $derived.by<{key: string, value: string}[]>(() => {
        try {
            return JSON.parse(value ?? "[]") as {key: string, value: string}[];
        } catch { return []; }
    });

    let keys = $derived(items.map(i => i.key));

    const expandedKeys = new SvelteSet<string>();
    let newKey = $state("");

    let editingKey = $state<string | null>(null);
    let editingKeyValue = $state("");
    let editingKeyInputEl = $state<HTMLInputElement>();
    $effect(() => { if (editingKey !== null) editingKeyInputEl?.focus(); });

    let allObjectNames = $state<string[]>([]);
    $effect(() => {
        if (keySource === "object") allObjectNames = getObjectNames() ?? [];
    });
    // Excludes the object itself (a verb requiring itself doesn't make sense) and objects
    // already added as entries.
    let availableObjectNames = $derived(
        allObjectNames.filter(n => n !== elementKey && !keys.includes(n))
    );

    function onAdd() {
        const k = newKey.trim();
        if (k) {
            const result = addScriptDictItem(elementKey, attribute, k);
            if (result === "ok") expandedKeys.add(k);
            newKey = "";
        }
    }

    function toggleExpanded(key: string) {
        if (expandedKeys.has(key)) expandedKeys.delete(key); else expandedKeys.add(key);
    }

    function startRename(key: string) {
        editingKey = key;
        editingKeyValue = key;
    }

    function cancelRename() {
        editingKey = null;
        editingKeyValue = "";
    }

    function commitRename() {
        if (!editingKey) return;
        const oldKey = editingKey;
        const newKeyValue = editingKeyValue.trim();
        if (!newKeyValue || newKeyValue === oldKey) {
            cancelRename();
            return;
        }
        const result = renameScriptDictItem(elementKey, attribute, oldKey, newKeyValue);
        if (result === "ok" && expandedKeys.has(oldKey)) {
            expandedKeys.delete(oldKey);
            expandedKeys.add(newKeyValue);
        }
        cancelRename();
    }
</script>

<div class="flex flex-col gap-1 w-full">
    {#if isLocked}
        <div class="flex items-center gap-2 py-1 px-2 mb-1 text-xs text-surface-600-400 italic border border-surface-200-800 rounded">
            <span class="flex-1">{t("scriptDictionaryEditor.inheritedReadOnly")}</span>
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0 not-italic"
                onclick={() => makeScriptDictEditable(elementKey, attribute)}
            >{t("common.makeEditableCopy")}</button>
        </div>
    {/if}
    {#if isLocked}
        {#each items as item (item.key)}
            <div class="border border-surface-200-800 rounded px-2 py-1 text-xs text-surface-600-400 opacity-60"><span class="font-medium">{item.key}</span> = <span class="italic">{item.value || t("scriptDictionaryEditor.emptyValue")}</span></div>
        {/each}
    {:else}
        {#each keys as key (key)}
            <div class="border border-surface-200-800 rounded">
                <div class="flex items-center gap-1 px-2 py-1">
                    {#if editingKey === key}
                        <input
                            type="text"
                            autocapitalize="off"
                            class="input text-xs py-0.5 px-1.5 flex-1"
                            aria-label={t("scriptDictionaryEditor.editKeyInputAriaLabel")}
                            bind:this={editingKeyInputEl}
                            bind:value={editingKeyValue}
                            onkeydown={(e) => {
                                if (e.key === "Enter") commitRename();
                                else if (e.key === "Escape") cancelRename();
                            }}
                            onblur={commitRename}
                        />
                    {:else}
                        <button
                            type="button"
                            class="flex-1 text-left text-xs font-medium hover:text-primary-600-400"
                            onclick={() => toggleExpanded(key)}
                        >
                            <span class="mr-1 text-surface-600-400">{expandedKeys.has(key) ? "▼" : "▶"}</span>{key}
                        </button>
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500 text-xs px-1.5 py-0.5 flex-shrink-0"
                            onclick={() => startRename(key)}
                        >{t("scriptDictionaryEditor.editKey")}</button>
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-error-500 text-xs px-1.5 py-0.5 flex-shrink-0"
                            onclick={() => removeScriptDictItem(elementKey, attribute, key)}
                        >✕</button>
                    {/if}
                </div>
                {#if expandedKeys.has(key)}
                    <div class="px-2 pb-2 border-t border-surface-100-900">
                        <ScriptEditor elementKey={elementKey} attribute="{attribute}[{key}]" />
                    </div>
                {/if}
            </div>
        {/each}
        <div class="flex items-center gap-1 mt-0.5">
            {#if keySource === "object"}
                <select
                    class="select text-xs py-0.5 px-1.5 flex-1"
                    aria-label={keyLabel ? t("scriptDictionaryEditor.addEntryKeyAriaLabelWithLabel", { label: keyLabel }) : t("scriptDictionaryEditor.addEntryKeyAriaLabel")}
                    title={keyPrompt ?? undefined}
                    data-staging
                    bind:value={newKey}
                >
                    <option value="">{keyLabel ? t("scriptDictionaryEditor.selectObjectOptionWithLabel", { label: keyLabel }) : t("scriptDictionaryEditor.selectObjectOption")}</option>
                    {#each availableObjectNames as name (name)}
                        <option value={name}>{name}</option>
                    {/each}
                </select>
            {:else}
                <input
                    type="text"
                    autocapitalize="off"
                    class="input text-xs py-0.5 px-1.5 flex-1"
                    placeholder={keyLabel ? t("scriptDictionaryEditor.addEntryKeyPlaceholderWithLabel", { label: keyLabel }) : t("scriptDictionaryEditor.addEntryKeyPlaceholder")}
                    aria-label={keyLabel ? t("scriptDictionaryEditor.addEntryKeyAriaLabelWithLabel", { label: keyLabel }) : t("scriptDictionaryEditor.addEntryKeyAriaLabel")}
                    title={keyPrompt ?? undefined}
                    data-staging
                    bind:value={newKey}
                    onkeydown={(e) => { if (e.key === "Enter") onAdd(); }}
                />
            {/if}
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs px-2 py-0.5 flex-shrink-0"
                disabled={!newKey.trim()}
                onclick={onAdd}
            >{t("common.add")}</button>
        </div>
    {/if}
</div>
