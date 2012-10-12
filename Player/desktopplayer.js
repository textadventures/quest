var webPlayer = false;

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

function goUrl(href) {
    UIEvent("GoURL", href);
}

function sendEndWait() {
    UIEvent("EndWait", "");
}

function UIEvent(cmd, parameter) {
    questCefInterop.UIEvent(cmd, parameter);
}

function SetBackground(color) {
    document.body.style.background = color;
    $("#gamePanel").css("background-color", color);
    $("#gridPanel").css("background-color", color);
}

function disableMainScrollbar() {
    $("body").css("overflow", "hidden");
}

function ui_init() {
    $("#gameTitle").hide();
    $("#cmdSave").hide();
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
}

function panesVisibleEval(visible) {
    panesVisible(eval(visible));
}