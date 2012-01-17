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
    $(".stringlist-add")
        .button()
        .click(function () {
            $("#dialog-input-text-entry").val("");
            $("#dialog-input-text-prompt").html($(this).attr("data-prompt") + ":");
            var key = $(this).attr("data-key");
            $("#dialog-input-text").data("dialog_ok", function () {
                sendAdditionalAction("stringlist add " + key + ";" + $("#dialog-input-text-entry").val());
            });
            $("#dialog-input-text").dialog("open");
        });
    $(".stringlist-edit")
        .button()
        .click(function () {
            stringListEdit($(this).attr("data-key"), $(this).attr("data-prompt"));
        });
    $(".stringlist-delete")
        .button()
        .click(function () {
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
    $(".script-add")
        .button()
        .click(function () {
            var key = $(this).attr("data-key");
            $("#dialog-add-script").data("dialog_ok", function () {
                // TO DO: Replace "msg (\"\")" with "create" string for selected script
                sendAdditionalAction("script add " + key + ";msg (\"\")");
            });
            $("#dialog-add-script").dialog("open");
        });
    $(".script-delete")
        .button()
        .click(function () {
        });
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