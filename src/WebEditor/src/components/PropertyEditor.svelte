<script lang="ts">
  import { selectedKey, selectedAttributes } from '$lib/editor-store'
</script>

<div class="flex flex-col flex-1 bg-surface-50-950 overflow-hidden">
  <div class="px-3 py-2 text-xs font-semibold uppercase text-surface-500-400 border-b border-surface-200-800 bg-surface-100-900">
    Properties
  </div>

  {#if $selectedKey === null}
    <p class="px-3 py-4 text-sm text-surface-400-500">Select an object to view its properties.</p>
  {:else if $selectedAttributes === null}
    <p class="px-3 py-4 text-sm text-surface-400-500">No properties available.</p>
  {:else}
    <div class="grid grid-cols-2 px-3 py-1 text-xs font-semibold text-surface-500-400 bg-surface-100-900 border-b border-surface-200-800">
      <span>Attribute</span>
      <span>Value</span>
    </div>
    <div class="flex-1 overflow-y-auto">
      {#each Object.entries($selectedAttributes) as [attr, value] (attr)}
        <div class="grid grid-cols-2 px-3 py-1 text-sm border-b border-surface-100-900 hover:bg-surface-100-900">
          <span class="text-surface-600-400 overflow-hidden text-ellipsis whitespace-nowrap" title={attr}>{attr}</span>
          {#if value !== null}
            <span class="overflow-hidden text-ellipsis whitespace-nowrap" title={value}>{value}</span>
          {:else}
            <em class="text-surface-400-500">null</em>
          {/if}
        </div>
      {/each}
    </div>
  {/if}
</div>
