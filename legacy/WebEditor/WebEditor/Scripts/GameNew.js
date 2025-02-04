$(function () {
    $("#submit-button").button();

    $("#SelectedType").change(function () {
        if ($(this).val() == "Gamebook") {
            $("#templateSection").hide();
        }
        else {
            $("#templateSection").show();
        }
    });
});