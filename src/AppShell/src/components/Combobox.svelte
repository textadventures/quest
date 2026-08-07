<script lang="ts">
    import type { ControlOption } from "$lib/types";

    let { value, options, onchange, oninput, onEnter, class: className = "", wrapperClass = "" }: {
        value: string;
        options: ControlOption[];
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
    } = $props();

    // Unique ID prefix for ARIA references
    let uid = Math.random().toString(36).slice(2, 7);

    let open = $state(false);
    let inputValue = $state("");
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
        if (!open) inputValue = value;
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

    let filtered = $derived(
        inputValue === ""
            ? options
            : options.filter(o =>
                o.label.toLowerCase().includes(inputValue.toLowerCase()) ||
                    o.value.toLowerCase().includes(inputValue.toLowerCase())
            )
    );

    let activeDescendant = $derived(
        open && activeIndex >= 0 ? `${uid}-opt-${activeIndex}` : undefined
    );

    function select(optValue: string) {
        inputValue = optValue;
        open = false;
        activeIndex = -1;
        onchange(optValue);
    }

    function handleFocus() {
        inputValue = "";
        updatePosition();
        open = true;
    }

    function handleClick() {
        if (!open) {
            inputValue = "";
            updatePosition();
            open = true;
        }
    }

    function handleBlur() {
        open = false;
        activeIndex = -1;
        if (inputValue === "") {
            inputValue = value;
        } else {
            onchange(inputValue);
        }
    }

    function handleInput(e: Event) {
        inputValue = (e.target as HTMLInputElement).value;
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
        } else if (e.key === "Enter") {
            if (open && activeIndex >= 0) {
                e.preventDefault();
                select(filtered[activeIndex].value);
            } else if (open && filtered.length === 1) {
                e.preventDefault();
                select(filtered[0].value);
            } else if (open) {
                open = false;
                // Mirrors handleBlur: nothing typed since focus (which clears inputValue to "")
                // means "leave it as-is", not "clear the field" — e.g. accepting a pre-filled
                // default by tabbing in and pressing Enter without typing over it.
                onchange(inputValue === "" ? value : inputValue);
            }
            onEnter?.();
        } else if (e.key === "Escape") {
            open = false;
            inputValue = value;
            activeIndex = -1;
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
        placeholder={value === "" && hasEmptyOption ? "(none)" : ""}
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
                    class="px-2 py-1 text-xs cursor-pointer {i === activeIndex ? "bg-primary-100 dark:bg-primary-900" : "hover:bg-surface-100-900"} {opt.value === value ? "font-medium" : ""}"
                    onmousedown={(e) => { e.preventDefault(); select(opt.value); }}
                    onmouseenter={() => { activeIndex = i; }}
                >
                    {opt.label || opt.value || "(none)"}
                </div>
            {/each}
        </div>
    {/if}
</div>
