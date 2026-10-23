// The player-UI adapter: everything wasm-player.js (the host) asks of whichever UI a game is
// playing on. Each UI registers one as window.QuestPlayerUis[name], and the host calls the UI
// only through it (see preparePlayerUi in wasm-player.js). legacy/adapter.js is the other
// implementation.
//
// This is internal, not part of the 610 UI contract: games never call it, so it can change
// whenever the host and both UIs change with it.

/** One item of an inventory or places-and-objects list, as WasmPlayerBridge serialises it. */
export type ListItems = Record<string, string>;

export interface PlayerUiAdapter {
    // ── Engine calls: WasmPlayerBridge's JSImports, passed on by wasm-player.js ──

    addTextAndScroll(html: string): void;
    createNewDiv(alignment: string): void;
    bindMenu(linkId: string, verbs: string, text: string, elementId: string): void;
    /** A pre-600 game's ShowMenu dialog. Options map each value to its caption. */
    showMenu(caption: string, options: Record<string, string>, allowCancel: boolean): void;
    showQuestion(caption: string): void;
    beginWait(): void;
    /** Whether the engine is suspended mid-turn (wait, menu, question, pause), so can't save. */
    setTurnPending(pending: boolean): void;
    beginPause(ms: number): void;
    updateLocation(location: string): void;
    setGameName(name: string): void;
    clearScreen(): void;
    panesVisible(visible: boolean): void;
    updateStatus(text: string): void;
    setBackground(colour: string): void;
    setForeground(colour: string): void;
    updateList(listName: string, items: ListItems): void;
    updateCompass(data: string): void;
    updateObjectLinks(verbs: Record<string, string>): void;
    gameFinished(): void;
    requestNextTimerTick(seconds: number): void;
    /** `request (Show, …)`: "#gamePanes", "#location" or "#txtCommandDiv". */
    uiShow(element: string): void;
    uiHide(element: string): void;
    setCompassDirections(directions: string[]): void;
    setInterfaceString(name: string, text: string): void;
    setPanelContents(html: string): void;
    /** Restores a save's transcript, as getOutputHtml returned it. */
    loadHtml(html: string): void;
    markScrollPosition(): void;
    scrollToEnd(): void;
    requestSave(): void;

    // ── Lifecycle ──

    /** Once the game has loaded and its scripts have run, before the engine's Begin(). */
    start(): void | Promise<void>;
    /** A restart or load is swapping in fresh markup: drop output state kept outside it. */
    resetOutput(): void;
    /** ?clock=manual (a test hook): never start the timer, so time only moves when a test says. */
    useManualClock(): void;

    // ── Saving ──

    getOutputHtml(): string;
    gameSaved(): void;
    clearUnsavedProgress(): void;
    setCanSave(value: boolean): void;

    // ── The host's wait, pause and sound plumbing ──

    /** The engine has resumed after a wait and reached its next stopping point. */
    waitEnded(): void;
    pauseEnded(): void;
    /** A synchronous sound holds the turn: returns whether to show the command bar after it. */
    beginSoundWait(): boolean;
    endSoundWait(showCommandBar: boolean): void;

    rememberImageSize(url: string, width: number, height: number): void;
    /** False while a walkthrough fast-forwards: jump straight to the end instead. */
    setAnimateScroll(value: boolean): void;
}
