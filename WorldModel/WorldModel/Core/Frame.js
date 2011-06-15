var textFrame = null;
var topFrame = null;

function beginUsingTextFrame() {
    textFrame = $("#divText");
    topFrame = $("#divFrame");
    setFrameHeight();
    disableMainScrollbar();
    
    $(window).resize(function() {
        setFrameHeight();
    });
    
    window.addText = function(text) {
        textFrame.append(text);
        scrollToEnd();
    }

    window.scrollToEnd = function () {
        textFrame.scrollTop(textFrame.attr("scrollHeight"));
    }
}

function setFrameHeight() {
    if (webPlayer) {
        textFrame.height($("#divOutput").height() - 6);
    }
    else {
        textFrame.height($(window).height() - topFrame.position().top - topFrame.height() - 6);
    }
}

function setFramePicture(filename) {
    topFrame.html("<img src=\"" + filename + "\" onload=\"setFrameHeight()\"/>");
}