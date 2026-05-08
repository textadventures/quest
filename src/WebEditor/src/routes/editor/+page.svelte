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

<div class="flex flex-col h-svh overflow-hidden">
  <Toolbar />
  <div class="flex flex-1 overflow-hidden">
    <TreePanel />
    <PropertyEditor />
  </div>
</div>
