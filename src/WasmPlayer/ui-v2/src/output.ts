// <qv-output>: the transcript, a live region the game's text is appended to.
//
// Text goes into blocks: a block is a <div> with one alignment, opened by setAlignment
// (createNewDiv) and by output sections. Each block records the sections that were open when it
// was created, so hideSection and disableSection can find them. Output HTML is inserted as
// markup, without running any <script> in it: a 610 game runs its JavaScript through JS.eval
// or its own script files.

import { decorateLinks, deactivate, resetRestoredLinks, setLinkEnabled } from './links';
import { player } from './player';
import { emit, state } from './state';

const knownImageSizes = new Map<string, { width: number; height: number }>();

export function rememberImageSize(url: string, width: number, height: number): void {
    knownImageSizes.set(url, { width, height });
}

// The gap left above the start of a turn's output when it's scrolled into view.
const TURN_START_GAP = 8;

export class QvOutput extends HTMLElement {
    #current: HTMLDivElement | null = null;
    #sections: string[] = [];
    // Where the current turn's output starts, in document coordinates.
    #turnStart = 0;

    connectedCallback(): void {
        this.setAttribute('role', 'log');
        this.setAttribute('aria-live', 'polite');
        // An image that's still downloading renders at no size, so the scroll position worked
        // out when it was added falls short once it arrives. load doesn't bubble, hence capture.
        this.addEventListener('load', this.#onImageSettled, true);
        this.addEventListener('error', this.#onImageSettled, true);
    }

    disconnectedCallback(): void {
        this.removeEventListener('load', this.#onImageSettled, true);
        this.removeEventListener('error', this.#onImageSettled, true);
    }

    #onImageSettled = (e: Event): void => {
        if (e.target instanceof HTMLImageElement) this.scrollToEnd();
    };

    addText(html: string): void {
        const block = this.#current ?? this.setAlignment('left');
        block.insertAdjacentHTML('beforeend', html);
        applyKnownImageSizes(block);
        decorateLinks(block);
        emit('outputAdded', { html });
    }

    /** Starts a new block with this alignment (left, center, right or justify). */
    setAlignment(alignment: string): HTMLDivElement {
        // Sections open and close a block each time, often with nothing written in between.
        if (this.#current && !this.#current.hasChildNodes()) this.#current.remove();
        const block = document.createElement('div');
        block.className = 'qv-block';
        block.style.textAlign = alignment;
        if (this.#sections.length) block.dataset.qvSections = this.#sections.join(' ');
        this.append(block);
        this.#current = block;
        return block;
    }

    clearScreen(): void {
        this.replaceChildren();
        this.#current = null;
        this.setAlignment('left');
        this.#turnStart = this.#top();
        if (window.scrollY > this.#turnStart) window.scrollTo({ top: this.#turnStart, behavior: 'instant' });
    }

    /** Restores a saved transcript (getHtml's output). */
    loadHtml(html: string): void {
        this.innerHTML = html;
        decorateLinks(this);
        resetRestoredLinks(this);
        // Show where the restored game left off, once it's laid out.
        requestAnimationFrame(() => {
            const root = document.scrollingElement ?? document.documentElement;
            window.scrollTo({ top: root.scrollHeight, behavior: 'instant' });
        });
        // Carry on in a new block: the restored ones may be inside sections long since closed.
        this.#current = null;
        this.#sections = [];
    }

    getHtml(): string {
        return this.innerHTML;
    }

    reset(): void {
        this.replaceChildren();
        this.#current = null;
        this.#sections = [];
        this.#turnStart = 0;
    }

    // ── Output sections ─────────────────────────────────────────────────────

    startSection(name: string): void {
        if (this.#sections.includes(name)) return;
        this.#sections.push(name);
        this.setAlignment('left');
    }

    endSection(name: string): void {
        const index = this.#sections.indexOf(name);
        if (index === -1) return;
        this.#sections.splice(index, 1);
        this.setAlignment('left');
    }

    hideSection(name: string): void {
        this.endSection(name);
        for (const block of this.#sectionBlocks(name)) {
            block.classList.add('qv-hiding');
            disableLinks(block);
            // Long enough for the fade in v2.css, which reduced motion turns off.
            setTimeout(() => block.remove(), 250);
        }
    }

    disableSection(name: string): void {
        for (const block of this.#sectionBlocks(name)) disableLinks(block);
    }

    disableCommandLinks(): void {
        for (const link of this.querySelectorAll('a.commandlink')) deactivate(link);
        player()?.verbMenu?.closeIfStale();
    }

    #sectionBlocks(name: string): HTMLElement[] {
        return [...this.querySelectorAll<HTMLElement>('[data-qv-sections]')]
            .filter((block) => block.dataset.qvSections!.split(' ').includes(name));
    }

    // ── Turns and scrolling ─────────────────────────────────────────────────

    /** Fades what's already there, when the game asked for that (fadePreviousOutput). */
    beginTurn(): void {
        if (!state.fadePreviousOutput) return;
        for (const child of this.children) child.classList.add('qv-previous-output');
    }

    clearPreviousOutputFade(): void {
        for (const child of this.querySelectorAll('.qv-previous-output')) child.classList.remove('qv-previous-output');
    }

    /** Records where the coming turn's output will start, for scrollToEnd. */
    markScrollPosition(): void {
        this.#turnStart = this.#top() + this.offsetHeight;
    }

    /**
     * Brings the current turn's output into view: as much of it as fits, starting from the top
     * of the turn, so a long turn is read from its beginning. Only ever scrolls forwards.
     */
    scrollToEnd(): void {
        if (!state.autoScroll) return;
        const root = document.scrollingElement ?? document.documentElement;
        const bottom = root.scrollHeight - window.innerHeight;
        const top = state.animateScroll
            ? Math.min(this.#turnStart - stickyHeaderHeight() - TURN_START_GAP, bottom)
            : bottom;
        if (top <= window.scrollY) return;
        const smooth = state.animateScroll && !matchMedia('(prefers-reduced-motion: reduce)').matches;
        window.scrollTo({ top, behavior: smooth ? 'smooth' : 'instant' });
    }

    #top(): number {
        return this.getBoundingClientRect().top + window.scrollY;
    }
}

// The top bar (location, Save) stays at the top of the viewport, covering whatever is under it.
function stickyHeaderHeight(): number {
    const topbar = document.querySelector<HTMLElement>('qv-player .qv-topbar');
    return topbar && !topbar.hidden ? topbar.offsetHeight : 0;
}

function disableLinks(scope: Element): void {
    for (const link of scope.querySelectorAll('a.cmdlink, a[data-qv-event]')) setLinkEnabled(link, false);
    player()?.verbMenu?.closeIfStale();
}

// Gives an image the size wasm-player.js found when it preloaded it, so it takes up its real
// space straight away instead of pushing the text below it down when it arrives. Only images with
// no sizing of their own: an author's width, height or style keeps meaning what it did.
function applyKnownImageSizes(scope: Element): void {
    for (const img of scope.querySelectorAll('img:not([width]):not([height]):not([style])')) {
        const size = knownImageSizes.get(img.getAttribute('src') ?? '');
        if (!size) continue;
        img.setAttribute('width', String(size.width));
        img.setAttribute('height', String(size.height));
        img.classList.add('qv-sized');
    }
}
