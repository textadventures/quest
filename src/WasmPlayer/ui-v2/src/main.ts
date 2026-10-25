// Player UI v2 (docs/ui-v2-plan.md): a jQuery-free UI for games saved as ASL 610 or later.
//
// The host (wasm-player.js) loads this once a game has been given v2, then swaps in
// wwwroot/v2/player.htm's markup and talks to v2 only through the adapter registered here as
// window.QuestPlayerUis.v2. Games talk to it through QuestUI (questui.ts).
//
// Not yet here: layout presets and theming (Phase 2).

import './host';
import type { PlayerUiAdapter } from './adapter';
import { QvCommandBar } from './command-bar';
import { renderCompass, resetCompass, updateCompass, updateExitStrip } from './compass';
import { bindMenu } from './links';
import { resetMap } from './map';
import { finishTextEffects } from './text-effects';
import { sendCommand, sendEvent } from './engine';
import { QvOutput, rememberImageSize } from './output';
import { setPictureFrame } from './picture-frame';
import { player, QvPlayer } from './player';
import * as prompts from './prompts';
import { QvPane, setDrawerOpen, updateList } from './panes';
import { renderStatus, resetStatus, updateStatus } from './status';
import { QuestUI, setCompassDirections, setInterfaceString, setLocation } from './questui';
import { emit, resetState, state } from './state';
import * as timer from './timer';
import { QvVerbMenu } from './verb-menu';

customElements.define('qv-player', QvPlayer);
customElements.define('qv-output', QvOutput);
customElements.define('qv-command-bar', QvCommandBar);
customElements.define('qv-verb-menu', QvVerbMenu);
customElements.define('qv-pane', QvPane);

window.QuestUI = QuestUI;
// The globals author JS has always used, kept as QuestUI's aliases (as on legacy).
window.sendCommand = (text, metadata) => void sendCommand(text, metadata);
window.ASLEvent = sendEvent;
window.canSendCommand = true;

window.notifyQuestUITurn = (started) => {
    const output = player()?.output;
    if (started) {
        // Whatever was chosen in the drawer has been sent: show the transcript for its answer.
        setDrawerOpen(false, { restoreFocus: false });
        output?.beginTurn();
        output?.setAttribute('aria-busy', 'true');
        emit('turnStarted');
    } else {
        output?.removeAttribute('aria-busy');
        emit('turnEnded');
        player()?.commandBar?.focusInput();
        player()?.focusFirstChoice();
    }
};

window.resetQuestUIState = () => {
    resetState();
    timer.stop();
    prompts.resetPrompts();
    resetCompass();
    resetStatus();
    resetMap();
    finishTextEffects();
};

// Any key continues a wait, as on legacy, except Tab (so the keyboard can still reach links) and
// keys typed into a field, such as the host's save dialog. So does a click anywhere once the game
// has asked for that (QuestUI.setContinueOnClick), except on a link or control, which keeps its
// own meaning.
document.addEventListener('keydown', (e) => {
    if (!state.waiting || e.repeat || ['Tab', 'Shift', 'Control', 'Alt', 'Meta'].includes(e.key)) return;
    if (e.target instanceof Element && e.target.closest('input, textarea, select, [contenteditable], dialog')) return;
    e.preventDefault();
    prompts.endWait();
});
// Leaving the page loses whatever happened since the last save, so ask first, as legacy does.
window.addEventListener('beforeunload', (e) => {
    if (!state.hasUnsavedProgress || !state.canSave || !player()) return;
    e.preventDefault();
    e.returnValue = '';
});

document.addEventListener('click', (e) => {
    if (!state.waiting || !state.continueOnClick) return;
    if (e.target instanceof Element && e.target.closest('a, button, input, textarea, select, summary, dialog')) return;
    prompts.endWait();
});

const output = () => player()?.output;

function updateSaveButton(): void {
    const button = player()?.saveButton;
    if (button) button.disabled = state.turnPending;
}
const ignore = (): void => {};

// Element names the engine's Show/Hide requests use (WasmPlayerBridge.ElementMap).
const requestParts: Record<string, string> = {
    '#gamePanes': 'panes',
    '#location': 'location',
    '#txtCommandDiv': 'command-bar',
};

const adapter: PlayerUiAdapter = {
    // Engine calls
    addTextAndScroll(html) {
        output()?.addText(html);
        output()?.scrollToEnd();
    },
    createNewDiv: (alignment) => void output()?.setAlignment(alignment),
    bindMenu: (linkId, verbs, _text, elementId) => bindMenu(linkId, verbs, elementId),
    showMenu: prompts.showMenu,
    showQuestion: prompts.showQuestion,
    beginWait: prompts.beginWait,
    // Saving needs the engine between turns: a wait, menu, question or pause can't be saved.
    setTurnPending(pending) {
        state.turnPending = pending;
        updateSaveButton();
        updateExitStrip();
    },
    beginPause: prompts.beginPause,
    updateLocation: setLocation,
    setGameName(name) {
        document.title = name;
    },
    clearScreen: () => output()?.clearScreen(),
    panesVisible: (visible) => player()?.setVisible('panes', visible),
    updateStatus,
    setBackground: (colour) => player()?.setTokens({ '--qv-bg': colour }),
    setForeground: ignore,
    updateList,
    updateCompass,
    updateObjectLinks: (verbs) => player()?.updateLinks('object', verbs),
    gameFinished() {
        state.gameFinished = true;
        timer.stop();
        state.waiting = false;
        prompts.holdCommandBar('finished', true);
        updateExitStrip();
    },
    requestNextTimerTick: timer.requestNextTimerTick,
    uiShow: (element) => player()?.setVisible(requestParts[element] ?? '', true),
    uiHide: (element) => player()?.setVisible(requestParts[element] ?? '', false),
    setCompassDirections,
    setInterfaceString,
    setPanelContents: setPictureFrame,
    loadHtml: (html) => output()?.loadHtml(html),
    markScrollPosition: () => output()?.markScrollPosition(),
    scrollToEnd: () => output()?.scrollToEnd(),
    requestSave() {
        setTimeout(() => void GameSaver.save(), 100);
    },

    // Lifecycle
    start() {
        player()?.saveButton?.addEventListener('click', () => {
            if (WebPlayer.onSaveClick) WebPlayer.onSaveClick();
            else void GameSaver.save();
        });
        updateSaveButton();
        renderCompass();
        renderStatus();
        // The host translates the panes button's label at boot only, not after a restart.
        const label = WebPlayer.chromeStrings?.ShowPanesButton;
        if (label) {
            for (const button of player()?.querySelectorAll('.qv-panes-button') ?? []) button.setAttribute('aria-label', label);
        }
        player()?.commandBar?.focusInput();
    },
    resetOutput() {
        output()?.reset();
        timer.stop();
        state.hasUnsavedProgress = false;
    },
    useManualClock: timer.useManualClock,

    // Saving
    getOutputHtml: () => output()?.getHtml() ?? '',
    gameSaved() {
        output()?.addText('Game saved.<br>');
        state.hasUnsavedProgress = false;
    },
    clearUnsavedProgress() {
        state.hasUnsavedProgress = false;
    },
    setCanSave(value) {
        state.canSave = value;
    },

    // The host's wait, pause and sound plumbing
    waitEnded: prompts.waitEnded,
    pauseEnded: prompts.pauseEnded,
    beginSoundWait() {
        const showCommandBar = !player()?.commandBar?.hidden;
        state.soundWait = true;
        prompts.holdCommandBar('sound', true);
        return showCommandBar;
    },
    endSoundWait() {
        state.soundWait = false;
        setTimeout(async () => {
            prompts.holdCommandBar('sound', false);
            await WebPlayer.uiEndWait();
        }, 100);
    },

    rememberImageSize,
    setAnimateScroll(value) {
        state.animateScroll = value;
    },
};

window.QuestPlayerUis = window.QuestPlayerUis ?? {};
window.QuestPlayerUis.v2 = adapter;
