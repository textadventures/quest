var $_GET = {};
var webPlayer = true;
var tmrTick = null;
var tickCount = 0;
var sendNextGameTickerAfter = 0;
var canSendCommand = true;
var apiRoot = "https://textadventures.co.uk/";
var outputBufferId;
var gameSessionLogData;

$(() => {
    canSendCommand = true;
    
    // TODO: Play.aspx used this:
    // init('<%= ApiRoot() %>', '<%= GameSessionLogId() %>')
    
    init(null, null);
});

document.location.search.replace(/\??(?:([^=]+)=([^&]*)&?)/g, function () {
    function decode(s) {
        return decodeURIComponent(s.split("+").join(" "));
    }

    $_GET[decode(arguments[1])] = decode(arguments[2]);
});

function init(url, gameSessionLogId) {
    apiRoot = url;
    $("#jquery_jplayer").jPlayer({ supplied: "wav, mp3" });

    // TODO: Actually implement this properly
    // // TODO: Temporarily always showing Save button here - need to work out where the game gets
    // // saved (localStorage?), and if we implement server-side saving below
    // $("#cmdSave").show();

    if (apiRoot) {
        $.ajax({
            url: apiRoot + "games/cansave",
            success: function (result) {
                if (result) {
                    $("#cmdSave").show();
                }
            },
            xhrFields: {
                withCredentials: true
            }
        });   
    }

    if (gameSessionLogId) {
        $.ajax({
            url: apiRoot + "games/startsession/?gameId=" + $_GET["id"] + "&blobId=" + gameSessionLogId,
            success: function (result) {
                if (result) {
                    gameSessionLogData = result;
                    setUpSessionLog();
                }
            },
            type: "POST",
            xhrFields: {
                withCredentials: true
            }
        });
    }
}

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
        // TODO: Check metadata format
        await WebPlayer.sendCommand(text, getTickCountAndStopTimer(), {
            command: text,
            metadata
        });
    }, 100);
}

function ASLEvent(event, parameter) {
    if (!canSendCommand) return false;
    canSendCommand = false;
    // using setTimeout here to work around a double-submission race condition which seems to only affect Firefox,
    // even though we use "return false" to suppress submission of the form with the Enter key.
    window.setTimeout(async function () {
        await WebPlayer.uiSendEvent(event, parameter);
    }, 100);
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
    window.setTimeout(async function () {
        const saveData = $("#divOutput").html();
        await WebPlayer.uiSaveGame(saveData);
    }, 100);
}

function saveGameResponse(data) {
    addText("Saving game...<br/>");
    if (apiRoot) {
        $.ajax({
            url: apiRoot + "games/save/?id=" + $_GET["id"],
            success: function (result) {
                if (result.Success) {
                    addText("Game saved successfully.<br/>");
                } else {
                    addText("Failed to save game: " + result.Reason + "<br/>");
                }
            },
            error: function (xhr, status, err) {
                console.log(status);
                console.log(err);
                addText("Failed to save game.<br/>");
            },
            xhrFields: {
                withCredentials: true
            },
            type: "POST",
            data: {
                data: data
            }
        });
    }
    else {
        console.log("TODO: Save game", data);
    }
}

function addExternalScript(url) {
    const script = document.createElement("script");
    script.src = url;
    document.head.appendChild(script);
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