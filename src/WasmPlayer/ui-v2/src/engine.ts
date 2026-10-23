// Calls from the UI (and from authors' JS) to the engine: QuestUI.sendCommand/sendEvent, and the
// sendCommand/ASLEvent globals kept as their aliases.

import { player } from './player';
import { state } from './state';
import { takeTickCount } from './timer';

/** Whether a command can go to the engine now: not while a turn is in progress or held. */
export function canSendCommand(): boolean {
    return window.canSendCommand && !state.gameFinished && !state.soundWait;
}

/** Returns whether the command was sent. */
export function sendCommand(text: string, metadata?: Record<string, string>): boolean {
    if (!canSendCommand()) return false;
    window.canSendCommand = false;
    player()?.output?.markScrollPosition();
    state.hasUnsavedProgress = true;
    const tickCount = takeTickCount();
    // Deferred, as legacy does, so a call from a game's own JS in the middle of a turn doesn't
    // start another turn inside it. The host sets canSendCommand again once the turn is over.
    setTimeout(() => {
        void WebPlayer.sendCommand(text, tickCount, metadata ?? null);
    }, 0);
    return true;
}

export function sendEvent(name: string, param?: unknown): boolean {
    state.hasUnsavedProgress = true;
    // "" + param, as legacy's ASLEvent always did: author JS relies on how it stringifies.
    const value = '' + param;
    setTimeout(() => {
        void WebPlayer.uiSendEvent(name, value);
    }, 1);
    return true;
}
