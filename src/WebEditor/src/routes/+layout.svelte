<script lang="ts">
  import type { Snippet } from 'svelte'
  let { children }: { children: Snippet } = $props()
</script>

<svelte:head>
  <meta name="color-scheme" content="light dark" />
</svelte:head>

{@render children()}

<style>
  :global(*, *::before, *::after) {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
  }

  :global(body) {
    font-family: system-ui, sans-serif;
    font-size: 14px;
    color: #222;
    background: #f5f5f5;
  }

  :global(button) {
    cursor: pointer;
    font: inherit;
  }
</style>
