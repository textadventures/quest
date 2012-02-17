$(function () {
    $("#file-upload-submit").button().click(function () {
    });
    window.parent.registerFileUploadInit(fileUploadInit);
});

function fileUploadInit(element, key, extensions) {
    $("#Key").val(element);
    $("#Attribute").val(key);

    $("#existingFiles").empty();
    var extensionsList = extensions.split(";");
    var filesList = $("#AllFiles").val().split(":");

    $.each(filesList, function (index, value) {
        $.each(extensionsList, function (extIdx, extValue) {
            if (endsWith(value.toLowerCase(), extValue.toLowerCase())) {
                $("#existingFiles").append($("<option/>").attr("value", key).text(value));
            }
        });
    });
}

function endsWith(str, suffix) {
    return str.indexOf(suffix, str.length - suffix.length) !== -1;
}