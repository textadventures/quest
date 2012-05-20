var webPlayer = false;

function scrollToEnd() {
    $('html, body').animate({ scrollTop: $(document).height() }, 0);
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

function gameFinished() {
    // we don't need to do anything - this function is just required for compatibility with WebPlayer
}

function disableMainScrollbar() {
    $("body").css("overflow", "hidden");
}

function setPanelHeight() {
    if (_showGrid) return;
    setTimeout(function () {
        var height = $("#gamePanel").height();
        if ($("#gamePanel").html() == "") {
            // workaround for IE weirdness where an empty div has height
            height = 0;
            $("#gamePanel").hide();
        }
        else {
            $("#gamePanel").show();
        }
        $("#gamePanelSpacer").height(height);
        scrollToEnd();
    }, 100);
}

function setPanelContents(html) {
    $("#gamePanel").html(html);
    setPanelHeight();
}

function ui_init() {
}