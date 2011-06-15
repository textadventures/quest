var textFrame = null;

function beginUsingTextFrame() {
    textFrame = $("#divText");
    setFrameHeight();
    $("body").css("overflow", "hidden");
    
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
    textFrame.height($(window).height() - textFrame.offset().top - 6);
}