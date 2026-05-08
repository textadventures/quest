<script lang="ts">
  import { goto } from "$app/navigation";
  import { openGame } from "$lib/editor-store";

  let loading = $state(false);
  let error = $state<string | null>(null);

  async function handleFile(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;
    loading = true;
    error = null;
    try {
      const ok = await openGame(file);
      if (ok) {
        goto("/editor");
      } else {
        error = "Failed to load game file.";
      }
    } catch (err) {
      error = String(err);
    } finally {
      loading = false;
    }
  }
</script>

<main class="flex flex-col items-center justify-center min-h-svh gap-6 p-8">
  <h1 class="text-3xl font-semibold">Quest Viva Editor</h1>
  <p class="text-surface-500-400">Open an <code>.aslx</code> game file to begin editing.</p>

  <label class="btn preset-filled-primary-500 cursor-pointer" class:opacity-60={loading} class:pointer-events-none={loading}>
    {#if loading}
      Loading…
    {:else}
      Open game file
    {/if}
    <input type="file" accept=".aslx" onchange={handleFile} disabled={loading} class="hidden" />
  </label>

  {#if error}
    <p class="text-error-500 max-w-[40ch] text-center">{error}</p>
  {/if}
</main>
