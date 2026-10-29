// The panes beside the transcript: <qv-pane> elements in the "panes" region (an <aside>). The
// layout puts the region in a column beside the game, a drawer opened from the top bar's panes
// button, a sheet opened from the bottom bar's, or nowhere (player.ts, v2.css).
//
// Built-in panes are in player.htm; authors add their own with QuestUI.addPane. Each carries
// data-qv-part="pane" and data-qv-pane (the contract's part names), and is a <details>, so it
// can be collapsed.
//
// A list pane's items (inventory, places and objects) are buttons that open the same verb menu as
// an object link in the transcript (#2613, decision 1).

import { decorateLinks } from './links';
import { player } from './player';
import { state } from './state';

/** The panes named in the contract, plus any an author adds. */
export const builtInPanes = ['inventory', 'objects', 'status', 'compass', 'commands', 'customstatus'];

// Interface strings that label each pane (setInterfaceString).
const paneLabels: Record<string, string> = {
    InventoryLabel: 'inventory',
    PlacesObjectsLabel: 'objects',
    StatusLabel: 'status',
    CompassLabel: 'compass',
};

export class QvPane extends HTMLElement {
    #details!: HTMLDetailsElement;
    #summary!: HTMLElement;
    #body!: HTMLElement;

    connectedCallback(): void {
        if (this.#details) return;
        this.dataset.qvPart = 'pane';
        this.#details = document.createElement('details');
        this.#details.open = true;
        this.#summary = document.createElement('summary');
        this.#summary.className = 'qv-pane-heading';
        this.#summary.textContent = this.getAttribute('label') ?? '';
        this.#body = document.createElement('div');
        this.#body.className = 'qv-pane-body';
        // Anything already inside (an author's pane, built with its content) goes in the body.
        this.#body.append(...this.childNodes);
        this.#details.append(this.#summary, this.#body);
        this.append(this.#details);
    }

    get name(): string {
        return this.dataset.qvPane ?? '';
    }

    get body(): HTMLElement {
        return this.#body;
    }

    setLabel(html: string): void {
        this.#summary.innerHTML = html;
    }

    setContent(html: string): void {
        this.#body.innerHTML = html;
        decorateLinks(this.#body);
    }
}

/** The pane with this name, built-in or an author's. */
export function pane(name: string): QvPane | null {
    return player()?.querySelector<QvPane>(`qv-pane[data-qv-pane="${CSS.escape(name)}"]`) ?? null;
}

// ── Lists: inventory and places/objects ─────────────────────────────────────

interface ListItem {
    Text: string;
    Verbs: string[];
    ElementId: string | null;
    ElementName: string;
}

// The engine's list names (ListHandler) for each pane.
const listPanes: Record<string, string> = { inventory: 'inventory', placesobjects: 'objects' };

/** The adapter's updateList: the whole list, in order, each item as JSON. */
export function updateList(listName: string, items: Record<string, string>): void {
    const target = pane(listPanes[listName] ?? '');
    if (!target) return;
    const directions = new Set(state.compassDirections);
    const list = document.createElement('ul');
    list.className = 'qv-pane-list';
    for (const json of Object.values(items)) {
        const item = JSON.parse(json) as ListItem;
        // Exits are listed among places and objects, but the compass already has them.
        if (listName === 'placesobjects' && directions.has(item.Text)) continue;
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'qv-pane-item';
        button.innerHTML = item.Text;
        button.setAttribute('aria-haspopup', 'menu');
        button.setAttribute('aria-expanded', 'false');
        button.dataset.verbs = (item.Verbs ?? []).join('/');
        // The verb menu sends "<verb> <name>", with the name mapped to the element in metadata,
        // as legacy's verb buttons do. The display text can differ from the name.
        button.dataset.qvName = item.ElementName;
        if (item.ElementId) button.dataset.elementid = item.ElementId;
        const li = document.createElement('li');
        li.append(button);
        list.append(li);
    }
    if (!list.childElementCount) {
        const empty = document.createElement('p');
        empty.className = 'qv-pane-empty';
        empty.innerHTML = state.interfaceStrings.get('EmptyListLabel') ?? '';
        target.body.replaceChildren(empty);
    } else {
        target.body.replaceChildren(list);
    }
    player()?.verbMenu?.closeIfStale();
}

// ── Commands and custom status ──────────────────────────────────────────────

/**
 * QuestUI.setCommands: the command pane's links. Each entry is "Label" or "Label:command", as
 * legacy's setCommands took them (as one ";"-separated string, which still works).
 */
export function setCommands(list: string[] | string): void {
    const target = pane('commands');
    if (!target) return;
    const entries = (Array.isArray(list) ? list.map(String) : String(list ?? '').split(';')).filter((e) => e.trim());
    const html = entries.map((entry) => {
        const [label, command] = entry.split(':');
        const escaped = (s: string) => s.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;');
        return `<a class="cmdlink commandlink" data-elementid="" data-command="${escaped((command ?? label).trim().toLowerCase())}">${escaped(label.trim())}</a>`;
    }).join(' ');
    target.setContent(html);
}

/** QuestUI.setPaneContent: an author pane's HTML, or the custom status pane's. */
export function setPaneContent(id: string, html: string): void {
    if (id === 'map' || (id !== 'customstatus' && builtInPanes.includes(id))) return;
    pane(id)?.setContent(html);
}

/**
 * QuestUI.addPane: an author's pane, after the built-in ones. An existing name is ignored, as is
 * "map", the name of the map's own pane (setMapPosition).
 */
export function addPane(id: string, title?: string, html?: string): void {
    const region = player()?.panes;
    if (!id || id === 'map' || !region || pane(id)) return;
    const element = document.createElement('qv-pane') as QvPane;
    element.dataset.qvPane = id;
    element.setAttribute('label', title ?? id);
    region.append(element);
    element.setContent(html ?? '');
}

/** QuestUI.setPaneOrder: these panes first, in this order; the rest keep their places after. */
export function setPaneOrder(parts: string[] | string): void {
    const region = player()?.panes;
    if (!region) return;
    const names = Array.isArray(parts) ? parts.map(String) : String(parts ?? '').split(';');
    for (const name of [...names].reverse()) {
        const element = pane(name);
        if (element) region.prepend(element);
    }
}

export function setPaneLabel(interfaceString: string, html: string): boolean {
    const name = paneLabels[interfaceString];
    if (!name) return false;
    pane(name)?.setLabel(html);
    return true;
}

// ── The drawer or sheet ─────────────────────────────────────────────────────

export function isDrawerOpen(): boolean {
    return !!player()?.panes?.classList.contains('qv-drawer-open');
}

export function setDrawerOpen(open: boolean, { restoreFocus = true } = {}): void {
    const root = player();
    const region = root?.panes;
    const button = root?.panesButton;
    if (!region || !button || open === isDrawerOpen()) return;
    region.classList.toggle('qv-drawer-open', open);
    button.setAttribute('aria-expanded', String(open));
    if (open) {
        document.addEventListener('pointerdown', closeOnOutsidePress, true);
        region.querySelector<HTMLElement>('summary, button, a[href], [tabindex="0"]')?.focus();
    } else {
        document.removeEventListener('pointerdown', closeOnOutsidePress, true);
        if (restoreFocus && region.contains(document.activeElement)) button.focus();
    }
}

function closeOnOutsidePress(e: PointerEvent): void {
    const root = player();
    const target = e.target instanceof Node ? e.target : null;
    if (!target || root?.panes?.contains(target) || root?.panesButton?.contains(target)) return;
    // The verb menu belongs to a pane item when it's open from the drawer.
    if (root?.verbMenu?.contains(target)) return;
    setDrawerOpen(false, { restoreFocus: false });
}
