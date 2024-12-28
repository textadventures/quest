var webPlayer = false;
var canSendCommand = true;

(async function () {
    await CefSharp.BindObjectAsync("questCefInterop");
})();

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

// SaveTranscript added by KV to write/append to GAMENAME-transcript.html in Documents\Quest Transcripts
function SaveTranscript(data) {
    data = data + "<style>*{color:black !important;background:white !important;text-align:left !important}</style>";
    if (!webPlayer && transcriptString != '') { UIEvent("SaveTranscript", data); }
    transcriptString += data;
}

// Write/append to GAMENAME-log.txt in Documents\Quest Logs
function WriteToLog(data) {
    if (typeof (data) != 'string') {
        data = "[" + typeof(data) + "]";
    }
    UIEvent("WriteToLog", getTimeAndDateForLog() + " " + data);
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
