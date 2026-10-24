// Links in the game's output, handled through listeners delegated from <qv-player>, so the
// markup carries no inline handlers (docs/ui-v2-contract.md, "Most link markup is already
// delegated"):
//
//   <a class="cmdlink elementmenu" data-elementid="lamp">        object: opens its verb menu
//   <a class="cmdlink exitlink" data-command="go north">          exit: sends the command
//   <a class="cmdlink commandlink" data-command="…"
//      data-deactivateonclick="true">                             command: sends it, maybe once
//   <a class="cmdlink" data-qv-event="…" data-qv-param="…">       sends an event (ShowMenu options)
//   <button data-qv-popup="id"> + <div id data-qv-popup-content> {popup:}
//
// A link is disabled with the "disabled" class (which saved transcripts carry, and which legacy
// uses too) plus aria-disabled. Link state that has to survive a save is kept in attributes:
// data-verbs, and data-qv-deactivated for a command link that only works once.

import { canSendCommand, sendCommand, sendEvent } from './engine';
import type { QvVerbMenu } from './verb-menu';

export const supportsPopover = typeof HTMLElement.prototype.showPopover === 'function';

/** Makes newly added links reachable and announced as what they do. */
export function decorateLinks(scope: Element): void {
    for (const link of scope.querySelectorAll<HTMLElement>('a.cmdlink, a[data-qv-event]')) {
        if (!link.hasAttribute('tabindex')) link.tabIndex = 0;
        link.setAttribute('role', 'button');
        if (link.classList.contains('elementmenu')) {
            link.setAttribute('aria-haspopup', 'menu');
            link.setAttribute('aria-expanded', 'false');
        }
        link.toggleAttribute('aria-disabled', link.classList.contains('disabled'));
    }
    for (const trigger of scope.querySelectorAll<HTMLElement>('[data-qv-popup]')) {
        trigger.setAttribute('aria-controls', trigger.dataset.qvPopup ?? '');
        trigger.setAttribute('aria-expanded', 'false');
    }
}

/** Clears open state that a saved transcript may have captured. */
export function resetRestoredLinks(scope: Element): void {
    for (const content of scope.querySelectorAll('[data-qv-popup-content].qv-popup-open')) {
        content.classList.remove('qv-popup-open');
    }
    for (const link of scope.querySelectorAll('[aria-expanded="true"]')) link.setAttribute('aria-expanded', 'false');
}

export function setLinkEnabled(link: Element, enabled: boolean): void {
    link.classList.toggle('disabled', !enabled);
    if (enabled) link.removeAttribute('aria-disabled');
    else link.setAttribute('aria-disabled', 'true');
}

export const isDisabled = (link: Element): boolean => link.classList.contains('disabled');

/** A command link that only works once, or that disableCommandLinks turned off for good. */
export function deactivate(link: Element): void {
    link.setAttribute('data-qv-deactivated', '');
    setLinkEnabled(link, false);
}

function asList(data: unknown): string[] {
    return Array.isArray(data) ? data.map(String) : [];
}

/**
 * QuestUI.updateLinks: after each turn, 610 Core passes what's still in scope, and links to
 * anything else are disabled. object: { name: "Look at/Take" }; exit and command: name lists.
 */
export function updateLinks(scope: Element, kind: string, data: unknown): void {
    if (kind === 'object') {
        const verbs = (data && typeof data === 'object' ? data : {}) as Record<string, unknown>;
        for (const link of scope.querySelectorAll<HTMLElement>('a.elementmenu')) {
            const linkVerbs = verbs[link.dataset.elementid ?? ''];
            if (linkVerbs) link.dataset.verbs = String(linkVerbs);
            setLinkEnabled(link, !!linkVerbs);
        }
    } else if (kind === 'exit') {
        const exits = asList(data);
        for (const link of scope.querySelectorAll<HTMLElement>('a.exitlink')) {
            setLinkEnabled(link, exits.includes(link.dataset.elementid ?? ''));
        }
    } else if (kind === 'command') {
        const commands = asList(data);
        for (const link of scope.querySelectorAll<HTMLElement>('a.commandlink:not([data-qv-deactivated])')) {
            // A command link that matched no single command (elementid "") always applies.
            const id = link.dataset.elementid ?? '';
            setLinkEnabled(link, id === '' || commands.includes(id));
        }
    }
}

/** The adapter's bindMenu: a pre-540 game's object link, given its verbs directly. */
export function bindMenu(linkId: string, verbs: string, elementId: string): void {
    const link = document.getElementById(linkId);
    if (!link) return;
    link.classList.add('cmdlink', 'elementmenu');
    link.dataset.verbs = verbs;
    if (!link.dataset.elementid) link.dataset.elementid = elementId;
    decorateLinks(link.parentElement ?? link);
}

// ── Delegated handlers ──────────────────────────────────────────────────────

export function handleClick(e: MouseEvent, menu: QvVerbMenu | null): void {
    const target = e.target instanceof Element ? e.target : null;
    if (!target) return;

    const trigger = target.closest<HTMLElement>('[data-qv-popup]');
    if (trigger) {
        e.preventDefault();
        togglePopup(trigger);
        return;
    }
    // A compass or exits strip button (compass.ts) sends its direction.
    const direction = target.closest<HTMLElement>('[data-qv-direction]');
    if (direction) {
        if (!(direction as HTMLButtonElement).disabled) sendCommand(direction.dataset.qvDirection ?? '');
        return;
    }

    // A pane's list item (panes.ts) opens the same verb menu as an object link.
    const paneItem = target.closest<HTMLElement>('.qv-pane-item');
    if (paneItem) {
        menu?.toggle(paneItem);
        return;
    }

    // As on legacy, a click anywhere in an open popup closes it.
    const content = target.closest<HTMLElement>('[data-qv-popup-content]');
    if (content) closePopup(content);

    const link = target.closest<HTMLAnchorElement>('a');
    if (!link) return;
    if (link.matches('.cmdlink, [data-qv-event]')) {
        activateLink(e, link, menu);
    } else if (link.hasAttribute('href')) {
        openExternalLink(e, link);
    }
}

function activateLink(e: MouseEvent, link: HTMLAnchorElement, menu: QvVerbMenu | null): void {
    if (link.dataset.qvEvent !== undefined) {
        e.preventDefault();
        if (isDisabled(link)) return;
        // A ShowMenu's options are only good for one answer: disable them straight away, so a
        // second click can't send another response before the engine has disabled the menu
        // itself (DisableMenu) at the end of the turn. An author's own event links stay live.
        if (link.dataset.qvEvent === 'ShowMenuResponse') {
            for (const option of link.ownerDocument.querySelectorAll('a[data-qv-event="ShowMenuResponse"]')) {
                setLinkEnabled(option, false);
            }
        }
        sendEvent(link.dataset.qvEvent, link.dataset.qvParam ?? '');
    } else if (link.classList.contains('elementmenu')) {
        e.preventDefault();
        if (!isDisabled(link)) menu?.toggle(link);
    } else if (link.classList.contains('exitlink')) {
        e.preventDefault();
        if (!isDisabled(link)) sendCommand(link.dataset.command ?? '');
    } else if (link.classList.contains('commandlink')) {
        e.preventDefault();
        if (isDisabled(link) || !canSendCommand()) return;
        if (link.dataset.deactivateonclick === 'true') deactivate(link);
        sendCommand(link.dataset.command ?? '');
    }
    // Any other .cmdlink is an author's, with its own handler.
}

// An author's ordinary link opens in a new tab rather than navigating away from the game, as on
// legacy. In-page anchors and javascript: links keep their usual behaviour.
function openExternalLink(e: MouseEvent, link: HTMLAnchorElement): void {
    const href = link.getAttribute('href') ?? '';
    if (href.startsWith('#') || /^\s*javascript:/i.test(href)) return;
    e.preventDefault();
    window.open(link.href, '_blank', 'noopener');
}

/** Enter and Space activate a link, as they would a button: a link with no href gets neither. */
export function handleKeyDown(e: KeyboardEvent): void {
    if (e.key !== 'Enter' && e.key !== ' ') return;
    const link = e.target instanceof HTMLAnchorElement ? e.target : null;
    if (!link?.matches('.cmdlink, [data-qv-event]')) return;
    e.preventDefault();
    link.click();
}

// ── {popup:} ────────────────────────────────────────────────────────────────

function popupFor(trigger: HTMLElement): HTMLElement | null {
    const content = document.getElementById(trigger.dataset.qvPopup ?? '');
    return content?.hasAttribute('data-qv-popup-content') ? content : null;
}

function togglePopup(trigger: HTMLElement): void {
    const content = popupFor(trigger);
    if (!content) return;
    if (content.classList.contains('qv-popup-open')) {
        closePopup(content);
        return;
    }
    // As a popover it's in the top layer, and closes on Escape or a click outside it.
    if (supportsPopover && !content.popover) {
        content.popover = 'auto';
        // beforetoggle rather than toggle, which comes a task later: until the open class goes,
        // the content stays on screen even though the popover has closed.
        content.addEventListener('beforetoggle', (e) => {
            if ((e as ToggleEvent).newState === 'closed') setPopupOpen(content, false);
        });
    }
    setPopupOpen(content, true);
    if (supportsPopover) content.showPopover();
}

function closePopup(content: HTMLElement): void {
    if (supportsPopover && content.matches(':popover-open')) content.hidePopover();
    setPopupOpen(content, false);
}

function setPopupOpen(content: HTMLElement, open: boolean): void {
    content.classList.toggle('qv-popup-open', open);
    for (const trigger of document.querySelectorAll(`[data-qv-popup="${CSS.escape(content.id)}"]`)) {
        trigger.setAttribute('aria-expanded', String(open));
    }
}
