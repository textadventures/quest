<script lang="ts">
    import { onMount } from "svelte";
    import { goto } from "$app/navigation";
    import { base } from "$app/paths";
    import { get } from "svelte/store";
    import { PUBLIC_SHOW_HOME } from "$env/static/public";
    import { isLoaded } from "$lib/editor-store";
    import PlayCatalog from "$components/PlayCatalog.svelte";

    const showHome = PUBLIC_SHOW_HOME === "true";

    // The editor lives at /edit, never here — root always means the Play tab
    // when showHome is true, full stop, with no isLoaded-based branching (that
    // used to make root show the editor whenever a game happened to still be
    // loaded in the background, even after explicitly navigating to Play).
    // When showHome is false (textadventures.co.uk), root has no content of
    // its own — it's a redirect-only entry point, matching its previous
    // behaviour exactly (editor if something's loaded, else /open).
    onMount(() => {
        const gameId = new URLSearchParams(window.location.search).get("game");
        if (gameId) {
            void goto(`${base}/edit?game=${encodeURIComponent(gameId)}`);
        } else if (!showHome && get(isLoaded)) {
            void goto(`${base}/edit`);
        } else if (!showHome) {
            void goto(`${base}/open`);
        }
    });
</script>

{#if showHome}
    <PlayCatalog />
{/if}
