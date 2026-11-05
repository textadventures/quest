<script lang="ts">
    // The Appearance tab's preview (an "appearancepreview" control): a little page of sample text
    // in the game's font and colours, as the v2 player will show it, in light mode and dark mode
    // side by side, or once with the reason when the game looks the same in both. It shows the
    // location bar and the command bar when they're turned on, and links when objects are links.
    // The colours are worked out in lib/appearance.ts.
    import { computeAppearance, type SchemeLook } from "#lib/appearance.js";
    import { locale, t } from "#lib/i18n/index.js";

    let { get }: {
        // The game's value for an attribute, its own or its theme's
        get: (attribute: string) => string | null;
    } = $props();

    let look = $derived(computeAppearance(get));
    let showLocation = $derived(/^true$/i.test(get("showlocation") ?? ""));
    let showCommandBar = $derived(/^true$/i.test(get("showcommandbar") ?? ""));
    let links = $derived(/^true$/i.test(get("enablehyperlinks") ?? ""));
    // v2's interface font (v2.css --qv-font-ui), for the location bar
    let uiFont = $derived((get("interfacefont") ?? "").trim() || 'system-ui, -apple-system, "Segoe UI", Roboto, sans-serif');
    let placeholder = $derived((get("commandplaceholder") ?? "").trim() || t("appearancePreview.typeHere"));

    // The web font, from Google Fonts as the player loads it (CoreOutput.aslx's SetWebFontName).
    // Each family is added once and kept, so going back to it doesn't fetch it again.
    $effect(() => {
        const family = look.webFont;
        if (!family) return;
        const href = "https://fonts.googleapis.com/css?family=" + family.replace(/ /g, "+");
        if (document.head.querySelector(`link[data-appearance-font="${CSS.escape(href)}"]`)) return;
        const link = document.createElement("link");
        link.rel = "stylesheet";
        link.href = href;
        link.dataset.appearanceFont = href;
        document.head.append(link);
    });

    function label(scheme: SchemeLook): string {
        if (look.schemes.length === 1) return t("appearancePreview.bothModes");
        return scheme.scheme === "dark" ? t("appearancePreview.darkMode") : t("appearancePreview.lightMode");
    }
</script>

{#snippet sample(scheme: SchemeLook)}
    <figure class="flex flex-col gap-1 m-0 min-w-0 flex-1 basis-64 max-w-md" data-appearance-scheme={scheme.scheme}>
        <figcaption class="text-xs font-semibold text-surface-600-400">{label(scheme)}</figcaption>
        <div class="appearance-margin rounded-container" style:background={scheme.margin}>
            <div
                class="appearance-page"
                style:background={scheme.background}
                style:color={scheme.text}
                style:font-family={look.font}
                style:font-size="{look.fontSizePt}pt"
            >
                {#if showLocation}
                    <div
                        class="appearance-location"
                        style:background={scheme.locationBackground}
                        style:color={scheme.locationText}
                        style:font-family={uiFont}
                        style:border-bottom={scheme.locationBorder ? `1px solid ${scheme.locationBorder}` : `1px solid color-mix(in srgb, ${scheme.text} 10%, ${scheme.background})`}
                        data-appearance-location
                    >{t("appearancePreview.room")}</div>
                {/if}
                <p class="appearance-text">
                    {t("appearancePreview.textBefore")}{#if links}<span
                        class="appearance-link"
                        style:color={scheme.link}
                        style:text-decoration={look.underlineLinks ? "underline" : "none"}
                        data-appearance-link
                    >{t("appearancePreview.object1")}</span>{:else}{t("appearancePreview.object1")}{/if}{t("appearancePreview.textBetween")}{#if links}<span
                        class="appearance-link"
                        style:color={scheme.link}
                        style:text-decoration={look.underlineLinks ? "underline" : "none"}
                    >{t("appearancePreview.object2")}</span>{:else}{t("appearancePreview.object2")}{/if}{t("appearancePreview.textAfter")}
                </p>
                {#if showCommandBar}
                    <div
                        class="appearance-command"
                        style:border-top="1px solid color-mix(in srgb, {scheme.text} 20%, {scheme.background})"
                        data-appearance-command
                    >
                        <span style:color={scheme.accent} data-appearance-accent>&gt;</span>
                        <span style:color={scheme.muted}>{placeholder}</span>
                    </div>
                {/if}
            </div>
        </div>
    </figure>
{/snippet}

<div class="flex flex-col gap-1 px-3 py-1.5" data-appearance-preview role="group" aria-label={t("appearancePreview.label")}>
    <div class="flex flex-wrap gap-3">
        {#each look.schemes as scheme (scheme.scheme)}
            {@render sample(scheme)}
        {/each}
    </div>
    {#if look.fixedReason}
        <p class="text-xs text-surface-600-400 italic" data-appearance-fixed>
            {#if look.fixedReason === "lightOnlyColours" && look.missingDark.includes("colourblend")}
                {t("appearancePreview.fixedBlend")}
            {:else if look.fixedReason === "lightOnlyColours"}
                {t("appearancePreview.fixedLightOnly", { colours: new Intl.ListFormat($locale, { type: "conjunction" }).format(look.missingDark.map(a => t(`appearancePreview.colour.${a}`))) })}
            {:else}
                {t(look.fixedReason === "ownDarkColours" ? "appearancePreview.fixedOwnDarkColours" : "appearancePreview.fixedOwnColours")}
            {/if}
        </p>
    {/if}
</div>

<style>
    .appearance-margin {
        padding: 0.6rem 1.25rem;
    }

    .appearance-page {
        line-height: 1.6;
        overflow: hidden;
    }

    .appearance-location {
        padding: 0.2rem 0.75rem;
        font-size: 0.95rem;
    }

    .appearance-text {
        margin: 0;
        padding: 0.6rem 0.75rem;
    }

    .appearance-command {
        display: flex;
        gap: 0.4em;
        padding: 0.35rem 0.75rem;
    }
</style>
