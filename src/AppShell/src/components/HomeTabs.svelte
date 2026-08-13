<script lang="ts">
    import { page } from "$app/state";
    import { base } from "$app/paths";
    import { t } from "$lib/i18n";

    let { forceDark = false }: { forceDark?: boolean } = $props();

    const rootPath = base || "/";
    const isPlayTab = $derived(page.url.pathname === rootPath || page.url.pathname.startsWith(`${base}/play/`));

    // The auto-switching "paired" utilities (e.g. text-surface-600-400) are
    // light-dark() pairs that resolve against :root's color-scheme — which
    // follows the app-wide theme (see app.css), NOT this component's own
    // background. So a Play tab that's force-dark while the app theme is
    // light can't be made dark by wrapping its content in a dark background;
    // the paired utilities would still resolve to their light-mode value.
    // forceDark instead picks the dark-side member of each pair explicitly.
    const activeText = $derived(forceDark ? "text-surface-100" : "text-surface-900-100");
    const inactiveText = $derived(forceDark ? "text-surface-400 hover:text-surface-100" : "text-surface-600-400 hover:text-surface-900-100");
</script>

<nav class="flex gap-1 px-4 border-b {forceDark ? "border-surface-800" : "border-surface-300-700"}">
    <a
        href={rootPath}
        class="px-4 py-3 text-sm border-b-2 transition-colors {isPlayTab
            ? `border-primary-500 ${activeText}`
            : `border-transparent ${inactiveText}`}"
    >{t("homeTabs.play")}</a>
    <a
        href="{base}/open"
        class="px-4 py-3 text-sm border-b-2 transition-colors {!isPlayTab
            ? `border-primary-500 ${activeText}`
            : `border-transparent ${inactiveText}`}"
    >{t("homeTabs.create")}</a>
</nav>
