var selectSizeWithoutStatus = 8;
var selectSizeWithStatus = 6;
var numCommands = 0;
var thisCommand = 0;
var commandsList = new Array();

function init() {
    $("#jquery_jplayer").jPlayer({ supplied: "wav, mp3" });
    $("#placeVerbs").hide();
    showStatusVisible(false);
}

function showStatusVisible(visible) {
    if (visible) {
        $("#statusVars").show();
        $("#lstInventory").attr("size", selectSizeWithStatus);
        $("#lstPlacesObjects").attr("size", selectSizeWithStatus);
    }
    else {
        $("#statusVars").hide();
        $("#lstInventory").attr("size", selectSizeWithoutStatus);
        $("#lstPlacesObjects").attr("size", selectSizeWithoutStatus);
    }
}

var _currentDiv = null;

function addText(text) {
    if (_currentDiv == null) {
        createNewDiv("left");
    }

    _currentDiv.append(text);
    scrollToEnd();
}

var _divCount = 0;

function createNewDiv(alignment) {
    _divCount++;
    jQuery("<div/>", {
        id: "divOutputAlign" + _divCount,
        style: "text-align: " + alignment
    }).appendTo("#divOutput");
    _currentDiv = $("#divOutputAlign" + _divCount);
}

function scrollToEnd() {
    // Use this line instead for non-pane layout
    // $('html, body').animate({ scrollTop: $(document).height() }, 10);

    $("#divOutput").scrollTop($("#divOutput").attr("scrollHeight"));
}

function updateLocation(text) {
    $("#location").html(text);
}

function setGameName(text) {
    $("#gameTitle").html(text);
}

function clearScreen() {
    $("#divOutput").html("");
    createNewDiv("left");
}

var _waitMode = false;

function beginWait() {
    _waitMode = true;
    $("#endWaitLink").show();
    $("#txtCommand").hide();
}

function endWait() {
    _waitMode = false;
    $("#endWaitLink").hide();
    $("#txtCommand").show();
    $("#fldUIMsg").val("endwait");
    $("#cmdSubmit").click();
}

function enterCommand(command) {
    $("#fldCommand").val(command);
    $("#cmdSubmit").click();
}

function keyPressCode(e) {
    var keynum
    if (window.event) {
        keynum = e.keyCode
    } else if (e.which) {
        keynum = e.which
    }
    return keynum;
}

function globalKey(e) {
    if (_waitMode) {
        endWait();
        return;
    }
}

function commandKey(e) {
    switch (keyPressCode(e)) {
        case 13:
            runCommand();
            break;
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
    numCommands++;
    commandsList[numCommands] = command;
    thisCommand = numCommands + 1;

    // hitting Enter automatically causes the form to be submitted
    $("#fldCommand").val(command);
    $("#txtCommand").val("");
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
    $("#fldUIMsg").val("msgbox " + text);
    $("#cmdSubmit").click();
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
        $("#fldUIMsg").val("choice " + _menuSelection);
        $("#cmdSubmit").click();
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
    $("#fldUIMsg").val("choicecancel");
    $("#cmdSubmit").click();
}

function sessionTimeout() {
    $("#txtCommand").hide();
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

function playAudio(filename, format, sync, looped) {
    _currentPlayer = "jplayer";

    $("#jquery_jplayer").unbind($.jPlayer.event.ended);

    if (looped) {
        // TO DO: This works in Firefox. In Chrome the event does fire but the audio doesn't restart.
        endFunction = function () { $("#jquery_jplayer").jPlayer("play"); };
    }
    else if (sync) {
        endFunction = function () { finishSync(); };
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

function finishSync() {
    $("#fldUIMsg").val("endwait");
    $("#cmdSubmit").click();
}

function panesVisible(visible) {
    if (visible) {
        $("#gamePanes").show();
    }
    else {
        $("#gamePanes").hide();
    }
}

var _places;

function updateList(listName, listData) {
    // TO DO: We currently discard the verb data we receive - this is fine for v4.x games
    // and earlier, but this will need to be implemented for v5 games. Currently we're using
    // a hacky _places array to store the list of objects which are actually places (and which
    // therefore need the "go to" button to be displayed)

    var listElement = "";

    if (listName == "inventory") listElement = "#lstInventory";
    if (listName == "placesobjects") {
        listElement = "#lstPlacesObjects";
        _places = new Array();
    }

    $(listElement).empty();
    $.each(listData, function (key, value) {
        var splitString = value.split(":");
        var objectDisplayName = splitString[0];
        var objectVerbs = splitString[1];
        $(listElement).append(
            $("<option/>").attr("value", key).text(objectDisplayName)
        );
        if (value.indexOf("Go to") != -1) {
            _places.push(key);
        }
    });
}

function updateCompass(listData) {
    var directions = listData.split("/");
    updateDir(directions, "NW", "northwest");
    updateDir(directions, "N", "north");
    updateDir(directions, "NE", "northeast");
    updateDir(directions, "W", "west");
    updateDir(directions, "Out", "out");
    updateDir(directions, "E", "east");
    updateDir(directions, "SW", "southwest");
    updateDir(directions, "S", "south");
    updateDir(directions, "SE", "southeast");
    updateDir(directions, "U", "up");
    updateDir(directions, "D", "down");
}

function updateDir(directions, label, dir) {
    $("#cmdCompass" + label).attr("disabled", $.inArray(dir, directions) == -1);
}

function paneButtonClick(target, verb) {
    var selectedObject = $(target + " option:selected").text();
    if (selectedObject.length > 0) {
        sendCommand(verb + " " + selectedObject);
    }
}

function compassClick(direction) {
    sendCommand(direction);
}

function sendCommand(text) {
    $("#fldCommand").val(text);
    $("#cmdSubmit").click();
}

function updateVerbs() {
    var selectedObject = $("#lstPlacesObjects").val();
    if (selectedObject.length > 0) {
        var showGoTo = ($.inArray(selectedObject, _places) != -1);
        if (showGoTo) {
            $("#placeVerbs").show();
            $("#objectVerbs").hide();
        }
        else {
            $("#objectVerbs").show();
            $("#placeVerbs").hide();
        }
    }
}

function updateStatus(text) {
    if (text.length > 0) {
        showStatusVisible(true);
        $("#statusVars").html(text);
    }
    else {
        showStatusVisible(false);
    }
}

function setBackground(col) {
    $("#divOutput").css("background-color", col);
}