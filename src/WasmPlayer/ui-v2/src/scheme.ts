// The player's light or dark choice (docs/ui-v2-contract.md, "Light and dark").
//
// qv-player's data-qv-scheme says which scheme v2.css draws: "dark" or "light". It's the player's
// choice from the toolbar's menu ("system" follows their device), unless the game has colours of
// its own for one scheme only (a --qv-bg token, one for both schemes), when it's whichever its
// background suits and the menu isn't offered. The choice is a reader's preference, so it's kept for every game: in the
// desktop app's settings (player-preload.ts's electronPlayerSettings, since its origin changes
// every launch), or else this browser's localStorage, which may be unavailable (then it lasts
// for this page only).

export type SchemeChoice = 'system' | 'light' | 'dark';

declare global {
    interface Window {
        electronPlayerSettings?: {
            getColourScheme(): string | null;
            setColourScheme(value: string): Promise<void>;
        };
    }
}

const STORAGE_KEY = 'questviva-colour-scheme';
const choices: readonly SchemeChoice[] = ['system', 'light', 'dark'];

let choice: SchemeChoice = readChoice();
const systemDark = typeof matchMedia === 'function' ? matchMedia('(prefers-color-scheme: dark)') : null;

function readChoice(): SchemeChoice {
    try {
        const stored = window.electronPlayerSettings
            ? window.electronPlayerSettings.getColourScheme()
            : localStorage.getItem(STORAGE_KEY);
        if (stored && (choices as readonly string[]).includes(stored)) return stored as SchemeChoice;
    } catch {
        // Storage blocked (a private window, or site data turned off), or the desktop app's
        // settings unreadable
    }
    return 'system';
}

function saveChoice(value: SchemeChoice): void {
    try {
        if (window.electronPlayerSettings) void window.electronPlayerSettings.setColourScheme(value).catch(() => {});
        else if (value === 'system') localStorage.removeItem(STORAGE_KEY);
        else localStorage.setItem(STORAGE_KEY, value);
    } catch {
        // As readChoice
    }
}

export function getSchemeChoice(): SchemeChoice {
    return choice;
}

/** Whether the game's colours suit both schemes, so the player can choose. */
function canChoose(player: HTMLElement): boolean {
    return !player.style.getPropertyValue('--qv-bg');
}

/**
 * Whether a game's own colours (for one scheme) are dark ones, judged by its background: then it's
 * drawn as the dark scheme, so the host's dialogs and the browser's own controls suit it.
 */
function hasDarkBackground(player: HTMLElement): boolean {
    const probe = document.createElement('span');
    probe.style.color = player.style.getPropertyValue('--qv-bg');
    probe.hidden = true;
    player.append(probe);
    const match = /rgba?\(([\d.]+),\s*([\d.]+),\s*([\d.]+)/.exec(getComputedStyle(probe).color);
    probe.remove();
    if (!match) return false;
    const [r, g, b] = match.slice(1, 4).map((c) => {
        const v = Number(c) / 255;
        return v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4;
    });
    return 0.2126 * r + 0.7152 * g + 0.0722 * b < 0.18;
}

/** Sets data-qv-scheme from the choice and the game's colours, and shows the menu where it applies. */
export function applyScheme(player: HTMLElement): void {
    const choosable = canChoose(player);
    const dark = choosable
        ? choice === 'dark' || (choice === 'system' && !!systemDark?.matches)
        : hasDarkBackground(player);
    player.setAttribute('data-qv-scheme', dark ? 'dark' : 'light');
    // Colours for one scheme are shown as they are, whichever that is (v2.css)
    player.toggleAttribute('data-qv-fixed-scheme', !choosable);
    // And the host's dialogs (Save/Load, the debugger: chrome.css), which aren't inside qv-player
    document.documentElement.setAttribute('data-qv-scheme', dark ? 'dark' : 'light');

    const wrapper = player.querySelector<HTMLElement>('.qv-scheme');
    if (!wrapper) return;
    if (wrapper.hidden === choosable) {
        wrapper.hidden = !choosable;
        if (!choosable) closeMenu(player, false);
    }
    for (const icon of wrapper.querySelectorAll<HTMLElement>('[data-qv-scheme-icon]')) {
        icon.hidden = icon.dataset.qvSchemeIcon !== choice;
    }
    for (const item of wrapper.querySelectorAll<HTMLElement>('[data-qv-scheme-choice]')) {
        item.setAttribute('aria-checked', String(item.dataset.qvSchemeChoice === choice));
    }
}

export function setSchemeChoice(player: HTMLElement, value: SchemeChoice): void {
    choice = value;
    saveChoice(value);
    applyScheme(player);
}

function menuOf(player: HTMLElement): HTMLElement | null {
    return player.querySelector<HTMLElement>('.qv-scheme-menu');
}

function items(menu: HTMLElement): HTMLElement[] {
    return [...menu.querySelectorAll<HTMLElement>('[data-qv-scheme-choice]')];
}

function openMenu(player: HTMLElement): void {
    const menu = menuOf(player);
    const button = player.querySelector<HTMLElement>('.qv-scheme-button');
    if (!menu || !button) return;
    menu.hidden = false;
    button.setAttribute('aria-expanded', 'true');
    const checked = items(menu).find((item) => item.getAttribute('aria-checked') === 'true');
    (checked ?? items(menu)[0])?.focus();
}

function closeMenu(player: HTMLElement, refocus: boolean): void {
    const menu = menuOf(player);
    const button = player.querySelector<HTMLElement>('.qv-scheme-button');
    if (!menu || menu.hidden) return;
    menu.hidden = true;
    button?.setAttribute('aria-expanded', 'false');
    if (refocus) button?.focus();
}

/** The qv-player that follows the device's own scheme while the choice is "system". */
let current: HTMLElement | null = null;
systemDark?.addEventListener('change', () => {
    if (current?.isConnected) applyScheme(current);
});

/** Wires the toolbar's menu. */
export function bindScheme(player: HTMLElement): void {
    // A restart swaps in a fresh qv-player
    current = player;
    applyScheme(player);

    const button = player.querySelector<HTMLElement>('.qv-scheme-button');
    const menu = menuOf(player);
    if (!button || !menu) return;

    button.addEventListener('click', () => {
        if (menu.hidden) openMenu(player);
        else closeMenu(player, true);
    });
    button.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
            e.preventDefault();
            openMenu(player);
        }
    });
    menu.addEventListener('click', (e) => {
        const item = e.target instanceof Element ? e.target.closest<HTMLElement>('[data-qv-scheme-choice]') : null;
        if (!item) return;
        setSchemeChoice(player, item.dataset.qvSchemeChoice as SchemeChoice);
        closeMenu(player, true);
    });
    menu.addEventListener('keydown', (e) => {
        const list = items(menu);
        const index = list.indexOf(document.activeElement as HTMLElement);
        let next = -1;
        if (e.key === 'ArrowDown') next = (index + 1) % list.length;
        else if (e.key === 'ArrowUp') next = (index - 1 + list.length) % list.length;
        else if (e.key === 'Home') next = 0;
        else if (e.key === 'End') next = list.length - 1;
        else if (e.key === 'Escape') {
            // Not the drawer's or the game's Escape as well
            e.preventDefault();
            e.stopPropagation();
            closeMenu(player, true);
            return;
        } else if (e.key === 'Tab') {
            closeMenu(player, false);
            return;
        }
        if (next >= 0) {
            e.preventDefault();
            list[next]?.focus();
        }
    });
    // A click or focus anywhere else closes it
    document.addEventListener('pointerdown', (e) => {
        if (!menu.hidden && e.target instanceof Node && !menu.contains(e.target) && !button.contains(e.target)) {
            closeMenu(player, false);
        }
    });
    menu.addEventListener('focusout', (e) => {
        if (e.relatedTarget instanceof Node && !menu.contains(e.relatedTarget) && e.relatedTarget !== button) {
            closeMenu(player, false);
        }
    });
}
