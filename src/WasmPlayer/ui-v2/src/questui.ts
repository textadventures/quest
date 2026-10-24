// QuestUI on v2: the 610 UI contract (contract.ts, docs/ui-v2-contract.md).
//
// Every member exists, because 610 Core calls whichever it needs. The ones for parts v2 doesn't
// have yet (panes, compass, status, picture frame, map, media, text effects, the transcript) do
// nothing for now, which the contract allows: calls describe intent, and unknown names are
// ignored.

import type { QuestUIContract, QuestUIMap, QuestUITranscript } from './contract';
import { renderCompass } from './compass';
import type { StatusAttribute } from './contract';
import { setStatusAttributes } from './status';
import { sendCommand, sendEvent } from './engine';
import { addPane, setCommands, setPaneContent, setPaneLabel, setPaneOrder } from './panes';
import { player } from './player';
import { emit, off, on, state } from './state';

const ignore = (): void => {};

const map: QuestUIMap = {
    show: ignore,
    respondToClicks: ignore,
    setInteractive: ignore,
    setScale: ignore,
    setCentre: ignore,
    drawBox: ignore,
    drawLabel: ignore,
    drawPlayer: ignore,
    drawLine: ignore,
    drawArrow: ignore,
    drawSquare: ignore,
    drawShape: ignore,
    addShapePoint: ignore,
    drawImage: ignore,
    drawSvg: ignore,
    loadSvg: ignore,
    drawGridLines: ignore,
    showCustomLayer: ignore,
    clearCustomLayer: ignore,
    clearAllLayers: ignore,
};

const transcript: QuestUITranscript = {
    write: ignore,
    enable: ignore,
    disable: ignore,
    kill: ignore,
    revive: ignore,
    setName: ignore,
    setPrefix: ignore,
    stampDateAndTime: ignore,
    promptForName: ignore,
    show: ignore,
};

// ASL lists and dictionaries arrive as JSON values, but an author's JS may pass a JSON string.
function parsed<T>(value: T | string): T {
    return typeof value === 'string' ? JSON.parse(value) as T : value;
}

export function setLocation(text: string): void {
    const location = text ?? '';
    player()?.setLocation(location);
    emit('locationChanged', { location });
}

export function setInterfaceString(name: string, text: string): void {
    state.interfaceStrings.set(name, text);
    setPaneLabel(name, text);
    if (name === 'InButtonLabel' || name === 'OutButtonLabel' || name === 'CompassLabel') renderCompass();
    if (name === 'TypeHereLabel') player()?.commandBar?.updatePlaceholder();
    if (name === 'ContinueLabel') player()?.commandBar?.updateContinueLabel();
}

/** Both QuestUI.setCompassDirections and the adapter's: a list, or legacy's ";"-separated string. */
export function setCompassDirections(list: string[] | string): void {
    state.compassDirections = Array.isArray(list) ? list.map(String) : String(list ?? '').split(';');
    renderCompass();
}

export const QuestUI: QuestUIContract = {
    // Engine
    sendCommand,
    sendEvent,
    restart: () => window.RestartGame?.(),

    // Output
    addText: (html) => player()?.output?.addText(String(html)),
    clearScreen: () => player()?.output?.clearScreen(),
    setAlignment: (align) => void player()?.output?.setAlignment(String(align)),
    startSection: (name) => player()?.output?.startSection(String(name)),
    endSection: (name) => player()?.output?.endSection(String(name)),
    hideSection: (name) => player()?.output?.hideSection(String(name)),
    disableSection: (name) => player()?.output?.disableSection(String(name)),
    disableCommandLinks: () => player()?.output?.disableCommandLinks(),
    updateLinks: (kind, data) => player()?.updateLinks(String(kind), parsed(data as object | string)),
    scrollToEnd: () => player()?.output?.scrollToEnd(),
    addYouTube: ignore,
    addVimeo: ignore,
    textEffect: ignore,

    // Layout and panes
    setVisible: (part, visible) => player()?.setVisible(String(part), !!visible),
    setPaneOrder: (parts) => setPaneOrder(parsed(parts as string[] | string)),
    setLayoutPreset: ignore,
    setPictureFrame: ignore,
    setInterfaceString,
    setCompassDirections: (list) => setCompassDirections(parsed(list as string[] | string)),
    setLocation,
    setStatusAttributes: (items) => setStatusAttributes(parsed<StatusAttribute[]>(items)),

    // Theming
    setTokens: (tokens) => player()?.setTokens(parsed(tokens) ?? {}),
    setCommandBarStyle: (css) => player()?.commandBar?.setStyle(String(css ?? '')),
    setCommandBarPreset: ignore,
    setCommandPrompt: (text) => player()?.commandBar?.setPrompt(String(text ?? '')),
    addStylesheet: (url) => addExternalStylesheet(String(url)),

    map,
    transcript,

    // Author hooks
    addPane: (id, title, html) => addPane(String(id), title == null ? undefined : String(title), html == null ? undefined : String(html)),
    setPaneContent: (id, html) => setPaneContent(String(id), String(html ?? '')),
    setCommands: (list) => setCommands(list),
    setAutoScroll: (on) => {
        state.autoScroll = !!on;
    },
    setContinueOnClick: (on) => {
        state.continueOnClick = !!on;
    },
    fadePreviousOutput: (enabled) => {
        state.fadePreviousOutput = !!enabled;
        if (!state.fadePreviousOutput) player()?.output?.clearPreviousOutputFade();
    },
    setCommandPlaceholder: (text) => {
        state.commandPlaceholder = text == null ? null : String(text);
        player()?.commandBar?.updatePlaceholder();
    },
    setMapInteractive: ignore,
    on,
    off,
};
