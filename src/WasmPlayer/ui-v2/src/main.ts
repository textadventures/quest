// Player UI v2 (docs/ui-v2-plan.md): a jQuery-free UI for games saved as ASL 610 or later.
//
// The host (wasm-player.js) loads this once a game has been given v2, then swaps in
// wwwroot/v2/player.htm's markup and talks to v2 only through the adapter registered here as
// window.QuestPlayerUis.v2. Games talk to it through QuestUI (questui.ts).
//
// Not yet here: saving and accessibility (#2595's last PR), then panes, layouts and theming
// (Phase 2).

import './host';
import type { PlayerUiAdapter } from './adapter';
import { QvCommandBar } from './command-bar';
import { bindMenu } from './links';
import { sendCommand, sendEvent } from './engine';
import { QvOutput, rememberImageSize } from './output';
import { player, QvPlayer } from './player';
import * as prompts from './prompts';
import { QuestUI, setInterfaceString, setLocation } from './questui';
import { emit, resetState, state } from './state';
import * as timer from './timer';
import { QvVerbMenu } from './verb-menu';

customElements.define('qv-player', QvPlayer);
customElements.define('qv-output', QvOutput);
customElements.define('qv-command-bar', QvCommandBar);
customElements.define('qv-verb-menu', QvVerbMenu);

window.QuestUI = QuestUI;
// The globals author JS has always used, kept as QuestUI's aliases (as on legacy).
window.sendCommand = (text, metadata) => void sendCommand(text, metadata);
window.ASLEvent = sendEvent;
window.canSendCommand = true;

window.notifyQuestUITurn = (started) => {
    const output = player()?.output;
    if (started) {
        output?.beginTurn();
        output?.setAttribute('aria-busy', 'true');
        emit('turnStarted');
    } else {
        output?.removeAttribute('aria-busy');
        emit('turnEnded');
        player()?.commandBar?.focusInput();
    }
};

window.resetQuestUIState = () => {
    resetState();
    timer.stop();
    prompts.resetPrompts();
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
document.addEventListener('click', (e) => {
    if (!state.waiting || !state.continueOnClick) return;
    if (e.target instanceof Element && e.target.closest('a, button, input, textarea, select, summary, dialog')) return;
    prompts.endWait();
});

const output = () => player()?.output;
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
    setTurnPending(pending) {
        state.turnPending = pending;
    },
    beginPause: prompts.beginPause,
    updateLocation: setLocation,
    setGameName(name) {
        document.title = name;
    },
    clearScreen: () => output()?.clearScreen(),
    panesVisible: ignore,
    updateStatus: ignore,
    setBackground: (colour) => player()?.setTokens({ '--qv-bg': colour }),
    setForeground: ignore,
    updateList: ignore,
    updateCompass: ignore,
    updateObjectLinks: (verbs) => player()?.updateLinks('object', verbs),
    gameFinished() {
        state.gameFinished = true;
        timer.stop();
        state.waiting = false;
        prompts.holdCommandBar('finished', true);
    },
    requestNextTimerTick: timer.requestNextTimerTick,
    uiShow: (element) => player()?.setVisible(requestParts[element] ?? '', true),
    uiHide: (element) => player()?.setVisible(requestParts[element] ?? '', false),
    setCompassDirections: ignore,
    setInterfaceString,
    setPanelContents: ignore,
    loadHtml: (html) => output()?.loadHtml(html),
    markScrollPosition: () => output()?.markScrollPosition(),
    scrollToEnd: () => output()?.scrollToEnd(),
    requestSave() {
        setTimeout(() => void GameSaver.save(), 100);
    },

    // Lifecycle
    start() {
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
