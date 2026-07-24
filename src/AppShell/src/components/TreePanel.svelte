<script lang="ts">
    import { untrack } from "svelte";
    import { TreeView, createTreeViewCollection } from "@skeletonlabs/skeleton-svelte";
    import Search from "@lucide/svelte/icons/search";
    import X from "@lucide/svelte/icons/x";
    import {
        treeNodes, selectedKey, selectNode,
        openAddModal, createExit, createTurnScript, createCommand, createVerb,
        createIncludedLibrary, createJavascript,
        deleteElement,
    } from "$lib/editor-store";
    import type { TreeNode } from "$lib/types";

    let { width, onactivate }: { width?: number; onactivate?: () => void } = $props();

    interface HierNode {
        id: string
        text: string
        nodeType: string
        children?: HierNode[]
    }


    // Build HierNode tree from flat list
    function buildHierTree(nodes: TreeNode[]): HierNode[] {
        // Deduplicate by key (last write wins, matching C# upsert behaviour in OnAddedNode)
        // eslint-disable-next-line svelte/prefer-svelte-reactivity
        const byKey = new Map<string, TreeNode>();
        for (const n of nodes) byKey.set(n.key, n);

        // eslint-disable-next-line svelte/prefer-svelte-reactivity
        const byParent = new Map<string | null, TreeNode[]>();
        for (const n of byKey.values()) {
            const p = n.parent ?? null;
            if (!byParent.has(p)) byParent.set(p, []);
            byParent.get(p)!.push(n);
        }
        const build = (node: TreeNode): HierNode => {
            const children = byParent.get(node.key);
            return {
                id: node.key,
                text: node.text,
                nodeType: node.nodeType,
                ...(children ? { children: children.map(build) } : {}),
            };
        };
        return (byParent.get(null) ?? []).map(build);
    }

    // ── Filtering ──────────────────────────────────────────────────────────────

    let filterText = $state("");
    let filterInputEl = $state<HTMLInputElement | undefined>();

    // Keep a node if its own text matches, or one of its descendants does. A node
    // that matches directly keeps its full, unfiltered subtree so authors can still
    // browse into e.g. a matched room and see everything inside it.
    function filterHierTree(nodes: HierNode[], query: string): HierNode[] {
        const result: HierNode[] = [];
        for (const node of nodes) {
            const selfMatch = node.text.toLowerCase().includes(query);
            if (node.children) {
                const filteredChildren = selfMatch ? node.children : filterHierTree(node.children, query);
                if (selfMatch || filteredChildren.length > 0) {
                    result.push({ ...node, children: filteredChildren });
                }
            } else if (selfMatch) {
                result.push(node);
            }
        }
        return result;
    }

    function collectBranchIds(nodes: HierNode[]): string[] {
        const ids: string[] = [];
        for (const node of nodes) {
            if (node.children) {
                ids.push(node.id, ...collectBranchIds(node.children));
            }
        }
        return ids;
    }

    let isFiltering = $derived(filterText.trim().length > 0);
    let rawHierTree = $derived(buildHierTree($treeNodes));
    let filteredHierTree = $derived(
        isFiltering ? filterHierTree(rawHierTree, filterText.trim().toLowerCase()) : rawHierTree
    );

    function clearFilter() {
        filterText = "";
        filterInputEl?.focus();
    }

    // ── Expansion state ────────────────────────────────────────────────────────

    let expandedIds = $state<string[]>([]);
    let loadedGameKey = $state<string | null>(null);

    // On game load, expand all nodes except _advanced
    $effect(() => {
        const nodes = $treeNodes;
        if (nodes.length === 0) { loadedGameKey = null; return; }
        const gameKey = nodes.find(n => n.nodeType === "game")?.key ?? null;
        if (gameKey === loadedGameKey) return;
        loadedGameKey = gameKey;
        expandedIds = nodes.filter(n => n.key !== "_advanced").map(n => n.key);
    });

    function toggleExpand(id: string) {
        // While filtering, all matching branches are force-expanded so results stay
        // visible; manual collapse would have no visible effect, so ignore it.
        if (isFiltering) return;
        if (expandedIds.includes(id)) {
            // If the selected node is inside this branch, select the branch itself first
            const nodeMap = new Map($treeNodes.map(n => [n.key, n]));
            let cur = nodeMap.get($selectedKey ?? "");
            while (cur?.parent) {
                if (cur.parent === id) { selectNode(id); break; }
                cur = nodeMap.get(cur.parent);
            }
            expandedIds = expandedIds.filter(x => x !== id);
        } else {
            expandedIds = [...expandedIds, id];
        }
    }

    // Auto-expand the ancestor chain whenever the selected node changes
    $effect(() => {
        const key = $selectedKey;
        const nodes = $treeNodes;
        if (!key || !nodes.length) return;
        const nodeMap = new Map(nodes.map(n => [n.key, n]));
        const toExpand: string[] = [];
        let cur: TreeNode | undefined = nodeMap.get(key);
        while (cur?.parent) {
            toExpand.push(cur.parent);
            cur = nodeMap.get(cur.parent);
        }
        // Use untrack so reading expandedIds doesn't make this effect re-run when
        // the user manually collapses a branch (which would immediately re-expand it).
        const current = untrack(() => expandedIds);
        if (toExpand.some(id => !current.includes(id))) {
            expandedIds = [...new Set([...current, ...toExpand])];
        }
    });

    let collection = $derived(
        createTreeViewCollection<HierNode>({
            nodeToValue: (n) => n.id,
            nodeToString: (n) => n.text,
            rootNode: { id: "__root__", text: "", nodeType: "header", children: filteredHierTree },
        })
    );

    let effectiveExpandedIds = $derived(
        isFiltering ? collectBranchIds(filteredHierTree) : expandedIds
    );

    // ── Add / delete ───────────────────────────────────────────────────────────

    let dropdownKey = $state<string | null>(null);
    let dropdownPos = $state<{ x: number; y: number } | null>(null);
    let dropdownOpts = $state<Array<{ label: string; action: () => void }>>([]);

    function closeDropdown() { dropdownKey = null; dropdownPos = null; }

    $effect(() => {
        if (!dropdownKey) return;
        function onOutside(e: MouseEvent) {
            const t = e.target as HTMLElement;
            if (!t.closest(".node-actions") && !t.closest(".tree-dropdown")) closeDropdown();
        }
        document.addEventListener("mousedown", onOutside);
        return () => document.removeEventListener("mousedown", onOutside);
    });

    function toggleDropdown(key: string, opts: Array<{ label: string; action: () => void }>, e: MouseEvent) {
        e.stopPropagation();
        if (dropdownKey === key) { closeDropdown(); return; }
        const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
        dropdownKey = key;
        dropdownPos = { x: rect.left, y: rect.bottom + 2 };
        dropdownOpts = opts;
    }

    function dropdownAction(fn: () => void) {
        fn();
        closeDropdown();
    }

    function handleDelete(id: string) {
        deleteElement(id);
    }

    // TreeView's onSelectionChange only fires when the selected value actually
    // changes, so tapping the already-selected node (e.g. after coming back
    // from the mobile properties pane via its back button) never fires it —
    // the tap appeared to do nothing. Handle that case directly at the row.
    function activateIfAlreadySelected(id: string) {
        if (id === $selectedKey) onactivate?.();
    }

    function isDeletable(nt: string): boolean {
        return nt !== "header" && nt !== "game" && nt !== "other";
    }

    function nodeMenuOptions(node: HierNode): Array<{ label: string; action: () => void }> {
        const opts: Array<{ label: string; action: () => void }> = [];
        const { id, text, nodeType: nt } = node;

        if (nt === "header") {
            if (id === "_objects") opts.push({ label: "Add Room", action: () => openAddModal("room", null) });
            else if (id === "_functions") opts.push({ label: "Add Function", action: () => openAddModal("function", null) });
            else if (id === "_timers") opts.push({ label: "Add Timer", action: () => openAddModal("timer", null) });
            else if (id === "_gameVerbs") opts.push({ label: "Add Verb", action: () => createVerb(null) });
            else if (id === "_gameCommands") opts.push({ label: "Add Command", action: () => createCommand(null) });
            else if (id === "_walkthrough") opts.push({ label: "Add Walkthrough", action: () => openAddModal("walkthrough", null) });
            else if (id === "_template") opts.push({ label: "Add Template", action: () => openAddModal("template", null) });
            else if (id === "_dynamictemplate") opts.push({ label: "Add Dynamic Template", action: () => openAddModal("dynamictemplate", null) });
            else if (id === "_objecttype") opts.push({ label: "Add Type", action: () => openAddModal("type", null) });
            else if (id === "_include") opts.push({ label: "Add Library", action: () => createIncludedLibrary() });
            else if (id === "_javascript") opts.push({ label: "Add JavaScript", action: () => createJavascript() });
        } else if (nt === "room") {
            opts.push(
                { label: "Add Object here", action: () => openAddModal("object", id) },
                { label: "Add Room here", action: () => openAddModal("room", id) },
                { label: "Add Exit", action: () => createExit(id) },
                { label: "Add Command", action: () => createCommand(id) },
                { label: "Add Verb", action: () => createVerb(id) },
                { label: "Add Turn Script", action: () => createTurnScript(id) },
            );
        } else if (nt === "object") {
            opts.push(
                { label: "Add Command", action: () => createCommand(id) },
                { label: "Add Verb", action: () => createVerb(id) },
                { label: "Add Turn Script", action: () => createTurnScript(id) },
            );
        }

        if (isDeletable(nt)) {
            opts.push({ label: `Delete "${text}"`, action: () => handleDelete(id) });
        }

        return opts;
    }
</script>

<div
    class="flex flex-col shrink-0 border-r border-surface-200-800 bg-surface-50-950 {width === undefined ? "w-full" : ""}"
    style={width !== undefined ? `width: ${width}px` : undefined}
>
    <div class="px-3 py-2 text-xs font-semibold uppercase text-surface-500-400 border-b border-surface-200-800">
        Game Objects
    </div>
    <div class="p-1.5 border-b border-surface-200-800 relative">
        <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 size-3.5 text-surface-400 pointer-events-none" />
        <input
            bind:this={filterInputEl}
            type="text"
            autocapitalize="off"
            bind:value={filterText}
            placeholder="Filter..."
            aria-label="Filter game objects"
            class="input text-xs py-1 pl-7 pr-6 w-full"
            onkeydown={(e) => { if (e.key === "Escape" && filterText) { e.stopPropagation(); clearFilter(); } }}
        />
        {#if filterText}
            <button
                type="button"
                class="absolute right-3 top-1/2 -translate-y-1/2 size-4 flex items-center justify-center text-surface-400 hover:text-surface-900-50"
                onclick={clearFilter}
                aria-label="Clear filter"
            ><X class="size-3.5" /></button>
        {/if}
    </div>
    {#if isFiltering && (collection.rootNode.children ?? []).length === 0}
        <div class="px-3 py-2 text-xs text-surface-400">No matches</div>
    {/if}
    <div class="flex-1 overflow-y-auto p-1 text-xs">
        <TreeView
            {collection}
            selectionMode="single"
            expandOnClick={false}
            expandedValue={effectiveExpandedIds}
            selectedValue={$selectedKey ? [$selectedKey] : []}
            onSelectionChange={(e) => { if (e.selectedValue[0]) { selectNode(e.selectedValue[0]); onactivate?.(); } }}
        >
            {#each collection.rootNode.children ?? [] as node, i (node.id)}
                {@render treeNode(node, [i])}
            {/each}
        </TreeView>
    </div>
    {#if dropdownKey && dropdownPos}
        <div
            class="tree-dropdown fixed z-[999] w-44 bg-surface-50-950 border border-surface-200-800 rounded shadow-lg py-1"
            style="left: {dropdownPos.x}px; top: {dropdownPos.y}px"
        >
            {#each dropdownOpts as opt (opt.label)}
                <button
                    class="w-full text-left px-3 py-1 text-xs text-surface-900-50 hover:bg-surface-200-800"
                    onclick={() => dropdownAction(opt.action)}
                >{opt.label}</button>
            {/each}
        </div>
    {/if}
</div>

{#snippet nodeActions(node: HierNode)}
    {@const opts = nodeMenuOptions(node)}
    {#if opts.length > 0}
        <span class="node-actions flex items-center ml-auto pr-1">
            <button
                class="size-5 flex items-center justify-center rounded text-surface-400 hover:text-primary-500 hover:bg-surface-200-800 text-sm leading-none"
                onclick={(e) => toggleDropdown(node.id + ":menu", opts, e)}
                title="Options"
            >⋯</button>
        </span>
    {/if}
{/snippet}

{#snippet treeNode(node: HierNode, indexPath: number[])}
    <TreeView.NodeProvider value={{ node, indexPath }}>
        {#if node.children}
            <TreeView.Branch>
                <TreeView.BranchControl class="group" ondblclick={() => toggleExpand(node.id)} onclick={() => activateIfAlreadySelected(node.id)}>
                    <button
                        type="button"
                        tabindex="-1"
                        class="flex-shrink-0 size-4 flex items-center justify-center transition-transform duration-150 {effectiveExpandedIds.includes(node.id) ? "rotate-90" : ""}"
                        onclick={(e) => { e.stopPropagation(); toggleExpand(node.id); }}
                        aria-label={effectiveExpandedIds.includes(node.id) ? "Collapse" : "Expand"}
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="size-4"><polyline points="9 18 15 12 9 6" /></svg>
                    </button>
                    <TreeView.BranchText class="flex-1 min-w-0 truncate">{node.text}</TreeView.BranchText>
                    <span class="opacity-0 group-hover:opacity-100 pointer-coarse:opacity-100">
                        {@render nodeActions(node)}
                    </span>
                </TreeView.BranchControl>
                <TreeView.BranchContent>
                    <TreeView.BranchIndentGuide />
                    {#each node.children as child, ci (child.id)}
                        {@render treeNode(child, [...indexPath, ci])}
                    {/each}
                </TreeView.BranchContent>
            </TreeView.Branch>
        {:else}
            <TreeView.Item class="group flex items-center" onclick={() => activateIfAlreadySelected(node.id)}>
                <span class="flex-1 min-w-0 truncate">{node.text}</span>
                <span class="opacity-0 group-hover:opacity-100 pointer-coarse:opacity-100">
                    {@render nodeActions(node)}
                </span>
            </TreeView.Item>
        {/if}
    </TreeView.NodeProvider>
{/snippet}
