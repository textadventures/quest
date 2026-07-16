<script lang="ts">
    import { page } from "$app/state";
    import { base } from "$app/paths";

    const rootPath = base || "/";
    const isPlayTab = $derived(page.url.pathname === rootPath || page.url.pathname.startsWith(`${base}/play/`));
</script>

<nav class="flex gap-1 px-4 border-b border-surface-300-700">
    <a
        href={rootPath}
        class="px-4 py-3 text-sm border-b-2 transition-colors {isPlayTab
            ? "border-primary-500 text-surface-900-100"
            : "border-transparent text-surface-500-400 hover:text-surface-900-100"}"
    >Play</a>
    <a
        href="{base}/open"
        class="px-4 py-3 text-sm border-b-2 transition-colors {!isPlayTab
            ? "border-primary-500 text-surface-900-100"
            : "border-transparent text-surface-500-400 hover:text-surface-900-100"}"
    >Create</a>
</nav>
