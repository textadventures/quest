var $_GET = {};

document.location.search.replace(/\??(?:([^=]+)=([^&]*)&?)/g, function () {
    function decode(s) {
        return decodeURIComponent(s.split("+").join(" "));
    }

    $_GET[decode(arguments[1])] = decode(arguments[2]);
});

$(function () {
    var id = $_GET["id"];
    console.log("getting...  " + "http://textadventures.co.uk/games/load/" + id);
    $.ajax({
        url: "http://textadventures.co.uk/games/load/" + id,
        success: function(result) {
            console.log(result);
            console.log("done");
        },
        xhrFields: {
            withCredentials: true
        }
    });
});