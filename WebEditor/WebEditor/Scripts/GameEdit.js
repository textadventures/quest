function initialiseElementEditor() {
    $("#elementEditorTabs").tabs();
    $("#centerPane").scrollTop(0);
    $(".stringlist-add").click(function () {
        $("#dialog-input-text-entry").val("");
        var key = $(this).attr("data-key");
        $("#dialog-input-text").data("dialog_ok", function () {
            alert(key + ", add: " + $("#dialog-input-text-entry").val());
        });
        $("#dialog-input-text").dialog("open");
    });
}