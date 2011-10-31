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
        });
    $(".stringlist").dblclick(function () {
        stringListEdit($(this).attr("data-key"), $(this).attr("data-prompt"));
    });
}

function stringListEdit(key, prompt) {
    var selectElement = $("#select-" + key  + " option:selected");
    $("#dialog-input-text-entry").val(selectElement.text());
    $("#dialog-input-text-prompt").html(prompt + ":");
    $("#dialog-input-text").data("dialog_ok", function () {
        // TO DO: Don't send if old value = new value
        sendAdditionalAction("stringlist edit " + key + ";" + selectElement.val() + ";" + $("#dialog-input-text-entry").val());
    });
    $("#dialog-input-text").dialog("open");
}

function sendAdditionalAction(action) {
    $("#_additionalAction").val(action);
    $("#_additionalActionTab").val($("#elementEditorTabs").tabs('option', 'selected'));
    $("#elementEditorSave").submit();
}