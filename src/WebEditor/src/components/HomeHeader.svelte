<script lang="ts">
    import { base } from "$app/paths";

    let { forceDark = false }: { forceDark?: boolean } = $props();
</script>

<header class="flex items-center gap-3 px-4 pt-4 pb-2 {forceDark ? "text-surface-100" : "text-surface-900-100"}">
    <img src="{base}/logo.svg" alt="" class="h-7" />
    <h1 class="text-xl font-semibold">Quest Viva</h1>
</header>
