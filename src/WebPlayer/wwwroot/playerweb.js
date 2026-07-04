class WebPlayer {
    static dotNetHelper;
    static gameDotNetHelper;
    static gameId;
    static slotsDialogCanBeClosed = false;

    // Fixed at class-definition time (not set conditionally later), so there's
    // no ordering/race concern with when initPlayerUI() binds #cmdSave's click
    // handler to this — matches the shared onSaveClick hook in playercore.js.
    static onSaveClick = async () => {
        await WebPlayer.gameDotNetHelper?.invokeMethodAsync("OpenSaveManager");
    }

    static setDotNetHelper(value) {
        WebPlayer.dotNetHelper = value;
    }

    static setGameDotNetHelper(value) {
        WebPlayer.gameDotNetHelper = value;
    }

    static setGameId(id) {
        WebPlayer.gameId = id;
    }

    static listSaves = async () => {
        return await GameSaver.listSaves();
    }

    static loadSlot = async (slot) => {
        return await GameSaver.load(slot);
    }

    static saveNew = async (name) => {
        return await GameSaver.save(name || undefined);
    }

    static deleteSlot = async (slotIndex) => {
        return await GameSaver.deleteSlot(slotIndex);
    }

    // Clears stale transcript/div-tracking state from any previously-running
    // game before a mid-session restart (Start from the beginning / Load) —
    // without this, #divOutput keeps whatever the last game wrote, and the
    // new game's output gets appended after it instead of replacing it.
    // A fresh page load doesn't need this (the div starts empty already),
    // so it's harmless to call unconditionally at the top of every StartGame.
    static resetOutput() {
        resetGameOutput();
    }

    static async downloadSaveAsFile(filename) {
        const bytes = await WebPlayer.uiSaveGame($("#divOutput").html());
        const blob = new Blob([bytes], {type: 'application/xml'});
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        a.click();
        URL.revokeObjectURL(url);
        clearUnsavedProgress();
    }

    static initSlotsDialog() {
        const slots = document.getElementById("questVivaSlots");
        slots.addEventListener('cancel', (event) => {
            if (!WebPlayer.slotsDialogCanBeClosed) {
                event.preventDefault();
                slots.focus();
            }
        });

        // workaround for Escape key _still_ closing the dialog
        document.addEventListener('keydown', (e) => {
            const slots = document.getElementById("questVivaSlots");
            if (e.key === 'Escape' && slots.open && !WebPlayer.slotsDialogCanBeClosed) {
                e.preventDefault();
                e.stopPropagation();
            }
        }, true);
    }

    static showSlots(cancellable) {
        const slots = document.getElementById("questVivaSlots");
        WebPlayer.slotsDialogCanBeClosed = cancellable;
        slots.showModal();
    }

    static closeSlots() {
        const slots = document.getElementById("questVivaSlots");
        slots.close();
    }

    static closeDebugger() {
        const dialog = document.getElementById("questVivaDebugger");
        dialog.close();
    }

    static initUI() {
        initPlayerUI();
    }

    static setCanDebug(value) {
        const cmdDebug = document.getElementById("cmdDebug");
        cmdDebug.style.display = value ? "initial" : "none";
    }

    static setCanSave(value) {
        const cmdSave = document.getElementById("cmdSave");
        cmdSave.style.display = value ? "initial" : "none";
        if (!value) {
            window.saveGame = () => addText("Disabled");
        }
    }

    static setAnimateScroll(value) {
        _animateScroll = value;
    }

    static async sendCommand(command, tickCount, metadata) {
        await WebPlayer.dotNetHelper.invokeMethodAsync("UiSendCommandAsync", command, tickCount, metadata);
        canSendCommand = true;
    }

    static runJs(scripts) {
        // We need globalEval so that calls which add functions add them to the global scope.
        // e.g. spondre evals strings like "function blah() { ... }" which need to be in the global scope so
        // they can be called from subsequent evals.
        const globalEval = window.eval;
        for (const script of scripts) {
            try {
                globalEval(script);
            } catch (e) {
                console.error(e);
            }
        }
    }

    static async uiChoice(choice) {
        await WebPlayer.dotNetHelper.invokeMethodAsync("UiChoiceAsync", choice);
    }

    static async uiChoiceCancel() {
        await WebPlayer.dotNetHelper.invokeMethodAsync("UiChoiceCancelAsync");
    }

    static async uiTick(tickCount) {
        await WebPlayer.dotNetHelper.invokeMethodAsync("UiTickAsync", tickCount);
    }

    static async uiEndWait() {
        await WebPlayer.dotNetHelper.invokeMethodAsync("UiEndWaitAsync");
    }

    static async uiEndPause() {
        await WebPlayer.dotNetHelper.invokeMethodAsync("UiEndPauseAsync");
    }

    static async uiSetQuestionResponse(response) {
        await WebPlayer.dotNetHelper.invokeMethodAsync("UiSetQuestionResponseAsync", response);
    }

    static async uiSendEvent(eventName, param) {
        await WebPlayer.dotNetHelper.invokeMethodAsync("UiSendEventAsync", eventName, param);
        canSendCommand = true;
    }

    static async uiSaveGame(html) {
        return await WebPlayer.dotNetHelper.invokeMethodAsync("UiSaveGameAsync", html);
    }
}

window.WebPlayer = WebPlayer;

$(function () {
    var width = $_GET["w"];
    if (width) {
        setGameWidth(width);
    }
});

function ui_init() {
}

function sendEndWait() {
    window.setTimeout(async function () {
        await WebPlayer.uiEndWait();
    }, 100);
    waitEnded();
}

function sessionTimeout() {
    disableInterface();
}

function afterSendCommand() {
}

var _audio = null;

function playSound(url, synchronous, looped) {
    stopSound();
    _audio = new Audio(url);
    if (looped) {
        _audio.loop = true;
    }
    if (synchronous) {
        var showCmdDiv = isElementVisible("#txtCommandDiv");
        _waitingForSoundToFinish = true;
        $("#txtCommandDiv").hide();
        _audio.addEventListener('ended', function () {
            finishSync(showCmdDiv);
        });
    }
    _audio.play();
}

function stopSound() {
    if (_audio !== null) {
        _audio.pause();
        _audio.src = '';
        _audio = null;
    }
}

function finishSync(showCommandDiv) {
    _waitingForSoundToFinish = false;
    window.setTimeout(async function () {
        if (showCommandDiv) $("#txtCommandDiv").show();
        await WebPlayer.uiEndWait();
    }, 100);
}

var platform = "webplayer";