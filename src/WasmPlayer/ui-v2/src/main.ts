// Player UI v2 (docs/ui-v2-plan.md): a jQuery-free UI for games saved as ASL 610 or later.
//
// The host (wasm-player.js) loads this once a game has been given v2, then swaps in
// wwwroot/v2/player.htm's markup and talks to v2 only through the adapter registered here as
// window.QuestPlayerUis.v2. Games talk to it through QuestUI (questui.ts).
//
// Not yet here: links and verb menus, waits, pauses, menus and questions, saving and
// accessibility (#2595's later PRs), then panes, layouts and theming (Phase 2).

import './host';
import type { PlayerUiAdapter } from './adapter';
import { QvCommandBar } from './command-bar';
import { sendCommand, sendEvent } from './engine';
import { QvOutput, rememberImageSize } from './output';
import { player, QvPlayer } from './player';
import { QuestUI, setInterfaceString, setLocation } from './questui';
import { emit, resetState, state } from './state';
import * as timer from './timer';

customElements.define('qv-player', QvPlayer);
customElements.define('qv-output', QvOutput);
customElements.define('qv-command-bar', QvCommandBar);

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

const commandBarHiddenBy = new Set<string>();

window.resetQuestUIState = () => {
    resetState();
    timer.stop();
    commandBarHiddenBy.clear();
};

const output = () => player()?.output;
const ignore = (): void => {};

// Hides the command bar for a reason of v2's own (the game finishing, a synchronous sound),
// separately from the game's setVisible("command-bar", …).
function holdCommandBar(reason: string, held: boolean): void {
    if (held) commandBarHiddenBy.add(reason);
    else commandBarHiddenBy.delete(reason);
    player()?.toggleAttribute('data-qv-input-held', commandBarHiddenBy.size > 0);
}

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
    bindMenu: ignore,
    showMenu: ignore,
    showQuestion: ignore,
    beginWait: ignore,
    setTurnPending: ignore,
    beginPause: ignore,
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
    updateObjectLinks: ignore,
    gameFinished() {
        state.gameFinished = true;
        timer.stop();
        holdCommandBar('finished', true);
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
    waitEnded: ignore,
    pauseEnded: ignore,
    beginSoundWait() {
        const showCommandBar = !player()?.commandBar?.hidden;
        state.soundWait = true;
        holdCommandBar('sound', true);
        return showCommandBar;
    },
    endSoundWait() {
        state.soundWait = false;
        setTimeout(async () => {
            holdCommandBar('sound', false);
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
