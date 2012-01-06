var $_GET = {};
var numCommands = 0;
var thisCommand = 0;
var commandsList = new Array();
var webPlayer = true;
var tmrTick = null;
var tickCount = 0;
var sendNextGameTickerAfter = 0;
var inventoryVerbs = null;
var placesObjectsVerbs = null;
var verbButtonCount = 9;
var beginningOfCurrentTurnScrollPosition = 0;

document.location.search.replace(/\??(?:([^=]+)=([^&]*)&?)/g, function () {
    function decode(s) {
        return decodeURIComponent(s.split("+").join(" "));
    }

    $_GET[decode(arguments[1])] = decode(arguments[2]);
});

function init() {
    $("button").button();
    $("#gamePanesRunning").multiOpenAccordion({ active: [0, 1, 2, 3] });
    $("#jquery_jplayer").jPlayer({ supplied: "wav, mp3" });
    showStatusVisible(false);

    $("#cmdSave").click(function () {
        saveGame();
        afterSave();
    });

    ui_init();
}

function showStatusVisible(visible) {
    if (visible) {
        $("#statusVars").show();
        $("#statusVarsAccordion").show();
        $("#statusVarsLabel").show();
    }
    else {
        $("#statusVars").hide();
        $("#statusVarsAccordion").hide();
        $("#statusVarsLabel").hide();
    }
}

function markScrollPosition() {
    beginningOfCurrentTurnScrollPosition = $("#gameContent").height();
}

function setGameName(text) {
    $("#gameTitle").hide();
    document.title = text;
}

var _waitMode = false;
var _pauseMode = false;
var _waitingForSoundToFinish = false;

function beginPause(ms) {
    _pauseMode = true;
    $("#txtCommandDiv").hide();
    window.setTimeout(function () {
        endPause()
    }, ms);
}

function endPause() {
    _pauseMode = false;
    $("#txtCommandDiv").show();
    window.setTimeout(function () {
        $("#fldUIMsg").val("endpause");
        $("#cmdSubmit").click();
    }, 100);
}

function globalKey(e) {
    if (_waitMode) {
        endWait();
        return;
    }
}

function commandKey(e) {
    if (_waitMode) return false;
    switch (keyPressCode(e)) {
        case 13:
            runCommand();
            return false;
        case 38:
            thisCommand--;
            if (thisCommand == 0) thisCommand = numCommands;
            $("#txtCommand").val(commandsList[thisCommand]);
            break;
        case 40:
            thisCommand++;
            if (thisCommand > numCommands) thisCommand = 1;
            $("#txtCommand").val(commandsList[thisCommand]);
            break;
        case 27:
            thisCommand = numCommands + 1;
            $("#txtCommand").val("");
            break;
    }
}

function runCommand() {
    var command = $("#txtCommand").val();
    if (command.length > 0) {
        numCommands++;
        commandsList[numCommands] = command;
        thisCommand = numCommands + 1;
        sendCommand(command);
        $("#txtCommand").val("");
    }
}

function prepareCommand(command) {
    $("#fldUITickCount").val(getTickCountAndStopTimer());
    $("#fldUIMsg").val("command " + command);
}

function showQuestion(title) {
    $("#msgboxCaption").html(title);

    var msgboxOptions = {
        modal: true,
        autoOpen: false,
        buttons: [
            {
                text: "Yes",
                click: function () { msgboxSubmit("yes"); }
            },
            {
                text: "No",
                click: function () { msgboxSubmit("no"); }
            }
        ],
        closeOnEscape: false,
        open: function (event, ui) { $(".ui-dialog-titlebar-close").hide(); }    // suppresses "close" button
    };

    $("#msgbox").dialog(msgboxOptions);
    $("#msgbox").dialog("open");
}

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

function uiShow(element) {
    if (element == "#gamePanes") {
        panesVisible(true);
    }
    else {
        $(element).show();
    }
}

function uiHide(element) {
    if (element == "#gamePanes") {
        panesVisible(false);
    }
    else {
        $(element).hide();
    }
}

var _compassDirs = ["northwest", "north", "northeast", "west", "east", "southwest", "south", "southeast", "up", "down", "in", "out"];

function updateCompass(listData) {
    var directions = listData.split("/");
    updateDir(directions, "NW", _compassDirs[0]);
    updateDir(directions, "N", _compassDirs[1]);
    updateDir(directions, "NE", _compassDirs[2]);
    updateDir(directions, "W", _compassDirs[3]);
    updateDir(directions, "E", _compassDirs[4]);
    updateDir(directions, "SW", _compassDirs[5]);
    updateDir(directions, "S", _compassDirs[6]);
    updateDir(directions, "SE", _compassDirs[7]);
    updateDir(directions, "U", _compassDirs[8]);
    updateDir(directions, "D", _compassDirs[9]);
    updateDir(directions, "In", _compassDirs[10]);
    updateDir(directions, "Out", _compassDirs[11]);
}

function updateDir(directions, label, dir) {
    if ($.inArray(dir, directions) == -1) {
        $("#cmdCompass" + label).button("disable");
    }
    else {
        $("#cmdCompass" + label).button("enable");
    }
}

function paneButtonClick(target, verb) {
    var selectedObject = $(target + " option:selected").text();
    if (selectedObject.length > 0) {
        var cmd = verb + " " + selectedObject;
        sendCommand(cmd.toLowerCase());
    }
}

function compassClick(direction) {
    sendCommand(direction);
}

function sendCommand(text) {
    if (_pauseMode || _waitingForSoundToFinish || _waitMode) return;
    markScrollPosition();
    window.setTimeout(function () {
        prepareCommand(text);
        $("#cmdSubmit").click();
    }, 100);
    afterSendCommand();
}

function updateStatus(text) {
    if (text.length > 0) {
        showStatusVisible(true);
        $("#statusVars").html(text.replace(/\n/g, "<br/>"));
    }
    else {
        showStatusVisible(false);
    }
}

function setForeground(col) {
    $("#txtCommandPrompt").css("color", col);
}

function ASLEvent(event, parameter) {
    // using setTimeout here to work around a double-submission race condition which seems to only affect Firefox,
    // even though we use "return false" to suppress submission of the form with the Enter key.
    window.setTimeout(function () {
        $("#fldUIMsg").val("event " + event + ";" + parameter);
        $("#cmdSubmit").click();
    }, 100);
}

function disableMainScrollbar() {
    $("#divOutput").css("overflow", "hidden");
}

function startTimer() {
    tickCount = 0;
    tmrTick = setInterval(function () {
        timerTick();
    }, 1000);
}

function stopTimer() {
    clearInterval(tmrTick);
}

function timerTick() {
    tickCount++;
    if (sendNextGameTickerAfter > 0 && tickCount >= sendNextGameTickerAfter) {
        $("#fldUITickCount").val(getTickCountAndStopTimer());
        $("#fldUIMsg").val("tick");
        $("#cmdSubmit").click();
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

function setCompassDirections(directions) {
    _compassDirs = directions;
    $("#cmdCompassNW").attr("title", _compassDirs[0]);
    $("#cmdCompassN").attr("title", _compassDirs[1]);
    $("#cmdCompassNE").attr("title", _compassDirs[2]);
    $("#cmdCompassW").attr("title", _compassDirs[3]);
    $("#cmdCompassE").attr("title", _compassDirs[4]);
    $("#cmdCompassSW").attr("title", _compassDirs[5]);
    $("#cmdCompassS").attr("title", _compassDirs[6]);
    $("#cmdCompassSE").attr("title", _compassDirs[7]);
    $("#cmdCompassU").attr("title", _compassDirs[8]);
    $("#cmdCompassD").attr("title", _compassDirs[9]);
    $("#cmdCompassIn").attr("title", _compassDirs[10]);
    $("#cmdCompassOut").attr("title", _compassDirs[11]);
}

function saveGame() {
    window.setTimeout(function () {
        $("#fldUIMsg").val("save");
        $("#cmdSubmit").click();
    }, 100);
}