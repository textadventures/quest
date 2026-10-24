// QuestUI for the legacy UI: the 610 UI contract (docs/ui-v2-contract.md) implemented as a thin
// layer over playercore.js/player.js. 610 games play on this UI until v2 becomes their default,
// and games with game.classicinterface play on it for good, so 610 Core's JS.QuestUI.* calls must
// work here too.
//
// Contract rules that shape this file:
// - Unknown names (parts, panes, tokens, kinds, events) are ignored, never an error.
// - Calls describe intent. Where legacy has no equivalent (e.g. the status-bar part), the call is
//   accepted and does nothing.
// - Additions only: nothing here may be removed or change meaning once 6.1 ships.
//
// Legacy's existing behaviour doesn't change: pre-610 games never call QuestUI. The only edits
// outside this file are the data-qv-part attributes in playercore.htm, the map-interaction check
// in grid.js, and wasm-player.js's reset and turn hooks.

(function () {
    "use strict";

    // Each pane is a label/accordion pair, or a single div for the command and custom status
    // panes. Author panes from addPane() are added to this map.
    var builtInPanes = {
        inventory: ["#inventoryLabel", "#inventoryAccordion"],
        objects: ["#placesObjectsLabel", "#placesObjectsAccordion"],
        status: ["#statusVarsLabel", "#statusVarsAccordion"],
        compass: ["#compassLabel", "#compassAccordion"],
        commands: ["#commandPane"],
        customstatus: ["#customStatusPane"]
    };
    var authorPanes = {};

    function paneSelectors(name) {
        return builtInPanes[name] || authorPanes[name] || null;
    }

    // jQuery's .show() sets an inline display, which a stylesheet rule can't beat unless it's
    // !important. So a part the game hid stays hidden even when legacy code later re-shows it
    // (updateStatus re-showing the status pane, setPanelContents the picture frame, ...).
    var forceHiddenClass = "qv-force-hidden";
    var forceHiddenStyle = document.createElement("style");
    forceHiddenStyle.textContent = "." + forceHiddenClass + " { display: none !important; }";
    document.head.appendChild(forceHiddenStyle);

    var simpleParts = {
        output: "#divOutput",
        exits: "#qv-exits",
        "picture-frame": "#gamePanel",
        map: "#gridPanel"
    };

    function setForceHidden(selector, hidden) {
        $(selector).toggleClass(forceHiddenClass, hidden);
    }

    // ── Events ──────────────────────────────────────────────────────────────

    var listeners = {};

    function emit(event, detail) {
        var set = listeners[event];
        if (!set) return;
        set.forEach(function (fn) {
            try {
                fn(detail);
            } catch (e) {
                console.error(e);
            }
        });
    }

    // Wrapping the globals rather than editing each call site: playercore.js and the host's JS
    // imports look these up by name at call time, so the wrappers see every call.
    function wrapGlobal(name, after) {
        var original = window[name];
        if (typeof original !== "function") return;
        window[name] = function () {
            var result = original.apply(this, arguments);
            after.apply(this, arguments);
            return result;
        };
    }

    var autoScroll = true;
    var fadeOutput = false;

    // Called by wasm-player.js around each engine round trip the player starts: a command, an
    // event, or answering a wait, menu or question. Timer ticks don't count as turns.
    window.notifyQuestUITurn = function (started) {
        if (started) {
            if (fadeOutput) $("#divOutput").children().addClass("qv-previous-output");
            emit("turnStarted");
        } else {
            emit("turnEnded");
        }
    };
    wrapGlobal("addText", function (html) {
        emit("outputAdded", { html: html });
    });
    wrapGlobal("updateLocation", function (text) {
        emit("locationChanged", { location: text });
    });

    var originalScrollToEnd = window.scrollToEnd;
    window.scrollToEnd = function () {
        if (autoScroll) originalScrollToEnd.apply(this, arguments);
    };

    var continueOnClick = false;
    document.addEventListener("click", function (e) {
        // A click on a link or control inside the output keeps its own meaning.
        if (!continueOnClick || !_waitMode) return;
        if ($(e.target).closest("a, button, input, textarea, select").length) return;
        endWait();
    });

    // ── Link markup ─────────────────────────────────────────────────────────

    // ShowMenu's fallback links in 610 Core: <a class="cmdlink" data-qv-event="..." data-qv-param="...">.
    // Bound now, before playercore.js's document-ready handlers, so it runs ahead of the
    // "disable every bare ShowMenu link" handler, which would otherwise mark this link disabled
    // before it could fire.
    $(document).on("click", "a[data-qv-event]", function (e) {
        e.preventDefault();
        var $this = $(this);
        if ($this.hasClass("disabled")) return;
        ASLEvent($this.attr("data-qv-event"), $this.attr("data-qv-param") || "");
    });

    // {popup:} in 610 Core: a trigger button plus a content element, with no inline handlers.
    $(document).on("click", "[data-qv-popup]", function (e) {
        e.preventDefault();
        $(document.getElementById($(this).attr("data-qv-popup"))).addClass("qv-popup-open");
    });
    $(document).on("click", "[data-qv-popup-content]", function () {
        $(this).removeClass("qv-popup-open");
    });

    // ── Theming ─────────────────────────────────────────────────────────────

    function px(value) {
        return /^-?[\d.]+$/.test(value) ? value + "px" : value;
    }

    // Each recognised token is also translated into the legacy call that styled the same thing
    // before the contract existed. Order matters where legacy's calls depend on each other:
    // setBackground reads the opacity SetBackgroundOpacity stored.
    function applyLegacyTokens(t) {
        var body = $("body");
        if (t["--qv-bg-opacity"]) SetBackgroundOpacity(parseFloat(t["--qv-bg-opacity"]));
        if (t["--qv-bg"]) setBackground(t["--qv-bg"]);
        if (t["--qv-text"]) body.css("color", t["--qv-text"]);
        if (t["--qv-font-body"]) body.css("font-family", t["--qv-font-body"]);
        if (t["--qv-font-size"]) body.css("font-size", t["--qv-font-size"]);
        if (t["--qv-link"]) addCSSRule("a.cmdlink").style.color = t["--qv-link"];
        if (t["--qv-link-decoration"]) addCSSRule("a.cmdlink").style.textDecoration = t["--qv-link-decoration"];
        if (t["--qv-page-bg"]) body.css("background-color", t["--qv-page-bg"]);
        if (t["--qv-bg-image"]) body.css("background-image", t["--qv-bg-image"]);
        if (t["--qv-bg-blend-top"] && t["--qv-bg-blend-bottom"]) {
            colourBlend(t["--qv-bg-blend-top"], t["--qv-bg-blend-bottom"]);
        }
        if (t["--qv-output-width"]) setGameWidth(parseFloat(t["--qv-output-width"]));
        ["top", "bottom", "left", "right"].forEach(function (side) {
            var value = t["--qv-padding-" + side];
            if (value) $("#gameContent").css("padding-" + side, px(value));
        });
        if (t["--qv-border-colour"]) {
            $("#gameBorder").css({
                "border-left": "1px solid " + t["--qv-border-colour"],
                "border-right": "1px solid " + t["--qv-border-colour"]
            });
        }

        // setPanes takes 2, 4 or 5 colours: with 2 it swaps them for the headings, and below 5
        // it defaults the highlight to orange.
        if (t["--qv-pane-fg"] && t["--qv-pane-bg"]) {
            var paneArgs = [t["--qv-pane-fg"], t["--qv-pane-bg"]];
            var headingFg = t["--qv-pane-heading-fg"] || t["--qv-pane-bg"];
            var headingBg = t["--qv-pane-heading-bg"] || t["--qv-pane-fg"];
            if (t["--qv-pane-heading-fg"] || t["--qv-pane-heading-bg"] || t["--qv-pane-highlight"]) {
                paneArgs.push(headingFg, headingBg);
            }
            if (t["--qv-pane-highlight"]) paneArgs.push(t["--qv-pane-highlight"]);
            setPanes.apply(null, paneArgs);
        }

        var status = $("#qv-status");
        if (t["--qv-status-bg"]) status.css("background", t["--qv-status-bg"]);
        if (t["--qv-status-fg"]) status.css("color", t["--qv-status-fg"]);
        if (t["--qv-status-border"]) status.css("border", t["--qv-status-border"]);
        if (t["--qv-status-image"]) {
            if (t["--qv-status-image"] === "none") {
                status.css("background-image", "none");
            } else {
                // As the location bar image always did: on every jQuery UI header, so the pane
                // headings get it too.
                $(".ui-widget-header").css("background-image", t["--qv-status-image"]);
            }
        }

        if (t["--qv-menu-bg"]) SetMenuBackground(t["--qv-menu-bg"]);
        if (t["--qv-menu-fg"]) SetMenuForeground(t["--qv-menu-fg"]);
        if (t["--qv-menu-hover-bg"]) SetMenuHoverBackground(t["--qv-menu-hover-bg"]);
        if (t["--qv-menu-hover-fg"]) SetMenuHoverForeground(t["--qv-menu-hover-fg"]);
        if (t["--qv-menu-font"]) SetMenuFontName(t["--qv-menu-font"]);
        if (t["--qv-menu-font-size"]) SetMenuFontSize(t["--qv-menu-font-size"]);

        if (t["--qv-command-fg"]) $("#txtCommandDiv, #txtCommand").css("color", t["--qv-command-fg"]);
        if (t["--qv-command-bg"]) $("#txtCommand").css("background-color", t["--qv-command-bg"]);
    }

    var commandBarPresets = {
        borderless: "#txtCommand { outline:none; border:none; margin:0; padding:0; width:100px; background-color:transparent; }",
        shadow: "#txtCommand { outline:none; border:none; margin:2px; padding:7px; box-shadow: 0px 0px 15px 5px #888; }"
    };

    // ── Map ─────────────────────────────────────────────────────────────────

    // Same arguments as the Grid_* functions, in the same order: frozen as they are today.
    var map = {
        show: function (size) { ShowGrid(size); },
        setInteractive: function (on) { setMapInteractive(on); },
        respondToClicks: function (on) { respondToGridClicks(!!on); },
        setScale: function () { Grid_SetScale.apply(null, arguments); },
        setCentre: function () { Grid_SetCentre.apply(null, arguments); },
        drawBox: function () { Grid_DrawBox.apply(null, arguments); },
        drawLabel: function () { Grid_DrawLabel.apply(null, arguments); },
        drawPlayer: function () { Grid_DrawPlayer.apply(null, arguments); },
        drawLine: function () { Grid_DrawLine.apply(null, arguments); },
        drawArrow: function () { Grid_DrawArrow.apply(null, arguments); },
        drawSquare: function () { Grid_DrawSquare.apply(null, arguments); },
        drawShape: function () { Grid_DrawShape.apply(null, arguments); },
        addShapePoint: function () { Grid_AddNewShapePoint.apply(null, arguments); },
        drawImage: function () { Grid_DrawImage.apply(null, arguments); },
        drawSvg: function () { Grid_DrawSvg.apply(null, arguments); },
        loadSvg: function () { Grid_LoadSvg.apply(null, arguments); },
        drawGridLines: function () { Grid_DrawGridLines.apply(null, arguments); },
        showCustomLayer: function (visible) { Grid_ShowCustomLayer(visible); },
        clearCustomLayer: function () { Grid_ClearCustomLayer(); },
        clearAllLayers: function () { Grid_ClearAllLayers(); }
    };

    function setMapInteractive(on) {
        // Checked by grid.js's zoom and drag handlers.
        window._qvMapInteractive = !!on;
    }

    // ── Transcript ──────────────────────────────────────────────────────────

    var transcript = {
        write: function (html) { writeToTranscript(html); },
        enable: function (name) { enableTranscript(name); },
        disable: function () { disableTranscript(); },
        kill: function () { killTranscript(); },
        revive: function () { reviveTranscript(); },
        setName: function (name) { transcriptName = name; },
        setPrefix: function (text) { replaceTranscriptString(text); },
        stampDateAndTime: function () { printTranscriptDateAndTime(); },
        promptForName: function () { loadTranscriptNameInputVal(); },
        show: function (message, linkText) { showTranscript(message, linkText); }
    };

    // ── Panes ───────────────────────────────────────────────────────────────

    function paneDomId(id) {
        return "qv-pane-" + String(id).replace(/[^A-Za-z0-9_-]/g, "_");
    }

    // Built like the stock panes (see playercore.htm), taking jQuery UI's classes from the
    // inventory pane, so an author pane looks and behaves like its neighbours.
    function addPane(id, title, html) {
        if (!id || paneSelectors(id)) return;
        var domId = paneDomId(id);
        var label = $("<h3/>", { id: domId + "-label", "class": $("#inventoryLabel").attr("class") });
        var button = $("<button/>", {
            type: "button",
            "class": "accordion-header-text",
            "aria-controls": domId,
            "aria-expanded": "true"
        }).text(title || id);
        label.append(button);
        var content = $("<div/>", {
            id: domId,
            "class": $("#inventoryAccordion").attr("class"),
            "data-qv-part": "pane",
            "data-qv-pane": id
        }).html(html || "");
        button.on("click", function () {
            var open = button.attr("aria-expanded") === "true";
            button.attr("aria-expanded", open ? "false" : "true");
            content.toggle(!open);
        });
        $("#gamePanesRunning").append(label, content);
        authorPanes[id] = ["#" + domId + "-label", "#" + domId];
    }

    // ── QuestUI ─────────────────────────────────────────────────────────────

    window.QuestUI = {
        // Engine
        sendCommand: function (text) { sendCommand(text); },
        sendEvent: function (name, param) { ASLEvent(name, param); },
        restart: function () { RestartGame(); },

        // Output
        addText: function (html) { addText(html); },
        clearScreen: function () { clearScreen(); },
        setAlignment: function (align) { createNewDiv(align); },
        startSection: function (name) { StartOutputSection(name); },
        endSection: function (name) { EndOutputSection(name); },
        hideSection: function (name) { HideOutputSection(name); },
        disableSection: function (name) { disableMenuOutputSection(name); },
        disableCommandLinks: function () { disableAllCommandLinks(); },
        updateLinks: function (kind, data) {
            if (kind === "object") updateObjectLinks(data);
            else if (kind === "exit") updateExitLinks(data);
            else if (kind === "command") updateCommandLinks(data);
        },
        scrollToEnd: function () { scrollToEnd(); },
        addYouTube: function (id) { AddYouTube(id); },
        addVimeo: function (id) { AddVimeo(id); },
        textEffect: function (kind) {
            var args = Array.prototype.slice.call(arguments, 1);
            if (kind === "typewriter") TextFX.Typewriter.apply(TextFX, args);
            else if (kind === "unscramble") TextFX.Unscramble.apply(TextFX, args);
        },

        // Layout and panes
        setVisible: function (part, visible) {
            visible = !!visible;
            if (part === "panes") {
                panesVisible(visible);
            } else if (part === "location") {
                visible ? uiShow("#location") : uiHide("#location");
            } else if (part === "command-bar") {
                visible ? uiShow("#txtCommandDiv") : uiHide("#txtCommandDiv");
            } else if (part === "border") {
                $("#gameBorder").css("border", visible ? "" : "none");
            } else if (simpleParts[part]) {
                setForceHidden(simpleParts[part], !visible);
            } else {
                var selectors = paneSelectors(part);
                if (!selectors) return;
                var all = selectors.join(", ");
                setForceHidden(all, !visible);
                // The status pane shows itself whenever there's status text, so showing it
                // only lifts the override; the others start hidden and need showing.
                if (visible && part !== "status") $(all).show();
            }
            updateExitStrip();
        },
        setPaneOrder: function (parts) {
            if (typeof parts === "string") parts = parts.split(";");
            var container = $("#gamePanesRunning");
            for (var i = parts.length - 1; i >= 0; i--) {
                var selectors = paneSelectors(parts[i]);
                if (selectors) container.prepend($(selectors.join(", ")));
            }
        },
        setLayoutPreset: function (name, narrow) {
            if (name === "gamebook") {
                panesVisible(false);
                uiHide("#location");
                uiHide("#txtCommandDiv");
            }
            // Legacy has one layout for every width, so of the other presets only "no panes at
            // any width" means anything here.
            else if (name === "nopanes" && (narrow == null || narrow === "nopanes")) {
                panesVisible(false);
            }
        },
        setPictureFrame: function (html) { setPanelContents(html); },
        setInterfaceString: function (name, text) { setInterfaceString(name, text); },
        setCompassDirections: function (list) { setCompassDirections(list); },
        setLocation: function (text) { updateLocation(text); },
        setStatusAttributes: function (items) {
            if (typeof items === "string") items = JSON.parse(items);
            updateStatus((items || []).map(function (item) {
                return item.text != null ? item.text : "";
            }).join("<br/>"));
        },

        // Theming
        setTokens: function (tokens) {
            if (typeof tokens === "string") tokens = JSON.parse(tokens);
            var root = document.documentElement;
            var applied = {};
            Object.keys(tokens || {}).forEach(function (name) {
                var value = tokens[name] == null ? "" : String(tokens[name]);
                if (name.indexOf("--") !== 0) return;
                if (value === "") {
                    root.style.removeProperty(name);
                } else {
                    root.style.setProperty(name, value);
                    applied[name] = value;
                }
            });
            applyLegacyTokens(applied);
        },
        setCommandBarStyle: function (css) { setCommandBarStyle(css); },
        setCommandBarPreset: function (name) {
            if (commandBarPresets[name]) addScript("<style>" + commandBarPresets[name] + "</style>");
        },
        setCommandPrompt: function (text) { $("#txtCommandPrompt").html(text); },
        addStylesheet: function (url) { addExternalStylesheet(url); },

        map: map,
        transcript: transcript,

        // Author hooks
        addPane: addPane,
        setPaneContent: function (id, html) {
            if (id === "customstatus") {
                setCustomStatus(html);
            } else if (authorPanes[id]) {
                $("#" + paneDomId(id)).html(html);
            }
        },
        setCommands: function (list) {
            setCommands(Array.isArray(list) ? list.join(";") : list);
        },
        setAutoScroll: function (on) { autoScroll = !!on; },
        setContinueOnClick: function (on) { continueOnClick = !!on; },
        fadePreviousOutput: function (on) {
            fadeOutput = !!on;
            if (!fadeOutput) $("#divOutput .qv-previous-output").removeClass("qv-previous-output");
        },
        setCommandPlaceholder: function (text) { $("#txtCommand").attr("placeholder", text); },
        setMapInteractive: setMapInteractive,
        on: function (event, fn) {
            if (typeof fn !== "function") return;
            (listeners[event] = listeners[event] || new Set()).add(fn);
        },
        off: function (event, fn) {
            if (listeners[event]) listeners[event].delete(fn);
        }
    };

    // Called by wasm-player.js whenever it swaps in a fresh playercore.htm for a new game or a
    // restart. The game's InitInterface and its own scripts run again afterwards and set all of
    // this up afresh, so anything kept from the previous run would be applied twice.
    window.resetQuestUIState = function () {
        authorPanes = {};
        listeners = {};
        autoScroll = true;
        fadeOutput = false;
        continueOnClick = false;
        window._qvMapInteractive = true;
        var root = document.documentElement;
        for (var i = root.style.length - 1; i >= 0; i--) {
            var name = root.style[i];
            if (name.indexOf("--") === 0) root.style.removeProperty(name);
        }
    };
})();
