<script lang="ts">
    // The Interface tab's preview (an "interfacepreview" control): a miniature of the v2 player on
    // a wide screen and on a phone, in the game's colours, with the parts the game turns on (the
    // room name, border, picture frame, map, panes in their order, command bar) where the layout
    // puts them. Hovering a part names it. What's on and where is worked out in lib/layout.ts,
    // the colours in lib/appearance.ts (the light scheme, or the one scheme a game has).
    import { computeAppearance } from "#lib/appearance.js";
    import { computeLayout, type PaneId } from "#lib/layout.js";
    import { t } from "#lib/i18n/index.js";

    let { get, onselect }: {
        // The game's value for an attribute, its own or its theme's
        get: (attribute: string) => string | null;
        // Clicking a part: the attribute of the option on this tab that controls it
        onselect?: (attribute: string) => void;
    } = $props();

    // The option each part of the miniature comes from (a pointer shortcut: the options are all
    // right below, for the keyboard too)
    const PART_OPTIONS: Record<string, string> = {
        location: "showlocation",
        picture: "feature_pictureframe",
        map: "gridmap",
        command: "showcommandbar",
        exits: "turnoffcompass",
        panes: "layoutpreset",
        drawer: "narrowlayoutpreset",
    };
    const PANE_OPTIONS: Record<string, string> = {
        map: "mapposition",
        inventory: "turnoffinventory",
        objects: "turnoffplacesandobjects",
        status: "statusandcompassattop",
        compass: "turnoffcompass",
        commands: "commandpane",
        customstatus: "customstatuspane",
    };

    function onclick(e: MouseEvent) {
        if (!onselect) return;
        const target = e.target as HTMLElement;
        const pane = target.closest<HTMLElement>("[data-ip-pane]")?.dataset.ipPane;
        const part = target.closest<HTMLElement>("[data-ip-part]")?.dataset.ipPart;
        const phone = target.closest<HTMLElement>("[data-ip-screen]")?.dataset.ipScreen === "phone";
        const option = pane ? PANE_OPTIONS[pane]
            : part === "sheet" ? (phone ? "narrowlayoutpreset" : "layoutpreset")
            : part ? PART_OPTIONS[part] : undefined;
        if (option) onselect(option);
    }

    let layout = $derived(computeLayout(get));
    let appearance = $derived(computeAppearance(get));
    let look = $derived(appearance.schemes[0]);
    // The top bar's light or dark button, while the game follows the player's mode
    let schemeButton = $derived(appearance.schemes.length > 1);
    let uiFont = $derived((get("interfacefont") ?? "").trim() || 'system-ui, -apple-system, "Segoe UI", Roboto, sans-serif');
    let widePanes = $derived(layout.wide === "sidepanes" && layout.panes.length > 0);
    let hasPanes = $derived(layout.panes.length > 0);
    // On a phone with no panes, a map meant for the panes is above the game
    let phoneMap = $derived(layout.map === "header" || (layout.map === "panes" && layout.narrow === "nopanes"));

    function paneName(pane: PaneId): string {
        return t(`interfacePreview.pane.${pane}`);
    }

    // A one-line summary for screen readers, since the miniature is a picture
    let summary = $derived([
        t(`interfacePreview.summary.${layout.wide}`),
        hasPanes && layout.wide !== "nopanes" ? layout.panes.map(paneName).join(", ") : null,
        t(`interfacePreview.summaryPhone.${layout.narrow}`),
    ].filter(Boolean).join(". "));
</script>

{#snippet column(phone: boolean)}
    <div
        class="ip-column"
        class:ip-bordered={layout.border}
        style:background={look.background}
        style:color={look.text}
        style:--ip-hairline="color-mix(in srgb, {look.text} 12%, {look.background})"
        style:--ip-border={look.locationBorder ? look.locationBorder : `color-mix(in srgb, ${look.text} 12%, ${look.background})`}
    >
        <div
            class="ip-topbar"
            style:background={look.locationBackground}
            style:color={look.locationText}
            style:font-family={uiFont}
            title={t("interfacePreview.part.location")}
            data-ip-part="location"
        >
            <span class="ip-room">{layout.locationName ? t("appearancePreview.room") : ""}</span>
            <span class="ip-icons" aria-hidden="true">
                <span class="ip-icon"></span>
                {#if schemeButton}
                    <span class="ip-icon ip-round"></span>
                {/if}
                {#if phone && layout.narrow === "sidepanes" && hasPanes}
                    <span class="ip-icon ip-menu" title={t("interfacePreview.part.drawer")} data-ip-part="drawer">☰</span>
                {/if}
            </span>
        </div>
        {#if layout.pictureFrame}
            <div class="ip-block ip-picture" title={t("interfacePreview.part.picture")} data-ip-part="picture">{t("interfacePreview.part.picture")}</div>
        {/if}
        {#if phone ? phoneMap : layout.map === "header"}
            <div class="ip-block ip-map" title={t("interfacePreview.part.map")} data-ip-part="map">{t("interfacePreview.pane.map")}</div>
        {/if}
        <div class="ip-text" aria-hidden="true">
            <span class="ip-line" style="width: 92%"></span>
            <span class="ip-line" style="width: 70%"><span class="ip-link" style:background={look.link}></span></span>
            <span class="ip-line" style="width: 84%"></span>
            <span class="ip-line" style="width: 40%"></span>
        </div>
        <div class="ip-bottom">
            {#if phone && layout.exitsStrip}
                <div class="ip-exits" title={t("interfacePreview.part.exits")} data-ip-part="exits">
                    <span style:border-color={look.accent} style:color={look.accent}>↑</span>
                </div>
            {/if}
            <div class="ip-commandrow">
                {#if layout.commandBar}
                    <div class="ip-command" title={t("interfacePreview.part.command")} data-ip-part="command">
                        <span style:color={look.accent}>›</span>
                    </div>
                {:else}
                    <span class="ip-spacer"></span>
                {/if}
                {#if (phone ? layout.narrow : layout.wide) === "bottombar" && hasPanes}
                    <span class="ip-icon ip-menu ip-sheet" title={t("interfacePreview.part.sheet")} data-ip-part="sheet">☰</span>
                {/if}
            </div>
        </div>
    </div>
{/snippet}

<!-- svelte-ignore a11y_click_events_have_key_events, a11y_no_noninteractive_element_interactions -->
<div class="flex flex-col gap-1 px-3 py-1.5" class:ip-clickable={!!onselect} data-interface-preview role="img" aria-label={t("interfacePreview.label", { summary })} {onclick}>
    <div class="flex flex-wrap items-end gap-4">
        <figure class="ip-figure m-0" data-ip-screen="wide">
            <figcaption class="text-xs font-semibold text-surface-600-400">{t("interfacePreview.wide")}</figcaption>
            <div class="ip-screen ip-wide rounded-container" style:background={look.margin}>
                {@render column(false)}
                {#if widePanes}
                    <div class="ip-panes" style:color={look.text} style:font-family={uiFont} data-ip-part="panes">
                        {#each layout.panes as pane (pane)}
                            <div class="ip-pane" title={paneName(pane)} data-ip-pane={pane}>{paneName(pane)}</div>
                        {/each}
                    </div>
                {/if}
            </div>
        </figure>
        <figure class="ip-figure m-0" data-ip-screen="phone">
            <figcaption class="text-xs font-semibold text-surface-600-400">{t("interfacePreview.phone")}</figcaption>
            <div class="ip-screen ip-phone rounded-container" style:background={look.background}>
                {@render column(true)}
            </div>
        </figure>
    </div>
    {#if onselect}
        <p class="text-xs text-surface-600-400 italic m-0" aria-hidden="true">{t("interfacePreview.clickHint")}</p>
    {/if}
</div>

<style>
    .ip-clickable :global([data-ip-part]),
    .ip-clickable :global([data-ip-pane]) {
        cursor: pointer;
    }

    .ip-clickable :global([data-ip-part]:hover),
    .ip-clickable :global([data-ip-pane]:hover) {
        outline: 1px solid color-mix(in srgb, currentColor 55%, transparent);
        outline-offset: -1px;
    }

    .ip-figure {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
    }

    .ip-screen {
        display: flex;
        justify-content: center;
        gap: 0.5rem;
        overflow: hidden;
        border: 1px solid rgb(128 128 128 / 0.35);
        font-size: 9px;
        line-height: 1.2;
    }

    .ip-wide {
        width: 22rem;
        max-width: 100%;
        height: 12rem;
        padding: 0 0.5rem;
    }

    .ip-phone {
        width: 6.5rem;
        height: 12rem;
    }

    .ip-column {
        display: flex;
        flex-direction: column;
        flex: 0 1 13rem;
        min-width: 0;
    }

    .ip-phone .ip-column {
        flex: 1 1 auto;
    }

    .ip-bordered {
        border-inline: 1px solid var(--ip-hairline);
    }

    .ip-topbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.25rem;
        min-height: 1.4rem;
        padding: 0 0.4rem;
        border-bottom: 1px solid var(--ip-border);
        font-weight: 600;
    }

    .ip-room {
        overflow: hidden;
        white-space: nowrap;
        text-overflow: ellipsis;
    }

    .ip-icons {
        display: flex;
        gap: 0.2rem;
        align-items: center;
    }

    .ip-icon {
        display: inline-block;
        width: 0.55rem;
        height: 0.55rem;
        border: 1px solid currentColor;
        border-radius: 2px;
        opacity: 0.6;
    }

    .ip-round {
        border-radius: 50%;
    }

    .ip-menu {
        /* Above the top bar it sits in, so it, not the bar, takes a click */
        position: relative;
        z-index: 1;
        width: auto;
        height: auto;
        border: none;
        opacity: 0.8;
        font-size: 9px;
        line-height: 1;
    }

    .ip-sheet {
        padding: 0.1rem 0.2rem;
        border: 1px solid var(--ip-hairline);
        border-radius: 3px;
    }

    .ip-block {
        display: flex;
        align-items: center;
        justify-content: center;
        margin: 0.3rem 0.4rem 0;
        height: 1.6rem;
        border: 1px dashed var(--ip-hairline);
        border-radius: 3px;
        opacity: 0.8;
    }

    .ip-text {
        display: flex;
        flex-direction: column;
        gap: 0.3rem;
        flex: 1 1 auto;
        padding: 0.5rem 0.4rem;
    }

    .ip-line {
        position: relative;
        display: block;
        height: 0.25rem;
        border-radius: 2px;
        background: var(--ip-hairline);
        background: color-mix(in srgb, currentColor 35%, transparent);
    }

    .ip-link {
        position: absolute;
        right: 0;
        top: 0;
        width: 30%;
        height: 100%;
        border-radius: 2px;
    }

    .ip-bottom {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        padding: 0 0.4rem 0.35rem;
    }

    .ip-exits span {
        display: inline-block;
        padding: 0 0.25rem;
        border: 1px solid;
        border-radius: 3px;
    }

    .ip-commandrow {
        display: flex;
        align-items: center;
        gap: 0.25rem;
    }

    .ip-command {
        flex: 1 1 auto;
        padding-bottom: 0.1rem;
        border-bottom: 1px solid currentColor;
    }

    .ip-spacer {
        flex: 1 1 auto;
    }

    .ip-panes {
        display: flex;
        flex-direction: column;
        flex: 0 0 6rem;
        padding-top: 0.3rem;
    }

    .ip-pane {
        padding: 0.25rem 0.2rem;
        border-bottom: 1px solid color-mix(in srgb, currentColor 15%, transparent);
        font-size: 8px;
        font-weight: 600;
        letter-spacing: 0.04em;
        text-transform: uppercase;
        opacity: 0.75;
        overflow: hidden;
        white-space: nowrap;
        text-overflow: ellipsis;
    }
</style>
