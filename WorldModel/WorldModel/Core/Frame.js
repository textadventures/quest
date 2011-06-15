var textFrame = null;

// using the proxy pattern http://docs.jquery.com/Types#Proxy%5FPattern to override the addText function

(function () {
    var proxied = window.addText;
    window.addText = function (text) {
        if (textFrame == null) {
            // We can access the original addText function like this. This is required so we can add Frame.htm
            // to the output window in the first place!
            return proxied(text);
        }
        else {
            textFrame.append(text);
            textFrame.animate({ scrollTop: textFrame.height() }, 0);
        }
    };
})();

function beginUsingTextFrame() {
    textFrame = $("#divText");
    setFrameHeight();
    $("body").css("overflow", "hidden");
    $(window).resize(function() {
        setFrameHeight();
    });
}

function setFrameHeight() {
    textFrame.height($(window).height() - textFrame.offset().top - 6);
}