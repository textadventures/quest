// Per-game state that lives outside v2's markup. The markup itself (and whatever its elements
// hold) is swapped in afresh for each game and restart, so only this needs resetting then.

import type { QuestUIListener } from './contract';

export const state = {
    /** QuestUI.setAutoScroll. */
    autoScroll: true,
    /** False while a walkthrough fast-forwards (the host's setAnimateScroll). */
    animateScroll: true,
    /** QuestUI.fadePreviousOutput. */
    fadePreviousOutput: false,
    /** QuestUI.setContinueOnClick, kept for when v2 has waits. */
    continueOnClick: false,
    /** QuestUI.setCommandPlaceholder, which wins over the TypeHereLabel interface string. */
    commandPlaceholder: null as string | null,
    /** Strings from setInterfaceString, kept for parts that don't exist yet. */
    interfaceStrings: new Map<string, string>(),
    gameFinished: false,
    /** A synchronous sound is playing: the turn is held until it ends. */
    soundWait: false,
    /** The game is waiting for the player to continue (wait). */
    waiting: false,
    /** The game has paused for a set time (pause). */
    pausing: false,
    /** A ShowMenu or Ask dialog is open, or its answer hasn't reached the engine yet. */
    inDialog: false,
    /** The engine is suspended mid-turn (a wait, menu, question or pause), so can't save. */
    turnPending: true,
    hasUnsavedProgress: false,
    canSave: false,
};

const listeners = new Map<string, Set<QuestUIListener>>();

export function on(event: string, fn: QuestUIListener): void {
    if (typeof fn !== 'function') return;
    let set = listeners.get(event);
    if (!set) listeners.set(event, set = new Set());
    set.add(fn);
}

export function off(event: string, fn: QuestUIListener): void {
    listeners.get(event)?.delete(fn);
}

/** Calls each listener, so one author's broken handler can't stop the others or the turn. */
export function emit(event: string, detail?: unknown): void {
    for (const fn of listeners.get(event) ?? []) {
        try {
            fn(detail);
        } catch (e) {
            console.error(e);
        }
    }
}

export function resetState(): void {
    state.autoScroll = true;
    state.fadePreviousOutput = false;
    state.continueOnClick = false;
    state.commandPlaceholder = null;
    state.interfaceStrings.clear();
    state.gameFinished = false;
    state.soundWait = false;
    state.waiting = false;
    state.pausing = false;
    state.inDialog = false;
    state.turnPending = true;
    state.hasUnsavedProgress = false;
    listeners.clear();
}
