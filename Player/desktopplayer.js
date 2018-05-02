var webPlayer = false;
var canSendCommand = true;

function sendCommand(text, metadata) {
    markScrollPosition();
    var data = new Object();
    data["command"] = text;
    if (typeof metadata != "undefined") {
        data["metadata"] = metadata;
    }
    UIEvent("RunCommand", JSON.stringify(data));
}

function ASLEvent(event, parameter) {
    UIEvent("ASLEvent", event + ";" + parameter);
}


// RestartGame added by KV
function RestartGame() {
    UIEvent("RestartGame", "");
}

// SaveTranscript added by KV
function SaveTranscript(data) {
    data = data + "<style>*{color:black !important;background:white !important;text-align:left !important}</style>";
    var pre = transcriptName + "@@@TRANSCRIPTNAME@@@";
    if (!webPlayer && transcriptString != '') { UIEvent("SaveTranscript", pre + data); }
    transcriptString += data;
}

// Added by KV to write/append to log.txt in the current directory
function WriteToLog(data) {
    if (!webPlayer && data != '' && typeof (data) == 'string') {
        UIEvent("WriteToLog", gameName + "@@@GAMENAME@@@" + getTimeAndDateForLog() + " " + data);
    }
}

function goUrl(href) {
    UIEvent("GoURL", href);
}

function sendEndWait() {
    UIEvent("EndWait", "");
}

function doSave() {
    UIEvent("Save", $("#divOutput").html());
}

function UIEvent(cmd, parameter) {
    questCefInterop.uiEvent(cmd, parameter);
}

function disableMainScrollbar() {
    $("body").css("overflow", "hidden");
}

function ui_init() {
    $("#gameTitle").remove();
    $("#cmdExitFullScreen").click(function () {
        UIEvent("ExitFullScreen", "");
    });
}

function updateListEval(listName, listData) {
    updateList(listName, eval("(" + listData + ")"));
}

function showExitFullScreenButton(show) {
    if (eval(show)) {
        $("#cmdExitFullScreen").show();
    }
    else {
        $("#cmdExitFullScreen").hide();
    }
    updateStatusVisibility();
}

function panesVisibleEval(visible) {
    panesVisible(eval(visible));
}

function setCompassDirectionsEval(list) {
    setCompassDirections(eval(list));
}

function selectText(containerid) {
    var range = document.createRange();
    range.selectNodeContents(document.getElementById(containerid));
    var sel = window.getSelection();
    sel.removeAllRanges();
    sel.addRange(range);
}
