<script lang="ts">
    import { onMount } from "svelte";
    import { fetchDownloadLinks, type DownloadLinks } from "$lib/download-links";

    let links = $state<DownloadLinks | null>(null);
    let expanded = $state(false);

    // Built as one string rather than adjacent {expr}{#if} template
    // mustaches — Svelte's whitespace trimming ate the space before "·" when
    // it was split across a text node and an #if block.
    let versionLine = $derived(
        links?.version ? (links.releaseDate ? `${links.version} · released ${links.releaseDate}` : links.version) : null,
    );

    onMount(async () => {
        links = await fetchDownloadLinks();
    });
</script>

{#if links}
    <div class="flex flex-col items-center gap-1 text-sm">
        {#if versionLine}
            <p class="text-surface-500 text-xs">{versionLine}</p>
        {/if}
        {#if links.primary}
            <a href={links.primary.url} class="btn btn-sm preset-outlined-primary-500">
                {links.primary.label}
            </a>
        {/if}
        {#if links.others.length > 0 || !links.primary}
            <button
                type="button"
                class="text-surface-400 hover:text-surface-200 underline text-xs"
                onclick={() => (expanded = !expanded)}
            >
                {links.primary ? "Other platforms" : "Download the desktop app"}
            </button>
        {/if}
        {#if expanded || !links.primary}
            <div class="flex flex-col items-center gap-1 mt-1">
                {#each links.others as other (other.url)}
                    <a href={other.url} class="text-surface-300 hover:text-surface-100 underline text-xs">
                        {other.label}
                    </a>
                {/each}
                <a href={links.releasePage} target="_blank" rel="noopener" class="text-surface-400 hover:text-surface-200 underline text-xs">
                    All downloads
                </a>
            </div>
        {/if}
    </div>
{/if}
