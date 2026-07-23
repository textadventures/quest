<script lang="ts">
    import type { ControlOption } from "$lib/types";

    let { value, options, onchange, oninput, class: className = "" }: {
        value: string;
        options: ControlOption[];
        onchange: (value: string) => void;
        // Fires on every keystroke, unlike onchange (which only commits on blur/select/Enter) —
        // for callers that need the live typed text, e.g. to drive a submit button's disabled
        // state without waiting for the field to lose focus.
        oninput?: (value: string) => void;
        class?: string;
    } = $props();

    // Unique ID prefix for ARIA references
    let uid = Math.random().toString(36).slice(2, 7);

    let open = $state(false);
    let inputValue = $state("");
    let activeIndex = $state(-1);
    let listboxEl = $state<HTMLDivElement | null>(null);
    let inputEl = $state<HTMLInputElement | null>(null);
    // Defaults to opening downward, but flips upward when there isn't room below — otherwise a
    // combobox pinned near the bottom of a panel (e.g. VerbsEditor's "Add Verb" row) renders its
    // suggestion list past the bottom of the screen/scroll container, invisible.
    let openUpward = $state(false);

    let hasEmptyOption = $derived(options.some(o => o.value === ""));

    function updateOpenDirection() {
        if (!inputEl) return;
        const rect = inputEl.getBoundingClientRect();
        const spaceBelow = window.innerHeight - rect.bottom;
        const spaceAbove = rect.top;
        openUpward = spaceBelow < 200 && spaceAbove > spaceBelow;
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
        updateOpenDirection();
        open = true;
    }

    function handleClick() {
        if (!open) {
            inputValue = "";
            updateOpenDirection();
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
            if (!open) { updateOpenDirection(); open = true; activeIndex = 0; return; }
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
                onchange(inputValue);
            }
        } else if (e.key === "Escape") {
            open = false;
            inputValue = value;
            activeIndex = -1;
        }
    }
</script>

<div class="relative">
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
            id="{uid}-listbox"
            role="listbox"
            class="absolute z-50 left-0 min-w-full max-h-48 overflow-y-auto rounded border border-surface-200-800 bg-white dark:bg-surface-800 shadow-md {openUpward ? "bottom-full mb-0.5" : "top-full mt-0.5"}"
        >
            {#each filtered as opt, i (opt.value)}
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
