<script lang="ts">
  import { AppBar } from "@skeletonlabs/skeleton-svelte";
  import { gameFilename, saveGame, undo, redo } from "$lib/editor-store";

  function handleSave() {
    const xml = saveGame();
    const blob = new Blob([xml], { type: "application/xml" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = $gameFilename ?? "game.aslx";
    a.click();
    URL.revokeObjectURL(url);
  }
</script>

<AppBar>
  <AppBar.Toolbar class="grid-cols-[auto_1fr_auto]">
    <AppBar.Lead>
      <span class="font-semibold">Quest Viva Editor</span>
      {#if $gameFilename}
        <span class="ml-3 text-sm text-surface-500-400">{$gameFilename}</span>
      {/if}
    </AppBar.Lead>
    <AppBar.Trail>
      <div class="flex gap-2">
        <button type="button" class="btn btn-sm preset-outlined" onclick={undo} title="Undo">↩ Undo</button>
        <button type="button" class="btn btn-sm preset-outlined" onclick={redo} title="Redo">↪ Redo</button>
        <button type="button" class="btn btn-sm preset-filled-primary-500" onclick={handleSave} title="Save">💾 Save</button>
      </div>
    </AppBar.Trail>
  </AppBar.Toolbar>
</AppBar>
