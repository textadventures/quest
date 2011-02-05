function init() {
    $("#jquery_jplayer").jPlayer( { supplied: "wav, mp3" } );
}

function addText(text) {
    $("#divOutput").append(text);
    scrollToEnd();
}

function scrollToEnd() {
    $("#divOutput").scrollTop($("#divOutput").attr("scrollHeight"));
}

function updateLocation(text) {
    $("#location").html("<b>" + text + "</b>");
}

function setGameName(text) {
    $("#gameTitle").html(text);
}

function clearScreen() {
    $("#divOutput").html("");
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
    if (window.event) { // IE
        keynum = e.keyCode
    } else if (e.which) { // Netscape/Firefox/Opera
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
        //                case 38:  
        //                    thisCommand--;  
        //                    if (thisCommand == 0) thisCommand = numCommands;  
        //                    command.value = commandsList[thisCommand];  
        //                    break;  
        //                case 40:  
        //                    thisCommand++;  
        //                    if (thisCommand > numCommands) thisCommand = 1;  
        //                    command.value = commandsList[thisCommand];  
        //                    break;  
        //                case 27:  
        //                    thisCommand = numCommands + 1;  
        //                    command.value = '';  
        //                    break;  
    }
}

function runCommand() {
    //numCommands++;
    //commandsList[numCommands] = command.value;
    //thisCommand = numCommands + 1;

    // hitting Enter automatically causes the form to be submitted
    $("#fldCommand").val($("#txtCommand").val());
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

function playWav(filename) {
    if (!document.createElement('audio').canPlayType) {
        // no <audio> support, so we must play WAVs using <embed> as the 
        // jPlayer Flash fallback does not support WAV.
        $("#audio_embed").html("<embed src=\"" + filename + "\" autostart=\"true\" width=\"0\" height=\"0\" type=\"audio/wav\">");
        _currentPlayer = "embed";
    }
    else {
        playAudio(filename, "wav");
    }
}

function playMp3(filename) {
    playAudio(filename, "mp3");
}

function playAudio(filename, format) {
    _currentPlayer = "jplayer";
    $("#jquery_jplayer").bind($.jPlayer.event.error, function (event) { alert(event.jPlayer.error.type); });
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