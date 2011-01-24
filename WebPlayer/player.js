function addText(text) {
    $("#divOutput").html($("#divOutput").html() + text);

    // TO DO: Add text within its own <div> with its own id, then use
    // jQuery's .position() to get actual position of latest text, and use
    // that value for scrollTop.
    $("#divOutput").scrollTop($("#divOutput").scrollTop() + 500);
}

function updateLocation(text) {
    $("#location").html("<b>" + text + "</b>");
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