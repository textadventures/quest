<script lang="ts">
    import { page } from "$app/state";
    import { base } from "$app/paths";

    let { forceDark = false }: { forceDark?: boolean } = $props();

    const rootPath = base || "/";
    const isPlayTab = $derived(page.url.pathname === rootPath || page.url.pathname.startsWith(`${base}/play/`));

    // Tailwind's dark: variant here follows the OS colour-scheme media query
    // (not a class toggle — see globals.css), so the auto-switching "paired"
    // utilities (e.g. text-surface-500-400) can't be forced dark by wrapping
    // them in a dark background; they'd still resolve to their light-mode
    // value if the OS is in light mode. forceDark instead picks the dark-side
    // member of each pair explicitly.
    const activeText = $derived(forceDark ? "text-surface-100" : "text-surface-900-100");
    const inactiveText = $derived(forceDark ? "text-surface-400 hover:text-surface-100" : "text-surface-500-400 hover:text-surface-900-100");
</script>

<nav class="flex gap-1 px-4 border-b {forceDark ? "border-surface-800" : "border-surface-300-700"}">
    <a
        href="{rootPath}?view=play"
        class="px-4 py-3 text-sm border-b-2 transition-colors {isPlayTab
            ? `border-primary-500 ${activeText}`
            : `border-transparent ${inactiveText}`}"
    >Play</a>
    <a
        href="{base}/open"
        class="px-4 py-3 text-sm border-b-2 transition-colors {!isPlayTab
            ? `border-primary-500 ${activeText}`
            : `border-transparent ${inactiveText}`}"
    >Create</a>
</nav>
