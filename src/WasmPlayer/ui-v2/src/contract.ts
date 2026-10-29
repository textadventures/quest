// QuestUI, the 610 UI contract's JS API (docs/ui-v2-contract.md). 610 Core calls it as
// JS.QuestUI.<member>(…), and authors' own JavaScript may call it too.
//
// The contract freezes with the first 6.1 release. From then on it only ever gains additions:
// members, trailing optional parameters, and new part, token and event names. Nothing here may
// be removed or change meaning.
//
// Arguments arrive as strings, numbers, booleans, and ASL lists and dictionaries as JSON arrays
// and objects (or, from an author's JS, whatever they pass), so parameters are typed as loosely
// as the engine may send them. Unknown names (parts, tokens, kinds, events) are ignored.

/** A status attribute, from 610 Core's UpdateStatusAttributes. */
export interface StatusAttribute {
    name?: string;
    label?: string;
    value?: unknown;
    max?: unknown;
    /** The formatted line, for a UI that doesn't draw bars. */
    text?: string;
    /** Keep it in view on a small screen (game.pinnedstatusattributes). */
    pinned?: boolean;
}

export type QuestUIEvent = 'turnStarted' | 'turnEnded' | 'outputAdded' | 'locationChanged';
export type QuestUIListener = (detail?: unknown) => void;

/** Same arguments, in the same order, as the Grid_* functions they replace. */
export interface QuestUIMap {
    show(size: unknown): void;
    respondToClicks(on: unknown): void;
    setInteractive(on: unknown): void;
    setScale(...args: unknown[]): void;
    setCentre(...args: unknown[]): void;
    drawBox(...args: unknown[]): void;
    drawLabel(...args: unknown[]): void;
    drawPlayer(...args: unknown[]): void;
    drawLine(...args: unknown[]): void;
    drawArrow(...args: unknown[]): void;
    drawSquare(...args: unknown[]): void;
    drawShape(...args: unknown[]): void;
    addShapePoint(...args: unknown[]): void;
    drawImage(...args: unknown[]): void;
    drawSvg(...args: unknown[]): void;
    loadSvg(...args: unknown[]): void;
    drawGridLines(...args: unknown[]): void;
    showCustomLayer(visible: unknown): void;
    clearCustomLayer(): void;
    clearAllLayers(): void;
}

export interface QuestUITranscript {
    write(html: string): void;
    enable(name?: string): void;
    disable(): void;
    kill(): void;
    revive(): void;
    setName(name: string): void;
    setPrefix(text: string): void;
    stampDateAndTime(): void;
    promptForName(): void;
    show(...args: unknown[]): void;
}

export interface QuestUIContract {
    // Engine
    sendCommand(text: string, metadata?: Record<string, string>): void;
    sendEvent(name: string, param?: unknown): void;
    restart(): void;

    // Output
    addText(html: string): void;
    clearScreen(): void;
    setAlignment(align: string): void;
    startSection(name: string): void;
    endSection(name: string): void;
    hideSection(name: string): void;
    disableSection(name: string): void;
    disableCommandLinks(): void;
    updateLinks(kind: string, data: unknown): void;
    scrollToEnd(): void;
    addYouTube(id: string): void;
    addVimeo(id: string): void;
    textEffect(kind: string, ...args: unknown[]): void;

    // Layout and panes
    setVisible(part: string, visible: unknown): void;
    setPaneOrder(parts: string[] | string): void;
    setLayoutPreset(name: string, narrow?: string): void;
    setMapPosition(position: string): void;
    setPictureFrame(html: string): void;
    setInterfaceString(name: string, text: string): void;
    setCompassDirections(list: string[] | string): void;
    setLocation(text: string): void;
    setStatusAttributes(items: StatusAttribute[] | string): void;

    // Theming
    setTokens(tokens: Record<string, unknown> | string): void;
    setCommandBarStyle(css: string): void;
    setCommandBarPreset(name: string): void;
    setCommandPrompt(text: string): void;
    addStylesheet(url: string): void;

    map: QuestUIMap;
    transcript: QuestUITranscript;

    // Author hooks
    addPane(id: string, title?: string, html?: string): void;
    setPaneContent(id: string, html: string): void;
    setCommands(list: string[] | string): void;
    setAutoScroll(on: unknown): void;
    setContinueOnClick(on: unknown): void;
    fadePreviousOutput(on: unknown): void;
    setCommandPlaceholder(text: string): void;
    setMapInteractive(on: unknown): void;
    on(event: QuestUIEvent | string, fn: QuestUIListener): void;
    off(event: QuestUIEvent | string, fn: QuestUIListener): void;
}
