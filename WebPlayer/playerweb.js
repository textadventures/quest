$(function () {
    var width = $_GET["w"];
    if (width) {
        $("#gameBorder").width(width);
        $("#status").width(width - 2);
        $("#gamePanel").css("margin-left", "-" + (width / 2 - 19) + "px");
        $("#gridPanel").css("margin-left", "-" + (width / 2 - 19) + "px");
        $("#gamePanes").css("margin-left", (width / 2 - 220) + "px");
    }
});

function panesVisible(visible) {
    var screenWidth = $("#gameBorder").width();

    if (visible) {    
        $("#gamePanes").show();
        $("#gameContent").width(screenWidth - 250);
        $("#txtCommand").width(screenWidth - 270);
        $("#updating").css("margin-left", (screenWidth / 2 - 290) + "px");
        $("#gamePanel").width(screenWidth - 250);
        $("#gridPanel").width(screenWidth - 250);
    }
    else {
        $("#gamePanes").hide();
        $("#gameContent").width(screenWidth - 40);
        $("#txtCommand").width(screenWidth - 60);
        $("#updating").css("margin-left", (screenWidth / 2 - 70) + "px");
        $("#gamePanel").width(screenWidth - 40);
        $("#gridPanel").width(screenWidth - 40);
    }
}

function scrollToEnd() {
    $('html, body').animate({ scrollTop: beginningOfCurrentTurnScrollPosition - 50 - $("#gamePanelSpacer").height() }, 200);
}

function setBackground(col) {
    $("#gameBorder").css("background-color", col);
    $("#txtCommandDiv").css("background-color", col);
    $("#gamePanel").css("background-color", col);
    $("#gridPanel").css("background-color", col);
}

function setPanelHeight() {
    if (_showGrid) return;
    setTimeout(function () {
        $("#gamePanelSpacer").height($("#gamePanel").height());
        scrollToEnd();
    }, 100);
}

function setPanelContents(html) {
    $("#gamePanel").html(html);
    setPanelHeight();
}

function ui_init() {
    $("#lstInventory").change(function () {
        updateVerbButtons($("#lstInventory"), inventoryVerbs, "cmdInventory");
    });

    $("#lstPlacesObjects").change(function () {
        updateVerbButtons($("#lstPlacesObjects"), placesObjectsVerbs, "cmdPlacesObjects");
    });
}

function beginWait() {
    _waitMode = true;
    $("#txtCommand").fadeTo(400, 0, function () {
        $("#endWaitLink").fadeTo(400, 1);
    });
    markScrollPosition();
}

function endWait() {
    if (!_waitMode) return;
    _waitMode = false;
    $("#endWaitLink").fadeOut(400, function () {
        if (!_waitMode) {
            $("#txtCommand").fadeTo(400, 1);
        }
    });
    window.setTimeout(function () {
        $("#fldUIMsg").val("endwait");
        $("#cmdSubmit").click();
    }, 100);
}

function sessionTimeout() {
    disableInterface();
}

function gameFinished() {
    disableInterface();
}

function disableInterface() {
    $("#txtCommandDiv").hide();
    $("#gamePanesRunning").hide();
    $("#gamePanesFinished").show();
}

function setInterfaceString(name, text) {
    switch (name) {
        case "InventoryLabel":
            $("#inventoryLabel a").html(text);
            break;
        case "PlacesObjectsLabel":
            $("#placesObjectsLabel a").html(text);
            break;
        case "CompassLabel":
            $("#compassLabel a").html(text);
            break;
        case "InButtonLabel":
            $("#cmdCompassIn").attr("value", text);
            break;
        case "OutButtonLabel":
            $("#cmdCompassOut").attr("value", text);
            break;
        case "EmptyListLabel":
            break;
        case "NothingSelectedLabel":
            break;
    }
}

function updateLocation(text) {
    $("#location").html(text);
}

function afterSendCommand() {
}

function updateList(listName, listData) {
    var listElement = "";

    if (listName == "inventory") {
        listElement = "#lstInventory";
        inventoryVerbs = new Array();
    }

    if (listName == "placesobjects") {
        listElement = "#lstPlacesObjects";
        placesObjectsVerbs = new Array();
    }

    $(listElement).empty();
    var count = 0;
    $.each(listData, function (key, value) {
        var splitString = value.split(":");
        var objectDisplayName = splitString[0];
        var objectVerbs = splitString[1];

        if (listName == "inventory") {
            inventoryVerbs.push(objectVerbs);
        }

        if (listName == "placesobjects") {
            placesObjectsVerbs.push(objectVerbs);
        }

        if (listName == "inventory" || $.inArray(objectDisplayName, _compassDirs) == -1) {
            $(listElement).append(
                $("<option/>").attr("value", key).text(objectDisplayName)
            );
            count++;
        }
    });

    var selectSize = count;
    if (selectSize < 3) selectSize = 3;
    if (selectSize > 12) selectSize = 12;
    $(listElement).attr("size", selectSize);
}

function updateVerbButtons(list, verbsArray, idprefix) {
    var selectedIndex = list.prop("selectedIndex");
    var verbs = verbsArray[selectedIndex].split("/");
    var count = 1;
    $.each(verbs, function (index, value) {
        $("#" + idprefix + count + " span").html(value);
        var target = $("#" + idprefix + count);
        target.data("verb", value);
        target.show();
        count++;
    });
    for (var i = count; i <= verbButtonCount; i++) {
        var target = $("#" + idprefix + i);
        target.hide();
    }
}

function afterSave() {
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
        _waitingForSoundToFinish = true;
        $("#txtCommandDiv").hide();
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
    _waitingForSoundToFinish = false;
    window.setTimeout(function () {
        $("#txtCommandDiv").show();
        $("#fldUIMsg").val("endwait");
        $("#cmdSubmit").click();
    }, 100);
}