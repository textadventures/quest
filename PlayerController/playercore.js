var _currentDiv = null;
var _allowMenuFontSizeChange = true;
var _showGrid = false;
var numCommands = 0;
var thisCommand = 0;
var commandsList = new Array();
var inventoryVerbs = null;
var placesObjectsVerbs = null;
var verbButtonCount = 9;
var beginningOfCurrentTurnScrollPosition = 0;

$(function () {
    $("button").button();
    $("#gamePanesRunning").multiOpenAccordion({ active: [0, 1, 2, 3] });
    showStatusVisible(false);

    $("#cmdSave").click(function () {
        saveGame();
        afterSave();
    });

    $("#lstInventory").change(function () {
        updateVerbButtons($("#lstInventory"), inventoryVerbs, "cmdInventory");
    });

    $("#lstPlacesObjects").change(function () {
        updateVerbButtons($("#lstPlacesObjects"), placesObjectsVerbs, "cmdPlacesObjects");
    });

    $("#cmdCompassNW").button({
        icons: { primary: "ui-icon-arrowthick-1-nw" }
    });
    $("#cmdCompassN").button({
        icons: { primary: "ui-icon-arrowthick-1-n" }
    });
    $("#cmdCompassNE").button({
        icons: { primary: "ui-icon-arrowthick-1-ne" }
    });
    $("#cmdCompassW").button({
        icons: { primary: "ui-icon-arrowthick-1-w" }
    });
    $("#cmdCompassE").button({
        icons: { primary: "ui-icon-arrowthick-1-e" }
    });
    $("#cmdCompassSW").button({
        icons: { primary: "ui-icon-arrowthick-1-sw" }
    });
    $("#cmdCompassS").button({
        icons: { primary: "ui-icon-arrowthick-1-s" }
    });
    $("#cmdCompassSE").button({
        icons: { primary: "ui-icon-arrowthick-1-se" }
    });
    $("#cmdCompassU").button({
        icons: { primary: "ui-icon-triangle-1-n" }
    });
    $("#cmdCompassD").button({
        icons: { primary: "ui-icon-triangle-1-s" }
    });
    // fix to make compass button icons centred
    $(".compassbutton span").css("left", "0.8em");

    ui_init();

    $("#txtCommand").focus();
});

function showStatusVisible(visible) {
    if (visible) {
        $("#statusVars").show();
        $("#statusVarsAccordion").show();
        $("#statusVarsLabel").show();
    }
    else {
        $("#statusVars").hide();
        $("#statusVarsAccordion").hide();
        $("#statusVarsLabel").hide();
    }
}

function markScrollPosition() {
    beginningOfCurrentTurnScrollPosition = $("#gameContent").height();
}

function setGameName(text) {
    $("#gameTitle").hide();
    document.title = text;
}

var _waitMode = false;
var _pauseMode = false;

function beginPause(ms) {
    _pauseMode = true;
    $("#txtCommandDiv").hide();
    window.setTimeout(function () {
        endPause()
    }, ms);
}

function endPause() {
    _pauseMode = false;
    $("#txtCommandDiv").show();
    window.setTimeout(function () {
        $("#fldUIMsg").val("endpause");
        $("#cmdSubmit").click();
    }, 100);
}

function globalKey(e) {
    if (_waitMode) {
        endWait();
        return;
    }
}

function commandKey(e) {
    if (_waitMode) return false;
    switch (keyPressCode(e)) {
        case 13:
            runCommand();
            return false;
        case 38:
            thisCommand--;
            if (thisCommand == 0) thisCommand = numCommands;
            $("#txtCommand").val(commandsList[thisCommand]);
            break;
        case 40:
            thisCommand++;
            if (thisCommand > numCommands) thisCommand = 1;
            $("#txtCommand").val(commandsList[thisCommand]);
            break;
        case 27:
            thisCommand = numCommands + 1;
            $("#txtCommand").val("");
            break;
    }
}

function runCommand() {
    var command = $("#txtCommand").val();
    if (command.length > 0) {
        numCommands++;
        commandsList[numCommands] = command;
        thisCommand = numCommands + 1;
        sendCommand(command);
        $("#txtCommand").val("");
    }
}

function showQuestion(title) {
    $("#msgboxCaption").html(title);

    var msgboxOptions = {
        modal: true,
        autoOpen: false,
        buttons: [
            {
                text: "Yes",
                click: function () { msgboxSubmit("yes"); }
            },
            {
                text: "No",
                click: function () { msgboxSubmit("no"); }
            }
        ],
        closeOnEscape: false,
        open: function (event, ui) { $(".ui-dialog-titlebar-close").hide(); }    // suppresses "close" button
    };

    $("#msgbox").dialog(msgboxOptions);
    $("#msgbox").dialog("open");
}

function uiShow(element) {
    if (element == "#gamePanes") {
        panesVisible(true);
    }
    else {
        $(element).show();
    }
}

function uiHide(element) {
    if (element == "#gamePanes") {
        panesVisible(false);
    }
    else {
        $(element).hide();
    }
}

function panesVisible(visible) {
    var screenWidth = $("#gameBorder").width();

    if (visible) {
        $("#gamePanes").show();
        $("#gameContent").width(screenWidth - 250);
        $("#txtCommand").width(screenWidth - 270);
        $("#updating").css("margin-left", (screenWidth / 2 - 290) + "px");
        $("#gamePanel").width(screenWidth - 250);
        $("#gridPanel").width(screenWidth - 250);
    }
    else {
        $("#gamePanes").hide();
        $("#gameContent").width(screenWidth - 40);
        $("#txtCommand").width(screenWidth - 60);
        $("#updating").css("margin-left", (screenWidth / 2 - 70) + "px");
        $("#gamePanel").width(screenWidth - 40);
        $("#gridPanel").width(screenWidth - 40);
    }
}

function scrollToEnd() {
    $('html, body').animate({ scrollTop: beginningOfCurrentTurnScrollPosition - 50 - $("#gamePanelSpacer").height() }, 200);
}

var _backgroundOpacity = 1;

function setBackgroundOpacity(opacity) {
    _backgroundOpacity = opacity;
}

function setBackground(col) {
    colNameToHex = colourNameToHex(col);
    if (colNameToHex) col = colNameToHex;
    rgbCol = hexToRgb(col);
    var cssBackground = "rgba(" + rgbCol.r + "," + rgbCol.g + "," + rgbCol.b + "," + _backgroundOpacity + ")";
    $("#gameBorder").css("background-color", cssBackground);

    $("#txtCommandDiv").css("background-color", col);
    $("#gamePanel").css("background-color", col);
    $("#gridPanel").css("background-color", col);
}

function setPanelHeight() {
    if (_showGrid) return;
    setTimeout(function () {
        $("#gamePanelSpacer").height($("#gamePanel").height());
        scrollToEnd();
    }, 100);
}

function setPanelContents(html) {
    if (html.length > 0) {
        $("#gamePanel").show()
    }
    else {
        $("#gamePanel").hide()
    }
    $("#gamePanel").html(html);
    setPanelHeight();
}

var _compassDirs = ["northwest", "north", "northeast", "west", "east", "southwest", "south", "southeast", "up", "down", "in", "out"];

function updateCompass(listData) {
    var directions = listData.split("/");
    updateDir(directions, "NW", _compassDirs[0]);
    updateDir(directions, "N", _compassDirs[1]);
    updateDir(directions, "NE", _compassDirs[2]);
    updateDir(directions, "W", _compassDirs[3]);
    updateDir(directions, "E", _compassDirs[4]);
    updateDir(directions, "SW", _compassDirs[5]);
    updateDir(directions, "S", _compassDirs[6]);
    updateDir(directions, "SE", _compassDirs[7]);
    updateDir(directions, "U", _compassDirs[8]);
    updateDir(directions, "D", _compassDirs[9]);
    updateDir(directions, "In", _compassDirs[10]);
    updateDir(directions, "Out", _compassDirs[11]);
}

function updateDir(directions, label, dir) {
    if ($.inArray(dir, directions) == -1) {
        $("#cmdCompass" + label).button("disable");
    }
    else {
        $("#cmdCompass" + label).button("enable");
    }
}

function paneButtonClick(target, button) {
    var selectedListItem = $(target + " option:selected");
    var selectedObject = selectedListItem.text();
    var selectedElementId = selectedListItem.data("elementid");
    var verb = button.data("verb");
    var metadata = new Object();
    metadata[selectedObject] = selectedElementId;
    var metadataString = JSON.stringify(metadata);

    if (selectedObject.length > 0) {
        var cmd = verb + " " + selectedObject;
        sendCommand(cmd.toLowerCase(), metadataString);
    }
}

function compassClick(direction) {
    sendCommand(direction);
}

function updateStatus(text) {
    if (text.length > 0) {
        showStatusVisible(true);
        $("#statusVars").html(text.replace(/\n/g, "<br/>"));
    }
    else {
        showStatusVisible(false);
    }
}

function setForeground(col) {
    $("#txtCommandPrompt").css("color", col);
}

function setCompassDirections(directions) {
    _compassDirs = directions;
    $("#cmdCompassNW").attr("title", _compassDirs[0]);
    $("#cmdCompassN").attr("title", _compassDirs[1]);
    $("#cmdCompassNE").attr("title", _compassDirs[2]);
    $("#cmdCompassW").attr("title", _compassDirs[3]);
    $("#cmdCompassE").attr("title", _compassDirs[4]);
    $("#cmdCompassSW").attr("title", _compassDirs[5]);
    $("#cmdCompassS").attr("title", _compassDirs[6]);
    $("#cmdCompassSE").attr("title", _compassDirs[7]);
    $("#cmdCompassU").attr("title", _compassDirs[8]);
    $("#cmdCompassD").attr("title", _compassDirs[9]);
    $("#cmdCompassIn").attr("title", _compassDirs[10]);
    $("#cmdCompassOut").attr("title", _compassDirs[11]);
}

function updateLocation(text) {
    $("#location").html(text);
}

function updateList(listName, listData) {
    var listElement = "";

    if (listName == "inventory") {
        listElement = "#lstInventory";
        inventoryVerbs = new Array();
    }

    if (listName == "placesobjects") {
        listElement = "#lstPlacesObjects";
        placesObjectsVerbs = new Array();
    }

    $(listElement).empty();
    var count = 0;
    $.each(listData, function (key, value) {
        var data = JSON.parse(value);
        var objectDisplayName = data["Text"];

        if (listName == "inventory") {
            inventoryVerbs.push(data);
        }

        if (listName == "placesobjects") {
            placesObjectsVerbs.push(data);
        }

        if (listName == "inventory" || $.inArray(objectDisplayName, _compassDirs) == -1) {
            $(listElement).append(
                $("<option/>").attr("value", key).data("elementid", data["ElementId"]).text(objectDisplayName)
            );
            count++;
        }
    });

    var selectSize = count;
    if (selectSize < 3) selectSize = 3;
    if (selectSize > 12) selectSize = 12;
    $(listElement).attr("size", selectSize);
}

function updateVerbButtons(list, verbsArray, idprefix) {
    var selectedIndex = list.prop("selectedIndex");
    var verbs = verbsArray[selectedIndex]["Verbs"];
    var count = 1;
    $.each(verbs, function (index, value) {
        $("#" + idprefix + count + " span").html(value);
        var target = $("#" + idprefix + count);
        target.data("verb", value);
        target.show();
        count++;
    });
    for (var i = count; i <= verbButtonCount; i++) {
        var target = $("#" + idprefix + i);
        target.hide();
    }
}

function beginWait() {
    _waitMode = true;
    $("#txtCommand").fadeTo(400, 0, function () {
        $("#endWaitLink").fadeTo(400, 1);
    });
    markScrollPosition();
}

function endWait() {
    if (!_waitMode) return;
    sendEndWait();
}

function waitEnded() {
    _waitMode = false;
    $("#endWaitLink").fadeOut(400, function () {
        if (!_waitMode) {
            $("#txtCommand").fadeTo(400, 1);
        }
    });
}

function gameFinished() {
    disableInterface();
}

function disableInterface() {
    $("#txtCommandDiv").hide();
    $("#gamePanesRunning").hide();
    $("#gamePanesFinished").show();
}

function addText(text) {
    if (_currentDiv == null) {
        createNewDiv("left");
    }

    _currentDiv.append(text);
    scrollToEnd();
}

var _divCount = 0;

function createNewDiv(alignment) {
    _divCount++;
    $("<div/>", {
        id: "divOutputAlign" + _divCount,
        style: "text-align: " + alignment
    }).appendTo("#divOutput");
    _currentDiv = $("#divOutputAlign" + _divCount);
}

function bindMenu(linkid, verbs, text, elementId) {
    var verbsList = verbs.split("/");
    var options = [];
    var metadata = new Object();
    metadata[text] = elementId;
    var metadataString = JSON.stringify(metadata);

    $.each(verbsList, function (key, value) {
        options = options.concat({
            title: value,
            action: {
                type: "fn",
                callback: "doMenuClick('" + value.toLowerCase() + " " + text.replace("'", "\\'") + "','" + metadataString + "');"
            }
        });
    });

    $("#" + linkid).jjmenu("both", options, {}, { show: "fadeIn", speed: 100, xposition: "left", yposition: "auto", "orientation": "auto" });
}

function doMenuClick(command, metadata) {
    $("div[id^=jjmenu]").remove();
    sendCommand(command, metadata);
}

function clearScreen() {
    $("#divOutput").html("");
    createNewDiv("left");
}

function keyPressCode(e) {
    var keynum
    if (window.event) {
        keynum = e.keyCode
    } else if (e.which) {
        keynum = e.which
    }
    return keynum;
}

function CheckFlashAndShowMsg() {
    var hasFlash = false;
    try {
        //IE 7
        var fo = new ActiveXObject('ShockwaveFlash.ShockwaveFlash');
        if (fo)
            hasFlash = true;
    } catch (e) {
        //IE 8+, and all other modern browsers
        if (navigator.mimeTypes["application/x-shockwave-flash"] != undefined)
            hasFlash = true;
    }

    if (!hasFlash)
        addText("<b><i>You must install <a href=\"http://get.adobe.com/flashplayer/\" target=\"_blank\">Adobe Flash Player</a> for Internet Explorer for videos to work.</i></b>");

    return hasFlash;
}

function AddYouTube(id) {
    if (!CheckFlashAndShowMsg())
        return;

    var url = "http://www.youtube.com/v/" + id + "?version=3&autoplay=1";
    var embedHTML = "<object width=\"425\" height=\"344\"><param name=\"movie\" value=\"" + url + "\"></param><param name=\"allowFullScreen\" value=\"true\"></param><param name=\"allowscriptaccess\" value=\"always\"></param><param name=\"wmode\" value=\"transparent\"></param><embed wmode=\"transparent\" src=\"" + url + "\" type=\"application/x-shockwave-flash\" allowscriptaccess=\"always\" allowfullscreen=\"true\" width=\"425\" height=\"344\"></embed></object>";
    addText(embedHTML);
}

function AddVimeo(id) {
    if (!CheckFlashAndShowMsg())
        return;

    var embedHTML = "<object width=\"400\" height=\"225\"><param name=\"allowfullscreen\" value=\"true\" /><param name=\"allowscriptaccess\" value=\"always\" /><param name=\"movie\" value=\"http://vimeo.com/moogaloop.swf?clip_id=" + id + "&amp;server=vimeo.com&amp;show_title=0&amp;show_byline=0&amp;show_portrait=0&amp;color=00adef&amp;fullscreen=1&amp;autoplay=1&amp;loop=0\" /><param name=\"wmode\" value=\"transparent\"></param><embed wmode=\"transparent\" src=\"http://vimeo.com/moogaloop.swf?clip_id=" + id + "&amp;server=vimeo.com&amp;show_title=0&amp;show_byline=0&amp;show_portrait=0&amp;color=00adef&amp;fullscreen=1&amp;autoplay=1&amp;loop=0\" type=\"application/x-shockwave-flash\" allowfullscreen=\"true\" allowscriptaccess=\"always\" width=\"400\" height=\"225\"></embed></object>";
    addText(embedHTML);
}

function SetMenuBackground(color) {
    var css = getCSSRule("div.jj_menu_item");
    css.style.backgroundColor = color;
}

function SetMenuForeground(color) {
    var css = getCSSRule("div.jj_menu_item");
    css.style.color = color;
}

function SetMenuHoverBackground(color) {
    var css = getCSSRule("div.jj_menu_item_hover");
    css.style.backgroundColor = color;
}

function SetMenuHoverForeground(color) {
    var css = getCSSRule("div.jj_menu_item_hover");
    css.style.color = color;
}

function SetMenuFontName(font) {
    var css = getCSSRule("div.jjmenu");
    css.style.fontFamily = font;
}

function SetMenuFontSize(size) {
    if (_allowMenuFontSizeChange) {
        var css = getCSSRule("div.jjmenu");
        css.style.fontSize = size;
    }
}

function TurnOffHyperlinksUnderline() {
    var css = getCSSRule("a.cmdlink");
    css.style.textDecoration = "none";
}

function SetBackgroundImage(url) {
    $("body").css("background-image", "url(" + url + ")");
}

function colourNameToHex(colour) {
    var colours = { "aliceblue": "#f0f8ff", "antiquewhite": "#faebd7", "aqua": "#00ffff", "aquamarine": "#7fffd4", "azure": "#f0ffff",
        "beige": "#f5f5dc", "bisque": "#ffe4c4", "black": "#000000", "blanchedalmond": "#ffebcd", "blue": "#0000ff", "blueviolet": "#8a2be2", "brown": "#a52a2a", "burlywood": "#deb887",
        "cadetblue": "#5f9ea0", "chartreuse": "#7fff00", "chocolate": "#d2691e", "coral": "#ff7f50", "cornflowerblue": "#6495ed", "cornsilk": "#fff8dc", "crimson": "#dc143c", "cyan": "#00ffff",
        "darkblue": "#00008b", "darkcyan": "#008b8b", "darkgoldenrod": "#b8860b", "darkgray": "#a9a9a9", "darkgreen": "#006400", "darkkhaki": "#bdb76b", "darkmagenta": "#8b008b", "darkolivegreen": "#556b2f",
        "darkorange": "#ff8c00", "darkorchid": "#9932cc", "darkred": "#8b0000", "darksalmon": "#e9967a", "darkseagreen": "#8fbc8f", "darkslateblue": "#483d8b", "darkslategray": "#2f4f4f", "darkturquoise": "#00ced1",
        "darkviolet": "#9400d3", "deeppink": "#ff1493", "deepskyblue": "#00bfff", "dimgray": "#696969", "dodgerblue": "#1e90ff",
        "firebrick": "#b22222", "floralwhite": "#fffaf0", "forestgreen": "#228b22", "fuchsia": "#ff00ff",
        "gainsboro": "#dcdcdc", "ghostwhite": "#f8f8ff", "gold": "#ffd700", "goldenrod": "#daa520", "gray": "#808080", "green": "#008000", "greenyellow": "#adff2f",
        "honeydew": "#f0fff0", "hotpink": "#ff69b4",
        "indianred ": "#cd5c5c", "indigo ": "#4b0082", "ivory": "#fffff0", "khaki": "#f0e68c",
        "lavender": "#e6e6fa", "lavenderblush": "#fff0f5", "lawngreen": "#7cfc00", "lemonchiffon": "#fffacd", "lightblue": "#add8e6", "lightcoral": "#f08080", "lightcyan": "#e0ffff", "lightgoldenrodyellow": "#fafad2",
        "lightgrey": "#d3d3d3", "lightgreen": "#90ee90", "lightpink": "#ffb6c1", "lightsalmon": "#ffa07a", "lightseagreen": "#20b2aa", "lightskyblue": "#87cefa", "lightslategray": "#778899", "lightsteelblue": "#b0c4de",
        "lightyellow": "#ffffe0", "lime": "#00ff00", "limegreen": "#32cd32", "linen": "#faf0e6",
        "magenta": "#ff00ff", "maroon": "#800000", "mediumaquamarine": "#66cdaa", "mediumblue": "#0000cd", "mediumorchid": "#ba55d3", "mediumpurple": "#9370d8", "mediumseagreen": "#3cb371", "mediumslateblue": "#7b68ee",
        "mediumspringgreen": "#00fa9a", "mediumturquoise": "#48d1cc", "mediumvioletred": "#c71585", "midnightblue": "#191970", "mintcream": "#f5fffa", "mistyrose": "#ffe4e1", "moccasin": "#ffe4b5",
        "navajowhite": "#ffdead", "navy": "#000080",
        "oldlace": "#fdf5e6", "olive": "#808000", "olivedrab": "#6b8e23", "orange": "#ffa500", "orangered": "#ff4500", "orchid": "#da70d6",
        "palegoldenrod": "#eee8aa", "palegreen": "#98fb98", "paleturquoise": "#afeeee", "palevioletred": "#d87093", "papayawhip": "#ffefd5", "peachpuff": "#ffdab9", "peru": "#cd853f", "pink": "#ffc0cb", "plum": "#dda0dd", "powderblue": "#b0e0e6", "purple": "#800080",
        "red": "#ff0000", "rosybrown": "#bc8f8f", "royalblue": "#4169e1",
        "saddlebrown": "#8b4513", "salmon": "#fa8072", "sandybrown": "#f4a460", "seagreen": "#2e8b57", "seashell": "#fff5ee", "sienna": "#a0522d", "silver": "#c0c0c0", "skyblue": "#87ceeb", "slateblue": "#6a5acd", "slategray": "#708090", "snow": "#fffafa", "springgreen": "#00ff7f", "steelblue": "#4682b4",
        "tan": "#d2b48c", "teal": "#008080", "thistle": "#d8bfd8", "tomato": "#ff6347", "turquoise": "#40e0d0",
        "violet": "#ee82ee",
        "wheat": "#f5deb3", "white": "#ffffff", "whitesmoke": "#f5f5f5",
        "yellow": "#ffff00", "yellowgreen": "#9acd32"
    };

    if (typeof colours[colour.toLowerCase()] != 'undefined')
        return colours[colour.toLowerCase()];

    return false;
}

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}

function getCSSRule(ruleName, deleteFlag) {
    ruleName = ruleName.toLowerCase();
    if (document.styleSheets) {
        for (var i = 0; i < document.styleSheets.length; i++) {
            var styleSheet = document.styleSheets[i];
            var ii = 0;
            var cssRule = false;
            do {
                if (styleSheet.cssRules) {
                    cssRule = styleSheet.cssRules[ii];
                } else if (styleSheet.rules) {
                    cssRule = styleSheet.rules[ii];
                }
                if (cssRule) {
                    if (typeof cssRule.selectorText != "undefined") {
                        if (cssRule.selectorText.toLowerCase() == ruleName) {
                            if (deleteFlag == 'delete') {
                                if (styleSheet.cssRules) {
                                    styleSheet.deleteRule(ii);
                                } else {
                                    styleSheet.removeRule(ii);
                                }
                                return true;
                            } else {
                                return cssRule;
                            }
                        }
                    }
                }
                ii++;
            } while (cssRule)
        }
    }
    return false;
}

function killCSSRule(ruleName) {
    return getCSSRule(ruleName, 'delete');
}

function addCSSRule(ruleName) {
    if (document.styleSheets) {
        if (!getCSSRule(ruleName)) {
            if (document.styleSheets[0].addRule) {
                document.styleSheets[0].addRule(ruleName, null, 0);
            } else {
                document.styleSheets[0].insertRule(ruleName + ' { }', 0);
            }
        }
    }
    return getCSSRule(ruleName);
}

function ShowGrid(height) {
    _showGrid = (height > 0);
    $("#gridPanel").show();
    $("#gridPanel").height(height);
    $("#gamePanelSpacer").height(height);
}

$(function () {
    $("#gridPanel").mousewheel(function (e, delta) {
        gridApi.zoomIn(delta);
        return false;
    });
});

// GRID FUNCTIONS ***********************************************************************************************************************

// gridApi is global for interop between PaperScript and JavaScript - a workaround until
// this tutorial exists: http://paperjs.org/tutorials/getting-started/paperscript-interoperability/

window.gridApi = {};
window.gridApi.onLoad = function () {
};

_canvasSupported = (window.HTMLCanvasElement);

function Grid_DrawGridLines(minX, minY, maxX, maxY, border) {
    if (!_canvasSupported) return;
    gridApi.drawGrid(parseInt(minX), parseInt(minY), parseInt(maxX), parseInt(maxY), border);
}

function Grid_SetScale(scale) {
    if (!_canvasSupported) return;
    gridApi.setScale(parseInt(scale));
}

function Grid_DrawBox(x, y, z, width, height, border, borderWidth, fill, sides) {
    if (!_canvasSupported) return;
    gridApi.drawBox(parseFloat(x), parseFloat(y), parseFloat(z), parseInt(width), parseInt(height), border, parseInt(borderWidth), fill, parseInt(sides));
}

function Grid_DrawLine(x1, y1, x2, y2, border, borderWidth) {
    if (!_canvasSupported) return;
    gridApi.drawLine(parseFloat(x1), parseFloat(y1), parseFloat(x2), parseFloat(y2), border, parseInt(borderWidth));
}

function Grid_DrawPlayer(x, y, z, radius, border, borderWidth, fill) {
    if (!_canvasSupported) return;
    gridApi.drawPlayer(parseFloat(x), parseFloat(y), parseFloat(z), parseInt(radius), border, parseInt(borderWidth), fill);
}

function Grid_DrawLabel(x, y, z, text) {
    if (!_canvasSupported) return;
    gridApi.drawLabel(parseFloat(x), parseFloat(y), parseFloat(z), text);
}

function Grid_ShowCustomLayer(visible) {
    if (!_canvasSupported) return;
    gridApi.showCustomLayer(visible == "true");
}

function Grid_ClearCustomLayer() {
    if (!_canvasSupported) return;
    gridApi.clearCustomLayer();
}

function Grid_SetCentre(x, y) {
    if (!_canvasSupported) return;
    gridApi.setCentre(parseFloat(x), parseFloat(y));
}

function Grid_DrawSquare(id, x, y, width, height, text, fill) {
    if (!_canvasSupported) return;
    gridApi.drawCustomLayerSquare(id, parseInt(x), parseInt(y), parseInt(width), parseInt(height), text, fill);
}

// JQUERY.MOUSEWEHEEL.JS ****************************************************************************************************************
// https://github.com/brandonaaron/jquery-mousewheel

/*! Copyright (c) 2011 Brandon Aaron (http://brandonaaron.net)
* Licensed under the MIT License (LICENSE.txt).
*
* Thanks to: http://adomas.org/javascript-mouse-wheel/ for some pointers.
* Thanks to: Mathias Bank(http://www.mathias-bank.de) for a scope bug fix.
* Thanks to: Seamus Leahy for adding deltaX and deltaY
*
* Version: 3.0.6
* 
* Requires: 1.2.2+
*/

(function ($) {

    var types = ['DOMMouseScroll', 'mousewheel'];

    if ($.event.fixHooks) {
        for (var i = types.length; i; ) {
            $.event.fixHooks[types[--i]] = $.event.mouseHooks;
        }
    }

    $.event.special.mousewheel = {
        setup: function () {
            if (this.addEventListener) {
                for (var i = types.length; i; ) {
                    this.addEventListener(types[--i], handler, false);
                }
            } else {
                this.onmousewheel = handler;
            }
        },

        teardown: function () {
            if (this.removeEventListener) {
                for (var i = types.length; i; ) {
                    this.removeEventListener(types[--i], handler, false);
                }
            } else {
                this.onmousewheel = null;
            }
        }
    };

    $.fn.extend({
        mousewheel: function (fn) {
            return fn ? this.bind("mousewheel", fn) : this.trigger("mousewheel");
        },

        unmousewheel: function (fn) {
            return this.unbind("mousewheel", fn);
        }
    });


    function handler(event) {
        var orgEvent = event || window.event, args = [].slice.call(arguments, 1), delta = 0, returnValue = true, deltaX = 0, deltaY = 0;
        event = $.event.fix(orgEvent);
        event.type = "mousewheel";

        // Old school scrollwheel delta
        if (orgEvent.wheelDelta) { delta = orgEvent.wheelDelta / 120; }
        if (orgEvent.detail) { delta = -orgEvent.detail / 3; }

        // New school multidimensional scroll (touchpads) deltas
        deltaY = delta;

        // Gecko
        if (orgEvent.axis !== undefined && orgEvent.axis === orgEvent.HORIZONTAL_AXIS) {
            deltaY = 0;
            deltaX = -1 * delta;
        }

        // Webkit
        if (orgEvent.wheelDeltaY !== undefined) { deltaY = orgEvent.wheelDeltaY / 120; }
        if (orgEvent.wheelDeltaX !== undefined) { deltaX = -1 * orgEvent.wheelDeltaX / 120; }

        // Add event and delta to the front of the arguments
        args.unshift(event, delta, deltaX, deltaY);

        return ($.event.dispatch || $.event.handle).apply(this, args);
    }

})(jQuery);
