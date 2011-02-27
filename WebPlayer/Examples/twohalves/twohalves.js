$("#txtCommand").hide();

function inputKeydown(id, event) {
    if (keyPressCode(event) == 13) {
        var text = (id == "mind1") ? $("#mind1").val() : $("#mind2").val();
        $("#mind1").val("");
        $("#mind2").val("");
        addNewRow();
        ASLEvent("ProcessInput", id + ";" + text);
    }
}

var currentSide = 0;
var currentLeft = null;
var currentRight = null;

// using the proxy pattern http://docs.jquery.com/Types#Proxy%5FPattern to override the addText function

(function () {
    var proxied = window.addText;
    window.addText = function (text) {
        if (currentSide == 0) {
            // We can access the original addText function like this. This is required so we can add twohalves.htm
            // to the output window in the first place!
            return proxied(text);
        }
        else {
            if (currentLeft == null) {
                addNewRow();
            }
            if (currentSide == 1) {
                currentLeft.append(text);
            }
            else {
                currentRight.append(text);
            }
            scrollToEnd();
        }
    };
})();

function setCurrentSide(side) {
    currentSide = side;
}

var rowCount = 0;

function addNewRow() {
    rowCount++;
    var output = "<tr><td class=\"l\" id=\"left" + rowCount + "\"></td><td class=\"r\" id=\"right" + rowCount + "\"></td></tr>";
    $("#splitTable tr:last").after(output);
    currentLeft = $("#left" + rowCount);
    currentRight = $("#right" + rowCount);
}