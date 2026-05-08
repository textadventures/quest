<script lang="ts">
  import { goto } from '$app/navigation'
  import { openGame } from '$lib/editor-store'

  let loading = $state(false)
  let error = $state<string | null>(null)

  async function handleFile(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0]
    if (!file) return
    loading = true
    error = null
    try {
      const ok = await openGame(file)
      if (ok) {
        goto('/editor')
      } else {
        error = 'Failed to load game file.'
      }
    } catch (err) {
      error = String(err)
    } finally {
      loading = false
    }
  }
</script>

<main>
  <h1>Quest Viva Editor</h1>
  <p>Open an <code>.aslx</code> game file to begin editing.</p>

  <label class="open-btn" class:disabled={loading}>
    {#if loading}
      Loading…
    {:else}
      Open game file
    {/if}
    <input type="file" accept=".aslx" onchange={handleFile} disabled={loading} />
  </label>

  {#if error}
    <p class="error">{error}</p>
  {/if}
</main>

<style>
  main {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 100svh;
    gap: 1.5rem;
    padding: 2rem;
  }

  h1 {
    font-size: 2rem;
    font-weight: 600;
  }

  .open-btn {
    display: inline-block;
    padding: 0.75rem 1.5rem;
    background: #1a73e8;
    color: white;
    border-radius: 6px;
    font-size: 1rem;
    cursor: pointer;
    transition: background 0.15s;
  }

  .open-btn:hover:not(.disabled) {
    background: #1558b0;
  }

  .open-btn.disabled {
    opacity: 0.6;
    cursor: default;
  }

  .open-btn input {
    display: none;
  }

  .error {
    color: #c0392b;
    max-width: 40ch;
    text-align: center;
  }
</style>
