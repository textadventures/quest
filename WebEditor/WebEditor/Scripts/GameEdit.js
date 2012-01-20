function initialiseElementEditor(tab) {
    var selectTab = $("#_additionalActionTab").val();
    $("#elementEditorTabs").tabs({
        create: function () {
            if (selectTab && selectTab.length > 0) {
                $("#elementEditorTabs").tabs("select", parseInt(selectTab));
            }
        }
    });
    $("#centerPane").scrollTop(0);
    $(".stringlist-add").button().click(function () {
        $("#dialog-input-text-entry").val("");
        $("#dialog-input-text-prompt").html($(this).attr("data-prompt") + ":");
        var key = $(this).attr("data-key");
        $("#dialog-input-text").data("dialog_ok", function () {
            sendAdditionalAction("stringlist add " + key + ";" + $("#dialog-input-text-entry").val());
        });
        $("#dialog-input-text").dialog("open");
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
        $("#_ignoreExpression").val(key);
        sendAdditionalAction("script settemplate " + key + ";" + text);
    });
}

function getSelectedScripts(key) {
    var result = "";
    $(".script-select").each(function (index, element) {
        var e = $(element);
        var id = e.attr("id");
        var checkboxKey = e.attr("data-key");
        if (checkboxKey == key && e.is(":checked")) {
            if (result.length > 0) result += ";";
            result += id.substring(10 + key.length);
        }
    });
    return result;
}

function getSelectedIfSections(key) {
    var result = "";
    $(".ifsection-select").each(function (index, element) {
        var e = $(element);
        var id = e.attr("id");
        var checkboxKey = e.attr("data-key");
        if (checkboxKey == key && e.is(":checked")) {
            if (result.length > 0) result += ";";
            result += id.substring(17 + key.length);
        }
    });
    return result;
}

function stringListEdit(key, prompt) {
    var selectElement = $("#select-" + key + " option:selected");
    var oldValue = selectElement.text();
    $("#dialog-input-text-entry").val(oldValue);
    $("#dialog-input-text-prompt").html(prompt + ":");
    $("#dialog-input-text").data("dialog_ok", function () {
        var newValue = $("#dialog-input-text-entry").val();
        if (oldValue != newValue) {
            sendAdditionalAction("stringlist edit " + key + ";" + selectElement.val() + ";" + newValue);
        }
    });
    $("#dialog-input-text").dialog("open");
}

function sendAdditionalAction(action) {
    $("#_additionalAction").val(action);
    $("#_additionalActionTab").val($("#elementEditorTabs").tabs('option', 'selected'));
    $("#elementEditorSave").submit();
}