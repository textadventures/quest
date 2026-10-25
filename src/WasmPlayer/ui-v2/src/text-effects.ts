// Text effects: QuestUI.textEffect, for TextFX_Typewriter and TextFX_Unscramble (#2627,
// decision 4).
//
// The text is plain text, as legacy's jQuery effects made it: markup in it is dropped. Its full
// text is in the output from the start, for screen readers and for copying, while an animated
// copy hidden from screen readers plays over it. The effect shows its text at once under reduced
// motion or a walkthrough, and a click or key press anywhere finishes every effect still running.
// A save taken while one is running restores with it finished.

import { player } from './player';
import { state } from './state';

interface Running {
    timer: number;
    finish(): void;
}

const running = new Set<Running>();

function num(value: unknown, fallback: number): number {
    const n = typeof value === 'number' ? value : parseFloat(String(value));
    return Number.isFinite(n) ? n : fallback;
}

function plainText(html: unknown): string {
    const template = document.createElement('template');
    template.innerHTML = String(html ?? '');
    return template.content.textContent ?? '';
}

// A random letter or digit, as legacy's unscramble used.
function randomAlphaNum(): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    return chars[Math.floor(Math.random() * chars.length)];
}

function shuffle<T>(items: T[]): void {
    for (let i = items.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [items[i], items[j]] = [items[j], items[i]];
    }
}

/** QuestUI.textEffect(kind, …): typewriter(text, speed, font, color, size), unscramble(text, speed, reveal, font, color, size). */
export function textEffect(kind: string, args: unknown[]): void {
    if (kind !== 'typewriter' && kind !== 'unscramble') return;
    const output = player()?.output;
    if (!output) return;
    const text = plainText(args[0]);
    const speed = Math.max(0, num(args[1], 100));
    const reveal = kind === 'unscramble' ? Math.max(1, Math.floor(num(args[2], 1))) : 0;
    const [font, color, size] = (kind === 'unscramble' ? args.slice(3, 6) : args.slice(2, 5)).map((v) => (v == null ? '' : String(v).trim()));

    const effect = document.createElement('span');
    effect.className = 'qv-text-effect';
    effect.dataset.qvEffect = kind;
    // Set as properties, so a value can't be anything but a value. An empty one is the game's own.
    if (font) effect.style.setProperty('font-family', font);
    if (color) effect.style.setProperty('color', color);
    if (size && num(size, 0) > 0) effect.style.setProperty('font-size', `${num(size, 0)}pt`);
    const full = document.createElement('span');
    full.className = 'qv-text-effect-full';
    full.textContent = text;
    const shown = document.createElement('span');
    shown.className = 'qv-text-effect-shown';
    shown.setAttribute('aria-hidden', 'true');
    effect.append(full, shown);
    output.addNodes([effect, document.createElement('br')]);

    const animate = state.animateScroll && !matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (!animate || text === '') {
        finishEffect(effect);
        return;
    }
    effect.classList.add('qv-text-effect-running');

    let step: () => boolean;
    if (kind === 'typewriter') {
        // One more character each tick, with a cursor every other tick, as legacy's.
        let progress = 0;
        step = () => {
            progress++;
            shown.textContent = text.substring(0, progress) + (progress % 2 === 1 && progress < text.length ? '_' : '');
            return progress >= text.length;
        };
    } else {
        // Every letter starts as a random one; each tick shows `reveal` more of the real ones.
        const letters = [...text];
        const hidden = letters.flatMap((c, i) => (/\s/.test(c) ? [] : [i]));
        shuffle(hidden);
        const current = letters.map((c) => (/\s/.test(c) ? c : randomAlphaNum()));
        shown.textContent = current.join('');
        step = () => {
            for (let r = 0; r < reveal && hidden.length; r++) {
                const i = hidden.pop()!;
                current[i] = letters[i];
            }
            for (const i of hidden) current[i] = randomAlphaNum();
            shown.textContent = current.join('');
            return hidden.length === 0;
        };
    }

    const effectRun: Running = {
        timer: 0,
        finish: () => {
            clearInterval(effectRun.timer);
            running.delete(effectRun);
            finishEffect(effect);
        },
    };
    effectRun.timer = window.setInterval(() => {
        if (!effect.isConnected || step()) effectRun.finish();
    }, speed);
    running.add(effectRun);
}

function finishEffect(effect: Element): void {
    const shown = effect.querySelector('.qv-text-effect-shown');
    if (shown) shown.textContent = effect.querySelector('.qv-text-effect-full')?.textContent ?? '';
    effect.classList.remove('qv-text-effect-running');
}

/** Finishes every effect still running (a click or key press, or a new game). */
export function finishTextEffects(): void {
    for (const effect of [...running]) effect.finish();
}

/** Effects in restored output were saved part-way: they're shown finished. */
export function finishRestoredEffects(scope: ParentNode): void {
    for (const effect of scope.querySelectorAll('.qv-text-effect')) finishEffect(effect);
}

document.addEventListener('keydown', () => finishTextEffects(), true);
document.addEventListener('pointerdown', () => finishTextEffects(), true);
