var $_GET = {};
var webPlayer = true;
var tmrTick = null;
var tickCount = 0;
var sendNextGameTickerAfter = 0;
var canSendCommand = true;
var apiRoot = "https://textadventures.co.uk/";
var outputBufferId;
var gameSessionLogData;

function pageLoad() {
    // triggered by ASP.NET every page load, including from the AJAX UpdatePanel
    canSendCommand = true;
}

document.location.search.replace(/\??(?:([^=]+)=([^&]*)&?)/g, function () {
    function decode(s) {
        return decodeURIComponent(s.split("+").join(" "));
    }

    $_GET[decode(arguments[1])] = decode(arguments[2]);
});

function init(url, gameSessionLogId) {
    apiRoot = url;
    $("#jquery_jplayer").jPlayer({ supplied: "wav, mp3" });
    setInterval(keepSessionAlive, 60000);

    if ($_GET["id"].substr(0, 7) === "editor/") return;

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

function keepSessionAlive() {
    $.post("KeepAlive.ashx");
}

var _waitingForSoundToFinish = false;

function msgboxSubmit(text) {
    $("#msgbox").dialog("close");
    window.setTimeout(function () {
        $("#fldUIMsg").val("msgbox " + text);
        $("#cmdSubmit").click();
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
            $("#fldUIMsg").val("choice " + _menuSelection);
            $("#cmdSubmit").click();
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
    window.setTimeout(function () {
        $("#fldUIMsg").val("choicecancel");
        $("#cmdSubmit").click();
    }, 100);
}

function sendCommand(text, metadata) {
    if (_pauseMode || _waitingForSoundToFinish || _waitMode || !canSendCommand) return;
    canSendCommand = false;
    markScrollPosition();
    if (typeof text == "string" && text.toLowerCase().trim() == "save") {
      //console.log("player.js: sendCommand: Bypassing the save command and emulating a Save button press!");
      saveGame();
      afterSave();
    }
    else {
      window.setTimeout(function () {
          $("#fldUITickCount").val(getTickCountAndStopTimer());
          var data = new Object();
          data["command"] = text;
          if (typeof metadata != "undefined") {
              data["metadata"] = metadata;
          }
          $("#fldUIMsg").val("command " + JSON.stringify(data));
          $("#cmdSubmit").click();
      }, 100);
    }
    afterSendCommand();
}

function ASLEvent(event, parameter) {
    if (!canSendCommand) return false;
    canSendCommand = false;
    // using setTimeout here to work around a double-submission race condition which seems to only affect Firefox,
    // even though we use "return false" to suppress submission of the form with the Enter key.
    window.setTimeout(function () {
        $("#fldUIMsg").val("event " + event + ";" + parameter);
        $("#cmdSubmit").click();
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
    tmrTick = setInterval(function () {
        timerTick();
    }, 1000);
}

function stopTimer() {
    if (!_timerRunning) return;
    _timerRunning = false;
    clearInterval(tmrTick);
}

var _submittingTimerTick = false;

function timerTick() {
    tickCount++;
    if (sendNextGameTickerAfter > 0 && tickCount >= sendNextGameTickerAfter) {
        $("#fldUITickCount").val(getTickCountAndStopTimer());
        $("#fldUIMsg").val("tick");
        _submittingTimerTick = true;
        $("#cmdSubmit").click();
        _submittingTimerTick = false;
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
    window.setTimeout(function () {
        var saveData = $("#divOutput").html().replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        $("#fldUIMsg").val("save " + saveData);
        $("#cmdSubmit").click();
    }, 100);
}

function saveGameResponse(data) {
    addText("Saving game...<br/>");
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

// Added by KV  
/**
  * Writes data to the transcript's item in localStorage.
  *
  * @param {string} text This is added to the transcriptData item in localStorage
*/
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
  var overwrite = false;
  if (data.indexOf("@@@OVERWRITEFILE@@@") > -1){
    overwrite = true;
    data = data.replace("@@@OVERWRITEFILE@@@", "");
  }
  if (data.indexOf("___SCRIPTDATA___") > -1) {
    tName = data.split("___SCRIPTDATA___")[0].trim() || tName;
    data = data.split("___SCRIPTDATA___")[1];
  }
  var oldData = "";
  if (!overwrite){
    oldData = localStorage.getItem("questtranscript-" + tName) || "";
  }
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


// NOTE:
// There is a Quest game on the site to handle viewing of transcipts for all online users:
// https://play.textadventures.co.uk/Play.aspx?id=4wqdac8qd0sf7-ilff8mia



// END OF TRANSCRIPT FUNCTIONS

/**
  * Adding this to this file because it exists in desktopplayer.js
  *
  * It is doing nothing here if called, but it is here just so it is defined.
  *
  * @param {data} text This would print to the log file if this were the desktop player. It is ignored here.
*/
function WriteToLog(data){
  /* Do nothing at all. */
  return;
}
