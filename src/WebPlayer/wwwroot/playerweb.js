class WebPlayer {
    static dotNetHelper;
    static gameId;
    static slotsDialogCanBeClosed = false;

    static setDotNetHelper(value) {
        WebPlayer.dotNetHelper = value;
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

var _currentPlayer = "";

function playWav(filename, sync, looped) {
    if (!document.createElement('audio').canPlayType) {
        // no <audio> support, so we must play WAVs using <embed> as the 
        // jPlayer Flash fallback does not support WAV.

        var extra = "";

        if (looped) {
            extra = " loop=\"true\"";
        }

        $("#audio_embed").html("<embed src=\"" + filename + "\" autostart=\"true\" width=\"0\" height=\"0\" type=\"audio/wav\"" + extra + ">");
        _currentPlayer = "embed";
    }
    else {
        playAudio(filename, "wav", sync, looped);
    }
}

function playMp3(filename, sync, looped) {
    playAudio(filename, "mp3", sync, looped);
}

var showCommandDiv = isElementVisible("#txtCommandDiv");
function playAudio(filename, format, sync, looped) {
    _currentPlayer = "jplayer";

    $("#jquery_jplayer").unbind($.jPlayer.event.ended);

    if (looped) {
        // This works in Firefox and Chrome.
        endFunction = function () { $("#jquery_jplayer").jPlayer("play"); };
    }
    else if (sync) {
        // Added the following line to set the variable properly. 3-4-2018
        showCommandDiv = isElementVisible("#txtCommandDiv");
        _waitingForSoundToFinish = true;
        // Altered finishSync to use the showCommandDiv variable.
        // It was using txtCommandDiv visibility, which the last line sets to false!
        endFunction = function () { finishSync(showCommandDiv); };
        $("#txtCommandDiv").hide();
    }
    else {
        endFunction = null;
    }

    //$("#jquery_jplayer").bind($.jPlayer.event.error, function (event) { alert(event.jPlayer.error.type); });

    if (endFunction != null) {
        $("#jquery_jplayer").bind($.jPlayer.event.ended, function (event) { endFunction(); });
    }

    if (format == "wav") $("#jquery_jplayer").jPlayer("setMedia", { wav: filename });
    if (format == "mp3") $("#jquery_jplayer").jPlayer("setMedia", { mp3: filename });
    $("#jquery_jplayer").jPlayer("play");
}

function stopAudio() {
    if (_currentPlayer == "jplayer") {
        $("#jquery_jplayer").jPlayer("stop");
    }
    else if (_currentPlayer == "embed") {
        $("#audio_embed").html("");
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