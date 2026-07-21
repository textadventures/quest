<script lang="ts">
    import type { ControlOption } from "$lib/types";

    let { value, options, onchange, class: className = "" }: {
        value: string;
        options: ControlOption[];
        onchange: (value: string) => void;
        class?: string;
    } = $props();

    // Unique ID prefix for ARIA references
    let uid = Math.random().toString(36).slice(2, 7);

    let open = $state(false);
    let inputValue = $state("");
    let activeIndex = $state(-1);
    let listboxEl = $state<HTMLDivElement | null>(null);

    let hasEmptyOption = $derived(options.some(o => o.value === ""));

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
        open = true;
    }

    function handleClick() {
        if (!open) {
            inputValue = "";
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
    }

    function handleKeydown(e: KeyboardEvent) {
        if (e.key === "ArrowDown") {
            e.preventDefault();
            if (!open) { open = true; activeIndex = 0; return; }
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
            class="absolute z-50 top-full left-0 mt-0.5 min-w-full max-h-48 overflow-y-auto rounded border border-surface-200-800 bg-white dark:bg-surface-800 shadow-md"
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
