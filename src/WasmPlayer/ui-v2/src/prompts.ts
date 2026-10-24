// Times the engine holds a turn for the player: wait (a Continue button), pause (nothing to do
// but wait), and, for games before 600, ShowMenu and Ask as <dialog>s. From 600 the engine
// draws menus and questions in the transcript as numbered command links instead
// (WorldModel.ShowInlinePromptAsync), which links.ts already handles.
//
// The host's sendEndWait/sendEndPause resume the engine after a short paint-yield, then call
// the adapter's waitEnded/pauseEnded once the resumed turn has reached its next stopping point.
// Only then does the command input come back, and only if that turn didn't wait or pause again,
// so it doesn't flash up between chained waits.

import { player } from './player';
import { state } from './state';

const commandBarHeldBy = new Set<string>();

/**
 * Hides the command bar for a reason of v2's own (the game finishing, a synchronous sound, a
 * pause), separately from the game's setVisible("command-bar", …). It stays hidden while any
 * reason holds.
 */
export function holdCommandBar(reason: string, held: boolean): void {
    if (held) commandBarHeldBy.add(reason);
    else commandBarHeldBy.delete(reason);
    player()?.toggleAttribute('data-qv-input-held', commandBarHeldBy.size > 0);
}

export function resetPrompts(): void {
    commandBarHeldBy.clear();
    clearTimeout(pauseTimer);
}

// ── Wait ────────────────────────────────────────────────────────────────────

export function beginWait(): void {
    state.waiting = true;
    player()?.commandBar?.setMode('continue');
    player()?.output?.markScrollPosition();
}

/** The Continue button, any key, or (with setContinueOnClick) a click anywhere. */
export function endWait(): void {
    if (!state.waiting) return;
    state.waiting = false;
    // No input until waitEnded: the resumed turn may wait again straight away.
    player()?.commandBar?.setMode('none');
    state.hasUnsavedProgress = true;
    sendEndWait();
}

export function waitEnded(): void {
    if (state.waiting) return;
    showInput();
}

// ── Pause ───────────────────────────────────────────────────────────────────

let pauseTimer: ReturnType<typeof setTimeout> | undefined;

export function beginPause(ms: number): void {
    state.pausing = true;
    holdCommandBar('pause', true);
    clearTimeout(pauseTimer);
    pauseTimer = setTimeout(endPause, ms);
}

function endPause(): void {
    if (!state.pausing) return;
    state.pausing = false;
    sendEndPause();
}

export function pauseEnded(): void {
    if (state.pausing) return;
    holdCommandBar('pause', false);
    showInput();
}

function showInput(): void {
    const bar = player()?.commandBar;
    if (!bar || state.gameFinished || state.waiting || state.inDialog) return;
    bar.setMode('input');
    bar.focusInput();
}

// ── Menus and questions, for games before 600 ───────────────────────────────

// Legacy's dialogs are in English too: these games have no translated strings for them.
const LABELS = { cancel: 'Cancel', yes: 'Yes', no: 'No' };

interface DialogChoice {
    label: string;
    choose(): Promise<void>;
}

// A modal <dialog> with the caption and one button per choice. Without a cancel, Escape is
// refused (and if the browser closes it anyway, it's opened again): the engine is waiting for
// an answer and has no other way to get one.
//
// The command bar is hidden from when the dialog opens until the engine has finished the turn the
// answer resumes. Otherwise a command could reach the engine in the 100ms before the answer does,
// and the answer would then be taken as the reply to whatever that command asked next.
function openDialog(caption: string, choices: DialogChoice[], cancel: (() => Promise<void>) | null): void {
    const root = player();
    if (!root) return;
    root.querySelector('dialog.qv-dialog')?.remove();

    const dialog = document.createElement('dialog');
    dialog.className = 'qv-dialog';
    dialog.dataset.qvPart = 'menu';
    const captionId = 'qv-dialog-caption';
    dialog.setAttribute('aria-labelledby', captionId);
    const text = document.createElement('div');
    text.id = captionId;
    text.className = 'qv-dialog-caption';
    text.innerHTML = caption;
    const list = document.createElement('div');
    list.className = 'qv-dialog-choices';

    let answered = false;
    const answer = (choose: () => Promise<void>) => {
        if (answered) return;
        answered = true;
        dialog.close();
        dialog.remove();
        state.hasUnsavedProgress = true;
        // As legacy does: let the dialog's closing paint before the engine takes the thread.
        setTimeout(async () => {
            try {
                await choose();
            } finally {
                state.inDialog = false;
                showInput();
            }
        }, 100);
    };

    for (const choice of choices) {
        const button = document.createElement('button');
        button.type = 'button';
        button.textContent = choice.label;
        button.addEventListener('click', () => answer(choice.choose));
        list.append(button);
    }
    dialog.append(text, list);
    if (cancel) {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'qv-dialog-cancel';
        button.textContent = LABELS.cancel;
        button.addEventListener('click', () => answer(cancel));
        dialog.append(button);
    }

    dialog.addEventListener('cancel', (e) => {
        e.preventDefault();
        if (cancel) answer(cancel);
    });
    dialog.addEventListener('close', () => {
        if (!answered && dialog.isConnected) dialog.showModal();
    });

    state.inDialog = true;
    root.commandBar?.setMode('none');
    root.append(dialog);
    dialog.showModal();
    list.querySelector('button')?.focus();
}

/** ShowMenu before 600. Options map each value to the text shown for it. */
export function showMenu(caption: string, options: Record<string, string>, allowCancel: boolean): void {
    const choices = Object.entries(options).map(([key, label]) => ({
        label,
        choose: () => WebPlayer.uiChoice(key),
    }));
    openDialog(caption, choices, allowCancel ? () => WebPlayer.uiChoiceCancel() : null);
}

/** Ask before 600. */
export function showQuestion(caption: string): void {
    openDialog(caption, [
        { label: LABELS.yes, choose: () => WebPlayer.uiSetQuestionResponse(true) },
        { label: LABELS.no, choose: () => WebPlayer.uiSetQuestionResponse(false) },
    ], null);
}
