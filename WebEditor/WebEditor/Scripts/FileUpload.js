$(function () {
    window.parent.registerFileUploadInit(fileUploadInit);
    window.parent.registerFileUploadSubmit(fileUploadSubmit);

    var postedFile = $("#PostedFile").val();
    if (postedFile.length > 0) {
        $("#PostedFile").val("");
        window.parent.filePosted(postedFile);
    }
});

function fileUploadInit(element, key, extensions) {
    $("#Key").val(element);
    $("#Attribute").val(key);

    $("#existingFiles").empty();
    var extensionsList = extensions.split(";");
    var filesList = $("#AllFiles").val().split(":");
    var anyFiles = false;

    $.each(filesList, function (index, value) {
        $.each(extensionsList, function (extIdx, extValue) {
            if (endsWith(value.toLowerCase(), extValue.toLowerCase())) {
                $("#existingFiles").append($("<option/>").attr("value", key).text(value));
                anyFiles = true;
            }
        });
    });

    if (anyFiles) {
        $("#file-upload-existing").show();
        $("#file-upload-new-header").show();
    }
}

function endsWith(str, suffix) {
    return str.indexOf(suffix, str.length - suffix.length) !== -1;
}

function fileUploadSubmit() {
    $("#file-upload-form").submit();
}