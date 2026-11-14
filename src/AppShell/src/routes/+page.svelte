<script lang="ts">
    // The editor lives at /edit, never here — root always means the Play tab,
    // with no isLoaded-based branching (that used to make root show the editor
    // whenever a game happened to still be loaded in the background, even
    // after explicitly navigating to Play).
    import PlayCatalog from "#components/PlayCatalog.svelte";
</script>

<PlayCatalog />
