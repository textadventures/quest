// <qv-player>: v2's root element (the "UI root" the contract's tokens are set on), and lookups
// for the parts inside it. A fresh copy is swapped in for each game and restart (player.htm), so
// these are looked up when they're needed rather than kept.

import type { QvCommandBar } from './command-bar';
import { updateExitStrip } from './compass';
import { updateStatusBar } from './status';
import { handleClick, handleKeyDown, updateLinks } from './links';
import { setMapVisible, toggleMapCollapsed, updateMap } from './map';
import type { QvOutput } from './output';
import { builtInPanes, pane, setDrawerOpen, isDrawerOpen } from './panes';
import type { QvVerbMenu } from './verb-menu';

/** The layout presets a game can choose for each width (QuestUI.setLayoutPreset). */
export const layoutPresets = ['sidepanes', 'nopanes', 'bottombar'];

/**
 * The layout in effect, from the preset for the player's current width:
 *   side    the panes in a column beside the game (sidepanes, wide)
 *   drawer  the panes in a drawer from the top bar's button (sidepanes, narrow)
 *   sheet   the panes in a sheet from a button in the bottom bar (bottombar)
 *   none    no panes (nopanes)
 */
export type Layout = 'side' | 'drawer' | 'sheet' | 'none';

// The width, of the player itself rather than the window, from which it counts as wide.
const WIDE_MIN_WIDTH = 900;

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
    #widePreset = 'sidepanes';
    #narrowPreset = 'sidepanes';

    connectedCallback(): void {
        if (this.#listening) return;
        this.#listening = true;
        // Every link and popup trigger in the UI is handled here, not by handlers of its own.
        this.addEventListener('click', (e) => handleClick(e, this.verbMenu));
        this.addEventListener('keydown', handleKeyDown);
        for (const button of this.querySelectorAll('.qv-panes-button')) {
            button.addEventListener('click', () => setDrawerOpen(!isDrawerOpen()));
        }
        this.mapButton?.addEventListener('click', toggleMapCollapsed);
        new ResizeObserver(() => this.#updateLayout()).observe(this);
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
        this.#updateLayout();
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

    /** The exits strip under the transcript, in the drawer and sheet layouts (compass.ts). */
    get exits(): HTMLElement | null {
        return this.querySelector('[data-qv-part="exits"]');
    }

    /** The pinned status bar in the top bar, in the drawer and sheet layouts (status.ts). */
    get statusBar(): HTMLElement | null {
        return this.querySelector('[data-qv-part="status-bar"]');
    }

    /**
     * Opens the panes drawer or sheet: the top bar's for the drawer, the bottom bar's for the
     * sheet. Shown only while there are panes.
     */
    get panesButton(): HTMLButtonElement | null {
        return this.querySelector(this.layout === 'sheet' ? '.qv-footer .qv-panes-button' : '.qv-topbar .qv-panes-button');
    }

    get layout(): Layout {
        return (this.dataset.qvLayout as Layout | undefined) ?? 'side';
    }

    /**
     * QuestUI.setLayoutPreset with sidepanes, nopanes or bottombar: this preset for every width,
     * or for wide widths with another for narrow ones. An unknown name changes nothing.
     *
     * gamebook (GamebookCore's, which ignores narrow) is a page in a reading column at every
     * width: no panes, location, command bar, exits strip or pinned status bar, and the page's
     * options drawn as choices (v2.css). The command bar is hidden with setVisible, so a
     * gamebook's GetInput can still show it.
     */
    setLayoutPreset(name: string, narrow?: string): void {
        if (name === 'gamebook') {
            this.toggleAttribute('data-qv-gamebook', true);
            this.#widePreset = this.#narrowPreset = 'nopanes';
            this.setVisible('command-bar', false);
            this.setVisible('location', false);
            this.#updateLayout();
            return;
        }
        if (!layoutPresets.includes(name)) return;
        this.removeAttribute('data-qv-gamebook');
        this.#widePreset = name;
        this.#narrowPreset = narrow != null && layoutPresets.includes(narrow) ? narrow : name;
        this.#updateLayout();
    }

    #updateLayout(): void {
        const narrow = this.clientWidth < WIDE_MIN_WIDTH;
        if (narrow !== this.hasAttribute('data-qv-narrow')) {
            this.toggleAttribute('data-qv-narrow', narrow);
            updateMap();
        }
        const preset = narrow ? this.#narrowPreset : this.#widePreset;
        const layout: Layout = preset === 'nopanes' ? 'none'
            : preset === 'bottombar' ? 'sheet'
            : narrow ? 'drawer' : 'side';
        if (this.dataset.qvLayout === layout) return;
        // Whatever was open belongs to the old layout.
        setDrawerOpen(false, { restoreFocus: false });
        // Moving into a new layout isn't the drawer opening or closing, so it isn't animated.
        this.toggleAttribute('data-qv-layout-changing', true);
        this.dataset.qvLayout = layout;
        requestAnimationFrame(() => requestAnimationFrame(() => this.removeAttribute('data-qv-layout-changing')));
        // The exits strip and pinned status bar show only where the panes aren't beside the game.
        updateExitStrip();
        updateStatusBar();
        this.updateTopbar();
    }

    /** The bar holding the location and the toolbar goes when it would be empty. */
    updateTopbar(): void {
        const topbar = this.topbar;
        if (!topbar) return;
        const location = this.location;
        // The panes button counts only where it's drawn, in the drawer layout.
        const anyButton = [...topbar.querySelectorAll<HTMLElement>('.qv-toolbar button')]
            .some((button) => !button.hidden && button.style.display !== 'none' && getComputedStyle(button).display !== 'none');
        const statusBar = this.statusBar;
        const anyStatus = !!statusBar && !statusBar.hidden && statusBar.classList.contains('qv-status-bar-active')
            && getComputedStyle(statusBar).display !== 'none';
        topbar.hidden = (!location || location.hidden) && !anyButton && !anyStatus;
    }

    /**
     * After a gamebook page has turned, the link that turned it is disabled and on its way out, so
     * keyboard focus would be lost: it moves to the new page's first choice instead (without
     * scrolling to it).
     */
    focusFirstChoice(): void {
        if (!this.hasAttribute('data-qv-gamebook')) return;
        // Focus somewhere that's staying (not the chosen option, now disabled and fading out) is
        // left alone.
        const active = document.activeElement;
        if (active && active !== document.body && this.contains(active)
            && !active.closest('.qv-hiding') && !active.classList.contains('disabled')) return;
        const choice = [...this.querySelectorAll<HTMLElement>('[data-qv-part="output"] [data-qv-sections] a.commandlink:not(.disabled)')]
            .find((a) => !a.closest('.qv-hiding'));
        choice?.focus({ preventScroll: true });
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

    get pictureFrame(): HTMLElement | null {
        return this.querySelector('[data-qv-part="picture-frame"]');
    }

    /** The map (map.ts), under the picture frame. */
    get map(): HTMLElement | null {
        return this.querySelector('[data-qv-part="map"]');
    }

    /** Collapses the map on a narrow layout. */
    get mapButton(): HTMLButtonElement | null {
        return this.querySelector('.qv-map-button');
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
            case 'picture-frame':
                this.#setHidden(this.pictureFrame, !visible);
                break;
            case 'exits':
                this.#setHidden(this.exits, !visible);
                break;
            case 'map':
                setMapVisible(visible);
                break;
            case 'status-bar':
                this.#setHidden(this.statusBar, !visible);
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
        // The exits strip follows the compass, and the pinned status bar the status pane: hiding
        // either pane, or all of them, hides those too.
        updateExitStrip();
        updateStatusBar();
    }

    // A panes button only makes sense while the region and at least one pane in it show.
    #updatePanesButton(): void {
        const region = this.panes;
        if (!region) return;
        const anyPane = [...region.querySelectorAll<HTMLElement>('qv-pane')]
            .some((p) => !p.hidden && !p.hasAttribute('data-qv-empty'));
        for (const button of this.querySelectorAll<HTMLButtonElement>('.qv-panes-button')) {
            button.hidden = region.hidden || !anyPane;
        }
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
