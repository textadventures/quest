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
    $(".stringlist-add").click(function () {
        $("#dialog-input-text-entry").val("");
        $("#dialog-input-text-prompt").html($(this).attr("data-prompt") + ":");
        var key = $(this).attr("data-key");
        $("#dialog-input-text").data("dialog_ok", function () {
            sendAdditionalAction("stringlist add " + key + ";" + $("#dialog-input-text-entry").val());
        });
        $("#dialog-input-text").dialog("open");
    });
}

function sendAdditionalAction(action) {
    $("#_additionalAction").val(action);
    $("#_additionalActionTab").val($("#elementEditorTabs").tabs('option', 'selected'));
    $("#elementEditorSave").submit();
}