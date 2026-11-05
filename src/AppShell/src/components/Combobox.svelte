<script lang="ts">
    import type { ControlOption } from "#lib/types.js";
    import { t } from "#lib/i18n/index.js";
    import type { Snippet } from "svelte";

    let { value, options, onchange, oninput, onEnter, strict = false, freeEdit = false, class: className = "", wrapperClass = "", style = "", placeholder, optionPrefix, ariaLabel, title }: {
        value: string;
        options: ControlOption[];
        // Only commit typed text that names one of the options (e.g. an object reference);
        // anything else reverts to the current value. Off by default, where free text is the
        // point (a colour parameter can be any CSS colour).
        strict?: boolean;
        // A field to type a value into, with the options as suggestions (ColourInput): it keeps
        // its text on focus, so it can be edited, and shows the raw value rather than an
        // option's label. The list opens as you type, or with showOptions(). Emptying it and
        // leaving commits "". Off by default, where the field is for choosing from the list and
        // empties on focus to show all of it.
        freeEdit?: boolean;
        onchange: (value: string) => void;
        // Fires on every keystroke, unlike onchange (which only commits on blur/select/Enter) —
        // for callers that need the live typed text, e.g. to drive a submit button's disabled
        // state without waiting for the field to lose focus.
        oninput?: (value: string) => void;
        // Fires after Enter commits a value (selecting an option or freeform typed text) — for
        // callers that treat Enter as "confirm and proceed" (e.g. a modal's primary action),
        // since Enter alone doesn't submit anything outside a <form>. Fires after onchange, so
        // the caller's value is already up to date if onchange updates synchronously.
        onEnter?: () => void;
        class?: string;
        // Applied to the root wrapper rather than the <input> — needed when a caller's flex
        // row relies on this component itself (not just the input inside it) to grow/shrink,
        // since flex sizing classes on the input don't affect its own wrapping element.
        wrapperClass?: string;
        // Inline style for the <input> — e.g. a per-instance pixel width (<width> control hint)
        // that Tailwind's static class scanning can't express since it's only known at runtime.
        style?: string;
        // Overrides the default placeholder ("(none)" while an empty option's value is current).
        placeholder?: string;
        // Rendered before each option's label in the list, e.g. ColourInput's colour swatch.
        optionPrefix?: Snippet<[ControlOption]>;
        ariaLabel?: string;
        title?: string;
    } = $props();

    // Unique ID prefix for ARIA references
    let uid = Math.random().toString(36).slice(2, 7);

    let open = $state(false);
    let inputValue = $state("");
    // True only while inputValue holds live typed text the user hasn't committed yet (via
    // handleInput) — false once it's just redisplaying an already-committed value/selection's
    // label (select(), the closed-state effect, Escape). Needed because inputValue is now the
    // *label*, not the raw value: handleBlur used to be able to re-send inputValue as-is after a
    // select() (label happened to equal the raw value), but doing that today would send the
    // display label instead of the option's value.
    let dirty = $state(false);
    let activeIndex = $state(-1);
    let listboxEl = $state<HTMLDivElement | null>(null);
    let inputEl = $state<HTMLInputElement | null>(null);
    // This field commonly sits inside a scrolled panel (a script row, a property tab). An
    // absolutely-positioned popover is still clipped by that panel's overflow regardless of
    // z-index, so the popover is portaled to <body> (see `portal` below) and positioned with
    // `fixed` viewport coordinates computed from the input's rect instead — mirrors
    // ExpressionInput.svelte's popover.
    let popoverStyle = $state("");

    function portal(node: HTMLElement) {
        document.body.appendChild(node);
        return {
            destroy() {
                node.remove();
            },
        };
    }

    let hasEmptyOption = $derived(options.some(o => o.value === ""));

    // The input shows an option's label while closed, never its raw value — e.g. a "(top
    // level)" option can have an internal sentinel value like "_objects" that a user should
    // never see.
    function labelFor(v: string) {
        return options.find(o => o.value === v)?.label ?? v;
    }

    // What the field shows for a committed value.
    function displayFor(v: string) {
        return freeEdit ? v : labelFor(v);
    }

    // Opens the whole list (a freeEdit field's ▾ button), keeping the field's focus and text.
    export function showOptions() {
        if (open) {
            open = false;
            return;
        }
        inputEl?.focus();
        dirty = false;
        updatePosition();
        open = true;
    }

    // Opens downward by default, flips upward when there isn't room below.
    function updatePosition() {
        if (!inputEl) return;
        const rect = inputEl.getBoundingClientRect();
        const approxPopoverHeight = 200;
        const spaceBelow = window.innerHeight - rect.bottom;
        const spaceAbove = rect.top;
        const openUpward = spaceBelow < approxPopoverHeight && spaceAbove > spaceBelow;
        const top = openUpward ? rect.top - 4 : rect.bottom + 4;
        popoverStyle = `position: fixed; left: ${rect.left}px; top: ${top}px; width: ${rect.width}px; transform: translateY(${openUpward ? "-100%" : "0"});`;
    }

    $effect(() => {
        if (!open) inputValue = displayFor(value);
    });

    // Reset highlight when filtered list changes
    $effect(() => {
        void filtered; // track
        activeIndex = -1;
    });

    // Scroll active option into view
    $effect(() => {
        if (activeIndex >= 0 && listboxEl) {
            const el = listboxEl.querySelector<HTMLElement>(`[data-idx="${activeIndex}"]`);
            el?.scrollIntoView({ block: "nearest" });
        }
    });

    // Re-run positioning on scroll/resize while open so the popover tracks the input if the
    // underlying panel scrolls (capture:true so this also fires for nested scroll containers).
    $effect(() => {
        if (!open) return;
        updatePosition();
        function onReposition() {
            updatePosition();
        }
        window.addEventListener("scroll", onReposition, true);
        window.addEventListener("resize", onReposition);
        return () => {
            window.removeEventListener("scroll", onReposition, true);
            window.removeEventListener("resize", onReposition);
        };
    });

    // A freeEdit field filters by what's been typed, not by the value it started with.
    let filterText = $derived(freeEdit && !dirty ? "" : inputValue);
    let filtered = $derived(
        filterText === ""
            ? options
            : options.filter(o =>
                o.label.toLowerCase().includes(filterText.toLowerCase()) ||
                    o.value.toLowerCase().includes(filterText.toLowerCase())
            )
    );

    let activeDescendant = $derived(
        open && activeIndex >= 0 ? `${uid}-opt-${activeIndex}` : undefined
    );

    function select(optValue: string) {
        inputValue = displayFor(optValue);
        dirty = false;
        open = false;
        activeIndex = -1;
        onchange(optValue);
    }

    // Exact label/value match first, then a unique case-insensitive one.
    function matchTyped(text: string): string | null {
        const exact = options.find(o => o.value === text || o.label === text);
        if (exact) return exact.value;
        const lower = text.toLowerCase();
        const matches = options.filter(o => o.value.toLowerCase() === lower || o.label.toLowerCase() === lower);
        return matches.length === 1 ? matches[0].value : null;
    }

    function commitTyped(text: string) {
        if (!strict) {
            onchange(text);
            return;
        }
        const match = matchTyped(text);
        if (match === null) {
            inputValue = labelFor(value);
        } else {
            inputValue = labelFor(match);
            onchange(match);
        }
    }

    function handleFocus() {
        if (freeEdit) return;
        inputValue = "";
        dirty = false;
        updatePosition();
        open = true;
    }

    function handleClick() {
        if (!open && !freeEdit) {
            inputValue = "";
            dirty = false;
            updatePosition();
            open = true;
        }
    }

    function handleBlur() {
        open = false;
        activeIndex = -1;
        if (freeEdit) {
            if (dirty) commitTyped(inputValue);
            else inputValue = displayFor(value);
        } else if (!dirty || inputValue === "") {
            inputValue = labelFor(value);
        } else {
            commitTyped(inputValue);
        }
        dirty = false;
    }

    function handleInput(e: Event) {
        inputValue = (e.target as HTMLInputElement).value;
        dirty = true;
        open = true;
        oninput?.(inputValue);
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "ArrowDown") {
            e.preventDefault();
            if (!open) { updatePosition(); open = true; activeIndex = 0; return; }
            activeIndex = Math.min(activeIndex + 1, filtered.length - 1);
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            if (!open) return;
            activeIndex = Math.max(activeIndex - 1, 0);
        } else if (e.key === "Enter" && freeEdit) {
            // What's typed, as typed, unless a suggestion is highlighted: "red" is red, not the
            // first of the reds the list suggests.
            e.preventDefault();
            if (open && activeIndex >= 0) {
                select(filtered[activeIndex].value);
            } else {
                open = false;
                if (dirty) commitTyped(inputValue);
                dirty = false;
            }
            onEnter?.();
        } else if (e.key === "Enter") {
            e.preventDefault();
            if (open && activeIndex >= 0) {
                select(filtered[activeIndex].value);
            } else if (open && filtered.length === 1) {
                select(filtered[0].value);
            } else if (open) {
                open = false;
                // Mirrors handleBlur: nothing typed since focus (which clears inputValue to "")
                // means "leave it as-is", not "clear the field" — e.g. accepting a pre-filled
                // default by tabbing in and pressing Enter without typing over it.
                if (inputValue === "") onchange(value);
                else commitTyped(inputValue);
                dirty = false;
            }
            onEnter?.();
        } else if (e.key === "Escape") {
            open = false;
            inputValue = displayFor(value);
            dirty = false;
            activeIndex = -1;
            if (freeEdit) oninput?.(inputValue);
        }
    }
</script>

<div class="relative {wrapperClass}">
    <input
        bind:this={inputEl}
        type="text"
        autocapitalize="off"
        role="combobox"
        aria-expanded={open}
        aria-autocomplete="list"
        aria-activedescendant={activeDescendant}
        aria-controls="{uid}-listbox"
        class={className}
        {style}
        placeholder={placeholder ?? (value === "" && hasEmptyOption ? t("common.none") : "")}
        aria-label={ariaLabel}
        {title}
        value={inputValue}
        onfocus={handleFocus}
        onclick={handleClick}
        onblur={handleBlur}
        oninput={handleInput}
        onkeydown={handleKeydown}
    />
    {#if open && filtered.length > 0}
        <div
            bind:this={listboxEl}
            use:portal
            style={popoverStyle}
            id="{uid}-listbox"
            role="listbox"
            class="z-50 max-h-48 overflow-y-auto rounded border border-surface-200-800 bg-white dark:bg-surface-800 shadow-md"
        >
            {#each filtered as opt, i (opt.value)}
                {#if opt.group && opt.group !== filtered[i - 1]?.group}
                    <div class="px-2 pt-1.5 pb-0.5 text-[10px] font-semibold uppercase text-surface-600-400 sticky top-0 bg-white dark:bg-surface-800">{opt.group}</div>
                {/if}
                <div
                    id="{uid}-opt-{i}"
                    data-idx={i}
                    role="option"
                    tabindex="-1"
                    aria-selected={opt.value === value}
                    class="px-2 py-1 text-xs cursor-pointer {optionPrefix ? "flex items-center gap-1.5" : ""} {i === activeIndex ? "bg-primary-100 dark:bg-primary-900" : "hover:bg-surface-100-900"} {opt.value === value ? "font-medium" : ""}"
                    onmousedown={(e) => { e.preventDefault(); select(opt.value); }}
                    onmouseenter={() => { activeIndex = i; }}
                >
                    <!-- On one line: whitespace between the two would end up in the option's text. -->
                    {#if optionPrefix}{@render optionPrefix(opt)}{/if}{opt.label || opt.value || t("common.none")}
                </div>
            {/each}
        </div>
    {/if}
</div>
