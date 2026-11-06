<script lang="ts">
    // The game's Theme chooser (a "dropdowntypes" control with <previews/>): each theme as a little
    // page in its own colours and font, so the author can see what they're choosing. A theme with
    // light and dark colours (or none of its own, which follows the player's light or dark mode)
    // is drawn half light, half dark; one with colours for one scheme, in just those.
    //
    // Toggle buttons with one tab stop, in the row's group (PropertyEditor names it): the arrow keys move between them, and a click,
    // Enter or Space chooses one. Moving doesn't choose, since choosing a theme also clears the
    // author's own look (<resetoverrides/>).
    import type { ControlOption, DropdownTypePreview } from "#lib/types.js";
    import Check from "@lucide/svelte/icons/check";

    let { value, options, previews, onchange }: {
        value: string;
        options: ControlOption[];
        previews: DropdownTypePreview[];
        onchange: (value: string) => void;
    } = $props();

    // v2's own colours (docs/ui-v2-contract.md), for what a theme leaves to the player
    const playerLight = { background: "#ffffff", text: "#1a1a1a", link: "#1f6f78" };
    const playerDark = { background: "#18181b", text: "#e4e4e7", link: "#7cc4cc" };

    interface Half { background: string; text: string; link: string }

    function halves(preview: DropdownTypePreview | undefined): Half[] {
        if (!preview) return [playerLight, playerDark];
        if (preview.background && !preview.darkBackground) {
            return [{ background: preview.background, text: preview.text ?? "Black", link: preview.link ?? "Blue" }];
        }
        const light = preview.background
            ? { background: preview.background, text: preview.text ?? "Black", link: preview.link ?? "Blue" }
            : { ...playerLight, link: preview.link ?? playerLight.link };
        const dark = preview.darkBackground
            ? { background: preview.darkBackground, text: preview.darkText ?? playerDark.text, link: preview.darkLink ?? playerDark.link }
            : playerDark;
        return [light, dark];
    }

    let previewOf = $derived(new Map(previews.map(p => [p.type, p])));
    let buttons: HTMLButtonElement[] = $state([]);
    let focusIndex = $state(-1);
    // The one tab stop: the chosen theme, until the arrows move off it
    let tabIndex = $derived(focusIndex >= 0 ? focusIndex : Math.max(0, options.findIndex(o => o.value === value)));

    function onKeydown(e: KeyboardEvent, index: number) {
        let next = -1;
        if (e.key === "ArrowRight" || e.key === "ArrowDown") next = (index + 1) % options.length;
        else if (e.key === "ArrowLeft" || e.key === "ArrowUp") next = (index - 1 + options.length) % options.length;
        else if (e.key === "Home") next = 0;
        else if (e.key === "End") next = options.length - 1;
        if (next < 0) return;
        e.preventDefault();
        focusIndex = next;
        buttons[next]?.focus();
    }
</script>

<div class="theme-picker">
    {#each options as opt, i (opt.value)}
        {@const preview = previewOf.get(opt.value)}
        {@const font = preview?.font ?? "Georgia, serif"}
        {@const chosen = opt.value === value}
        <button
            bind:this={buttons[i]}
            type="button"
            class="theme-card"
            class:chosen
            aria-pressed={chosen}
            tabindex={i === tabIndex ? 0 : -1}
            data-theme-value={opt.value}
            onclick={() => { focusIndex = i; if (!chosen) onchange(opt.value); }}
            onkeydown={(e) => onKeydown(e, i)}
            onblur={() => { if (focusIndex === i) focusIndex = -1; }}
        >
            <span class="theme-swatch" aria-hidden="true">
                {#each halves(preview) as half, h (h)}
                    <span class="theme-half" style:background-color={half.background} style:color={half.text} style:font-family={font}>
                        <span class="theme-aa">Aa</span>
                        <span class="theme-link" style:color={half.link}>link</span>
                    </span>
                {/each}
                {#if chosen}<span class="theme-check"><Check size={10} strokeWidth={3} /></span>{/if}
            </span>
            <span class="theme-name">{opt.label}</span>
        </button>
    {/each}
</div>

<style>
    .theme-picker {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(6.5rem, 1fr));
        gap: 0.5rem;
        width: 100%;
    }

    .theme-card {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        padding: 0.25rem;
        border: 1px solid var(--color-surface-300-700);
        border-radius: 0.5rem;
        background: none;
        color: inherit;
        text-align: start;
        cursor: pointer;
    }

    .theme-card:hover {
        border-color: var(--color-primary-500);
    }

    .theme-card:focus-visible {
        outline: 2px solid var(--color-primary-500);
        outline-offset: 2px;
    }

    .theme-card.chosen {
        border-color: var(--color-primary-500);
        box-shadow: 0 0 0 1px var(--color-primary-500);
    }

    .theme-swatch {
        position: relative;
        display: flex;
        height: 3rem;
        overflow: hidden;
        border-radius: 0.25rem;
        box-shadow: inset 0 0 0 1px rgb(0 0 0 / 0.1);
    }

    .theme-half {
        display: flex;
        flex: 1;
        flex-direction: column;
        justify-content: center;
        padding: 0 0.4rem;
        line-height: 1.1;
    }

    .theme-aa {
        font-size: 1.1rem;
    }

    .theme-link {
        font-size: 0.7rem;
        text-decoration: underline;
    }

    /* The chosen theme's tick sits on its swatch, not beside its name, so choosing one doesn't
       make the name wider and wrap it. */
    .theme-check {
        position: absolute;
        top: 0.2rem;
        right: 0.2rem;
        display: flex;
        padding: 0.1rem;
        border-radius: 9999px;
        background: var(--color-primary-500);
        color: white;
        box-shadow: 0 0 0 1px rgb(255 255 255 / 0.8);
    }

    .theme-name {
        overflow: hidden;
        font-size: 0.75rem;
        text-overflow: ellipsis;
        white-space: nowrap;
    }
</style>
