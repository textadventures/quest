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

function ui_init() {
}

function sendEndWait() {
    window.setTimeout(function () {
        $("#fldUIMsg").val("endwait");
        $("#cmdSubmit").click();
    }, 100);
    waitEnded();
}

function sessionTimeout() {
    disableInterface();
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

function afterSendCommand() {
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