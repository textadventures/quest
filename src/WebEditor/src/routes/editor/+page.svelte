<script lang="ts">
  import { onMount } from 'svelte'
  import { goto } from '$app/navigation'
  import { get } from 'svelte/store'
  import { isLoaded } from '$lib/editor-store'
  import Toolbar from '$components/Toolbar.svelte'
  import TreePanel from '$components/TreePanel.svelte'
  import PropertyEditor from '$components/PropertyEditor.svelte'

  onMount(() => {
    if (!get(isLoaded)) {
      goto('/')
    }
  })
</script>

<div class="layout">
  <Toolbar />
  <div class="body">
    <TreePanel />
    <PropertyEditor />
  </div>
</div>

<style>
  .layout {
    display: flex;
    flex-direction: column;
    height: 100svh;
    overflow: hidden;
  }

  .body {
    flex: 1;
    display: flex;
    overflow: hidden;
  }
</style>
