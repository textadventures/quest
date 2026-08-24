<script lang="ts">
    import { untrack, onDestroy } from "svelte";
    import { TreeView, createTreeViewCollection } from "@skeletonlabs/skeleton-svelte";
    import Search from "@lucide/svelte/icons/search";
    import X from "@lucide/svelte/icons/x";
    import SlidersHorizontal from "@lucide/svelte/icons/sliders-horizontal";
    import Check from "@lucide/svelte/icons/check";
    import { nodeIcon } from "$lib/node-icons";
    import DropdownMenu from "./DropdownMenu.svelte";
    import type { DropdownMenuItem } from "./DropdownMenu.svelte";
    import {
        treeNodes, selectedKey, selectNode, isGamebook,
        openAddModal, createExit, createTurnScript, createCommand, createVerb,
        openAddLibraryModal, openAddJavascriptModal,
        deleteElement,
        canMoveElement, openMoveModal, openMoveToFolderModal, copyElements, cutElements, canPasteElements, pasteElements,
        clipboardVersion, cutElementKeys,
        showLibraryElements, toggleShowLibraryElements,
        swapElements, getCurrentGameId,
        canMoveFunctionUp, canMoveFunctionDown,
        canMoveFunctionFolderUp, canMoveFunctionFolderDown, moveFunctionFolderUp, moveFunctionFolderDown,
    } from "$lib/editor-store";
    import type { TreeNode } from "$lib/types";
    import { t } from "$lib/i18n";
    import { loadTreeState, saveTreeState } from "$lib/tree-state";

    let { width, onactivate }: { width?: number; onactivate?: () => void } = $props();

    interface HierNode {
        id: string
        text: string
        nodeType: string
        isLibrary: boolean
        canDelete: boolean
        // Only meaningful on isLibrary nodes — see groupLibraryChildren.
        filename?: string | null
        // User-assigned folder name, currently only meaningful on non-library function nodes —
        // see groupLibraryChildren.
        folder?: string | null
        children?: HierNode[]
    }


    // Synthetic header nodes (key starts with "_", see WasmEditorBridge.GetNodeType) carry
    // their label as plain English text straight from EditorController.AddTreeHeader/OnAddedNode
    // in C#, which has no access to the frontend's i18n messages. Translate by fixed id here
    // instead; anything not in this map (there shouldn't be any) falls back to the C# text.
    const HEADER_LABEL_KEYS: Record<string, string> = {
        _advanced: "common.advanced",
        _functions: "treePanel.header.functions",
        _timers: "treePanel.header.timers",
        _walkthrough: "treePanel.header.walkthrough",
        _include: "treePanel.header.includedLibraries",
        _template: "treePanel.header.templates",
        _dynamictemplate: "treePanel.header.dynamicTemplates",
        _objecttype: "treePanel.header.objectTypes",
        _javascript: "treePanel.header.javascript",
        _gameVerbs: "verbsEditor.header",
        _gameCommands: "treePanel.header.commands",
    };

    function headerText(node: TreeNode): string {
        if (node.key === "_objects") return $isGamebook ? t("treePanel.header.pages") : t("treePanel.header.objects");
        const key = HEADER_LABEL_KEYS[node.key];
        return key ? t(key) : node.text;
    }

    // Folds consecutive children sharing the same grouping key — either a library filename (e.g.
    // every function from Core.aslx) or a user-assigned folder (Functions only, via "Move to
    // folder") — into a single synthetic, expand-on-demand "folder" node; a big library's (or
    // folder's) worth of functions/timers/etc. otherwise floods this tree flat. Purely a display
    // grouping over the existing SortIndex order (mirrors ElementsList's grouping). A lone
    // library-filename run isn't worth the extra click to expand, so it's left inline and only
    // runs of 2+ fold; a lone folder still folds (see the "folder" special-case below) since it's
    // a deliberate user action, not an incidental one. The library variant's synthetic node is
    // isLibrary:true, making it fall out of nodeMenuOptions/reorderOptions for free (same guard
    // real library rows use); the folder variant is a plain non-library node instead, since its
    // members are real user-authored functions that still need their own context menu. Either way
    // the id is deterministic so expand-state persistence (tree-state.ts) still works across
    // reloads.
    function groupKey(node: HierNode): { key: string; kind: "lib" | "folder" } | null {
        if (node.isLibrary && node.filename) return { key: node.filename, kind: "lib" };
        if (!node.isLibrary && node.folder) return { key: node.folder, kind: "folder" };
        return null;
    }

    function groupLibraryChildren(children: HierNode[], parentId: string): HierNode[] {
        const out: HierNode[] = [];
        let i = 0;
        while (i < children.length) {
            const node = children[i];
            const group = groupKey(node);
            if (group) {
                let j = i + 1;
                while (j < children.length) {
                    const next = groupKey(children[j]);
                    if (!next || next.kind !== group.kind || next.key !== group.key) break;
                    j++;
                }
                // A lone library-filename run isn't worth the extra click to expand (it's an
                // incidental grouping nobody chose), but a lone folder is a deliberate "Move to
                // folder" action - hiding it when it only has one member yet would make that
                // action look like it did nothing, so folders always fold, even at length 1.
                if (j - i > 1 || group.kind === "folder") {
                    // Suffixed with the run's first child id (globally unique, since it's a real
                    // element key) rather than just the group key — the same file/folder can
                    // produce more than one non-adjacent run under one parent (SortIndex order
                    // isn't guaranteed to keep same-group items contiguous), and two runs would
                    // otherwise collide on id.
                    out.push({
                        id: `${parentId}::${group.kind}::${group.key}::${node.id}`,
                        text: group.key,
                        nodeType: group.kind === "lib" ? "librarygroup" : "usergroup",
                        isLibrary: group.kind === "lib",
                        canDelete: false,
                        children: children.slice(i, j),
                    });
                } else {
                    out.push(node);
                }
                i = j;
            } else {
                out.push(node);
                i++;
            }
        }
        return out;
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
            const children = byParent.get(node.key)?.map(build);
            // "Included Libraries" lists the library *files* themselves (nodeType "include") —
            // each one's own name is already a filename, so grouping them by which file declared
            // the <include> just wraps a filename in an identically-named folder, and the same
            // library can be pulled in from more than one place, producing several duplicate-
            // labeled groups. Skip grouping at this level only; libraries defined *inside* a
            // browsed included library (its functions, objects, its own nested includes, ...)
            // still group normally, same as everywhere else.
            const grouped = children && children[0]?.nodeType !== "include"
                ? groupLibraryChildren(children, node.key)
                : children;
            return {
                id: node.key,
                text: node.nodeType === "header" ? headerText(node) : node.text,
                nodeType: node.nodeType,
                isLibrary: node.isLibrary,
                canDelete: node.canDelete,
                filename: node.filename,
                folder: node.folder,
                ...(grouped ? { children: grouped } : {}),
            };
        };
        return (byParent.get(null) ?? []).map(build);
    }

    // ── Empty advanced categories ────────────────────────────────────────────────

    // Advanced category headers are always present in the tree data (added
    // unconditionally by EditorController.AddTreeHeader) even when nothing of that
    // type exists yet. An empty header is noise for everyone, not just beginners —
    // hide these until the author adds one via that category's own "+ Add" button
    // (ElementsList), reached by expanding "_advanced". "_advanced" itself is
    // never hidden even when all its children are — it's the only entry point for
    // adding the first function/timer/walkthrough/etc. in a fresh game, so it must
    // stay clickable regardless of what it currently contains. "_objects" is never
    // hidden either.
    const HIDE_WHEN_EMPTY = new Set([
        "_functions", "_timers", "_walkthrough",
        "_include", "_template", "_dynamictemplate", "_objecttype", "_javascript",
    ]);

    function pruneEmptyAdvancedCategories(nodes: HierNode[]): HierNode[] {
        const result: HierNode[] = [];
        for (const node of nodes) {
            let children = node.children;
            if (children) children = pruneEmptyAdvancedCategories(children);
            if (HIDE_WHEN_EMPTY.has(node.id) && !children?.length) continue;
            result.push(children ? { ...node, children } : node);
        }
        return result;
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
    let rawHierTree = $derived(pruneEmptyAdvancedCategories(buildHierTree($treeNodes)));
    let filteredHierTree = $derived(
        isFiltering ? filterHierTree(rawHierTree, filterText.trim().toLowerCase()) : rawHierTree
    );

    function clearFilter() {
        filterText = "";
        filterInputEl?.focus();
    }

    // ── Expansion state ────────────────────────────────────────────────────────

    let expandedIds = $state<string[]>([]);
    // "State loaded for" marker — the `${gameKey}|${gameId}` of the tree the
    // current expandedIds came from. Guards both directions: a fresh game (new
    // key or id) re-applies the default + stored state, while a mid-session
    // refreshTree (element added/renamed, etc.) leaves the user's expansion
    // alone, and a TreePanel remount (navigating back into the editor) sees
    // its previously-null marker and re-applies instead of showing a blank,
    // fully-collapsed tree.
    let loadedStateFor = $state<string | null>(null);
    let saveTimer: ReturnType<typeof setTimeout> | undefined;
    // Latest (gameId, ids) not yet written out — flushed from onDestroy when
    // the panel unmounts (navigating Home etc.) before the debounce could fire.
    let pendingTreeState: { gameId: string; ids: string[] } | null = null;

    // On game load, default to a collapsed tree — only the top-level structural
    // header (the "_objects"/"_pages" one) starts open, so rooms, objects, turn
    // scripts, functions etc. are all collapsed (#827). The "game" node starts
    // collapsed too, hiding its Verbs/Commands headers until opened, and the
    // "_advanced" header stays collapsed like everything else — its subtree
    // (Included Libraries, Functions, ...) is the biggest source of default
    // clutter. The one exception: the room the player starts in is expanded all
    // the way down to the player object, so the game's opening scene is visible
    // at a glance. Any per-game state saved for this game (see the persistence
    // effect below) then overrides the default, so a returning author finds the
    // tree exactly as they left it.
    $effect(() => {
        const nodes = $treeNodes;
        if (nodes.length === 0) { loadedStateFor = null; return; }
        const gameKey = nodes.find(n => n.nodeType === "game")?.key ?? null;
        if (!gameKey) return;
        const gameId = getCurrentGameId();
        const stateKey = `${gameKey}|${gameId ?? ""}`;
        if (stateKey === loadedStateFor) return;
        loadedStateFor = stateKey;
        const defaultIds = nodes
            .filter(n => n.nodeType === "header" && !n.parent && n.key !== "_advanced")
            .map(n => n.key);
        const playerPath = startingRoomPath(nodes);
        expandedIds = [...new Set([...defaultIds, ...playerPath])];
        if (!gameId) return;
        void loadTreeState(gameId).then((stored) => {
            if (stored && stored.length > 0 && loadedStateFor === stateKey) {
                expandedIds = stored;
            }
        });
    });

    // The player object conventionally lives inside the starting room, possibly
    // nested in other objects. Return every ancestor of the player node so the
    // whole chain (room → … → player) is expanded by default. Match the player
    // by name, not nodeType: it's "object" in a text adventure but "page" in
    // gamebook mode (any object in GameBook style is typed "page"), so only
    // structural/library rows (headers, include files, library-origin nodes)
    // are excluded.
    function startingRoomPath(nodes: TreeNode[]): string[] {
        const player = nodes.find(n => n.key === "player" && n.nodeType !== "header" && n.nodeType !== "include" && !n.isLibrary);
        if (!player) return [];
        const nodeMap = new Map(nodes.map(n => [n.key, n]));
        const ids: string[] = [];
        let cur: TreeNode | undefined = player;
        while (cur?.parent) {
            ids.push(cur.parent);
            cur = nodeMap.get(cur.parent);
        }
        return ids;
    }

    function toggleExpand(id: string) {
        // While filtering, all matching branches are force-expanded so results stay
        // visible; manual collapse would have no visible effect, so ignore it.
        if (isFiltering) return;
        if (expandedIds.includes(id)) {
            // If the selected node is inside this branch, select the branch itself first
            selectBranchIfSelectedInside(id);
            expandedIds = expandedIds.filter(x => x !== id);
        } else {
            expandedIds = [...expandedIds, id];
        }
    }

    // If the currently-selected node sits inside the branch being collapsed,
    // select the branch node itself so the tree doesn't end up with its
    // selection hidden inside a closed branch.
    function selectBranchIfSelectedInside(id: string) {
        const nodeMap = new Map($treeNodes.map(n => [n.key, n]));
        let cur = nodeMap.get($selectedKey ?? "");
        while (cur?.parent) {
            if (cur.parent === id) { selectNode(id); break; }
            cur = nodeMap.get(cur.parent);
        }
    }

    // Debounced persistence of the expansion state, keyed by the game's stable
    // <gameid>. Skipped for legacy games without one (they keep the collapsed
    // default on every load). The timer itself is cleared on teardown — a
    // change made within the debounce window of navigating away (Home, opening
    // another game, …) would otherwise be lost — so the latest pending state is
    // flushed from onDestroy instead.
    $effect(() => {
        const ids = expandedIds;
        const gameId = getCurrentGameId();
        if (!loadedStateFor || !gameId) return;
        pendingTreeState = { gameId, ids };
        clearTimeout(saveTimer);
        saveTimer = setTimeout(() => {
            pendingTreeState = null;
            saveTreeState(gameId, ids);
        }, 300);
        return () => clearTimeout(saveTimer);
    });

    onDestroy(() => {
        if (pendingTreeState) saveTreeState(pendingTreeState.gameId, pendingTreeState.ids);
    });

    // Ancestor ids of targetId within the *hierarchical* (post-grouping) tree, root-first -
    // unlike a flat-tree parent-chain walk, this also passes through any synthetic folder/library
    // group node the target got folded into (see buildHierTree/groupLibraryChildren), since those
    // exist only as HierNode wrappers, never as a real element's own `parent` field.
    function findAncestorIds(nodes: HierNode[], targetId: string, path: string[] = []): string[] | null {
        for (const node of nodes) {
            if (node.id === targetId) return path;
            if (node.children) {
                const found = findAncestorIds(node.children, targetId, [...path, node.id]);
                if (found) return found;
            }
        }
        return null;
    }

    // Auto-expand the ancestor chain (including any folder/library group it's folded into)
    // whenever the selected node changes - e.g. a function just created inside a folder via "Add
    // Function here" should have that folder visibly open, not collapsed around it.
    $effect(() => {
        const key = $selectedKey;
        const tree = rawHierTree;
        if (!key || !tree.length) return;
        const toExpand = findAncestorIds(tree, key) ?? [];
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
            rootNode: { id: "__root__", text: "", nodeType: "header", isLibrary: false, canDelete: false, children: filteredHierTree },
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

    const DROPDOWN_WIDTH = 176; // matches the panel's w-44
    const VIEWPORT_MARGIN = 8;

    function toggleDropdown(key: string, opts: Array<{ label: string; action: () => void }>, e: MouseEvent) {
        e.stopPropagation();
        if (dropdownKey === key) { closeDropdown(); return; }
        const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
        // Anchoring the panel's left edge to the button's left edge (as if it
        // always had room to extend rightward) put it mostly off-screen for
        // the "..." button on a narrow/full-width mobile tree, since that
        // button sits close to the viewport's own right edge. Flip to
        // right-aligned against the button when there isn't room to the right.
        let x = rect.left;
        if (x + DROPDOWN_WIDTH > window.innerWidth - VIEWPORT_MARGIN) {
            x = rect.right - DROPDOWN_WIDTH;
        }
        x = Math.max(VIEWPORT_MARGIN, x);

        // Same idea vertically: flip to open upward if there isn't room below
        // for a row near the bottom of the tree pane. ~28px/item is an
        // estimate (matches the item's text-xs px-3 py-1 sizing) — good
        // enough to decide which way to open, doesn't need to be exact.
        const estimatedHeight = opts.length * 28 + 8;
        let y = rect.bottom + 2;
        if (y + estimatedHeight > window.innerHeight - VIEWPORT_MARGIN) {
            y = Math.max(VIEWPORT_MARGIN, rect.top - estimatedHeight - 2);
        }

        dropdownKey = key;
        dropdownPos = { x, y };
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

    // node.canDelete is authoritative (mirrors EditorController.CanDelete exactly — "game", the
    // gamebook player, and any built-in library are all already false there). "header" and
    // "other" (an element type with no dedicated tree icon, e.g. ElementType.Output) are kept as
    // an extra client-side guard since CanDelete doesn't know about tree presentation concerns.
    function isDeletable(node: HierNode): boolean {
        const { nodeType: nt } = node;
        return nt !== "header" && nt !== "other" && node.canDelete;
    }

    // Move up/down reorder the node among its same-parent siblings — the tree's
    // synthetic parent grouping always maps to a single world-model parent (e.g.
    // everything under "_objects" is a top-level element on the game), so
    // swapping the two SortIndex meta-fields is always well-defined (see
    // EditorController.SwapElements). Mirrors ElementsList's up/down buttons.
    // The array order of $treeNodes is the tree's display order, so the
    // adjacent flat-list entry is the swap target.
    function reorderOptions(node: HierNode): Array<{ label: string; action: () => void }> {
        const opts: Array<{ label: string; action: () => void }> = [];
        // Headers, the game node, and included-library file nodes are structural
        // or read-only — nothing is reorderable among them. (Library rows in
        // particular are meant to have no context menu at all, mirroring the
        // isLibrary guard in nodeMenuOptions.)
        if (node.nodeType === "header" || node.nodeType === "game" || node.nodeType === "include") return opts;
        if (node.nodeType === "usergroup") {
            // A folder's synthetic id has no single flat-tree entry to look up (see buildHierTree/
            // groupLibraryChildren) - node.text is the folder name itself, and moving the whole
            // block is handled server-side by name rather than by swapping two flat siblings.
            const folderName = node.text;
            if (canMoveFunctionFolderUp(folderName)) {
                opts.push({ label: t("common.moveUp"), action: () => moveFunctionFolderUp(folderName) });
            }
            if (canMoveFunctionFolderDown(folderName)) {
                opts.push({ label: t("common.moveDown"), action: () => moveFunctionFolderDown(folderName) });
            }
            return opts;
        }
        const flat = $treeNodes.find(n => n.key === node.id);
        if (!flat) return opts;
        // The game node itself appears under "_objects" alongside the game's
        // top-level elements — it's not a real sibling (nothing can be
        // reordered above or below it), so exclude it from the sibling list.
        const siblings = $treeNodes.filter(n => n.parent === flat.parent && n.nodeType !== "game");
        const index = siblings.findIndex(n => n.key === flat.key);
        if (index < 0) return opts;
        // A function's swap is also gated on canMoveFunctionUp/Down - a plain adjacent swap has
        // no concept of folders, and swapping past the near edge of a *different* multi-member
        // folder would split it in two by landing the mover between two of its other members
        // (mirrors ElementsList's own moveUp/moveDown guard).
        const isFunction = node.nodeType === "function";
        if (index > 0 && (!isFunction || canMoveFunctionUp(flat.key))) {
            opts.push({ label: t("common.moveUp"), action: () => swapElements(flat.key, siblings[index - 1].key) });
        }
        if (index < siblings.length - 1 && (!isFunction || canMoveFunctionDown(flat.key))) {
            opts.push({ label: t("common.moveDown"), action: () => swapElements(flat.key, siblings[index + 1].key) });
        }
        return opts;
    }

    function nodeMenuOptions(node: HierNode): Array<{ label: string; action: () => void }> {
        // Library-origin nodes (from Core.aslx or another included library) are read-only —
        // the only way to act on one is the "Copy into your game" banner in the properties
        // panel, so they get no context menu at all.
        if (node.isLibrary) return [];

        // Read (not used) so this function's callers re-run whenever the clipboard
        // changes — canPasteElements() below reads server-side state that Svelte
        // has no other reactive handle on.
        void $clipboardVersion;
        const opts: Array<{ label: string; action: () => void }> = [];
        const { id, text, nodeType: nt } = node;

        if (nt === "header") {
            if (id === "_objects") {
                opts.push($isGamebook
                    ? { label: t("elementAdders.page"), action: () => openAddModal("page", null) }
                    : { label: t("elementAdders.room"), action: () => openAddModal("room", null) });
                if (!$isGamebook) {
                    opts.push({ label: t("elementAdders.page"), action: () => openAddModal("page", null) });
                }
                if (canPasteElements(id)) {
                    opts.push({ label: t("common.paste"), action: () => pasteElements(id) });
                }
            }
            else if (id === "_functions") opts.push({ label: t("elementAdders.function"), action: () => openAddModal("function", null) });
            else if (id === "_timers") opts.push({ label: t("elementAdders.timer"), action: () => openAddModal("timer", null) });
            else if (id === "_gameVerbs") opts.push({ label: t("elementAdders.verb"), action: () => createVerb(null) });
            else if (id === "_gameCommands") opts.push({ label: t("elementAdders.command"), action: () => createCommand(null) });
            else if (id === "_walkthrough") opts.push({ label: t("elementAdders.walkthrough"), action: () => openAddModal("walkthrough", null) });
            else if (id === "_template") opts.push({ label: t("elementAdders.template"), action: () => openAddModal("template", null) });
            else if (id === "_dynamictemplate") opts.push({ label: t("elementAdders.dynamicTemplate"), action: () => openAddModal("dynamictemplate", null) });
            else if (id === "_objecttype") opts.push({ label: t("elementAdders.type"), action: () => openAddModal("type", null) });
            else if (id === "_include") opts.push({ label: t("elementAdders.library"), action: () => openAddLibraryModal() });
            else if (id === "_javascript") opts.push({ label: t("elementAdders.javascript"), action: () => openAddJavascriptModal() });
        } else if (nt === "room") {
            opts.push(
                { label: t("elementAdders.objectHere"), action: () => openAddModal("object", id) },
                { label: t("elementAdders.roomHere"), action: () => openAddModal("room", id) },
                { label: t("elementAdders.exit"), action: () => createExit(id) },
                { label: t("elementAdders.command"), action: () => createCommand(id) },
                { label: t("elementAdders.verb"), action: () => createVerb(id) },
                { label: t("elementAdders.turnScript"), action: () => createTurnScript(id) },
                { label: t("elementAdders.pageHere"), action: () => openAddModal("page", id) },
            );
        } else if (nt === "object") {
            opts.push(
                { label: t("elementAdders.objectHere"), action: () => openAddModal("object", id) },
                { label: t("elementAdders.command"), action: () => createCommand(id) },
                { label: t("elementAdders.verb"), action: () => createVerb(id) },
                { label: t("elementAdders.turnScript"), action: () => createTurnScript(id) },
                { label: t("elementAdders.pageHere"), action: () => openAddModal("page", id) },
            );
        } else if (nt === "usergroup") {
            opts.push({ label: t("elementAdders.functionHere"), action: () => openAddModal("function", null, text) });
        } else if (nt === "page" && !$isGamebook) {
            // Text Adventure dialogue pages can nest sub-pages; gamebook pages are
            // always flat, so this adder is TA-only.
            opts.push({ label: t("elementAdders.pageHere"), action: () => openAddModal("page", id) });
        } else if (nt === "walkthrough") {
            opts.push({ label: t("elementAdders.walkthroughHere"), action: () => openAddModal("walkthrough", id) });
            if (canMoveElement(id)) {
                opts.push({ label: t("common.moveTo"), action: () => openMoveModal(id) });
            }
        } else if (nt === "function") {
            opts.push({ label: t("common.moveToFolder"), action: () => openMoveToFolderModal(id) });
        }

        // Gamebook pages ("page") are plain ElementType.Object elements underneath —
        // v5's own desktop editor wires Cut/Copy/Paste/drag-move generically off
        // EditorController with no gamebook-vs-textadventure gating at all (only
        // GetPasteParent special-cases gamebook, always pasting at the flat top
        // level), so pages get the same menu entries as rooms/objects here.
        if (nt === "room" || nt === "object" || nt === "page") {
            if (canMoveElement(id)) {
                opts.push(
                    { label: t("common.moveTo"), action: () => openMoveModal(id) },
                    { label: t("common.cut"), action: () => cutElements([id]) },
                    { label: t("common.copy"), action: () => copyElements([id]) },
                );
            }
            if (canPasteElements(id)) {
                opts.push({ label: t("common.paste"), action: () => pasteElements(id) });
            }
        }

        // Reordering + expand/collapse-all are available on any node with
        // same-parent siblings or children (headers and the game node included)
        // — both operate on the tree's synthetic grouping, not the world model.
        opts.push(...reorderOptions(node));

        if (node.children?.length && !isFiltering) {
            // While filtering, every matching branch is force-expanded anyway
            // (see effectiveExpandedIds), so both actions would be no-ops.
            opts.push(
                { label: t("treePanel.expandAllBelow"), action: () => { expandedIds = [...new Set([...expandedIds, ...collectBranchIds([node])])]; } },
                { label: t("treePanel.collapseAllBelow"), action: () => { selectBranchIfSelectedInside(node.id); const toClose = new Set(collectBranchIds([node])); expandedIds = expandedIds.filter(id => !toClose.has(id)); } },
            );
        }

        if (isDeletable(node)) {
            opts.push({ label: t("treePanel.deleteNamed", { name: text }), action: () => handleDelete(id) });
        }

        return opts;
    }

    let libraryMenuItems = $derived<DropdownMenuItem[]>([
        {
            label: t("treePanel.showLibraryElements"),
            icon: $showLibraryElements ? Check : undefined,
            action: () => toggleShowLibraryElements(),
        },
    ]);
</script>

<div
    class="flex flex-col shrink-0 border-r border-surface-200-800 bg-surface-50-950 {width === undefined ? "w-full" : ""}"
    style={width !== undefined ? `width: ${width}px` : undefined}
>
    <div class="px-3 py-2 text-xs font-semibold uppercase text-surface-600-400 border-b border-surface-200-800">
        {$isGamebook ? t("treePanel.gamePages") : t("treePanel.gameObjects")}
    </div>
    <div class="p-1.5 border-b border-surface-200-800 flex items-center gap-1">
        <div class="relative flex-1">
            <Search class="absolute left-2.5 top-1/2 -translate-y-1/2 size-3.5 text-surface-400 pointer-events-none" />
            <input
                bind:this={filterInputEl}
                type="text"
                autocapitalize="off"
                bind:value={filterText}
                placeholder={t("treePanel.filterPlaceholder")}
                aria-label={t("treePanel.filterAriaLabel")}
                class="input text-xs py-1 pl-8 pr-7 w-full"
                onkeydown={(e) => { if (e.key === "Escape" && filterText) { e.stopPropagation(); clearFilter(); } }}
            />
            {#if filterText}
                <button
                    type="button"
                    class="absolute right-2 top-1/2 -translate-y-1/2 size-4 flex items-center justify-center text-surface-400 hover:text-surface-900-50"
                    onclick={clearFilter}
                    aria-label={t("common.clearFilter")}
                ><X class="size-3.5" /></button>
            {/if}
        </div>
        <DropdownMenu items={libraryMenuItems}>
            {#snippet trigger(toggle, open)}
                <button
                    type="button"
                    class="size-6 flex-shrink-0 flex items-center justify-center rounded {$showLibraryElements ? "text-primary-500" : "text-surface-400"} hover:text-primary-500 hover:bg-surface-200-800"
                    onclick={toggle}
                    title={t("treePanel.viewOptions")}
                    aria-label={t("treePanel.viewOptions")}
                    aria-haspopup="menu"
                    aria-expanded={open}
                ><SlidersHorizontal class="size-3.5" /></button>
            {/snippet}
        </DropdownMenu>
    </div>
    {#if isFiltering && (collection.rootNode.children ?? []).length === 0}
        <div class="px-3 py-2 text-xs text-surface-400">{t("treePanel.noMatches")}</div>
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
                title={t("treePanel.nodeOptions")}
            >⋯</button>
        </span>
    {/if}
{/snippet}

{#snippet treeNode(node: HierNode, indexPath: number[])}
    {@const Icon = nodeIcon(node.id, node.nodeType)}
    <TreeView.NodeProvider value={{ node, indexPath }}>
        {#if node.children}
            <TreeView.Branch>
                <TreeView.BranchControl class="group flex items-center gap-1.5" ondblclick={() => toggleExpand(node.id)} onclick={() => activateIfAlreadySelected(node.id)}>
                    <button
                        type="button"
                        tabindex="-1"
                        class="flex-shrink-0 size-4 flex items-center justify-center transition-transform duration-150 {effectiveExpandedIds.includes(node.id) ? "rotate-90" : ""}"
                        onclick={(e) => { e.stopPropagation(); toggleExpand(node.id); }}
                        aria-label={effectiveExpandedIds.includes(node.id) ? t("treePanel.collapse") : t("treePanel.expand")}
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="size-4"><polyline points="9 18 15 12 9 6" /></svg>
                    </button>
                    <Icon class="flex-shrink-0 size-3.5 text-surface-500 {node.isLibrary ? "opacity-60" : ""}" />
                    <TreeView.BranchText class="flex-1 min-w-0 truncate {node.isLibrary ? "text-surface-600-400" : ""} {$cutElementKeys.has(node.id) ? "opacity-50 italic" : ""}">{node.text}</TreeView.BranchText>
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
            <TreeView.Item class="group flex items-center gap-1.5" onclick={() => activateIfAlreadySelected(node.id)}>
                <Icon class="flex-shrink-0 size-3.5 text-surface-500 {node.isLibrary ? "opacity-60" : ""}" />
                <span class="flex-1 min-w-0 truncate {node.isLibrary ? "text-surface-600-400" : ""} {$cutElementKeys.has(node.id) ? "opacity-50 italic" : ""}">{node.text}</span>
                <span class="opacity-0 group-hover:opacity-100 pointer-coarse:opacity-100">
                    {@render nodeActions(node)}
                </span>
            </TreeView.Item>
        {/if}
    </TreeView.NodeProvider>
{/snippet}
