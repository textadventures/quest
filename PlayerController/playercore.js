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

    ui_init();
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
    // this is overridden by playerweb.js for WebPlayer
    if (visible) {
        $("#gamePanes").show();
    }
    else {
        $("#gamePanes").hide();
    }
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

function paneButtonClick(target, verb) {
    var selectedObject = $(target + " option:selected").text();
    if (selectedObject.length > 0) {
        var cmd = verb + " " + selectedObject;
        sendCommand(cmd.toLowerCase());
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
        var splitString = value.split(":");
        var objectDisplayName = splitString[0];
        var objectVerbs = splitString[1];

        if (listName == "inventory") {
            inventoryVerbs.push(objectVerbs);
        }

        if (listName == "placesobjects") {
            placesObjectsVerbs.push(objectVerbs);
        }

        if (listName == "inventory" || $.inArray(objectDisplayName, _compassDirs) == -1) {
            $(listElement).append(
                $("<option/>").attr("value", key).text(objectDisplayName)
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
    var verbs = verbsArray[selectedIndex].split("/");
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

function bindMenu(linkid, verbs, text) {
    var verbsList = verbs.split("/");
    var options = [];

    $.each(verbsList, function (key, value) {
        options = options.concat({ title: value, action: { type: "fn", callback: "doMenuClick('" + value.toLowerCase() + " " + text.replace("'", "\\'") + "');"} });
    });

    $("#" + linkid).jjmenu("both", options, {}, { show: "fadeIn", speed: 100, xposition: "left", yposition: "auto", "orientation": "auto" });
}

function doMenuClick(command) {
    $("div[id^=jjmenu]").remove();
    sendCommand(command);
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
