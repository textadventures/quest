<script lang="ts">
    import { SvelteSet } from "svelte/reactivity";
    import { treeNodes, selectedKey, selectNode, deleteElement, openAddModal, createVerb, createCommand, createTurnScript, openAddLibraryModal, openAddJavascriptModal, swapElements, isGamebook } from "$lib/editor-store";
    import { nodeIcon } from "$lib/node-icons";
    import { t } from "$lib/i18n";

    interface Props {
        elementKey: string;
        elementType: string;
        objectType?: string | null;
        listFilter?: string | null;
    }

    let { elementKey, elementType, objectType, listFilter }: Props = $props();

    function resolveNodeTypes(et: string, ot?: string | null, lf?: string | null): string[] {
        if (et === "object") {
            if (ot === "command") {
                if (lf === "verb") return ["verb"];
                return ["command"];
            }
            if (ot === "turnscript") return ["turnscript"];
            return ["room", "object", "page"];
        }
        return [et];
    }

    let nodeTypes = $derived(resolveNodeTypes(elementType, objectType, listFilter));
    let isHeaderNode = $derived(elementKey.startsWith("_"));
    let showAll = $derived(isHeaderNode && (nodeTypes.includes("room") || nodeTypes.includes("object")));

    let items = $derived(
        $treeNodes.filter(n =>
            nodeTypes.includes(n.nodeType) &&
                (showAll || n.parent === elementKey)
        )
    );

    // ── Library grouping ──────────────────────────────────────────────────────
    // Purely a display grouping over the existing flat, SortIndex-ordered `items` — no
    // reordering. A group header is inserted before each new contiguous run of library items
    // sharing a filename, so move up/down (which swaps SortIndex on the underlying list)
    // stays untouched and headers simply track wherever the runs currently fall.
    type Row = { kind: "header"; key: string; label: string } | { kind: "item"; index: number; item: typeof items[number] };

    let rows = $derived.by(() => {
        const out: Row[] = [];
        let lastGroup: string | null = null;
        items.forEach((item, index) => {
            const group = item.isLibrary && item.filename ? item.filename : null;
            if (group !== null && group !== lastGroup) {
                out.push({ kind: "header", key: `__group_${item.key}`, label: group });
            }
            lastGroup = group;
            out.push({ kind: "item", index, item });
        });
        return out;
    });

    // ── Selection ──────────────────────────────────────────────────────────────

    const selectedKeys = new SvelteSet<string>();

    // Derive selection in item-list order, excluding any stale keys (deleted items)
    let activeSelection = $derived(
        items.filter(item => selectedKeys.has(item.key)).map(item => item.key)
    );

    function toggleSelect(key: string, checked: boolean) {
        if (checked) selectedKeys.add(key); else selectedKeys.delete(key);
    }

    // ── Add actions ────────────────────────────────────────────────────────────

    let isObjectList = $derived(nodeTypes.includes("room") || nodeTypes.includes("object"));
    // Gamebook has no room/object distinction - everything is a page (see
    // EditorController.GetNodeType) - so it gets a single "+ Add Page" button instead of
    // separate Add Object / Add Room / Add Page buttons.
    let showRoomButton = $derived(isObjectList && !$isGamebook);
    let showPageButton = $derived(isObjectList && !$isGamebook);

    let addLabel = $derived(
        nodeTypes.includes("function") ? t("elementAdders.function") :
            nodeTypes.includes("timer") ? t("elementAdders.timer") :
                nodeTypes.includes("verb") ? t("elementAdders.verb") :
                nodeTypes.includes("command") ? t("elementAdders.command") :
                nodeTypes.includes("turnscript") ? t("elementAdders.turnScript") :
                nodeTypes.includes("walkthrough") ? t("elementAdders.walkthrough") :
                nodeTypes.includes("template") ? t("elementAdders.template") :
                nodeTypes.includes("dynamictemplate") ? t("elementAdders.dynamicTemplate") :
                nodeTypes.includes("type") ? t("elementAdders.type") :
                nodeTypes.includes("include") ? t("elementAdders.library") :
                nodeTypes.includes("javascript") ? t("elementAdders.javascript") :
                isObjectList ? ($isGamebook ? t("elementAdders.page") : t("elementAdders.object")) :
                null
    );

    function addPrimary() {
        const parent = isHeaderNode ? null : elementKey;
        if (nodeTypes.includes("function")) openAddModal("function", null);
        else if (nodeTypes.includes("timer")) openAddModal("timer", null);
        else if (nodeTypes.includes("verb")) createVerb(parent);
        else if (nodeTypes.includes("command")) createCommand(parent);
        else if (nodeTypes.includes("turnscript")) createTurnScript(parent ?? "");
        else if (nodeTypes.includes("walkthrough")) openAddModal("walkthrough", null);
        else if (nodeTypes.includes("template")) openAddModal("template", null);
        else if (nodeTypes.includes("dynamictemplate")) openAddModal("dynamictemplate", null);
        else if (nodeTypes.includes("type")) openAddModal("type", null);
        else if (nodeTypes.includes("include")) openAddLibraryModal();
        else if (nodeTypes.includes("javascript")) openAddJavascriptModal();
        else if (isObjectList) openAddModal($isGamebook ? "page" : "object", parent);
    }

    function addRoom() {
        openAddModal("room", isHeaderNode ? null : elementKey);
    }

    function addPage() {
        openAddModal("page", isHeaderNode ? null : elementKey);
    }

    // ── Move / delete ──────────────────────────────────────────────────────────

    function moveUp(index: number) {
        if (index <= 0) return;
        swapElements(items[index].key, items[index - 1].key);
    }

    function moveDown(index: number) {
        if (index >= items.length - 1) return;
        swapElements(items[index].key, items[index + 1].key);
    }

    function onMoveUpSelected() {
        const key = activeSelection[0];
        const idx = items.findIndex(i => i.key === key);
        if (idx > 0) swapElements(items[idx].key, items[idx - 1].key);
    }

    function onMoveDownSelected() {
        const key = activeSelection[0];
        const idx = items.findIndex(i => i.key === key);
        if (idx >= 0 && idx < items.length - 1) swapElements(items[idx].key, items[idx + 1].key);
    }

    function onDeleteSelected() {
        // Skips anything not deletable (e.g. a built-in library — see EditorController.CanDelete)
        // rather than leaving it half-selected; DeleteElement would silently no-op on it anyway.
        for (const key of activeSelection) {
            const item = items.find(i => i.key === key);
            if (!item?.canDelete) continue;
            if ($selectedKey === key) selectNode(elementKey);
            deleteElement(key);
            selectedKeys.delete(key);
        }
    }

    function onDeleteItem(key: string) {
        if ($selectedKey === key) selectNode(elementKey);
        deleteElement(key);
        selectedKeys.delete(key);
    }
</script>

<div class="flex flex-col w-full px-3 py-2">
    <!-- Add toolbar -->
    <div class="flex items-center gap-1 mb-2">
        {#if addLabel !== null}
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                onclick={addPrimary}
            >+ {addLabel}</button>
        {/if}
        {#if showRoomButton}
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                onclick={addRoom}
            >+ {t("elementAdders.room")}</button>
        {/if}
        {#if showPageButton}
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                onclick={addPage}
            >+ {t("elementAdders.page")}</button>
        {/if}
    </div>

    <!-- Item rows -->
    {#if items.length === 0}
        <p class="text-xs text-surface-600-400 italic">{t("elementsList.noItems")}</p>
    {:else}
        {#each rows as row (row.kind === "header" ? row.key : row.item.key)}
            {#if row.kind === "header"}
                <div class="text-[11px] font-semibold text-surface-600-400 uppercase tracking-wide mt-2 mb-1 px-1 truncate" title={row.label}>{row.label}</div>
            {:else}
                {@const item = row.item}
                {@const i = row.index}
                <div class="group relative border border-surface-200-800 rounded mb-1 bg-surface-50-950 flex items-center">
                    <label class="flex items-center pt-1 pb-1 pl-1.5 pr-0.5 cursor-pointer flex-shrink-0">
                        <input
                            type="checkbox"
                            class="checkbox size-3.5"
                            checked={selectedKeys.has(item.key)}
                            onchange={(e) => toggleSelect(item.key, (e.target as HTMLInputElement).checked)}
                        />
                    </label>
                    <!-- pr-20 keeps name clear of the hover buttons (≈3×24px) -->
                    <button
                        type="button"
                        class="flex-1 flex items-center gap-1.5 text-left text-xs px-1.5 py-1 pr-20 text-primary-600-400 hover:underline truncate"
                        onclick={() => selectNode(item.key)}
                    >
                        {#if isObjectList}
                            {@const Icon = nodeIcon(item.key, item.nodeType)}
                            <Icon size={13} class="flex-shrink-0 opacity-70" />
                        {/if}
                        <span class="truncate">{item.text}</span>
                    </button>
                    <!-- pointer-events-none when invisible so clicks reach the name button -->
                    <div class="absolute right-1 top-1/2 -translate-y-1/2 flex gap-0.5 opacity-0 pointer-events-none group-hover:opacity-100 group-hover:pointer-events-auto transition-opacity z-10">
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500 px-1 py-0 text-xs leading-none"
                            title={t("common.moveUp")}
                            disabled={i === 0}
                            onclick={() => moveUp(i)}
                        >↑</button>
                        <button
                            type="button"
                            class="btn btn-sm preset-outlined-primary-500 px-1 py-0 text-xs leading-none"
                            title={t("common.moveDown")}
                            disabled={i === items.length - 1}
                            onclick={() => moveDown(i)}
                        >↓</button>
                        <button
                            type="button"
                            class="btn btn-sm preset-tonal-error px-1 py-0 text-xs leading-none"
                            title={item.canDelete ? t("common.delete") : t("elementsList.cannotDelete")}
                            disabled={!item.canDelete}
                            onclick={() => onDeleteItem(item.key)}
                        >×</button>
                    </div>
                </div>
            {/if}
        {/each}
    {/if}

    <!-- Selection toolbar -->
    {#if activeSelection.length > 0}
        {@const sel = activeSelection}
        <div class="flex items-center gap-1 mt-1 px-1 py-1 bg-surface-100-900 rounded border border-surface-200-800 text-xs">
            <button
                type="button"
                class="btn btn-sm preset-tonal-error text-xs py-0.5"
                disabled={!sel.some(key => items.find(i => i.key === key)?.canDelete)}
                onclick={onDeleteSelected}
            >{t("common.delete")}</button>
            {#if sel.length === 1}
                {@const idx = items.findIndex(i => i.key === sel[0])}
                <span class="w-px h-4 bg-surface-300-700 mx-0.5"></span>
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    disabled={idx === 0}
                    onclick={onMoveUpSelected}
                >↑ {t("common.moveUp")}</button>
                <button
                    type="button"
                    class="btn btn-sm preset-outlined-primary-500 text-xs py-0.5"
                    disabled={idx === items.length - 1}
                    onclick={onMoveDownSelected}
                >↓ {t("common.moveDown")}</button>
            {/if}
            <span class="ml-auto text-surface-600-400">{t("elementsList.selectedCount", { count: sel.length })}</span>
        </div>
    {/if}
</div>
