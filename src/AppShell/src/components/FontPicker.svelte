<script lang="ts">
    // A font setting on the Appearance tab (a "font" control): the game's text font, or its
    // interface font. A list of fonts each shown in its own font: the game's own (added with the
    // Fonts dialog), the ones built into the player and the ones most computers have. Add fonts…
    // under the game's own opens the Fonts dialog. Custom… is for typing a CSS font-family list,
    // and a setting that's none of those shows as one. See lib/fonts.ts.
    import { tick } from "svelte";
    import ChevronDown from "@lucide/svelte/icons/chevron-down";
    import Check from "@lucide/svelte/icons/check";
    import Plus from "@lucide/svelte/icons/plus";
    import { BUILT_IN_FONTS, SYSTEM_FONTS, findFont, firstFamily, fontsDialogOpen, type FontChoice, type GameFont } from "#lib/fonts.js";
    import { t } from "#lib/i18n/index.js";
    import "#lib/builtin-fonts.js";

    let { value, fonts, allowDefault = false, label, onchange }: {
        value: string;
        // The game's own fonts
        fonts: GameFont[];
        // Whether there's a Default choice, which empties the setting (the interface font)
        allowDefault?: boolean;
        // The control's caption, for the list's name
        label: string;
        onchange: (value: string) => void;
    } = $props();

    const DEFAULT_UI_FONT = 'system-ui, -apple-system, "Segoe UI", Roboto, sans-serif';
    const id = `font-picker-${Math.random().toString(36).slice(2)}`;

    interface Option {
        key: string;
        label: string;
        // What choosing it sets, or null for Custom… and Add fonts…
        stack: string | null;
        // Add fonts…, which opens the Fonts dialog rather than being a font
        add?: boolean;
        // The font to show it in
        font: string;
    }
    interface Group { heading: string | null; options: Option[] }

    const choice = (prefix: string) => (f: FontChoice): Option => ({ key: `${prefix}:${f.stack}`, label: f.family, stack: f.stack, font: f.stack });

    let groups = $derived<Group[]>([
        ...(allowDefault ? [{ heading: null, options: [{ key: "default", label: t("fontPicker.default"), stack: "", font: DEFAULT_UI_FONT }] }] : []),
        {
            heading: t("fontPicker.gameFonts"),
            options: [
                ...fonts.filter(f => f.listed).map(choice("game")),
                { key: "add", label: t("fontPicker.addFonts"), stack: null, font: "inherit", add: true },
            ],
        },
        { heading: t("fontPicker.builtIn"), options: BUILT_IN_FONTS.map(choice("builtin")) },
        { heading: t("fontPicker.system"), options: SYSTEM_FONTS.map(choice("system")) },
        { heading: null, options: [{ key: "custom", label: t("fontPicker.custom"), stack: null, font: "inherit" }] },
    ]);
    let options = $derived(groups.flatMap(g => g.options));

    let found = $derived(value.trim() ? findFont(value, fonts) : null);
    // Typing a font list in, chosen with Custom… or because the setting is none of the fonts here
    let customChosen = $state(false);
    let custom = $derived(customChosen || (value.trim() !== "" && !found) || (value.trim() === "" && !allowDefault));
    let selectedKey = $derived(
        custom ? "custom"
            : !value.trim() ? "default"
            : options.find(o => o.stack !== null && o.stack.trim() === value.trim())?.key
                ?? options.find(o => o.stack && firstFamily(o.stack).toLowerCase() === firstFamily(value).toLowerCase())?.key
                ?? "custom");

    let shownLabel = $derived(custom ? t("fontPicker.customLabel") : !value.trim() ? t("fontPicker.default") : found?.font.family ?? value);
    let shownFont = $derived(custom ? "inherit" : !value.trim() ? DEFAULT_UI_FONT : value);

    let open = $state(false);
    let active = $state(0);
    let rootEl = $state<HTMLDivElement>();
    let listEl = $state<HTMLDivElement>();
    let buttonEl = $state<HTMLButtonElement>();
    let customEl = $state<HTMLInputElement>();

    async function openList() {
        open = true;
        active = Math.max(0, options.findIndex(o => o.key === selectedKey));
        await tick();
        listEl?.focus();
        scrollToActive();
    }

    function close(focusButton = true) {
        open = false;
        if (focusButton) buttonEl?.focus();
    }

    async function choose(option: Option) {
        if (option.add) {
            close(false);
            fontsDialogOpen.set(true);
            return;
        }
        close(option.stack !== null);
        if (option.stack === null) {
            customChosen = true;
            await tick();
            customEl?.focus();
            customEl?.select();
            return;
        }
        customChosen = false;
        if (option.stack !== value) onchange(option.stack);
    }

    function scrollToActive() {
        listEl?.querySelector(`[data-index="${active}"]`)?.scrollIntoView({ block: "nearest" });
    }

    function onListKeydown(e: KeyboardEvent) {
        let next = -1;
        if (e.key === "ArrowDown") next = Math.min(options.length - 1, active + 1);
        else if (e.key === "ArrowUp") next = Math.max(0, active - 1);
        else if (e.key === "Home") next = 0;
        else if (e.key === "End") next = options.length - 1;
        else if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            void choose(options[active]);
            return;
        } else if (e.key === "Escape") {
            e.preventDefault();
            e.stopPropagation();
            close();
            return;
        } else if (e.key === "Tab") {
            close(false);
            return;
        }
        if (next < 0) return;
        e.preventDefault();
        active = next;
        scrollToActive();
    }

    function onButtonKeydown(e: KeyboardEvent) {
        if (e.key === "ArrowDown" || e.key === "ArrowUp") {
            e.preventDefault();
            void openList();
        }
    }

    $effect(() => {
        if (!open) return;
        function onPointerDown(e: PointerEvent) {
            if (!rootEl?.contains(e.target as Node)) close(false);
        }
        document.addEventListener("pointerdown", onPointerDown, true);
        return () => document.removeEventListener("pointerdown", onPointerDown, true);
    });
</script>

<div class="font-picker" bind:this={rootEl}>
    <button
        bind:this={buttonEl}
        type="button"
        class="input text-xs py-0.5 px-1.5 font-picker-button"
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label={`${label}: ${shownLabel}`}
        data-font-picker-button
        onclick={() => open ? close() : openList()}
        onkeydown={onButtonKeydown}
    >
        <span class="font-picker-name" style:font-family={shownFont}>{shownLabel}</span>
        <ChevronDown size={12} />
    </button>
    {#if custom}
        <input
            bind:this={customEl}
            type="text"
            class="input text-xs py-0.5 px-1.5 font-picker-custom"
            aria-label={t("fontPicker.customInput", { label })}
            placeholder="'My Font', Georgia, serif"
            value={value}
            data-font-picker-custom
            onchange={(e) => onchange((e.target as HTMLInputElement).value.trim())}
        />
    {/if}
    {#if open}
        <div
            bind:this={listEl}
            id={id}
            role="listbox"
            tabindex="-1"
            aria-label={label}
            aria-activedescendant={`${id}-${active}`}
            class="font-picker-list"
            onkeydown={onListKeydown}
        >
            {#each groups as group, g (g)}
                {#if group.heading}
                    <div class="font-picker-heading" role="presentation">{group.heading}</div>
                {:else if g > 0}
                    <div class="font-picker-divider" role="presentation"></div>
                {/if}
                {#each group.options as option (option.key)}
                    {@const index = options.indexOf(option)}
                    <div
                        id={`${id}-${index}`}
                        role="option"
                        tabindex="-1"
                        aria-selected={option.key === selectedKey}
                        class="font-picker-option"
                        class:active={index === active}
                        data-index={index}
                        data-font-option={option.label}
                        onclick={(e) => {
                            // The picker can be inside a row's <label>, which would otherwise pass
                            // the click on to the button and open the list again
                            e.preventDefault();
                            void choose(option);
                        }}
                        onkeydown={() => {}}
                        onpointermove={() => { active = index; }}
                    >
                        {#if option.add}
                            <span class="font-picker-check"><Plus size={12} /></span>
                            <span class="font-picker-add">{option.label}</span>
                        {:else}
                            <span class="font-picker-check">{#if option.key === selectedKey}<Check size={12} />{/if}</span>
                            <span style:font-family={option.font}>{option.label}</span>
                        {/if}
                    </div>
                {/each}
            {/each}
        </div>
    {/if}
</div>

<style>
    .font-picker {
        position: relative;
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 0.375rem;
    }

    .font-picker-button {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.5rem;
        width: 12rem;
        max-width: 100%;
        text-align: start;
        cursor: pointer;
    }

    .font-picker-name {
        overflow: hidden;
        font-size: 0.875rem;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .font-picker-custom {
        width: 16rem;
        max-width: 100%;
        font-family: ui-monospace, monospace;
    }

    .font-picker-list {
        position: absolute;
        top: calc(100% + 0.25rem);
        left: 0;
        z-index: 40;
        width: 16rem;
        max-height: 20rem;
        overflow-y: auto;
        padding: 0.25rem 0;
        border: 1px solid var(--color-surface-300-700);
        border-radius: 0.375rem;
        background: var(--color-surface-50-950);
        box-shadow: 0 4px 16px rgb(0 0 0 / 0.15);
    }

    .font-picker-list:focus {
        outline: none;
    }

    .font-picker-heading {
        padding: 0.375rem 0.75rem 0.125rem;
        color: var(--color-surface-600-400);
        font-size: 0.6875rem;
        font-weight: 600;
        letter-spacing: 0.03em;
        text-transform: uppercase;
    }

    .font-picker-divider {
        margin: 0.25rem 0;
        border-top: 1px solid var(--color-surface-200-800);
    }

    .font-picker-add {
        color: var(--color-primary-700-300);
        font-size: 0.8125rem;
    }

    .font-picker-option {
        display: flex;
        align-items: center;
        gap: 0.25rem;
        padding: 0.25rem 0.75rem 0.25rem 0.5rem;
        font-size: 0.9375rem;
        cursor: pointer;
    }

    .font-picker-option.active {
        background: color-mix(in srgb, var(--color-primary-500) 15%, transparent);
    }

    .font-picker-check {
        display: flex;
        flex: none;
        width: 1rem;
        color: var(--color-primary-500);
    }
</style>
