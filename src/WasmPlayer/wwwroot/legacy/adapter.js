// The legacy UI's side of the player-UI adapter: everything wasm-player.js (the host) asks of
// whichever UI a game is playing on, typed as PlayerUiAdapter in ../../ui-v2/src/adapter.ts. The
// host loads this bundle only for games that get the legacy UI (see preparePlayerUi in
// wasm-player.js) and calls the UI only through this object.
//
// Each engine call is passed straight to the legacy global of the same name, looked up when it's
// called, so anything that wraps or replaces those globals (questui.js, or a game's own
// JavaScript) still sees it, exactly as when the host called them directly.

window.QuestPlayerUis = window.QuestPlayerUis || {};

window.QuestPlayerUis.legacy = {
    // Engine calls (WasmPlayerBridge's JSImports).
    addTextAndScroll: (html) => addTextAndScroll(html),
    createNewDiv: (alignment) => createNewDiv(alignment),
    bindMenu: (linkId, verbs, text, elementId) => bindMenu(linkId, verbs, text, elementId),
    showMenu: (caption, options, allowCancel) => showMenu(caption, options, allowCancel),
    showQuestion: (caption) => showQuestion(caption),
    beginWait: () => beginWait(),
    setTurnPending: (pending) => setTurnPending(pending),
    beginPause: (ms) => beginPause(ms),
    updateLocation: (location) => updateLocation(location),
    setGameName: (name) => setGameName(name),
    clearScreen: () => clearScreen(),
    panesVisible: (visible) => panesVisible(visible),
    updateStatus: (text) => updateStatus(text),
    setBackground: (colour) => setBackground(colour),
    setForeground: (colour) => setForeground(colour),
    updateList: (listName, items) => updateList(listName, items),
    updateCompass: (data) => updateCompass(data),
    updateObjectLinks: (verbs) => updateObjectLinks(verbs),
    gameFinished: () => gameFinished(),
    requestNextTimerTick: (seconds) => requestNextTimerTick(seconds),
    uiShow: (element) => uiShow(element),
    uiHide: (element) => uiHide(element),
    setCompassDirections: (directions) => setCompassDirections(directions),
    setInterfaceString: (name, text) => setInterfaceString(name, text),
    setPanelContents: (html) => setPanelContents(html),
    loadHtml: (html) => loadHtml(html),
    markScrollPosition: () => markScrollPosition(),
    scrollToEnd: () => scrollToEnd(),
    requestSave: () => saveGame(),

    // Lifecycle.
    async start() {
        await setupPaperJs();
        initPlayerUI();
    },
    resetOutput: () => resetGameOutput(),
    // Stops player.js's 1-second interval from ever starting.
    useManualClock() {
        window.startTimer = () => {};
    },

    // Saving.
    getOutputHtml: () => $("#divOutput").html(),
    gameSaved() {
        addText("Game saved.<br>");
        clearUnsavedProgress();
    },
    clearUnsavedProgress: () => clearUnsavedProgress(),
    setCanSave(value) {
        canSave = value;
        if (!value) window.saveGame = () => addText("Disabled");
    },

    // The host's wait, pause and sound plumbing.
    waitEnded: () => waitEnded(),
    pauseEnded: () => pauseEnded(),
    // A synchronous sound holds the turn like a wait: hide the command bar until it ends.
    // Returns whether to show it again afterwards. Counts a pause whose restore is still pending
    // as visible - see _pauseRestorePending - or the bar would stay hidden for good.
    beginSoundWait() {
        const showCommandBar = isElementVisible("#txtCommandDiv") || _pauseRestorePending;
        _waitingForSoundToFinish = true;
        $("#txtCommandDiv").hide();
        return showCommandBar;
    },
    endSoundWait(showCommandBar) {
        _waitingForSoundToFinish = false;
        window.setTimeout(async function () {
            if (showCommandBar) $("#txtCommandDiv").show();
            await WebPlayer.uiEndWait();
        }, 100);
    },

    rememberImageSize: (url, width, height) => rememberImageSize(url, width, height),
    setAnimateScroll(value) {
        _animateScroll = value;
    },
};

// The map: paper.js on a canvas, running grid.js as PaperScript. Set up afresh for each game and
// restart, since each one swaps in new markup.
async function setupPaperJs() {
    if (!window.paper) return;
    const canvas = document.createElement('canvas');
    canvas.id = 'gridCanvas';
    canvas.width = 700;
    canvas.height = 300;
    canvas.style.display = 'none';
    document.body.appendChild(canvas);
    paper.setup(canvas);
    const code = await fetch(versioned('legacy/grid.js')).then(r => r.text());
    paper.PaperScript.evaluate(code, paper);
}
