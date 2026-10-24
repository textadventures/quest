// Status attributes: the status pane, and the pinned status bar (the "status-bar" part) that keeps
// the attributes a game pins in view under the top bar on a narrow layout, where the pane is in
// the drawer.
//
// 610 Core sends structured items (QuestUI.setStatusAttributes): each has the formatted text, and
// may have a numeric value, a max (drawn as a bar) and pinned. Older games send pre-formatted
// lines instead (the adapter's updateStatus), which show as text.

import type { StatusAttribute } from './contract';
import { pane } from './panes';
import { player } from './player';

let items: StatusAttribute[] = [];

export function setStatusAttributes(list: StatusAttribute[]): void {
    items = Array.isArray(list) ? list.filter((item) => item && typeof item === 'object') : [];
    renderStatus();
}

/** The adapter's updateStatus: lines of text, from a game's request (SetStatus, …). */
export function updateStatus(text: string): void {
    setStatusAttributes(String(text ?? '').split(/\r?\n|<br\s*\/?>/i)
        .filter((line) => line.trim())
        .map((line) => ({ text: line })));
}

export function resetStatus(): void {
    items = [];
}

// A bar only for a number with a positive number to measure it against.
function measure(item: StatusAttribute): { value: number; max: number } | null {
    const value = Number(item.value);
    const max = Number(item.max);
    if (item.value == null || item.max == null || !Number.isFinite(value) || !Number.isFinite(max) || max <= 0) return null;
    return { value: Math.min(Math.max(value, 0), max), max };
}

let idCount = 0;

function itemElement(item: StatusAttribute, tag: 'li' | 'span'): HTMLElement {
    const element = document.createElement(tag);
    element.className = 'qv-status-item';
    if (item.name) element.dataset.qvStatus = String(item.name);
    const text = document.createElement('span');
    text.className = 'qv-status-text';
    text.id = `qv-status-${++idCount}`;
    // Formatted text, which a game's format string may have put markup in, as on legacy.
    text.innerHTML = String(item.text ?? '');
    element.append(text);
    const amount = measure(item);
    if (amount) {
        const bar = document.createElement('span');
        bar.className = 'qv-status-meter';
        bar.setAttribute('role', 'meter');
        bar.setAttribute('aria-valuemin', '0');
        bar.setAttribute('aria-valuemax', String(amount.max));
        bar.setAttribute('aria-valuenow', String(amount.value));
        bar.setAttribute('aria-labelledby', text.id);
        const fill = document.createElement('span');
        fill.className = 'qv-status-meter-fill';
        fill.style.width = `${(amount.value / amount.max) * 100}%`;
        bar.append(fill);
        element.append(bar);
    }
    return element;
}

export function renderStatus(): void {
    const statusPane = pane('status');
    if (statusPane) {
        const list = document.createElement('ul');
        list.className = 'qv-status-list';
        list.append(...items.map((item) => itemElement(item, 'li')));
        statusPane.body.replaceChildren(list);
        // With nothing to show the pane goes, as legacy's does; the game's setVisible still wins.
        statusPane.toggleAttribute('data-qv-empty', items.length === 0);
    }
    updateStatusBar();
}

/**
 * The pinned status bar: the pinned items only, shown on a narrow layout (v2.css) while the
 * status pane would be showing (the game hasn't hidden it or the panes).
 */
export function updateStatusBar(): void {
    const root = player();
    const bar = root?.statusBar;
    if (!root || !bar) return;
    const statusPane = pane('status');
    const shown = !!statusPane && !statusPane.hidden && !root.panes?.hidden;
    const pinned = shown ? items.filter((item) => item.pinned) : [];
    bar.replaceChildren(...pinned.map((item) => itemElement(item, 'span')));
    bar.classList.toggle('qv-status-bar-active', pinned.length > 0);
    root.updateTopbar();
}
