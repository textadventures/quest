<script lang="ts">
    // Editor for a colour value (a "colour" control, or a script command's "colour" simple
    // editor): a swatch of the current colour that opens the native colour picker, beside a text
    // field that takes any CSS colour and suggests the named colours. Empty is a first-class value
    // - "not set", or for the game's text, link and page colours (<playertheme/>) the player's
    // own light or dark theme colour - shown as a half-light, half-dark swatch.
    import type { ControlOption } from "#lib/types.js";
    import { t } from "#lib/i18n/index.js";
    import { isCssColour, toHexColour } from "#lib/colour.js";
    import Combobox from "./Combobox.svelte";
    import X from "@lucide/svelte/icons/x";

    let { value, options, onchange, playerTheme = false, ariaLabel, class: className = "", style = "" }: {
        value: string;
        // The suggested colour names (the control's <validvalues>).
        options: ControlOption[];
        onchange: (value: string) => void;
        playerTheme?: boolean;
        ariaLabel?: string;
        class?: string;
        style?: string;
    } = $props();

    // While the native picker is open, its live value - shown on the swatch without committing
    // an undo step for every drag; the picker's change event (on close) commits it.
    let pickingHex = $state<string | null>(null);

    let emptyLabel = $derived(playerTheme ? t("colourInput.playersOwn") : t("colourInput.notSet"));
    let emptyHint = $derived(playerTheme ? t("colourInput.playersOwnHint") : t("colourInput.notSetHint"));
    let allOptions = $derived([
        { value: "", label: emptyLabel },
        ...options.filter(o => o.value !== "").map(o => ({ value: o.value, label: o.label.trim() })),
    ]);
    let shown = $derived(pickingHex ?? value);
    let seedHex = $derived(toHexColour(value) ?? "#000000");

    function onPickerInput(e: Event) {
        pickingHex = (e.target as HTMLInputElement).value;
    }

    function onPickerChange(e: Event) {
        pickingHex = null;
        const hex = (e.target as HTMLInputElement).value;
        if (hex !== toHexColour(value)) onchange(hex);
    }
</script>

{#snippet swatch(colour: string, size: string)}
    {@const trimmed = colour.trim()}
    {#if trimmed === ""}
        <span class="colour-swatch {playerTheme ? "colour-swatch-player" : "colour-swatch-unset"} {size}" aria-hidden="true"></span>
    {:else if isCssColour(trimmed)}
        <span class="colour-swatch {size}" style:background-color={trimmed} aria-hidden="true"></span>
    {:else}
        <!-- Not something the browser can parse as a colour: a crossed-out swatch, so a typo
             reads as one instead of looking like a deliberately transparent colour. -->
        <span class="colour-swatch colour-swatch-invalid {size}" aria-hidden="true"></span>
    {/if}
{/snippet}

{#snippet optionSwatch(opt: ControlOption)}
    {@render swatch(opt.value, "size-3")}
{/snippet}

<div class="relative inline-flex items-center gap-0.5 max-w-full" data-colour-input>
    <!-- The text field comes first in the DOM, so a wrapping <label> (PropertyEditor's caption)
         labels it rather than the native colour input layered over the swatch. -->
    <Combobox
        {value}
        options={allOptions}
        {onchange}
        {ariaLabel}
        placeholder={value === "" ? emptyLabel : undefined}
        optionPrefix={optionSwatch}
        class={className}
        style="padding-left: 1.6rem; {style}"
    />
    <span class="absolute left-1 top-1/2 -translate-y-1/2 flex" title={value === "" ? emptyHint : t("colourInput.pick")}>
        {@render swatch(shown, "size-4")}
        <input
            type="color"
            class="absolute inset-0 size-full cursor-pointer opacity-0"
            aria-label={t("colourInput.pick")}
            value={seedHex}
            oninput={onPickerInput}
            onchange={onPickerChange}
            onblur={() => { pickingHex = null; }}
        />
    </span>
    {#if value !== ""}
        <button
            type="button"
            class="btn-icon size-6 shrink-0 rounded text-surface-600-400 hover:bg-surface-100-900 hover:text-surface-950-50"
            title={t("colourInput.clear")}
            aria-label={t("colourInput.clear")}
            onclick={() => onchange("")}
        ><X size={14} aria-hidden="true" /></button>
    {/if}
</div>

<style>
    .colour-swatch {
        display: inline-block;
        flex-shrink: 0;
        border-radius: 3px;
        /* A mid-grey edge plus an inner light ring, so White reads on the light theme and Black
           on the dark one. */
        border: 1px solid rgb(128 128 128 / 0.8);
        box-shadow: inset 0 0 0 1px rgb(255 255 255 / 0.25);
    }

    /* The player's light and dark page colours, split diagonally. */
    .colour-swatch-player {
        background: linear-gradient(135deg, #ffffff 50%, #1e1e1e 50%);
    }

    /* Not set: a transparency checkerboard. */
    .colour-swatch-unset {
        background-color: #ffffff;
        background-image: conic-gradient(#c8c8c8 25%, transparent 0 50%, #c8c8c8 0 75%, transparent 0);
        background-size: 6px 6px;
    }

    .colour-swatch-invalid {
        background:
            linear-gradient(to top right, transparent calc(50% - 1px), rgb(220 38 38) calc(50% - 1px), rgb(220 38 38) calc(50% + 1px), transparent calc(50% + 1px));
        border-style: dashed;
    }
</style>
