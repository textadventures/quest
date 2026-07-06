var $_GET = {};
var webPlayer = false;
var tmrTick = null;
var tickCount = 0;
var sendNextGameTickerAfter = 0;
var canSendCommand = true;
var outputBufferId;
var gameSessionLogData;

document.location.search.replace(/\??(?:([^=]+)=([^&]*)&?)/g, function () {
    function decode(s) {
        return decodeURIComponent(s.split("+").join(" "));
    }

    $_GET[decode(arguments[1])] = decode(arguments[2]);
});

function setOutputBufferId(id) {
    outputBufferId = id;
    setUpSessionLog();
}

function setUpSessionLog() {
    if (outputBufferId && gameSessionLogData) {
        $.post("/GameSession/Init/?userId=" + gameSessionLogData.UserId + "&sessionId=" + gameSessionLogData.SessionId + "&bufferId=" + outputBufferId);
    }
}

var _waitingForSoundToFinish = false;

function msgboxSubmit(response) {
    markUnsavedProgress();
    $("#msgbox").dialog("close");
    window.setTimeout(async function () {
        await WebPlayer.uiSetQuestionResponse(response);
    }, 100);
}

var _menuSelection = "";

function showMenu(title, options, allowCancel) {
    $("#dialogOptions").empty();
    $.each(options, function (key, value) {
        $("#dialogOptions").append(
            $("<option/>").attr("value", key).text(value)
        );
    });

    $("#dialogCaption").html(title);

    var dialogOptions = {
        modal: true,
        autoOpen: false,
        buttons: [{
            text: "Select",
            click: function () {
                dialogSelect();
            }
        }]
    };

    if (allowCancel) {
        dialogOptions.buttons = dialogOptions.buttons.concat([{
            text: "Cancel",
            click: function () {
                dialogCancel();
            }
        }]);
        dialogOptions.close = function (event, ui) {
            dialogClose();
        };
    } else {
        dialogOptions.closeOnEscape = false;
        dialogOptions.open = function (event, ui) {
            $(".ui-dialog-titlebar-close").hide();
        };    // suppresses "close" button
    }

    _menuSelection = "";
    $("#dialog").dialog(dialogOptions);

    $("#dialog").dialog("open");
}

function dialogSelect() {
    _menuSelection = $("#dialogOptions").val();
    if (_menuSelection.length > 0) {
        markUnsavedProgress();
        $("#dialog").dialog("close");
        window.setTimeout(function () {
            WebPlayer.uiChoice(_menuSelection);
        }, 100);
    }
}

function dialogCancel() {
    $("#dialog").dialog("close");
}

function dialogClose() {
    if (_menuSelection.length == 0) {
        dialogSendCancel();
    }
}

function dialogSendCancel() {
    window.setTimeout(async function () {
        await WebPlayer.uiChoiceCancel();
    }, 100);
}

function sendCommand(text, metadata) {
    if (_pauseMode || _waitingForSoundToFinish || _waitMode || !canSendCommand) return;
    canSendCommand = false;
    markScrollPosition();
    markUnsavedProgress();

    window.setTimeout(function () {
        WebPlayer.sendCommand(text, getTickCountAndStopTimer(), metadata);
    }, 0);
}

function ASLEvent(event, parameter) {
    markUnsavedProgress();
    window.setTimeout(async function () {
        await WebPlayer.uiSendEvent(event, "" + parameter);
    }, 1);
    return true;
}

function disableMainScrollbar() {
    $("#divOutput").css("overflow", "hidden");
}

var _timerRunning = false;

function startTimer() {
    if (_timerRunning) return;
    _timerRunning = true;
    tickCount = 0;
    tmrTick = setInterval(async function () {
        await timerTick();
    }, 1000);
}

function stopTimer() {
    if (!_timerRunning) return;
    _timerRunning = false;
    clearInterval(tmrTick);
}

async function timerTick() {
    tickCount++;
    if (sendNextGameTickerAfter > 0 && tickCount >= sendNextGameTickerAfter) {
        await WebPlayer.uiTick(getTickCountAndStopTimer());
    }
}

function getTickCountAndStopTimer() {
    stopTimer();
    return tickCount;
}

function requestNextTimerTick(seconds) {
    sendNextGameTickerAfter = seconds;
    startTimer();
}

function goUrl(href) {
    window.open(href);
}

function saveGame() {
    setTimeout(async () => {
        await GameSaver.save();
    }, 100);
}

// A restart (Start from the beginning / Load) re-runs Initialise() on the
// same live document rather than a fresh page load, and Initialise()
// unconditionally re-registers the game's external scripts. A real <script
// src> only ever runs once per document, so re-adding the same URL here would
// re-execute the game's setup code against DOM it already set up on the
// previous run (e.g. a custom sidebar pane), producing duplicates. Dedupe by
// URL so each script tag still only truly executes once per document, same
// as normal page-load semantics.
var _loadedExternalScripts = new Set();

function addExternalScript(url) {
    if (_loadedExternalScripts.has(url)) return Promise.resolve();
    _loadedExternalScripts.add(url);

    return new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = url;
        script.onload = () => resolve();
        script.onerror = () => {
            _loadedExternalScripts.delete(url);
            reject(new Error(`Failed to load script: ${url}`));
        };
        document.head.appendChild(script);
    });
}

function WriteToLog(data) {
    // Do nothing.
}

function WriteToTranscript(data) {
    if (noTranscript) {
        // Do nothing.
        return;
    }
    if (!isLocalStorageAvailable()) {
        console.error("There is no localStorage. Disabling transcript functionality.");
        noTranscript = true;
        savingTranscript = false;
        return;
    }
    var tName = transcriptName || "Transcript";
    if (data.indexOf("___SCRIPTDATA___") > -1) {
        tName = data.split("___SCRIPTDATA___")[0].trim() || tName;
        data = data.split("___SCRIPTDATA___")[1];
    }
    var oldData = localStorage.getItem("questtranscript-" + tName) || "";
    localStorage.setItem("questtranscript-" + tName, oldData + data);
}

// Make sure localStorage is available, hopefully without throwing any errors!

/* https://stackoverflow.com/a/16427747 */
function isLocalStorageAvailable() {
    var test = 'test';
    try {
        localStorage.setItem(test, test);
        localStorage.removeItem(test);
        return true;
    } catch (e) {
        return false;
    }
}