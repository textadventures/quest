const FOCUSABLE_SELECTOR =
    'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

function getFocusable(node: HTMLElement): HTMLElement[] {
    return Array.from(node.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR))
        .filter(el => el.offsetParent !== null);
}

// Traps Tab/Shift+Tab within `node` and restores focus to whatever was focused
// before the node appeared once it's gone - for a modal dialog's root element,
// so Tab can't leave the dialog to the page behind it, and closing it (by any
// path - Escape, a button, the backdrop) puts focus back where the user was.
export function trapFocus(node: HTMLElement) {
    const previouslyFocused = document.activeElement as HTMLElement | null;

    function onKeydown(event: KeyboardEvent) {
        if (event.key !== "Tab") return;
        const focusable = getFocusable(node);
        if (focusable.length === 0) return;
        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        const active = document.activeElement;

        if (event.shiftKey) {
            if (active === first || !node.contains(active)) {
                event.preventDefault();
                last.focus();
            }
        } else {
            if (active === last || !node.contains(active)) {
                event.preventDefault();
                first.focus();
            }
        }
    }

    node.addEventListener("keydown", onKeydown);

    return {
        destroy() {
            node.removeEventListener("keydown", onKeydown);
            previouslyFocused?.focus();
        },
    };
}
