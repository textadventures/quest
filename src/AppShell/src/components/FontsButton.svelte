<script lang="ts">
    // The Appearance tab's Fonts button (a "fonts" control), which opens the Fonts dialog, and
    // what keeps lib/fonts.ts's gameFonts up to date with the game's files for the font pickers.
    // The font pickers' Add fonts… choice opens the dialog too, through fontsDialogOpen.
    import { assets, isGamebook } from "#lib/editor-store.js";
    import { fontsDialogOpen, gameFonts, refreshGameFonts } from "#lib/fonts.js";
    import { tPlural, t } from "#lib/i18n/index.js";
    import FontsModal from "./FontsModal.svelte";

    let { gameKey, get }: {
        gameKey: string;
        // The game's value for an attribute, its own or its theme's
        get: (attribute: string) => string | null;
    } = $props();

    // Closed when the tab goes, so it isn't open on coming back to it
    $effect(() => () => fontsDialogOpen.set(false));

    $effect(() => {
        void $assets;
        void refreshGameFonts(get("fontfiles"));
    });

    let interfaceFont = $derived(!$isGamebook && !/^true$/i.test(get("classicinterface") ?? ""));
</script>

<div class="flex flex-wrap items-center gap-2 px-3 py-1.5" data-fonts-control>
    <button type="button" class="btn btn-xs preset-outlined-primary-500 text-xs px-2 py-0.5" onclick={() => fontsDialogOpen.set(true)}>
        {t("fontsButton.label")}
    </button>
    <span class="text-xs text-surface-600-400 italic">
        {$gameFonts.length ? tPlural("fontsButton.count", $gameFonts.length) + " " : ""}{t("fontsButton.help")}
    </span>
</div>

{#if $fontsDialogOpen}
    <FontsModal {gameKey} {get} {interfaceFont} oncancel={() => fontsDialogOpen.set(false)} />
{/if}
