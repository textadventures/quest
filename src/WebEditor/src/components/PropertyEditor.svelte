<script lang="ts">
  import { selectedKey, selectedAttributes } from '$lib/editor-store'
</script>

<div class="property-editor">
  <div class="panel-header">Properties</div>

  {#if $selectedKey === null}
    <p class="placeholder">Select an object to view its properties.</p>
  {:else if $selectedAttributes === null}
    <p class="placeholder">No properties available.</p>
  {:else}
    <div class="prop-key-header">
      <span>Attribute</span>
      <span>Value</span>
    </div>
    <div class="prop-list">
      {#each Object.entries($selectedAttributes) as [attr, value] (attr)}
        <div class="prop-row">
          <span class="attr-name" title={attr}>{attr}</span>
          {#if value !== null}
            <span class="attr-value" title={value}>{value}</span>
          {:else}
            <em class="attr-null">null</em>
          {/if}
        </div>
      {/each}
    </div>
  {/if}
</div>

<style>
  .property-editor {
    flex: 1;
    display: flex;
    flex-direction: column;
    background: #fff;
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

  .placeholder {
    padding: 1rem 0.75rem;
    color: #888;
    font-size: 0.875rem;
  }

  .prop-key-header {
    display: grid;
    grid-template-columns: 1fr 1fr;
    padding: 0.3rem 0.75rem;
    font-size: 0.75rem;
    font-weight: 600;
    color: #666;
    background: #f8f8f8;
    border-bottom: 1px solid #eee;
  }

  .prop-list {
    flex: 1;
    overflow-y: auto;
  }

  .prop-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    padding: 0.3rem 0.75rem;
    font-size: 0.875rem;
    border-bottom: 1px solid #f0f0f0;
  }

  .prop-row:hover {
    background: #f8f8f8;
  }

  .attr-name {
    color: #555;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .attr-value {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    color: #222;
  }

  em {
    color: #aaa;
    font-style: italic;
  }
</style>
