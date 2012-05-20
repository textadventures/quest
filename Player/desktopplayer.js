var webPlayer = false;

function scrollToEnd() {
    $('html, body').animate({ scrollTop: $(document).height() }, 0);
    $("#txtCommand").focus();
}

function sendCommand(text) {
    UIEvent("RunCommand", text);
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
    var elem = $("#_UIEvent");
    elem.html(cmd + " " + parameter);
    elem.click();
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