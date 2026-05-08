<script lang="ts">
  import { gameFilename, saveGame, undo, redo } from '$lib/editor-store'

  function handleSave() {
    const xml = saveGame()
    const blob = new Blob([xml], { type: 'application/xml' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = $gameFilename ?? 'game.aslx'
    a.click()
    URL.revokeObjectURL(url)
  }
</script>

<div class="toolbar">
  <span class="title">Quest Viva Editor</span>
  {#if $gameFilename}
    <span class="filename">{$gameFilename}</span>
  {/if}
  <div class="spacer"></div>
  <button onclick={undo} title="Undo">↩ Undo</button>
  <button onclick={redo} title="Redo">↪ Redo</button>
  <button class="primary" onclick={handleSave} title="Save">💾 Save</button>
</div>

<style>
  .toolbar {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background: #fff;
    border-bottom: 1px solid #ddd;
    height: 40px;
  }

  .title {
    font-weight: 600;
    margin-right: 0.5rem;
  }

  .filename {
    color: #666;
    font-size: 0.875rem;
  }

  .spacer {
    flex: 1;
  }

  button {
    padding: 0.3rem 0.75rem;
    border: 1px solid #ccc;
    border-radius: 4px;
    background: #f8f8f8;
    font-size: 0.875rem;
  }

  button:hover {
    background: #eee;
  }

  button.primary {
    background: #1a73e8;
    color: white;
    border-color: transparent;
  }

  button.primary:hover {
    background: #1558b0;
  }
</style>
