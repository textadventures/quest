function panesVisible(visible) {
//    if (visible) {
//        $("#gamePanes").show();
//        $("#gameContent").css("width", "700px");
//        $("#txtCommand").css("width", "680px");
//        $("#updating").css("margin-left", "185px");
//        $("#gamePanel").css("width", "700px");
//    }
//    else {
//        $("#gamePanes").hide();
//        $("#gameContent").css("width", "910px");
//        $("#txtCommand").css("width", "890px");
//        $("#updating").css("margin-left", "405px");
//        $("#gamePanel").css("width", "910px");
//    }
}

function scrollToEnd() {
    $('html, body').animate({ scrollTop: beginningOfCurrentTurnScrollPosition - 50 }, 200);
}

function setBackground(col) {
    $("#gameBorder").css("background-color", col);
    $("#txtCommandDiv").css("background-color", col);
}

function setPanelHeight() {
}

function setPanelContents(html) {
}

var resizeTimer;

function ui_init() {
//    resizeUI();
//    $(window).resize(function () {
//        clearTimeout(resizeTimer);
//        resizeTimer = setTimeout(function () {
//            resizeUI();
//        }, 100);
//    });
//    document.addEventListener("orientationChanged", resizeUI);
}

function resizeUI() {
}