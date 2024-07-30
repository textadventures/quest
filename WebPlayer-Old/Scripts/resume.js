var $_GET = {};
var apiRoot = "http://textadventures.co.uk/";

document.location.search.replace(/\??(?:([^=]+)=([^&]*)&?)/g, function () {
    function decode(s) {
        return decodeURIComponent(s.split("+").join(" "));
    }

    $_GET[decode(arguments[1])] = decode(arguments[2]);
});

$(function () {
    var id = $_GET["id"];
    $.ajax({
        url: apiRoot + "games/load/" + id,
        success: function(result) {
            $.post("/Resume", result, function() {
                window.location = "/Play.aspx?id=" + id;
            });
        },
        error: function() {
            $("#status").text("Failed to load game");
        },
        xhrFields: {
            withCredentials: true
        }
    });
});