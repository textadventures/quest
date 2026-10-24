// <qv-player>: v2's root element (the "UI root" the contract's tokens are set on), and lookups
// for the parts inside it. A fresh copy is swapped in for each game and restart (player.htm), so
// these are looked up when they're needed rather than kept.

import type { QvCommandBar } from './command-bar';
import { handleClick, handleKeyDown, updateLinks } from './links';
import type { QvOutput } from './output';
import { builtInPanes, pane, setDrawerOpen, isDrawerOpen } from './panes';
import type { QvVerbMenu } from './verb-menu';

// Tokens that are lengths, where the contract reads a bare number as pixels.
const lengthTokens = new Set([
    '--qv-font-size',
    '--qv-output-width',
    '--qv-padding-top',
    '--qv-padding-bottom',
    '--qv-padding-left',
    '--qv-padding-right',
    '--qv-menu-font-size',
    '--qv-radius',
    '--qv-space',
]);

export class QvPlayer extends HTMLElement {
    #listening = false;

    connectedCallback(): void {
        if (this.#listening) return;
        this.#listening = true;
        // Every link and popup trigger in the UI is handled here, not by handlers of its own.
        this.addEventListener('click', (e) => handleClick(e, this.verbMenu));
        this.addEventListener('keydown', handleKeyDown);
        this.panesButton?.addEventListener('click', () => setDrawerOpen(!isDrawerOpen()));
        this.panes?.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && isDrawerOpen() && !this.verbMenu?.isOpen) {
                e.preventDefault();
                setDrawerOpen(false);
            }
        });
        // The host shows and hides the toolbar's buttons itself, and adds Restart in previews.
        const toolbar = this.querySelector('.qv-toolbar');
        if (toolbar) {
            new MutationObserver(() => this.updateTopbar()).observe(toolbar, {
                subtree: true, childList: true, attributes: true, attributeFilter: ['style', 'hidden'],
            });
        }
        this.#updatePanesButton();
    }

    get topbar(): HTMLElement | null {
        return this.querySelector('.qv-topbar');
    }

    get saveButton(): HTMLButtonElement | null {
        return this.querySelector('#cmdSave');
    }

    /** The "panes" region: a column beside the game when it's wide, a drawer when it's narrow. */
    get panes(): HTMLElement | null {
        return this.querySelector('[data-qv-part="panes"]');
    }

    /** Opens the panes drawer. Shown only on a narrow layout, and only while there are panes. */
    get panesButton(): HTMLButtonElement | null {
        return this.querySelector('.qv-panes-button');
    }

    /** The bar holding the location and the toolbar goes when it would be empty. */
    updateTopbar(): void {
        const topbar = this.topbar;
        if (!topbar) return;
        const location = this.location;
        // The panes button counts only where it's drawn, on a narrow layout.
        const anyButton = [...topbar.querySelectorAll<HTMLElement>('.qv-toolbar button')]
            .some((button) => !button.hidden && button.style.display !== 'none' && getComputedStyle(button).display !== 'none');
        topbar.hidden = (!location || location.hidden) && !anyButton;
    }

    get verbMenu(): QvVerbMenu | null {
        return this.querySelector('qv-verb-menu');
    }

    /** QuestUI.updateLinks, and the engine's updateObjectLinks. */
    updateLinks(kind: string, data: unknown): void {
        updateLinks(this, kind, data);
        this.verbMenu?.closeIfStale();
    }

    get output(): QvOutput | null {
        return this.querySelector('qv-output');
    }

    get commandBar(): QvCommandBar | null {
        return this.querySelector('qv-command-bar');
    }

    get location(): HTMLElement | null {
        return this.querySelector('[data-qv-part="location"]');
    }

    get frame(): HTMLElement | null {
        return this.querySelector('[data-qv-part="border"]');
    }

    /** Shows or hides one of the parts v2 has so far. Any other part name is ignored. */
    setVisible(part: string, visible: boolean): void {
        switch (part) {
            case 'output':
                this.#setHidden(this.output, !visible);
                break;
            case 'command-bar':
                this.#setHidden(this.commandBar, !visible);
                break;
            case 'location':
                this.#setHidden(this.location, !visible);
                this.updateTopbar();
                break;
            case 'border':
                // The frame's border line, not the frame and what's in it.
                this.frame?.classList.toggle('qv-borderless', !visible);
                break;
            case 'panes':
                this.#setHidden(this.panes, !visible);
                if (!visible) setDrawerOpen(false, { restoreFocus: false });
                this.#updatePanesButton();
                break;
            default:
                // A pane by name: built-in, or one an author added.
                if (builtInPanes.includes(part) || pane(part)) {
                    this.#setHidden(pane(part), !visible);
                    this.#updatePanesButton();
                }
        }
    }

    // The drawer button only makes sense while the region and at least one pane in it show.
    #updatePanesButton(): void {
        const button = this.panesButton;
        const region = this.panes;
        if (!button || !region) return;
        const anyPane = [...region.querySelectorAll<HTMLElement>('qv-pane')].some((p) => !p.hidden);
        button.hidden = region.hidden || !anyPane;
        this.updateTopbar();
    }

    #setHidden(element: HTMLElement | null, hidden: boolean): void {
        if (element) element.hidden = hidden;
    }

    /**
     * Sets custom properties on this element: names starting "--" only, with style.setProperty
     * so a value can't inject anything but a value. An empty value unsets the token.
     */
    setTokens(tokens: Record<string, unknown>): void {
        for (const [name, raw] of Object.entries(tokens)) {
            if (!name.startsWith('--')) continue;
            let value = raw == null ? '' : String(raw).trim();
            if (value === '') {
                this.style.removeProperty(name);
                continue;
            }
            if (lengthTokens.has(name) && /^-?(\d+\.?\d*|\.\d+)$/.test(value)) value += 'px';
            this.style.setProperty(name, value);
        }
    }

    setLocation(html: string): void {
        const location = this.location;
        if (!location) return;
        location.innerHTML = html;
        location.classList.toggle('qv-empty', html.trim() === '');
    }
}

export function player(): QvPlayer | null {
    return document.querySelector<QvPlayer>('qv-player');
}
