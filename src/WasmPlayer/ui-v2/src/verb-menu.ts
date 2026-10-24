// <qv-verb-menu>: an object link's verbs ("Look at", "Take", …), the "menu" part. Choosing one
// sends "<verb> <object>", with metadata telling the parser exactly which object the link meant.
//
// It's a popover where the browser has the Popover API, so it sits in the top layer above
// everything else, and an ordinary fixed-position element otherwise. Either way, v2 opens and
// closes it itself (a "manual" popover): Escape and Tab close it, as does a click elsewhere.

import { sendCommand } from './engine';
import { isDisabled, supportsPopover } from './links';

// The object's name as commands use it: a pane item's element name, or an object link's text.
function menuName(link: HTMLElement): string {
    return (link.dataset.qvName ?? link.textContent ?? '').trim();
}

// Space left between the menu and its link, and between the menu and the viewport's edges.
const GAP = 4;
const EDGE = 8;

export class QvVerbMenu extends HTMLElement {
    #link: HTMLElement | null = null;

    connectedCallback(): void {
        this.setAttribute('role', 'menu');
        if (supportsPopover) this.popover = 'manual';
        this.addEventListener('click', this.#onClick);
        this.addEventListener('keydown', this.#onKeyDown);
    }

    get isOpen(): boolean {
        return this.#link !== null;
    }

    /** Opens the menu for this link, or closes it if it's already open for this link. */
    toggle(link: HTMLElement): void {
        if (this.#link === link) {
            this.close();
            return;
        }
        const verbs = (link.dataset.verbs ?? '').split('/').map((v) => v.trim()).filter(Boolean);
        // No verbs yet: the engine hasn't said what the object can do, so there's nothing to show.
        if (!verbs.length) return;
        this.close({ restoreFocus: false });

        const name = menuName(link);
        this.setAttribute('aria-label', (link.textContent ?? '').trim());
        this.replaceChildren(...verbs.map((verb) => {
            const item = document.createElement('button');
            item.type = 'button';
            item.setAttribute('role', 'menuitem');
            item.textContent = verb;
            item.dataset.command = `${verb.toLowerCase()} ${name}`;
            return item;
        }));

        this.#link = link;
        link.setAttribute('aria-expanded', 'true');
        this.classList.add('qv-open');
        if (supportsPopover) this.showPopover();
        this.#position();
        this.querySelector('button')?.focus({ preventScroll: true });

        document.addEventListener('pointerdown', this.#onPointerDownOutside, true);
        window.addEventListener('scroll', this.#onViewportChange, { passive: true });
        window.addEventListener('resize', this.#onViewportChange);
    }

    close({ restoreFocus = true } = {}): void {
        const link = this.#link;
        if (!link) return;
        this.#link = null;
        link.setAttribute('aria-expanded', 'false');
        const hadFocus = this.contains(document.activeElement);
        this.classList.remove('qv-open');
        if (supportsPopover && this.matches(':popover-open')) this.hidePopover();
        this.replaceChildren();
        document.removeEventListener('pointerdown', this.#onPointerDownOutside, true);
        window.removeEventListener('scroll', this.#onViewportChange);
        window.removeEventListener('resize', this.#onViewportChange);
        if (restoreFocus && hadFocus && link.isConnected) link.focus({ preventScroll: true });
    }

    /** Closes the menu if its link has just been disabled or removed. */
    closeIfStale(): void {
        if (this.#link && (!this.#link.isConnected || isDisabled(this.#link))) this.close({ restoreFocus: false });
    }

    #choose(item: HTMLElement): void {
        const link = this.#link;
        if (!link) return;
        const name = menuName(link);
        const metadata: Record<string, string> = {};
        if (link.dataset.elementid) metadata[name] = link.dataset.elementid;
        // Focus goes back to the command bar once the turn is over, not to the link.
        this.close({ restoreFocus: false });
        sendCommand(item.dataset.command ?? '', metadata);
    }

    #onClick = (e: MouseEvent): void => {
        const item = e.target instanceof Element ? e.target.closest<HTMLElement>('[role="menuitem"]') : null;
        if (item) this.#choose(item);
    };

    #onKeyDown = (e: KeyboardEvent): void => {
        const items = [...this.querySelectorAll<HTMLElement>('[role="menuitem"]')];
        const index = items.indexOf(document.activeElement as HTMLElement);
        let next: number | null = null;
        switch (e.key) {
            case 'Escape':
                e.preventDefault();
                this.close();
                return;
            case 'Tab':
                // Leaves the menu, as from any other menu; focus moves on from the link.
                this.close({ restoreFocus: false });
                return;
            case 'ArrowDown':
                next = (index + 1) % items.length;
                break;
            case 'ArrowUp':
                next = (index - 1 + items.length) % items.length;
                break;
            case 'Home':
                next = 0;
                break;
            case 'End':
                next = items.length - 1;
                break;
            default:
                return;
        }
        e.preventDefault();
        items[next]?.focus();
    };

    #onPointerDownOutside = (e: PointerEvent): void => {
        const target = e.target instanceof Node ? e.target : null;
        // A press on the menu's own link is left to the link's click, which toggles it.
        if (target && (this.contains(target) || this.#link?.contains(target))) return;
        this.close({ restoreFocus: false });
    };

    #onViewportChange = (): void => {
        this.closeIfStale();
        if (this.#link) this.#position();
    };

    // Below the link, or above it when there's no room below, and kept inside the viewport.
    #position(): void {
        if (!this.#link) return;
        const anchor = this.#link.getBoundingClientRect();
        const { offsetWidth: width, offsetHeight: height } = this;
        let top = anchor.bottom + GAP;
        if (top + height > window.innerHeight - EDGE && anchor.top - GAP - height >= EDGE) {
            top = anchor.top - GAP - height;
        }
        const left = Math.max(EDGE, Math.min(anchor.left, window.innerWidth - width - EDGE));
        this.style.top = `${Math.max(EDGE, top)}px`;
        this.style.left = `${left}px`;
    }
}
