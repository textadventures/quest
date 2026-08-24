<script lang="ts">
    import type { Snippet, Component } from "svelte";

    export interface DropdownMenuItem {
        label: string;
        action: () => void;
        icon?: Component<{ size?: number }>;
        disabled?: boolean;
        divider?: boolean;
        heading?: string;
    }

    interface Props {
        trigger: Snippet<[toggle: (e: MouseEvent) => void, open: boolean]>;
        items: DropdownMenuItem[];
        align?: "left" | "right";
    }

    let { trigger, items, align = "right" }: Props = $props();

    let open = $state(false);
    let rootEl = $state<HTMLDivElement | undefined>();
    let triggerEl: HTMLElement | undefined;
    let itemEls: (HTMLButtonElement | undefined)[] = $state([]);

    function toggle(e: MouseEvent) {
        e.stopPropagation();
        triggerEl = e.currentTarget as HTMLElement;
        open = !open;
    }

    function close(restoreFocus = true) {
        open = false;
        if (restoreFocus) triggerEl?.focus();
    }

    function select(item: DropdownMenuItem) {
        if (item.disabled) return;
        item.action();
        close();
    }

    function focusItem(index: number) {
        itemEls[index]?.focus();
    }

    // Roving focus between enabled items, wrapping past either end.
    function moveFocus(fromIndex: number, delta: number) {
        const n = items.length;
        if (n === 0) return;
        let i = fromIndex;
        for (let step = 0; step < n; step++) {
            i = (i + delta + n) % n;
            if (!items[i].disabled) {
                focusItem(i);
                return;
            }
        }
    }

    function onMenuKeydown(e: KeyboardEvent) {
        const currentIndex = itemEls.indexOf(document.activeElement as HTMLButtonElement);
        switch (e.key) {
            case "ArrowDown":
                e.preventDefault();
                moveFocus(currentIndex, 1);
                break;
            case "ArrowUp":
                e.preventDefault();
                moveFocus(currentIndex === -1 ? 0 : currentIndex, -1);
                break;
            case "Home":
                e.preventDefault();
                moveFocus(-1, 1);
                break;
            case "End":
                e.preventDefault();
                moveFocus(0, -1);
                break;
        }
    }

    $effect(() => {
        if (!open) return;
        const firstEnabled = items.findIndex(it => !it.disabled);
        if (firstEnabled !== -1) focusItem(firstEnabled);
    });

    $effect(() => {
        if (!open) return;
        function onOutside(e: MouseEvent) {
            if (!rootEl?.contains(e.target as Node)) close(false);
        }
        function onKeydown(e: KeyboardEvent) {
            if (e.key === "Escape") close();
        }
        document.addEventListener("mousedown", onOutside);
        document.addEventListener("keydown", onKeydown);
        return () => {
            document.removeEventListener("mousedown", onOutside);
            document.removeEventListener("keydown", onKeydown);
        };
    });
</script>

<div class="relative" bind:this={rootEl}>
    {@render trigger(toggle, open)}
    {#if open}
        <div
            role="menu"
            tabindex="-1"
            class="absolute {align === "right" ? "right-0" : "left-0"} top-full z-[999] mt-1 w-56 bg-surface-50-950 border border-surface-200-800 rounded shadow-lg py-1"
            onkeydown={onMenuKeydown}
        >
            {#each items as item, i (item.label)}
                {#if item.divider}
                    <div class="my-1 border-t border-surface-200-800"></div>
                {/if}
                {#if item.heading}
                    <div class="px-3 pt-1.5 pb-1 text-[11px] font-semibold text-surface-600-400 uppercase tracking-wide">{item.heading}</div>
                {/if}
                <button
                    bind:this={itemEls[i]}
                    type="button"
                    role="menuitem"
                    tabindex="-1"
                    class="w-full text-left px-3 py-1.5 text-xs flex items-center gap-2 hover:bg-surface-200-800 disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-transparent"
                    disabled={item.disabled}
                    onclick={() => select(item)}
                >
                    {#if item.icon}
                        <item.icon size={14} />
                    {/if}
                    {item.label}
                </button>
            {/each}
        </div>
    {/if}
</div>
