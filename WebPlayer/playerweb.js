function panesVisible(visible) {
    if (visible) {
        $("#gamePanes").show();
        $("#gameContent").css("width", "700px");
        $("#txtCommand").css("width", "680px");
        $("#updating").css("margin-left", "185px");
        $("#gamePanel").css("width", "700px");
    }
    else {
        $("#gamePanes").hide();
        $("#gameContent").css("width", "910px");
        $("#txtCommand").css("width", "890px");
        $("#updating").css("margin-left", "405px");
        $("#gamePanel").css("width", "910px");
    }
}

function scrollToEnd() {
    $('html, body').animate({ scrollTop: beginningOfCurrentTurnScrollPosition - 50 - $("#gamePanel").height() }, 200);
}

function setBackground(col) {
    $("#gameBorder").css("background-color", col);
    $("#txtCommandDiv").css("background-color", col);
    $("#gamePanel").css("background-color", col);
}

function setPanelHeight() {
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
}

function beginWait() {
    _waitMode = true;
    $("#txtCommand").fadeTo(400, 0, function () {
        $("#endWaitLink").fadeTo(400, 1);
    });
    markScrollPosition();
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