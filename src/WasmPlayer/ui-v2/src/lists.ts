// The lists the panes show, passed on to a game's own JavaScript so it can draw them its own way
// (#2905): QuestUI.lists() for what they are now, and an event each time one changes. The engine
// sends them after every turn and every timer tick, but only those that changed.

import type { QuestUIListItem, QuestUILists } from './contract';
import { emit } from './state';

/** One item as the adapter's updateList has it, from WasmPlayerBridge. */
export interface EngineListItem {
    Text: string;
    Verbs: string[];
    ElementId: string | null;
    ElementName: string;
}

const events = {
    inventory: 'inventoryChanged',
    objects: 'objectsChanged',
    exits: 'exitsChanged',
    status: 'statusChanged',
} as const;

type ListName = keyof typeof events;

let current: QuestUILists = { inventory: [], objects: [], exits: [], status: [] };
// The lists the engine has sent since the game started, so the first of each always fires.
const received = new Set<ListName>();

/** A copy, so an author's script can't change what the panes and other listeners see. */
export function lists(): QuestUILists {
    return structuredClone(current);
}

export function resetLists(): void {
    current = { inventory: [], objects: [], exits: [], status: [] };
    received.clear();
}

export function setList<K extends ListName>(name: K, value: QuestUILists[K]): void {
    // Status is sent every turn whether it changed or not, and a game only wants to hear of a change.
    if (received.has(name) && JSON.stringify(value) === JSON.stringify(current[name])) return;
    received.add(name);
    current[name] = value;
    const copy = structuredClone(value);
    emit(events[name], name === 'exits' ? { exits: copy } : { items: copy });
}

export function listItem(item: EngineListItem): QuestUIListItem {
    return {
        text: String(item.Text ?? ''),
        name: String(item.ElementName ?? ''),
        elementId: String(item.ElementId ?? ''),
        verbs: (item.Verbs ?? []).map(String),
    };
}
