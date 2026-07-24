<script lang="ts">
    import type { Snippet, Component } from "svelte";

    export interface DropdownMenuItem {
        label: string;
        action: () => void;
        icon?: Component<{ size?: number }>;
        disabled?: boolean;
        divider?: boolean;
    }

    interface Props {
        trigger: Snippet<[toggle: (e: MouseEvent) => void, open: boolean]>;
        items: DropdownMenuItem[];
        align?: "left" | "right";
    }

    let { trigger, items, align = "right" }: Props = $props();

    let open = $state(false);
    let rootEl = $state<HTMLDivElement | undefined>();

    function toggle(e: MouseEvent) {
        e.stopPropagation();
        open = !open;
    }

    function close() { open = false; }

    function select(item: DropdownMenuItem) {
        if (item.disabled) return;
        item.action();
        close();
    }

    $effect(() => {
        if (!open) return;
        function onOutside(e: MouseEvent) {
            if (!rootEl?.contains(e.target as Node)) close();
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
        <div class="absolute {align === "right" ? "right-0" : "left-0"} top-full z-[999] mt-1 w-56 bg-surface-50-950 border border-surface-200-800 rounded shadow-lg py-1">
            {#each items as item (item.label)}
                {#if item.divider}
                    <div class="my-1 border-t border-surface-200-800"></div>
                {/if}
                <button
                    type="button"
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
