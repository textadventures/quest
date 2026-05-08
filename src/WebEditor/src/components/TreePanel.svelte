<script lang="ts">
  import { TreeView, createTreeViewCollection } from '@skeletonlabs/skeleton-svelte'
  import { treeNodes, selectedKey, selectNode } from '$lib/editor-store'
  import type { TreeNode } from '$lib/types'

  interface HierNode {
    id: string
    text: string
    children?: HierNode[]
  }

  function buildHierTree(nodes: TreeNode[]): HierNode[] {
    const byParent = new Map<string | null, TreeNode[]>()
    for (const n of nodes) {
      const p = n.parent ?? null
      if (!byParent.has(p)) byParent.set(p, [])
      byParent.get(p)!.push(n)
    }
    function build(node: TreeNode): HierNode {
      const children = byParent.get(node.key)
      return { id: node.key, text: node.text, ...(children ? { children: children.map(build) } : {}) }
    }
    return (byParent.get(null) ?? []).map(build)
  }

  let collection = $derived(
    createTreeViewCollection<HierNode>({
      nodeToValue: (n) => n.id,
      nodeToString: (n) => n.text,
      rootNode: { id: '__root__', text: '', children: buildHierTree($treeNodes) }
    })
  )
</script>

<div class="flex flex-col w-60 min-w-[180px] border-r border-surface-200-800 bg-surface-50-950 overflow-hidden">
  <div class="px-3 py-2 text-xs font-semibold uppercase text-surface-500-400 border-b border-surface-200-800 bg-surface-100-900">
    Game Objects
  </div>
  <div class="flex-1 overflow-y-auto p-1">
    <TreeView
      {collection}
      selectionMode="single"
      selectedValue={$selectedKey ? [$selectedKey] : []}
      onSelectionChange={(e) => { if (e.selectedValue[0]) selectNode(e.selectedValue[0]) }}
    >
      {#snippet children()}
        {#each collection.rootNode.children ?? [] as node, i (node.id)}
          {@render treeNode(node, [i])}
        {/each}
      {/snippet}
    </TreeView>
  </div>
</div>

{#snippet treeNode(node: HierNode, indexPath: number[])}
  <TreeView.NodeProvider value={{ node, indexPath }}>
    {#if node.children}
      <TreeView.Branch>
        <TreeView.BranchControl>
          <TreeView.BranchIndicator />
          <TreeView.BranchText>{node.text}</TreeView.BranchText>
        </TreeView.BranchControl>
        <TreeView.BranchContent>
          <TreeView.BranchIndentGuide />
          {#each node.children as child, ci (child.id)}
            {@render treeNode(child, [...indexPath, ci])}
          {/each}
        </TreeView.BranchContent>
      </TreeView.Branch>
    {:else}
      <TreeView.Item>{node.text}</TreeView.Item>
    {/if}
  </TreeView.NodeProvider>
{/snippet}
