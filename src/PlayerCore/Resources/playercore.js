var _allowMenuFontSizeChange = true;
var _showGrid = false;
var _outputSections = new Array();
var numCommands = 0;
var thisCommand = 0;
var commandsList = new Array();
var inventoryVerbs = null;
var placesObjectsVerbs = null;
var verbButtonCount = 9;
var beginningOfCurrentTurnScrollPosition = 0;

function initPlayerUI() {
    // TODO: Implement this properly
    // $("#cmdSave").show();

    addPaperScript();
    $("#jquery_jplayer").jPlayer({ supplied: "wav, mp3" });
    
    $("#txtCommand").bind("inview", function (event, visible) {
        // allows spacebar to scroll browser when txtCommand is not visible
        if (visible == true) {
            $("#txtCommand").focus();
        } else {
            $("#txtCommand").blur();
        }
    });

    $("body").keydown(function (e) {
        if (_waitMode) {
            endWait();
        }
    });

    $("button").button();
    $("#gamePanesRunning").multiOpenAccordion({ active: [0, 1, 2, 3] });
    showStatusVisible(false);

    $("#cmdSave").click(function () {
        saveGame();
        afterSave();
    });
    
    const cmdDebug = document.getElementById("cmdDebug");
    cmdDebug.addEventListener("click", () => {
        const dialog = document.getElementById("questVivaDebugger");
        dialog.showModal();
    });
    
    $("#lstInventory").selectable({
        selected: function (event, ui) {
            $(ui.selected).siblings().removeClass("ui-selected");
            updateVerbButtons($(ui.selected), inventoryVerbs, "cmdInventory");
        }
    });

    $("#lstPlacesObjects").selectable({
        selected: function (event, ui) {
            $(ui.selected).siblings().removeClass("ui-selected");
            updateVerbButtons($(ui.selected), placesObjectsVerbs, "cmdPlacesObjects");
        }
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

    $("#txtCommand").bind('webkitspeechchange', function () {
        sendCommand($("#txtCommand").val());
        $("#txtCommand").val("");
    });
    
    $(document).on("click", "a", function (e) {
        var href = $(this).attr("href");
        if (href) {
            goUrl(href);
        }
        e.preventDefault();
    });

    $(document).on("click", ".elementmenu", function (event) {
        if (!$(this).hasClass("disabled")) {
            event.preventDefault();
            event.stopPropagation();
            $(this).jjmenu_popup();
            $(this).blur();
            return false;
        }
    });
    
    $(document).on("click", ".exitlink", function () {
        if (!$(this).hasClass("disabled")) {
            sendCommand($(this).attr("data-command"));
        }
    });
    
    $(document).on("click", ".commandlink", function () {
        var $this = $(this);
        if (!$this.hasClass("disabled") && canSendCommand) {
            if ($this.data("deactivateonclick")) {
                $this.addClass("disabled");
                $this.data("deactivated", true);
            }
            sendCommand($this.attr("data-command"));
        }
    });

    const sidebar = document.getElementById("sidebar");
    const cmdShowPanes = document.getElementById("cmdShowPanes");
    
    cmdShowPanes.addEventListener("click", function () {
        sidebar.style.display = (sidebar.style.display === "block") ? "none" : "block";
    });
    
    let gameWidth = 950;
    
    const isWindowWide = () => window.innerWidth > gameWidth;
    let wasWide = isWindowWide();
    
    const doLayout = () => {
        const sidebar = document.getElementById("sidebar");
        const cmdShowPanes = document.getElementById("cmdShowPanes");
        const gamePanes = document.getElementById("gamePanes");
        const gameContent = document.getElementById("gameContent");

        let isWide = isWindowWide();

        if (isWide) {
            sidebar.style.display = "block";
            sidebar.style.background = "initial";
            sidebar.style.boxShadow = "initial";
            cmdShowPanes.style.display = "none";
            gamePanes.style.left = "50%";
            gamePanes.style.marginLeft = (gameWidth / 2 - 220) + "px";
            gameContent.style.maxWidth = (gameWidth - 250) + "px";
        } else {
            if (wasWide) {
                sidebar.style.display = "none";
            }
            sidebar.style.background = "#fff";
            sidebar.style.boxShadow = "-2px 0 5px rgba(0,0,0,0.5)";
            cmdShowPanes.style.display = "inline";
            gamePanes.style.left = "initial";
            gamePanes.style.marginLeft = "0";
            gameContent.style.maxWidth = "initial";
        } 

        wasWide = isWide;
    }
    
    window.addEventListener("resize", doLayout);
    
    window.setGameWidth = function (width) {
        const gameBorder = document.getElementById("gameBorder");
        gameBorder.style.maxWidth = width + "px";
        
        const status = document.getElementById("status");
        status.style.maxWidth = (width - 2) + "px";

        $("#gamePanel").css("margin-left", "-" + (width / 2 - 19) + "px");
        $("#gridPanel").css("margin-left", "-" + (width / 2 - 19) + "px");
        
        gameWidth = width;
        doLayout();
    };
    
    doLayout();
    ui_init();
    updateStatusVisibility();
    
    if (document.layers) document.captureEvents(Event.MOUSEDOWN);

    $("#txtCommand").focus();
}

function loadHtml(html) {
    $("#divOutput").html(html);
}

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

// Variable added by KV
var gameName = document.title;

function setGameName(text) {
    $("#gameTitle").remove();
    document.title = text;
    // Added by KV
    gameName = text;
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
    
    // TODO: This is WebPlayer-specific, so shouldn't be in this shared file
    window.setTimeout(async function () {
        await WebPlayer.uiEndPause();
    }, 100);
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
            e.preventDefault();
            break;
        case 40:
            thisCommand++;
            if (thisCommand > numCommands) thisCommand = 1;
            $("#txtCommand").val(commandsList[thisCommand]);
            e.preventDefault();
            break;
        case 27:
            thisCommand = numCommands + 1;
            $("#txtCommand").val("");
            e.preventDefault();
            break;
    }
}

function runCommand() {
    var command = $("#txtCommand").val();
    if (command.length > 0 && canSendCommand) {
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
                click: function () { msgboxSubmit(true); }
            },
            {
                text: "No",
                click: function () { msgboxSubmit(false); }
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
        updateStatusVisibility();
    }
}

function uiHide(element) {
    if (element == "#gamePanes") {
        panesVisible(false);
    }
    else {
        $(element).hide();
        updateStatusVisibility();
    }
}

function updateStatusVisibility() {
    var anyVisible = isElementVisible("#location") || isElementVisible("#cmdSave") || isElementVisible("#cmdExitFullScreen");
    if (anyVisible) {
        $("#status").show();
        $("#divOutput").css("margin-top", "20px");
        $("#gamePanes").css("top", "24px");
        $("#gridPanel").css("top", "32px");
        $("#gamePanel").css("top", "32px");
    } else {
        $("#status").hide();
        $("#divOutput").css("margin-top", "0px");
        $("#gamePanes").css("top", "0px");
        $("#gridPanel").css("top", "0px");
        $("#gamePanel").css("top", "0px");
    }
}

function isElementVisible(element) {
    return $(element).css("display") != "none";
}

function panesVisible(visible) {
    var screenWidth = $("#gameBorder").width();
    
    // TODO: Verfy resize behaviour when an image or map panel is displayed,
    // then remove the commented-out code here.
    
    var gameContentPadding = parseInt($("#gameContent").css("padding-left").replace("px", "")) + parseInt($("#gameContent").css("padding-right").replace("px", ""));
    var promptSpacing = $("#txtCommandPrompt").width() + 5;

    if (visible) {
        $("#gamePanes").show();
        // $("#gameContent").width(screenWidth - 250);
        // $("#txtCommand").width(screenWidth - gameContentPadding - promptSpacing - 250);
        $("#updating").css("margin-left", (screenWidth / 2 - 290) + "px");
        // $("#gamePanel").width(screenWidth - 250);
        // $("#gridPanel").width(screenWidth - 250);
        // $("#gridCanvas").prop("width", screenWidth - 250);
        paper.view.viewSize.width = screenWidth - 250;
        var css = addCSSRule("div#gamePanel img");
        css.style.maxWidth = (screenWidth - 250) + "px";
        var css = addCSSRule("div#divOutput img");
        css.style.maxWidth = (screenWidth - 250) + "px";
    }
    else {
        $("#gamePanes").hide();
        // $("#gameContent").width(screenWidth - gameContentPadding);
        // $("#txtCommand").width(screenWidth - promptSpacing - 60);
        $("#updating").css("margin-left", (screenWidth / 2 - 70) + "px");
        // $("#gamePanel").width(screenWidth - 40);
        // $("#gridPanel").width(screenWidth - 40);
        // $("#gridCanvas").prop("width", screenWidth - 40);
        paper.view.viewSize.width = screenWidth - 40;
        var css = addCSSRule("div#gamePanel img");
        css.style.maxWidth = (screenWidth - 40) + "px";
        var css = addCSSRule("div#divOutput img");
        css.style.maxWidth = (screenWidth - 40) + "px";
    }
}

var _animateScroll = true;

function scrollToEnd() {
    if (!_animateScroll)
    {
        $('html,body').scrollTop(document.body.scrollHeight);
    }
    else {
        $('html,body').animate({ scrollTop: document.body.scrollHeight }, 'fast');
    }
    $("#txtCommand").focus();
}

function SetAnimateScroll(value) {
    _animateScroll = value;
}

var _backgroundOpacity = 1;

function SetBackgroundOpacity(opacity) {
    _backgroundOpacity = opacity;
}

function setBackground(col) {
    /* If '#rgb', convert to '#rrggbb'*/
    /* https://github.com/textadventures/quest/issues/1052 */
    if (col.charAt(0) === "#" && col.length == 4) {
        var colBak = "" + col + "";
        newCol = col.replace(/#([0-9a-fA-F])([0-9a-fA-F])([0-9a-fA-F])/, '#$1$1$2$2$3$3');
        col = newCol || col;
    }
    colNameToHex = colourNameToHex(col);
    if (colNameToHex) col = colNameToHex;
    rgbCol = hexToRgb(col);
    var cssBackground = "rgba(" + rgbCol.r + "," + rgbCol.g + "," + rgbCol.b + "," + _backgroundOpacity + ")";
    $("#gameBorder").css("background-color", cssBackground);
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

function setGamePadding(top, bottom, left, right) {
    $("#gameContent").css("padding-top", top);
    $("#gameContent").css("padding-bottom", bottom);
    $("#gameContent").css("padding-left", left);
    $("#gameContent").css("padding-right", right);
}

function hideBorder() {
    $("#gameBorder").css("border", "none");
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
    var selectedListItem = $(target + " .ui-selected");
    var selectedObject = selectedListItem.text();
    var selectedElementId = selectedListItem.data("elementid");
    var selectedElementName = selectedListItem.data("elementname");
    var verb = button.data("verb");
    var metadata = {};
    metadata[selectedElementName] = selectedElementId;

    if (selectedObject.length > 0) {
        var cmd = verb.toLowerCase() + " " + selectedElementName;
        sendCommand(cmd, metadata);
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
}

function setCompassDirections(directions) {
    if (typeof directions === "string") {
      _compassDirs = directions.split(";")
    } else {
      _compassDirs = directions;
    }
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


// Used in grid.js, but defined here so it can be set during startup
_respondToGridClicks = false;

function respondToGridClicks(flag) {
    _respondToGridClicks = flag;
}

function updateList(listName, listData) {
    var listElement = "";
    var buttonPrefix = "";

    if (listName == "inventory") {
        listElement = "#lstInventory";
        inventoryVerbs = new Array();
        buttonPrefix = "cmdInventory";
    }

    if (listName == "placesobjects") {
        listElement = "#lstPlacesObjects";
        placesObjectsVerbs = new Array();
        buttonPrefix = "cmdPlacesObjects";
    }

    var previousSelectionText = "";
    var previousSelectionKey = "";
    var foundPreviousSelection = false;

    var $selected = $(listElement + " .ui-selected");
    if ($selected.length > 0) {
        previousSelectionText = $selected.first().text();
        previousSelectionKey = $selected.first().data("key");
    }

    $(listElement).empty();
    var count = 0;
    $.each(listData, function (key, value) {
        var data = JSON.parse(value);
        var objectDisplayName = data["Text"];
        var verbsArray, idPrefix;

        if (listName == "inventory") {
            verbsArray = inventoryVerbs;
            idPrefix = "cmdInventory";
        } else {
            verbsArray = placesObjectsVerbs;
            idPrefix = "cmdPlacesObjects";
        }

        verbsArray.push(data);

        if (listName == "inventory" || $.inArray(objectDisplayName, _compassDirs) == -1) {
            var $newItem = $("<li/>").data("key", key).data("elementid", data["ElementId"]).data("elementname", data["ElementName"]).data("index", count).html(objectDisplayName);
            if (objectDisplayName == previousSelectionText && key == previousSelectionKey) {
                $newItem.addClass("ui-selected");
                foundPreviousSelection = true;
                updateVerbButtons($newItem, verbsArray, idPrefix);
            }
            $(listElement).append($newItem);
            count++;
        }
    });

    var selectSize = count;
    if (selectSize < 3) selectSize = 3;
    if (selectSize > 12) selectSize = 12;
    $(listElement).attr("size", selectSize);
    
    if (!foundPreviousSelection) {
        for (var i = 1; i <= verbButtonCount; i++) {
            var target = $("#" + buttonPrefix + i);
            target.hide();
        }
    }
}

function updateVerbButtons(selectedItem, verbsArray, idprefix) {
    var selectedIndex = selectedItem.data("index");
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
    $("#txtCommand").hide();
    $("#endWaitLink").show();
    markScrollPosition();
}

function endWait() {
    if (!_waitMode) return;
    sendEndWait();
}

function waitEnded() {
    _waitMode = false;
    $("#endWaitLink").hide();
    $("#txtCommand").show();
}

function gameFinished() {
    disableInterface();
}

function disableInterface() {
    $("#txtCommandDiv").hide();
    $("#gamePanesRunning").hide();
    $("#gamePanesFinished").show();
}

function setCommandBarStyle(style) {
    $("#txtCommand").attr("style", style);
}

function addTextAndScroll(text) {
    addText(text);
    scrollToEnd();
}

// Added by KV for the transcript
var savingTranscript = false;
var noTranscript = false;
var transcriptString = ""; /* Added in Quest 5.8, but never worked properly */


// This function altered for the transcript
function addText(text) {
    if (getCurrentDiv() == null) {
        createNewDiv("left");
    }
    if (savingTranscript && !noTranscript) {
        writeToTranscript(text);
    }
    getCurrentDiv().append(text);
    $("#divOutput").css("min-height", $("#divOutput").height());
}

function createNewDiv(alignment) {
    var classes = _outputSections.join(" ");
    setDivCount(getDivCount() + 1);
    $("<div/>", {
        id: "divOutputAlign" + getDivCount(),
        style: "text-align: " + alignment,
        "class": classes
    }).appendTo("#divOutput");
    setCurrentDiv("#divOutputAlign" + getDivCount());
}

var _currentDiv = null;

function getCurrentDiv() {
    if (_currentDiv) return _currentDiv;

    var divId = $("#outputData").attr("data-currentdiv");
    if (divId) {
        _currentDiv = $(divId);
        return _currentDiv;
    }

    return null;
}

function setCurrentDiv(div) {
    _currentDiv = $(div);
    $("#outputData").attr("data-currentdiv", div);
}

var _divCount = -1; 

function getDivCount() {
    if (_divCount < 1) {
        _divCount = parseInt($("#outputData").attr("data-divcount")) || 0;
    }
    return _divCount;
}

function setDivCount(count) {
    _divCount = count;
    $("#outputData").attr("data-divcount", _divCount);
}

function bindMenu(linkid, verbs, text, elementId) {
    var options = buildMenuOptions(verbs, text, elementId);
    $("#" + linkid).jjmenu(options);
}

function buildMenuOptions(verbs, text, elementId) {
    var verbsList = verbs.split("/");
    var options = [];
    var metadata = {};
    metadata[text] = elementId;

    $.each(verbsList, function (key, value) {
        options = options.concat({
            title: value,
            action: {
                callback: function (selectedValue) {
                    sendCommand(selectedValue.toLowerCase() + " " + text, metadata);
                }
            }
        });
    });

    return options;
}

function updateObjectLinks(data) {
    $(".elementmenu").each(function (index, e) {
        var $e = $(e);
        var verbs = data[$e.data("elementid")];
        if (verbs) {
            $e.removeClass("disabled");
            $e.data("verbs", verbs);
            // also set attribute so verbs are persisted to savegame
            $e.attr("data-verbs", verbs);
        } else {
            $e.addClass("disabled");
        }
    });
}

function updateExitLinks(data) {
    $(".exitlink").each(function (index, e) {
        var $e = $(e);
        var exitid = $e.data("elementid");
        var available = $.inArray(exitid, data) > -1;
        if (available) {
            $e.removeClass("disabled");
        } else {
            $e.addClass("disabled");
        }
    });
}

function updateCommandLinks(data) {
    $(".commandlink").each(function (index, e) {
        var $e = $(e);
        if (!$(e).data("deactivated")) {
            var elementid = $e.data("elementid");
            var available = $.inArray(elementid, data) > -1 || elementid.length == 0;
            if (available) {
                $e.removeClass("disabled");
            } else {
                $e.addClass("disabled");
            }
        }
    });
}

function disableAllCommandLinks() {
    $(".commandlink").each(function (index, e) {
        $(e).addClass("disabled");
        $(e).data("deactivated", true);
    });
}

var saveClearedText = false;
var clearedOnce = false;
function clearScreen() {
    $("#outputData").appendTo($("body"));
    if (!saveClearedText) {
        $("#divOutput").css("min-height", 0);
        $("#divOutput").html("");
        createNewDiv("left");
        beginningOfCurrentTurnScrollPosition = 0;
        setTimeout(function () {
            $("html,body").scrollTop(0);
        }, 100);
    } else {
        $("#divOutput").append("<hr class='clearedAbove' />");
        if (!clearedOnce) {
            addText('<style>#divOutput > .clearedScreen { display: none; }</style>');
        }
        clearedOnce = true;
        $('#divOutput').children().addClass('clearedScreen');
        $('.clearedScreen').attr('id',null);
        $('#divOutput').css('min-height', 0);
        createNewDiv('left');
        beginningOfCurrentTurnScrollPosition = 0;
        setTimeout(function () {
            $('html,body').scrollTop(0);
        }, 100);
    }
    $("#outputData").appendTo($("#divOutput"));
}

// Scrollback functions added by KV

function showScrollback() {
    var scrollbackDivString = '<div id="scrollback-dialog" style="display:none;"><div id="scrollbackdata"></div></div>';
    addText(scrollbackDivString);
    var scrollbackDialog = $("#scrollback-dialog").dialog({
        autoOpen: false,
        width: (window.innerWidth || document.documentElement.clientWidth || 
document.body.clientWidth) - 100,
        height: (window.innerHeight|| document.documentElement.clientHeight|| 
document.body.clientHeight) - 100,
        title: "Scrollback",
        buttons: {
            Ok: function () {
                $(this).dialog("close");
                $(this).remove();
                $("#scrollback-dialog,#scrollbackdata").remove();
            },
            Print: function () {
                printScrollbackDiv();
            },
        },
        show: { effect: "fadeIn", duration: 500 },
        modal: true,
    });
    $('#scrollbackdata').html($('#divOutput').html());
    $("#scrollbackdata a").addClass("disabled");
    scrollbackDialog.dialog("open");
    setTimeout(function () {
        $("#scrollbackdata a").addClass("disabled");
    }, 1);
}

function printScrollbackDiv() {
    var iframe = document.createElement('iframe');
    document.body.appendChild(iframe);
    iframe.contentWindow.document.write($("#scrollbackdata").html());
    iframe.contentWindow.print();
    document.body.removeChild(iframe);
    $("#scrollback-dialog").dialog("close");
    $("#scrollback-dialog").remove();
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

function addExternalStylesheet(source) {
    var link = $("<link>");
    link.attr({
        type: "text/css",
        rel: "stylesheet",
        href: source
    });
    $("head").append(link);
}

function setInterfaceString(name, text) {
    switch (name) {
        case "InventoryLabel":
            $("#inventoryLabel span.accordion-header-text").html(text);
            break;
        case "StatusLabel":
            $("#statusVarsLabel span.accordion-header-text").html(text);
            break;
        case "PlacesObjectsLabel":
            $("#placesObjectsLabel span.accordion-header-text").html(text);
            break;
        case "CompassLabel":
            $("#compassLabel span.accordion-header-text").html(text);
            break;
        case "InButtonLabel":
            $("#cmdCompassIn span").html(text);
            break;
        case "OutButtonLabel":
            $("#cmdCompassOut span").html(text);
            break;
        case "EmptyListLabel":
            break;
        case "NothingSelectedLabel":
            break;
        case "TypeHereLabel":
            $("#txtCommand").prop("placeholder", text);
            break;
        case "ContinueLabel":
            $("#endWaitLink").html(text);
            break;
    }
}

function AddYouTube(id) {
    var url = "https://www.youtube.com/embed/" + id + "?autoplay=1&rel=0";
    var embedHTML = "<iframe width=\"425\" height=\"344\" src=\"" + url + "\" frameborder=\"0\" allowfullscreen></iframe>";
    addText(embedHTML);
}

function AddVimeo(id) {
    var url = "https://player.vimeo.com/video/" + id + "?autoplay=1";
    var embedHTML = "<iframe sandbox=\"allow-same-origin allow-scripts allow-popups\" src=\"" + url + "\" width=\"500\" height=\"281\" frameborder=\"0\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>";
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

function StartOutputSection(name) {
    if ($.inArray(name, _outputSections) == -1) {
        _outputSections.push(name);
        createNewDiv("left");
    }
}

function EndOutputSection(name) {
    var index = $.inArray(name, _outputSections);
    if (index != -1) {
        _outputSections.splice(index, 1);
        createNewDiv("left");
    }
}

function HideOutputSection(name) {
    EndOutputSection(name);
    $("." + name + " a").attr("onclick", "");
    setTimeout(function() {
        $("." + name).hide(250, function () { $(this).remove(); });
        // Added by The Pixie, 04/Oct/17
        // This should close the gap when the menu is hidden
        $("#divOutput").animate({'min-height':0}, 250);
    }, 250);
}

var TextFX = new function() {
    var fxCount = 0;

    function addFx(text, font, color, size) {
        fxCount++;
        var style = "font-family:" + font + ";color:" + color + ";font-size:" + size + "pt";
        var html = "<span id=\"fx" + fxCount + "\" style=\"" + style + "\">" + text + " </span><br />";
        addText(html);
        return $("#fx" + fxCount);
    }

    this.Typewriter = function(text, speed, font, color, size) {
        var el = addFx(text, font, color, size);
        el.typewriter(speed);
    }

    this.Unscramble = function(text, speed, reveal, font, color, size) {
        var el = addFx(text, font, color, size);
        el.unscramble(speed, reveal);
    }
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
            try {
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
            } catch (e) {
                // Firefox throws a SecurityError if you try reading
                // a cross-domain stylesheet
                if (e.name !== "SecurityError") {
                    throw e;
                }
            }
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
    $("#gridCanvas").prop("height", height);
    paper.view.viewSize.height = height;
    $("#gamePanelSpacer").height(height);
}


// ----------------------------------        
// Section added by The Pixie

function endsWith(str, suffix) {
    return str.indexOf(suffix, str.length - suffix.length) !== -1;
}

function setCss(element, cssString) {
  el = $(element);
  ary = cssString.split(";");
  for (i = 0; i < ary.length; i++) {
    ary2 = ary[i].split(':');
    el.css(ary2[0], ary2[1]);
  }
}

function addScript(text) {
    $('body').prepend(text);
}

function colourBlend(colour1, colour2) {
  $('#gamePanes').css('background-color', 'transparent');
  $('#gameBorder').css('background-color', 'transparent');
  body = $('body');
  body.css('background-color', colour1);
  body.css('background-image', 'linear-gradient(to bottom,  ' + colour1 + ', ' + colour2 + ')');
  body.css('background-image', '-webkit-linear-gradient(top,  ' + colour1 + ', ' + colour2 + ')');
  body.css('background-attachment', 'fixed');
  body.css('background-position', 'left bottom');
}



elements = [
  '#statusVarsLabel', '#statusVarsAccordion',// '#statusVars',
  '#inventoryLabel', '#inventoryAccordion', '#inventoryAccordion.ui-widget-content',
  '#placesObjectsLabel', '#placesObjectsAccordion', '#placesObjectsAccordion.ui-widget-content',
  '#compassLabel', '#compassAccordion', '.ui-button', //'.ui-button-text',
  '#commandPane', '#customStatusPane'
];

dirs = ['N', 'E', 'S', 'W', 'NW', 'NE', 'SW', 'SE', 'U', 'In', 'D', 'Out'];

commandColour = 'orange'

function setElement(name, fore, back) {
  el = $(name);
  el.css('background', back);
  el.css('color', fore);
  el.css('border', 'solid 1px ' + fore);
  if (endsWith(name, "Accordion")) {
    el.css('border-top', 'none');
  }
}

function setCompass(name, fore, back) {
  el = $('#cmdCompass' + name);
  el.css('background', back);
  el.css('border', '2px solid ' + fore);
}

function setPanes(fore, back, secFore, secBack, highlight) {
  if (arguments.length == 2) {
    secFore = back;
    secBack = fore;
  }
  if (arguments.length < 5) {
    highlight = 'orange'
  }
  commandColour = fore;
  for (i = 0; i < elements.length; i++) {
    setElement(elements[i], fore, back);
  }
  for (i = 0; i < dirs.length; i++) {
    setElement(dirs[i], fore, back);
  }

  var head = $('head');
  head.append('<style>.ui-button-text { color: ' + fore + ';}</style>');
  head.append('<style>.ui-state-active { color: ' + fore + ';}</style>');
  head.append('<style>.ui-widget-content { color: ' + fore + ';}</style>');
  head.append('<style>.ui-widget-header .ui-state-default { background-color: ' + secBack + ';}</style>');
  head.append('<style>.ui-selecting { color: ' + secFore + '; background-color: ' + highlight + ';}</style>');
  head.append('<style>.ui-selected { color: ' + secFore + '; background-color: ' + secBack + ';}</style>');

  //$('.ui-button-text').css('color', fore);
  //$('.ui-state-active').css('color', fore);
  //$('.ui-widget-content').css('color', fore);
  
}

function setCommands(s, colour) {
  if (arguments.length == 2) commandColour = colour;
  ary = s.split(";");
  el = $('#commandPaneHeading');
  el.empty();
  for (i = 0; i < ary.length; i++) {
      ary2 = ary[i].split(":");
      comm = ary2[0];
      commLower = ary2[0].toLowerCase().replace(/ /g, "_");
      commComm = (ary2.length == 2 ? ary2[1] : ary2[0]).toLowerCase();
      //alert("ary[i]=" + ary[i] + ", Comm=" + comm + ", commComm=" + commComm + ", ary2[0].length=" + ary2.length);
      el.append(' <span id="' + commLower + '_command_button"  class="accordion-header-text" style="padding:5px;"><a id="verblink' + commLower + '" class="cmdlink commandlink" style="text-decoration:none;color:' + commandColour + ';font-size:12pt;" data-elementid="" data-command="' + commComm + '">' + comm + '</a></span> ');
  }
}

function setCustomStatus(s) {
    el = $('#customStatusPane');
    el.html(s);
}

        
// ----------------------------------        
        
        
$(function () {
    $("#gridPanel").mousewheel(function (e, delta) {
        gridApi.zoomIn(delta);
        return false;
    });
});
    
// ***********
// Section added by KV


function isMobilePlayer() {
    if (typeof (currentTab) === 'string') {
        return true;
    }
    return false;
};

function showPopup(title, text) {
    $('#msgboxCaption').html(text);

    var msgboxOptions = {
        modal: true,
        autoOpen: false,
        title: title,
        buttons: [
			{
			    text: 'OK',
			    click: function () { $(this).dialog('close'); }
			},
        ],
        closeOnEscape: false,
    };

    $('#msgbox').dialog(msgboxOptions);
    $('#msgbox').dialog('open');
};

function showPopupCustomSize(title, text, width, height) {
    $('#msgboxCaption').html(text);

    var msgboxOptions = {
        modal: true,
        autoOpen: false,
        title: title,
        width: width,
        height: height,
        buttons: [
			{
			    text: 'OK',
			    click: function () { $(this).dialog('close'); }
			},
        ],
        closeOnEscape: false,
    };

    $('#msgbox').dialog(msgboxOptions);
    $('#msgbox').dialog('open');
};

function showPopupFullscreen(title, text) {
    $('#msgboxCaption').html(text);

    var msgboxOptions = {
        modal: true,
        autoOpen: false,
        title: title,
        width: $(window).width(),
        height: $(window).height(),
        buttons: [
			{
			    text: 'OK',
			    click: function () { $(this).dialog('close'); }
			},
        ],
        closeOnEscape: false,
    };

    $('#msgbox').dialog(msgboxOptions);
    $('#msgbox').dialog('open');
};

// Log functions
// In-game HTML log introduced in 5.8. Deprecated in 5.9
// Its functions and variables are only here to avoid errors

var logVar = "";

function addLogEntry(data){
  // Do nothing.
}

var displayedInGameLogWarning = false;
function showLog(){
  // In-game HTML log introduced in 5.8. Removed in 5.9
  if (!displayedInGameLogWarning) {
    console.warning("The in-game HTML log feature has been removed.")
    displayedInGameLogWarning = true;
  }
  // Do nothing.
}

function setupLogDiv(){
  // Do nothing.
}

function showLogDiv(){
  // Do nothing.
}

function hideLogDiv(){
  // Do nothing.
}

function printLogDiv(){
  // Do nothing.
}

function saveLog(){
  // Do nothing.
}

// The transcript uses this function.
function getTimeAndDateForLog(){
    var date = new Date();
    var currentDateTime = date.toLocaleString('en-US', { timeZoneName: 'short' }).replace(/,/g, "").replace(/[A-Z][A-Z][A-Z].*/g, "");
    return currentDateTime;
}

// **********************************
// TRANSCRIPT FUNCTIONS

function writeToTranscript(text){
  if (!noTranscript && savingTranscript) {
    var faker = document.createElement('div');
    faker.innerHTML = text;
    text = faker.innerHTML;
    for (var key in Object.keys(faker.getElementsByTagName('img'))){
      var elem = faker.getElementsByTagName('img')[key];
      if (elem != null) {
        var altProp = $(faker.getElementsByTagName('img')[key]).attr('alt') || "";
        text = text.replace(elem.outerHTML, altProp) || text;
      }
    }
    for (var key in Object.keys(faker.getElementsByTagName('area'))){
      var elem = faker.getElementsByTagName('area')[key];
      if (elem != null) {
        var altProp = $(faker.getElementsByTagName('area')[key]).attr('alt') || "";
        text = text.replace(elem.outerHTML, altProp) || text;
      }
    }
    for (var key in Object.keys(faker.getElementsByTagName('input'))){
      var elem = faker.getElementsByTagName('input')[key];
      if (elem != null) {
        var altProp = $(faker.getElementsByTagName('input')[key]).attr('alt') || "";
        text = text.replace(elem.outerHTML, altProp) || text;
      }
    }
    WriteToTranscript(transcriptName + "___SCRIPTDATA___" + $("<div>" + text.replace(/<br\/>/g,"@@@NEW_LINE@@@").replace(/<br>/g,"@@@NEW_LINE@@@").replace(/<br \/>/g,"@@@NEW_LINE@@@").replace("&nbsp;"," ") + "</div>").text());
  }
}

function enableTranscript(name){
  if (noTranscript) return;
  transcriptName = name || transcriptName || gameName;
  savingTranscript = true;
}

function disableTranscript(){
  savingTranscript = false;
}

function killTranscript(){
  noTranscript = true;
  disableTranscript();
}

function setTranscriptStatus(status){
  switch (status){
    case "enabled":
      var name = transcriptName || gameName;
      enableTranscript(name);
      break;
    case "disabled":
      disableTranscript();
      break;
    case "prohibited":
    case "none":
    case "killed":
      killTranscript();
      break;
    case "allowed":
      noTranscript = false;
  }
}

var showedSaveTranscriptWarning = false;

/*
 * This function was missing from the webplayer in Quest 5.8.0
 * Leaving this here as a fallback
*/
function SaveTranscript(text){
  if(!showedSaveTranscriptWarning){
    console.log("[QUEST]: SaveTranscript has been deprecated. Using writeToTranscript instead.");
    showedSaveTranscriptWarning = true;
  }
  writeToTranscript(text);  
}

var transcriptUrl = 'TranscriptViewer/index.html';

// Another fallback to avoid errors
function showTranscript(){
  if (webPlayer){
    addTextAndScroll('Your transcripts are saved to the localStorage in your browser. You can view, download, or delete them here: <a href="' + transcriptUrl + '" target="_blank">Your Transcripts</a><br/>');
  }
  else {
    addTextAndScroll('Your transcripts are saved to "Documents\\Quest Transcripts\\".<br/>');
  }
}

function replaceTranscriptString(data) {
    // Added in 5.8, and never worked properly.
    // Do nothing.
}
function printTranscriptDiv() {
    // Added in 5.8, and never worked properly.
    // Do nothing.
}

function reviveTranscript(){
  noTranscript = false;
}

function getDateAndTimeForTranscript(){
  return new Date().toLocaleString(navigator.language, { timeZoneName: 'short' });
}

function printTranscriptDateAndTime(){
  addTextAndScroll('<b>DATE AND TIME:</b> ' + getDateAndTimeForTranscript());
}

function loadTranscriptNameInputVal(){
  $('input#txtCommand').val(transcriptName);
}

// ***********************************

function disableMenuOutputSection (el){
  $('.' + el + ' .cmdlink').addClass('disabled').attr('onclick',null);
}


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

function Grid_DrawArrow(id, x1, y1, x2, y2, border, borderWidth) {
    if (!_canvasSupported) return;
    gridApi.drawArrow(id, parseFloat(x1), parseFloat(y1), parseFloat(x2), parseFloat(y2), border, parseInt(borderWidth));
}

function Grid_DrawPlayer(x, y, z, radius, border, borderWidth, fill) {
    if (!_canvasSupported) return;
    gridApi.drawPlayer(parseFloat(x), parseFloat(y), parseFloat(z), parseInt(radius), border, parseInt(borderWidth), fill);
}

function Grid_DrawLabel(x, y, z, text, col) {
    if (col === undefined) col = "black";
    if (!_canvasSupported) return;
    gridApi.drawLabel(parseFloat(x), parseFloat(y), parseFloat(z), text, col);
}

function Grid_ShowCustomLayer(visible) {
    if (!_canvasSupported) return;
    gridApi.showCustomLayer(visible == "true");
}

function Grid_ClearCustomLayer() {
    if (!_canvasSupported) return;
    gridApi.clearCustomLayer();
}

function Grid_ClearAllLayers() {
    if (!_canvasSupported) return;
    gridApi.clearAllLayers();
}

function Grid_SetCentre(x, y) {
    if (!_canvasSupported) return;
    gridApi.setCentre(parseFloat(x), parseFloat(y));
}

function Grid_DrawSquare(id, x, y, width, height, text, fill) {
    if (!_canvasSupported) return;
    gridApi.drawCustomLayerSquare(id, parseInt(x), parseInt(y), parseInt(width), parseInt(height), text, fill);
}

function Grid_LoadSvg(data, id) {
    if (!_canvasSupported) return;
    gridApi.loadSvg(data, id);
}

function Grid_DrawSvg(id, symbolId, x, y, width, height) {
    if (!_canvasSupported) return;
    gridApi.drawCustomLayerSvg(id, symbolId, parseInt(x), parseInt(y), parseInt(width), parseInt(height));
}

function Grid_DrawImage(id, url, x, y, width, height) {
    if (!_canvasSupported) return;
    gridApi.drawCustomLayerImage(id, url, parseInt(x), parseInt(y), parseInt(width), parseInt(height));
}

function Grid_AddNewShapePoint(x, y) {
    if (!_canvasSupported) return;
    gridApi.addNewShapePoint(x, y);
}

function Grid_DrawShape(id, border, fill, opacity) {
    if (!_canvasSupported) return;
    gridApi.drawShape(id, border, fill, opacity);
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

// JQUERY.TEXT-EFFECTS.JS ***************************************************************************************************************
// https://github.com/jaz303/jquery-grab-bag/blob/master/javascripts/jquery.text-effects.js

// (c) 2008 Jason Frame (jason@onehackoranother.com)
// Released under The MIT License.

(function ($) {

    function shuffle(a) {
        var i = a.length, j;
        while (i) {
            var j = Math.floor((i--) * Math.random());
            var t = a[i];
            a[i] = a[j];
            a[j] = t;
        }
    }

    function randomAlphaNum() {
        var rnd = Math.floor(Math.random() * 62);
        if (rnd >= 52) return String.fromCharCode(rnd - 4);
        else if (rnd >= 26) return String.fromCharCode(rnd + 71);
        else return String.fromCharCode(rnd + 65);
    }

    $.fn.rot13 = function () {
        this.each(function () {
            $(this).text($(this).text().replace(/[a-z0-9]/ig, function (chr) {
                var cc = chr.charCodeAt(0);
                if (cc >= 65 && cc <= 90) cc = 65 + ((cc - 52) % 26);
                else if (cc >= 97 && cc <= 122) cc = 97 + ((cc - 84) % 26);
                else if (cc >= 48 && cc <= 57) cc = 48 + ((cc - 43) % 10);
                return String.fromCharCode(cc);
            }));
        });
        return this;
    };

    $.fn.scrambledWriter = function () {
        this.each(function () {
            var $ele = $(this), str = $ele.text(), progress = 0, replace = /[^\s]/g,
                random = randomAlphaNum, inc = 3;
            $ele.text('');
            var timer = setInterval(function () {
                $ele.text(str.substring(0, progress) + str.substring(progress, str.length).replace(replace, random));
                progress += inc
                if (progress >= str.length + inc) clearInterval(timer);
            }, 100);
        });
        return this;
    };

    $.fn.typewriter = function (speed) {
        this.each(function () {
            var $ele = $(this), str = $ele.text(), progress = 0;
            $ele.text('');
            var timer = setInterval(function () {
                $ele.text(str.substring(0, progress++) + ((progress & 1) && progress < str.length ? '_' : ''));
                if (progress >= str.length) clearInterval(timer);
            }, speed);
        });
        return this;
    };

    $.fn.unscramble = function (speed, reveal) {
        this.each(function () {
            var $ele = $(this), str = $ele.text(), replace = /[^\s]/,
                state = [], choose = [], random = randomAlphaNum;

            for (var i = 0; i < str.length; i++) {
                if (str.charAt(i).match(replace)) {
                    state.push(random());
                    choose.push(i);
                } else {
                    state.push(str.charAt(i));
                }
            }

            shuffle(choose);
            $ele.text(state.join(''));

            var timer = setInterval(function () {
                var i, r = reveal;
                while (r-- && choose.length) {
                    i = choose.pop();
                    state[i] = str.charAt(i);
                }
                for (i = 0; i < choose.length; i++) state[choose[i]] = random();
                $ele.text(state.join(''));
                if (choose.length == 0) clearInterval(timer);
            }, speed);
        });
        return this;
    };

})(jQuery);

// JQUERY.INVIEW.JS *********************************************************************************************************************

/**
 * author Christopher Blum
 *    - based on the idea of Remy Sharp, http://remysharp.com/2009/01/26/element-in-view-event-plugin/
 *    - forked from http://github.com/zuk/jquery.inview/
 */
(function ($) {
    var inviewObjects = {}, viewportSize, viewportOffset,
        d = document, w = window, documentElement = d.documentElement, expando = $.expando, timer;

    $.event.special.inview = {
        add: function (data) {
            inviewObjects[data.guid + "-" + this[expando]] = { data: data, $element: $(this) };

            // Use setInterval in order to also make sure this captures elements within
            // "overflow:scroll" elements or elements that appeared in the dom tree due to
            // dom manipulation and reflow
            // old: $(window).scroll(checkInView);
            //
            // By the way, iOS (iPad, iPhone, ...) seems to not execute, or at least delays
            // intervals while the user scrolls. Therefore the inview event might fire a bit late there
            // 
            // Don't waste cycles with an interval until we get at least one element that
            // has bound to the inview event.  
            if (!timer && !$.isEmptyObject(inviewObjects)) {
                timer = setInterval(checkInView, 250);
            }
        },

        remove: function (data) {
            try { delete inviewObjects[data.guid + "-" + this[expando]]; } catch (e) { }

            // Clear interval when we no longer have any elements listening
            if ($.isEmptyObject(inviewObjects)) {
                clearInterval(timer);
                timer = null;
            }
        }
    };

    function getViewportSize() {
        var mode, domObject, size = { height: w.innerHeight, width: w.innerWidth };

        // if this is correct then return it. iPad has compat Mode, so will
        // go into check clientHeight/clientWidth (which has the wrong value).
        if (!size.height) {
            mode = d.compatMode;
            if (mode || !$.support.boxModel) { // IE, Gecko
                domObject = mode === 'CSS1Compat' ?
                    documentElement : // Standards
                  d.body; // Quirks
                size = {
                    height: domObject.clientHeight,
                    width: domObject.clientWidth
                };
            }
        }

        return size;
    }

    function getViewportOffset() {
        return {
            top: w.pageYOffset || documentElement.scrollTop || d.body.scrollTop,
            left: w.pageXOffset || documentElement.scrollLeft || d.body.scrollLeft
        };
    }

    function checkInView() {
        var $elements = $(), elementsLength, i = 0;

        $.each(inviewObjects, function (i, inviewObject) {
            var selector = inviewObject.data.selector,
                $element = inviewObject.$element;
            $elements = $elements.add(selector ? $element.find(selector) : $element);
        });

        elementsLength = $elements.length;
        if (elementsLength) {
            viewportSize = viewportSize || getViewportSize();
            viewportOffset = viewportOffset || getViewportOffset();

            for (; i < elementsLength; i++) {
                // Ignore elements that are not in the DOM tree
                if (!$.contains(documentElement, $elements[i])) {
                    continue;
                }

                var $element = $($elements[i]),
                    elementSize = { height: $element.height(), width: $element.width() },
                    elementOffset = $element.offset(),
                    inView = $element.data('inview'),
                    visiblePartX,
                    visiblePartY,
                    visiblePartsMerged;

                // Don't ask me why because I haven't figured out yet:
                // viewportOffset and viewportSize are sometimes suddenly null in Firefox 5.
                // Even though it sounds weird:
                // It seems that the execution of this function is interferred by the onresize/onscroll event
                // where viewportOffset and viewportSize are unset
                if (!viewportOffset || !viewportSize) {
                    return;
                }

                if (elementOffset.top + elementSize.height > viewportOffset.top &&
                    elementOffset.top < viewportOffset.top + viewportSize.height &&
                    elementOffset.left + elementSize.width > viewportOffset.left &&
                    elementOffset.left < viewportOffset.left + viewportSize.width) {
                    visiblePartX = (viewportOffset.left > elementOffset.left ?
                      'right' : (viewportOffset.left + viewportSize.width) < (elementOffset.left + elementSize.width) ?
                      'left' : 'both');
                    visiblePartY = (viewportOffset.top > elementOffset.top ?
                      'bottom' : (viewportOffset.top + viewportSize.height) < (elementOffset.top + elementSize.height) ?
                      'top' : 'both');
                    visiblePartsMerged = visiblePartX + "-" + visiblePartY;
                    if (!inView || inView !== visiblePartsMerged) {
                        $element.data('inview', visiblePartsMerged).trigger('inview', [true, visiblePartX, visiblePartY]);
                    }
                } else if (inView) {
                    $element.data('inview', false).trigger('inview', [false]);
                }
            }
        }
    }

    $(w).bind("scroll resize scrollstop", function () {
        viewportSize = viewportOffset = null;
    });

    // IE < 9 scrolls to focused elements without firing the "scroll" event
    if (!documentElement.addEventListener && documentElement.attachEvent) {
        documentElement.attachEvent("onfocusin", function () {
            viewportOffset = null;
        });
    }
})(jQuery);

// JJMENU.JS ****************************************************************************************************************************

/* Heavily modified, based on jjmenu 1.1.2 by Jacek Jursza (okhan.pl@gmail.com)
 * http://jursza.net/dev/jjmenu/
 *  
 * copyright (c) 2009 Jacek Jursza (http://jursza.net/)
 * licence MIT [http://www.opensource.org/licenses/mit-license.php]    
 */

(function ($) {
    
    $(document).click(function (event) { if (event.button != 2) $("div[id^=jjmenu]").remove(); });

    $.fn.jjmenu = function (param) {
        this.click(function (event) {
            event.preventDefault();
            event.stopPropagation();
            $(this).jjmenu_popup(param);
            $(this).blur();
            return false;
        });
    };
    
    $.fn.jjmenu_popup = function (param) {
        var el = this;

        $("div[id^=jjmenu_main]").remove();

        var m = document.createElement('div');
        var ms = document.createElement('span');
        $(m).append(ms);

        m.className = "jjmenu";
        m.id = "jjmenu_main";
        $(m).css({ display: 'none' });
        $(document.body).append(m);

        positionMenu();

        if (typeof param === "undefined") {
            var verbs = el.data("verbs");
            var text = el.html();
            var elementId = el.data("elementid");
            param = buildMenuOptions(verbs, text, elementId);
        }

        for (var i in param) {
            putItem(param[i]);
        }

        checkPosition();
        showMenu();

        function positionMenu() {
            var pos = $(el).offset();
            var t = pos.top;
            var l = pos.left;
            $(m).css({ position: "absolute", top: t + "px", left: l + "px" });
        }

        function checkPosition() {

            var isHidden = $(m).css("display") == "none";
            if (isHidden) $(m).show();

            var positionTop = $(m).offset().top;
            var positionLeft = $(m).offset().left;
            if (isHidden) $(m).hide();

            var xPos = positionTop - $(window).scrollTop();

            $(m).css({ left: "0px" });
            var menuHeight = $(m).outerHeight();
            var menuWidth = $(m).outerWidth();
            $(m).css({ left: positionLeft + "px" });

            var nleft = positionLeft;
            if (positionLeft + menuWidth > $(window).width()) {
                nleft = $(window).width() - menuWidth;
            }

            var spaceBottom = true;

            if (xPos + menuHeight + $(el).outerHeight() > $(window).height()) {
                spaceBottom = false;
            }

            var spaceTop = true;

            if (positionTop - menuHeight < 0) {
                spaceTop = false;
            }

            var ntop;

            if (!spaceBottom && spaceTop) {
                // top orientation
                ntop = parseInt(positionTop, 10) - parseInt(menuHeight, 10);
                $(m).addClass("topOriented");

            } else {
                // bottom orientation
                $(m).addClass("bottomOriented");
                positionTop = positionTop + $(el).outerHeight();
                ntop = parseInt(positionTop, 10);
            }

            $(m).css({ "top": ntop + "px", "left": nleft + "px" });
        }

        function showMenu() {
            var speed = 100;
            $(m).fadeIn(speed);
        }

        function putItem(n) {
            var item = document.createElement('div');
            $(item).hover(function () {
                $(this).addClass("jj_menu_item_hover");
            },
            function () {
                $(this).removeClass("jj_menu_item_hover");
            });

            $(item).click(function (event) {
                event.stopPropagation();
                $("div[id^=jjmenu]").remove();
                n.action.callback(n.title);
            });

            var span = document.createElement('span');
            $(item).append(span);

            item.className = "jj_menu_item";

            $(span).html(n.title);
            $(ms).append(item);
        }
    }

})(jQuery);

function whereAmI() {
  ASLEvent("WhereAmI", platform);
}
var platform = "desktop";
