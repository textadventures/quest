var $_GET = {};
var webPlayer = true;
var tmrTick = null;
var tickCount = 0;
var sendNextGameTickerAfter = 0;
var canSendCommand = true;
var apiRoot = "http://textadventures.co.uk/";
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

function msgboxSubmit(text) {
    $("#msgbox").dialog("close");
    quest.setQuestionResponse(text);
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
        quest.setMenuResponse(_menuSelection);
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
    var tickCount = getTickCountAndStopTimer();
    quest.sendCommand(text, tickCount, metadata);
    canSendCommand = true;
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

function timerTick() {
    tickCount++;
    if (sendNextGameTickerAfter > 0 && tickCount >= sendNextGameTickerAfter) {
        quest.tick(getTickCountAndStopTimer());
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
        var saveData = $("#divOutput").html();
        quest.save(saveData, function (data) {
            saveGameResponse(btoa(data));
        });
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