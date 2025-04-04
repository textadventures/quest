var $_GET = {};
var webPlayer = true;
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
            click: function () { dialogSelect(); }
        }]
    };

    if (allowCancel) {
        dialogOptions.buttons = dialogOptions.buttons.concat([{
            text: "Cancel",
            click: function () { dialogCancel(); }
        }]);
        dialogOptions.close = function (event, ui) { dialogClose(); };
    }
    else {
        dialogOptions.closeOnEscape = false;
        dialogOptions.open = function (event, ui) { $(".ui-dialog-titlebar-close").hide(); };    // suppresses "close" button
    }

    _menuSelection = "";
    $("#dialog").dialog(dialogOptions);

    $("#dialog").dialog("open");
}

function dialogSelect() {
    _menuSelection = $("#dialogOptions").val();
    if (_menuSelection.length > 0) {
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
    
    // TODO: See if setTimeout is still needed here
    window.setTimeout(async function () {
        await WebPlayer.sendCommand(text, getTickCountAndStopTimer(), metadata);
    }, 100);
}

function ASLEvent(event, parameter) {
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

function addExternalScript(url) {
    return new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = url;
        script.onload = () => resolve();
        script.onerror = () => reject(new Error(`Failed to load script: ${url}`));
        document.head.appendChild(script);
    });
}

function WriteToLog(data){
    // Do nothing.
  }
  
function WriteToTranscript(data){
    if (noTranscript){
      // Do nothing.
      return;
    }
    if (!isLocalStorageAvailable()){
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
function isLocalStorageAvailable(){
    var test = 'test';
    try {
        localStorage.setItem(test, test);
        localStorage.removeItem(test);
        return true;
    } catch(e) {
        return false;
    }
}