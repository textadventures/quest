function initialiseDialogBoxes() {
    $("#dialog-input-text").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: 400,
        buttons: {
            "OK": function () {
                $(this).dialog("close");
                $(this).data("dialog_ok")();
            },
            "Cancel": function () {
                $(this).dialog("close");
            }
        }
    });
    $("#dialog-input-text").keyup(function (e) {
        if (e.keyCode == 13) {
            $(this).dialog("close");
            $(this).data("dialog_ok")();
        }
    });
    $("#dialog-add-script").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: 400,
        buttons: {
            "OK": function () {
                $(this).dialog("close");
                $(this).data("dialog_ok")();
            },
            "Cancel": function () {
                $(this).dialog("close");
            }
        }
    });
    $("#dialog-error").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: 400,
        buttons: {
            "OK": function () {
                $(this).dialog("close");
            }
        }
    });
}

function showDialog(prompt, defaultText, ok, list, listPrompt) {
    $("#dialog-input-text-entry").val(defaultText);
    $("#dialog-input-text-prompt").html(prompt + ":");
    var showList = false;
    var parent = "";
    if (typeof list != "undefined") {
        showList = (list.length > 1);
        if (list.length == 1) {
            parent = list[0];
        }
    }
    if (showList) {
        $("#dialog-input-text-list-prompt").html(listPrompt + ":");
        $("#dialog-input-text-options").empty();
        $.each(list, function (index, value) {
            $("#dialog-input-text-options").append($("<option/>").text(value));
        });
        $("#dialog-input-text-list").show();
    }
    else {
        $("#dialog-input-text-list").hide();
    }
    $("#dialog-input-text").data("dialog_ok", function () {
        if (showList) {
            parent = $("#dialog-input-text-options option:selected").text();
        }
        ok($("#dialog-input-text-entry").val(), parent);
    });
    $("#dialog-input-text").dialog("open");
}

function initialiseButtons() {
    $("#button-addroom").button().click(function () {
        showDialog("Please enter a name for the new room", "", function (text) {
            toplevelAdditionalAction("main addroom " + text);
        })
    });
    $("#button-addobject").button().click(function () {
        var possibleParents = $("#_newObjectPossibleParents").val().split(";");
        showDialog("Please enter a name for the new object", "", function (text, parent) {
            toplevelAdditionalAction("main addobject " + text + ";" + parent);
        }, possibleParents, "Parent")
    });
    $("#button-delete").button().click(function () {
        toplevelAdditionalAction("main delete");
    });
    $("#button-undo").button().click(function () {
        toplevelAdditionalAction("main undo");
    });
    $("#button-redo").button().click(function () {
        toplevelAdditionalAction("main redo");
    });
}

function initialiseElementEditor(tab) {
    var selectTab = $("#_additionalActionTab").val();
    var refreshTreeSelectElement = $("#_refreshTreeSelectElement").val();
    if (refreshTreeSelectElement.length > 0) {
        refreshTree(refreshTreeSelectElement);
        return;
    }
    $("#elementEditorTabs").tabs({
        create: function () {
            if (selectTab && selectTab.length > 0) {
                $("#elementEditorTabs").tabs("select", parseInt(selectTab));
            }
        }
    });
    $("#centerPane").scrollTop(0);
    $(".stringlist-add").button().click(function () {
        var key = $(this).attr("data-key");
        showDialog($(this).attr("data-prompt"), "", function (text) {
            sendAdditionalAction("stringlist add " + key + ";" + text);
        });
    });
    $(".stringlist-edit").button().click(function () {
        stringListEdit($(this).attr("data-key"), $(this).attr("data-prompt"));
    });
    $(".stringlist-delete").button().click(function () {
        var key = $(this).attr("data-key");
        var selectElement = $("#select-" + key + " option:selected");
        sendAdditionalAction("stringlist delete " + key + ";" + selectElement.val());
    });
    $(".stringlist").dblclick(function () {
        stringListEdit($(this).attr("data-key"), $(this).attr("data-prompt"));
    });
    $(".stringlist-edit").each(function () {
        $(this).attr("disabled", "disabled");
    });
    $(".stringlist-delete").each(function () {
        $(this).attr("disabled", "disabled");
    });
    $(".stringlist").change(function () {
        var editButton = $("#stringlist-" + $(this).attr("data-key") + "-edit");
        var deleteButton = $("#stringlist-" + $(this).attr("data-key") + "-delete");
        var selectElement = $("#" + this.id + " option:selected");
        if (selectElement.val() === undefined) {
            editButton.attr("disabled", "disabled");
            deleteButton.attr("disabled", "disabled");
        }
        else {
            editButton.removeAttr("disabled");
            deleteButton.removeAttr("disabled");
        }
    });
    $(".script-add").button().click(function () {
        var key = $(this).attr("data-key");
        $("#dialog-add-script").data("dialog_ok", function () {
            var create = $("#dialog-add-script-form input[type='radio']:checked").val();
            sendAdditionalAction("script add " + key + ";" + create);
        });
        $("#dialog-add-script").dialog("open");
        $("#dialog-add-script-accordion").accordion({
            autoHeight: false
        });
    });
    $(".script-delete").button().click(function () {
        var key = $(this).attr("data-key");
        var selected = getSelectedScripts(key);
        if (selected.length > 0) {
            sendAdditionalAction("script delete " + key + ";" + selected);
        }
    });
    $(".script-if-add-else").button().click(function () {
        var key = $(this).attr("data-key");
        sendAdditionalAction("script addelse " + key );
    });
    $(".script-if-add-elseif").button().click(function () {
        var key = $(this).attr("data-key");
        sendAdditionalAction("script addelseif " + key);
    });
    $(".script-select").click(function () {
        var key = $(this).attr("data-key");
        var selectedScripts = getSelectedScripts(key);
        if (selectedScripts.length > 0) {
            $("#script-toolbar-" + key).show(200);
        }
        else {
            $("#script-toolbar-" + key).hide(200);
        }
    });
    $(".ifsection-select").click(function () {
        var key = $(this).attr("data-key");
        var selectedSections = getSelectedIfSections(key);
        if (selectedSections.length > 0) {
            $("#ifsection-toolbar-" + key).show(200);
        }
        else {
            $("#ifsection-toolbar-" + key).hide(200);
        }
    });
    $(".ifsection-delete").button().click(function () {
        var key = $(this).attr("data-key");
        sendAdditionalAction("script deleteifsection " + key + ";" + getSelectedIfSections(key));
    });
    $(".expression-dropdown").change(function () {
        var key = $(this).attr("data-key");
        var showExpression = ($(this).find('option:selected').text() == "expression");
        if (showExpression) {
            $("#" + key + "-simpleeditor").hide();
            $("#" + key + "-expressioneditor").show();
        }
        else {
            $("#" + key + "-expressioneditor").hide();
            $("#" + key + "-simpleeditor").show();
        }
    });
    $(".template-dropdown").change(function () {
        var key = $(this).attr("data-key");
        var text = $(this).find('option:selected').text();
        if (text == "expression") {
            $("#" + key + "-templateeditor").hide();
            $("#" + key).show();
        }
        else {
            $("#_ignoreExpression").val(key);
            sendAdditionalAction("script settemplate " + key + ";" + text);
        }
    });
    $(".script-dictionary-add").button().click(function () {
        var key = $(this).attr("data-key");
        showDialog($(this).attr("data-prompt"), "", function (text) {
            sendAdditionalAction("scriptdictionary add " + key + ";" + text);
        });
    });
    $(".error-clear").button().click(function () {
        var key = $(this).attr("data-key");
        sendAdditionalAction("error clear " + key);
    });
    $(".scriptDictionarySection-select").click(function () {
        var key = $(this).attr("data-key");
        var selectedSections = getSelectedScriptDictionaryItems(key);
        if (selectedSections.length > 0) {
            $("#scriptDictionarySection-toolbar-" + key).show(200);
        }
        else {
            $("#scriptDictionarySection-toolbar-" + key).hide(200);
        }
    });
    $(".scriptDictionarySection-delete").button().click(function () {
        var key = $(this).attr("data-key");
        sendAdditionalAction("scriptdictionary delete " + key + ";" + getSelectedScriptDictionaryItems(key));
    });
    $('textarea.richtext').tinymce({
        script_url: '../../Scripts/tiny_mce/tiny_mce.js',
        theme: "advanced",
        plugins: "inlinepopups,searchreplace,paste,directionality",
        theme_advanced_buttons1: "bold,italic,underline",
        theme_advanced_buttons2: "",
        theme_advanced_buttons3: "",
        theme_advanced_toolbar_location: "top",
        theme_advanced_toolbar_align: "left",
        theme_advanced_statusbar_location: "none",
        forced_root_block: "",
        force_br_newlines : true,
        force_p_newlines : false,
        gecko_spellcheck: true
    });
    $(".multi-dropdown").change(function () {
        var key = $(this).attr("data-key");
        var value = $(this).find('option:selected').attr("value");
        sendAdditionalAction("multi set " + key + ";" + value);
    });
    $(".types-dropdown").change(function () {
        var key = $(this).attr("data-key");
        var value = $(this).find('option:selected').attr("value");
        sendAdditionalAction("types set " + key + ";" + value);
    });

    var popupError = $("#_popupError").val();
    if (popupError.length > 0) {
        $("#dialog-error-message").html(popupError);
        $("#dialog-error").dialog("open");
    }
}

function getSelectedScripts(key) {
    return getSelectedItems(key, ".script-select", 10);
}

function getSelectedIfSections(key) {
    return getSelectedItems(key, ".ifsection-select", 17);
}

function getSelectedScriptDictionaryItems(key) {
    return getSelectedItems(key, ".scriptDictionarySection-select", 18);
}

function getSelectedItems(key, checkboxClass, prefixLength) {
    var result = "";
    $(checkboxClass).each(function (index, element) {
        var e = $(element);
        var id = e.attr("id");
        var checkboxKey = e.attr("data-key");
        if (checkboxKey == key && e.is(":checked")) {
            if (result.length > 0) result += ";";
            result += id.substring(prefixLength + key.length);
        }
    });
    return result;
}

function stringListEdit(key, prompt) {
    var selectElement = $("#select-" + key + " option:selected");
    var oldValue = selectElement.text();
    showDialog(prompt, oldValue, function (newValue) {
        if (oldValue != newValue) {
            sendAdditionalAction("stringlist edit " + key + ";" + selectElement.val() + ";" + newValue);
        }
    })
}

function sendAdditionalAction(action) {
    $("#_additionalAction").val(action);
    $("#_additionalActionTab").val($("#elementEditorTabs").tabs('option', 'selected'));
    $("#elementEditorSave").submit();
}