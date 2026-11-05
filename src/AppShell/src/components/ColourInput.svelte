<script lang="ts">
    // Editor for a colour value (a "colour" control, or a script command's "colour" simple
    // editor): a text field that takes any CSS colour, such as a name or #rrggbb, with a swatch
    // of what's typed and the named colours as suggestions, and a button beside it that opens
    // the native colour picker (so does the swatch). Empty is a first-class value
    // - "not set", or for the game's text, link and page colours (<playertheme/>) the player's
    // own light or dark theme colour - shown as a half-light, half-dark swatch.
    import type { ControlOption } from "#lib/types.js";
    import { t } from "#lib/i18n/index.js";
    import { isCssColour, toHexColour } from "#lib/colour.js";
    import Combobox from "./Combobox.svelte";
    import X from "@lucide/svelte/icons/x";
    import ChevronDown from "@lucide/svelte/icons/chevron-down";
    import Palette from "@lucide/svelte/icons/palette";

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
    // While the field is being typed in, what's typed, so the swatch follows it before it's
    // committed (on Enter or leaving the field).
    let typed = $state<string | null>(null);
    let combobox = $state<ReturnType<typeof Combobox> | null>(null);
    let pickerEl = $state<HTMLInputElement | null>(null);

    let emptyLabel = $derived(playerTheme ? t("colourInput.playersOwn") : t("colourInput.notSet"));
    let emptyHint = $derived(playerTheme ? t("colourInput.playersOwnHint") : t("colourInput.notSetHint"));
    let allOptions = $derived([
        { value: "", label: emptyLabel },
        ...options.filter(o => o.value !== "").map(o => ({ value: o.value, label: o.label.trim() })),
    ]);
    let shown = $derived(pickingHex ?? typed ?? value);
    let seedHex = $derived(toHexColour(value) ?? "#000000");

    function onPickerInput(e: Event) {
        pickingHex = (e.target as HTMLInputElement).value;
    }

    function onPickerChange(e: Event) {
        pickingHex = null;
        const hex = (e.target as HTMLInputElement).value;
        if (hex !== toHexColour(value)) commit(hex);
    }

    function commit(v: string) {
        typed = null;
        const trimmed = v.trim();
        if (trimmed !== value) onchange(trimmed);
    }

    function openPicker() {
        if (!pickerEl) return;
        pickerEl.value = seedHex;
        try {
            pickerEl.showPicker();
        } catch {
            pickerEl.click();
        }
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

<!-- Sized like a plain text field with the same classes, so a colour lines up with the font and
     size boxes around it: an invisible plain input sets the width (in the same grid cell), and the
     real field fills it, with the swatch and buttons inside its padding. The picker button sits
     after it. -->
<span class="inline-flex max-w-full items-center gap-1">
    <div class="relative inline-grid max-w-full" data-colour-input>
        <!-- The text field comes first in the DOM, so a wrapping <label> (PropertyEditor's
             caption) labels it rather than the colour input in the picker button. -->
        <Combobox
            bind:this={combobox}
            {value}
            options={allOptions}
            freeEdit
            onchange={commit}
            oninput={(v) => { typed = v; }}
            {ariaLabel}
            placeholder={value === "" ? emptyLabel : undefined}
            optionPrefix={optionSwatch}
            class={className}
            wrapperClass="col-start-1 row-start-1 w-0 min-w-full"
            style="{style ? `${style}; ` : ""}width: 100%; padding-left: 1.6rem; padding-right: {value !== "" ? "2.6rem" : "1.4rem"};"
        />
        <input
            type="text"
            class="{className} invisible col-start-1 row-start-1"
            {style}
            tabindex="-1"
            aria-hidden="true"
            inert
        />
        <button
            type="button"
            class="absolute left-1 top-1/2 -translate-y-1/2 flex cursor-pointer"
            title={shown === "" ? emptyHint : t("colourInput.pick")}
            tabindex="-1"
            aria-hidden="true"
            onclick={openPicker}
        >{@render swatch(shown, "size-4")}</button>
        <span class="absolute right-0.5 top-1/2 -translate-y-1/2 flex items-center">
            {#if value !== ""}
                <button
                    type="button"
                    class="size-5 flex items-center justify-center rounded text-surface-600-400 hover:bg-surface-100-900 hover:text-surface-950-50"
                    title={t("colourInput.clear")}
                    aria-label={t("colourInput.clear")}
                    onclick={() => commit("")}
                ><X size={14} aria-hidden="true" /></button>
            {/if}
            <!-- mousedown keeps the field's focus, so the list opens on it and the field doesn't
                 commit half-typed text. -->
            <button
                type="button"
                class="size-5 flex items-center justify-center rounded text-surface-600-400 hover:bg-surface-100-900 hover:text-surface-950-50"
                title={t("colourInput.showNames")}
                aria-label={t("colourInput.showNames")}
                tabindex="-1"
                onmousedown={(e) => e.preventDefault()}
                onclick={() => combobox?.showOptions()}
            ><ChevronDown size={14} aria-hidden="true" /></button>
        </span>
    </div>
    <!-- A real button that opens the native picker (openPicker). The colour input itself sits
         under it, invisible and out of the way of the pointer, so the browser anchors its popup
         here; laid over the button instead, it didn't get the click. -->
    <span class="relative inline-flex shrink-0">
        <input
            bind:this={pickerEl}
            type="color"
            class="pointer-events-none absolute inset-0 size-full opacity-0"
            tabindex="-1"
            aria-hidden="true"
            value={seedHex}
            oninput={onPickerInput}
            onchange={onPickerChange}
            onblur={() => { pickingHex = null; }}
        />
        <button
            type="button"
            class="relative inline-flex size-6 items-center justify-center rounded border border-surface-300-700 text-surface-700-300 hover:bg-surface-100-900"
            title={t("colourInput.pick")}
            aria-label={t("colourInput.pick")}
            onclick={openPicker}
        ><Palette size={14} aria-hidden="true" /></button>
    </span>
</span>

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
