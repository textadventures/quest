var textFrame = null;
var topFrame = null;

function beginUsingTextFrame() {
    textFrame = $("#divText");
    topFrame = $("#divFrame");
    clearFramePicture();
    setFrameHeight();
    disableMainScrollbar();

    $(window).resize(function () {
        setFrameHeight();
    });

    window.scrollToEnd = function () {
        textFrame.scrollTop(textFrame.attr("scrollHeight"));
    }

    window.clearScreen = function () {
        textFrame.html("");
    }

    window.createNewDiv = function (alignment) {
        _divCount++;
        textFrame.append(
            $("<div/>", {
                id: "divOutputAlign" + _divCount,
                style: "text-align: " + alignment
            })
        );
        _currentDiv = $("#divOutputAlign" + _divCount);
    }

    createNewDiv("left");
}

function setFrameHeight() {
    if (webPlayer) {
        setTimeout(function () {
            if (topFrame.is(":visible")) {
                textFrame.height($("#divOutput").height() - topFrame.height() - 6);
            }
            else {
                textFrame.height($("#divOutput").height() - 6);
            }
            scrollToEnd();
        }, 100);
    }
    else {
        if (topFrame.is(":visible")) {
            textFrame.height($(window).height() - topFrame.position().top - topFrame.height() - 6);
        }
        else {
            textFrame.height($(window).height() - topFrame.position().top - 22);
        }
        scrollToEnd();
    }
}

function setFramePicture(filename) {
    topFrame.show();
    topFrame.html("<img src=\"" + filename + "\" onload=\"setFrameHeight()\"/>");
}

function clearFramePicture(filename) {
    topFrame.hide();
    setFrameHeight();
}