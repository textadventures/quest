<script lang="ts">
  import { treeNodes, selectedKey, selectNode } from '$lib/editor-store'
  import type { TreeNode } from '$lib/types'

  interface FlatNode extends TreeNode {
    depth: number
  }

  function buildFlatTree(nodes: TreeNode[]): FlatNode[] {
    const byParent = new Map<string | null, TreeNode[]>()
    for (const n of nodes) {
      const p = n.parent ?? null
      if (!byParent.has(p)) byParent.set(p, [])
      byParent.get(p)!.push(n)
    }

    const result: FlatNode[] = []
    function traverse(parentKey: string | null, depth: number) {
      for (const n of byParent.get(parentKey) ?? []) {
        result.push({ ...n, depth })
        traverse(n.key, depth + 1)
      }
    }
    traverse(null, 0)
    return result
  }

  let flat = $derived(buildFlatTree($treeNodes))
</script>

<div class="tree-panel">
  <div class="panel-header">Game Objects</div>
  <div class="tree-scroll">
    {#each flat as node (node.key)}
      <button
        class="tree-node"
        class:selected={$selectedKey === node.key}
        style="padding-left: {0.75 + node.depth * 1.25}rem"
        onclick={() => selectNode(node.key)}
      >
        {node.text}
      </button>
    {/each}
  </div>
</div>

<style>
  .tree-panel {
    display: flex;
    flex-direction: column;
    width: 240px;
    min-width: 180px;
    border-right: 1px solid #ddd;
    background: #fafafa;
    overflow: hidden;
  }

  .panel-header {
    padding: 0.5rem 0.75rem;
    font-weight: 600;
    font-size: 0.75rem;
    text-transform: uppercase;
    color: #666;
    border-bottom: 1px solid #ddd;
    background: #f0f0f0;
  }

  .tree-scroll {
    flex: 1;
    overflow-y: auto;
  }

  .tree-node {
    display: block;
    width: 100%;
    text-align: left;
    padding-top: 0.3rem;
    padding-bottom: 0.3rem;
    padding-right: 0.75rem;
    background: none;
    border: none;
    border-radius: 0;
    font-size: 0.875rem;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .tree-node:hover {
    background: #e8e8e8;
  }

  .tree-node.selected {
    background: #1a73e8;
    color: white;
  }
</style>
