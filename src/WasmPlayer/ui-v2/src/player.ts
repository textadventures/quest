// <qv-player>: v2's root element (the "UI root" the contract's tokens are set on), and lookups
// for the parts inside it. A fresh copy is swapped in for each game and restart (player.htm), so
// these are looked up when they're needed rather than kept.

import type { QvCommandBar } from './command-bar';
import type { QvOutput } from './output';

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
                break;
            case 'border':
                // The frame's border line, not the frame and what's in it.
                this.frame?.classList.toggle('qv-borderless', !visible);
                break;
        }
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
