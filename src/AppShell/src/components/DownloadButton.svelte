<script lang="ts">
    import { onMount } from "svelte";
    import DownloadIcon from "@lucide/svelte/icons/download";
    import { fetchDownloadLinks, type DownloadLinks } from "$lib/download-links";

    // compact renders as a small labeled header button with a dropdown, for
    // HomeHeader.svelte (shown on both the Play and Create tabs). The
    // non-compact form is the original inline widget, kept for
    // site/src/components/DownloadButton.astro's sibling use case elsewhere
    // if this is ever embedded somewhere content-width rather than
    // header-width.
    let { compact = false }: { compact?: boolean } = $props();

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

    // Same click-outside pattern as Toolbar.svelte's "Add" dropdown.
    $effect(() => {
        if (!compact || !expanded) return;
        function onOutside(e: MouseEvent) {
            if (!(e.target as HTMLElement).closest(".download-dropdown")) expanded = false;
        }
        document.addEventListener("mousedown", onOutside);
        return () => document.removeEventListener("mousedown", onOutside);
    });
</script>

{#if links}
    {#if compact}
        <div class="download-dropdown relative">
            <button
                type="button"
                class="btn btn-sm preset-outlined-primary-500 gap-1"
                onclick={() => (expanded = !expanded)}
                title={links.primary ? `Download desktop app — ${links.primary.label}` : "Download the desktop app"}
                aria-label="Download the desktop app"
                aria-expanded={expanded}
            >
                <DownloadIcon size={14} />
                Desktop app
            </button>
            {#if expanded}
                <div class="absolute right-0 top-full z-[999] mt-1 w-56 bg-surface-50-950 border border-surface-200-800 rounded shadow-lg py-2 flex flex-col items-stretch gap-1 text-surface-900-100">
                    {#if versionLine}
                        <p class="text-surface-500 text-xs px-3">{versionLine}</p>
                    {/if}
                    {#if links.primary}
                        <a href={links.primary.url} class="px-3 py-1 text-sm hover:bg-surface-200-800">{links.primary.label}</a>
                    {/if}
                    {#each links.others as other (other.url)}
                        <a href={other.url} class="px-3 py-1 text-sm hover:bg-surface-200-800">{other.label}</a>
                    {/each}
                    <a href={links.releasePage} target="_blank" rel="noopener" class="px-3 py-1 text-sm hover:bg-surface-200-800">
                        All downloads
                    </a>
                </div>
            {/if}
        </div>
    {:else}
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
{/if}
