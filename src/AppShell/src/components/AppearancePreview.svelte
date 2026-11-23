<script lang="ts">
    // The Appearance tab's preview (an "appearancepreview" control): a little page of sample text
    // in the game's font and colours, as the v2 player will show it, in light mode and dark mode
    // side by side, or once with the reason when the game looks the same in both. It shows the
    // location bar and the command bar when they're turned on, and links when objects are links,
    // with a verb menu open under one of them in the menu font. Clicking a link opens or closes
    // its menu.
    // For a gamebook, it's a page with its choices, drawn as v2's gamebook layout draws them.
    // The colours are worked out in lib/appearance.ts.
    import { computeAppearance, type SchemeLook } from "#lib/appearance.js";
    import { locale, t } from "#lib/i18n/index.js";
    // The fonts built into the player. The game's own are loaded by lib/fonts.ts.
    import "#lib/builtin-fonts.js";

    let { get, gamebook = false }: {
        // The game's value for an attribute, its own or its theme's
        get: (attribute: string) => string | null;
        gamebook?: boolean;
    } = $props();

    let look = $derived(computeAppearance(get));
    let showLocation = $derived(/^true$/i.test(get("showlocation") ?? ""));
    let showCommandBar = $derived(/^true$/i.test(get("showcommandbar") ?? ""));
    let links = $derived(/^true$/i.test(get("enablehyperlinks") ?? ""));
    // v2's interface font (v2.css --qv-font-ui), for the location bar
    let uiFont = $derived((get("interfacefont") ?? "").trim() || 'system-ui, -apple-system, "Segoe UI", Roboto, sans-serif');
    let placeholder = $derived((get("commandplaceholder") ?? "").trim() || t("appearancePreview.typeHere"));
    let menuFont = $derived(look.menu.font || uiFont);

    const objects = $derived([
        { name: t("appearancePreview.object1"), verbs: [t("appearancePreview.verbLookAt"), t("appearancePreview.verbTake")] },
        { name: t("appearancePreview.object2"), verbs: [t("appearancePreview.verbLookAt"), t("appearancePreview.verbRead")] },
    ]);
    // The link whose menu is open, in both schemes at once
    let openMenu = $state<number | null>(0);
    // The open menu's height, for the room it needs under the text
    let menuHeight = $state(0);

    // A web font from before the Fonts dialog (defaultwebfont), from Google Fonts as the player
    // loads it (CoreOutput.aslx's SetWebFontName). Only a game that already has one gets here.
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

{#snippet object(scheme: SchemeLook, index: number)}
    {@const item = objects[index]}
    {#if links}<span class="appearance-link-wrap"><button
        type="button"
        class="appearance-link"
        style:color={scheme.link}
        style:text-decoration={look.underlineLinks ? "underline" : "none"}
        aria-expanded={openMenu === index}
        data-appearance-link
        onclick={() => { openMenu = openMenu === index ? null : index; }}
    >{item.name}</button>{#if openMenu === index}<span
        class="appearance-menu"
        style:background={look.menu.colours?.background ?? scheme.background}
        style:color={look.menu.colours?.text ?? scheme.text}
        style:border-color={scheme.border}
        style:font-family={menuFont}
        style:font-size={look.menu.size || "1rem"}
        bind:offsetHeight={menuHeight}
        aria-hidden="true"
        data-appearance-menu
    >{#each item.verbs as verb, i (i)}<span
        class="appearance-menu-item"
        style:background={i === 0 ? (look.menu.colours?.hoverBackground ?? scheme.accent) : null}
        style:color={i === 0 ? (look.menu.colours?.hoverText ?? scheme.background) : null}
    >{verb}</span>{/each}</span>{/if}</span>{:else}{item.name}{/if}
{/snippet}

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
                {#if gamebook}
                    <p class="appearance-text">{t("appearancePreview.gamebookText")}</p>
                    <div class="appearance-choices">
                        {#each [t("appearancePreview.gamebookChoice1"), t("appearancePreview.gamebookChoice2")] as choice, i (i)}
                            <span
                                class="appearance-choice"
                                style:border-color={scheme.border}
                                style:background={scheme.surface}
                                style:color={scheme.link}
                                data-appearance-choice
                            >{choice}</span>
                        {/each}
                    </div>
                {:else}
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
                    <p class="appearance-text" style:padding-bottom={links && openMenu !== null ? `calc(0.6rem + ${menuHeight}px)` : null}>
                        {t("appearancePreview.textBefore")}{@render object(scheme, 0)}{t("appearancePreview.textBetween")}{@render object(scheme, 1)}{t("appearancePreview.textAfter")}
                    </p>
                    {#if showCommandBar}
                        <div class="appearance-command" data-appearance-command={look.commandBar}>
                            <div
                                class="appearance-command-form"
                                style:border-bottom-color={look.commandBar === "underline" ? scheme.border : null}
                                style:box-shadow={look.commandBar === "shadow" ? `0 0 15px 5px color-mix(in srgb, ${scheme.text} 35%, transparent)` : null}
                            >
                                {#if look.commandPrompt}
                                    <span data-appearance-prompt>{look.commandPrompt}</span>
                                {:else}
                                    <span class="appearance-command-mark" style:color={scheme.accent} data-appearance-accent>›</span>
                                {/if}
                                <span style:color={`color-mix(in srgb, ${scheme.text} 50%, transparent)`}>{placeholder}</span>
                            </div>
                        </div>
                    {/if}
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

    .appearance-choices {
        display: flex;
        flex-direction: column;
        gap: 0.4rem;
        padding: 0 0.75rem 0.6rem;
    }

    .appearance-choice {
        padding: 0.25rem 0.75rem;
        border: 1px solid;
        border-radius: 6px;
    }

    .appearance-command {
        padding: 0.35rem 0.75rem 0.6rem;
    }

    /* As v2.css draws the command bar for each of its presets */
    .appearance-command-form {
        display: flex;
        align-items: center;
        gap: 0.5ch;
        padding: 0.25rem 0;
        border-bottom: 1.5px solid transparent;
    }

    [data-appearance-command="borderless"] .appearance-command-form {
        border-bottom: 0;
    }

    [data-appearance-command="shadow"] .appearance-command-form {
        padding: 0.4rem 0.75rem;
        border-bottom: 0;
        border-radius: 6px;
    }

    .appearance-command-mark {
        font-size: 1.5em;
        line-height: 1;
    }

    .appearance-link-wrap {
        position: relative;
    }

    .appearance-link {
        padding: 0;
        border: 0;
        background: none;
        font: inherit;
        cursor: pointer;
    }

    /* v2's qv-verb-menu, smaller */
    .appearance-menu {
        position: absolute;
        top: calc(100% + 2px);
        left: 0;
        z-index: 1;
        display: flex;
        flex-direction: column;
        min-width: 8rem;
        padding: 3px;
        border: 1px solid;
        border-radius: 6px;
        box-shadow: 0 4px 16px rgb(0 0 0 / 0.2);
        line-height: 1.4;
        white-space: nowrap;
    }

    .appearance-menu-item {
        padding: 0.2rem 0.6rem;
        border-radius: 4px;
    }
</style>
