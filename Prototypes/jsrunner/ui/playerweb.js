$(function () {
    var width = $_GET["w"];
    if (width) {
        setGameWidth(width);
    }
});

function ui_init() {
}

function sendEndWait() {
    setTimeout(function () {
        quest.endWait();
        waitEnded();
    }, 1);
}

function sessionTimeout() {
    disableInterface();
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
        endFunction = function () { finishSync($("#txtCommandDiv").is(":visible")); };
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
    window.setTimeout(function () {
        if (showCommandDiv) $("#txtCommandDiv").show();
        $("#fldUIMsg").val("endwait");
        $("#cmdSubmit").click();
    }, 100);
}