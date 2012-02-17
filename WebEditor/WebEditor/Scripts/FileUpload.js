$(function () {
    $("#File").change(function () {
        var value = $("#File").val();
        if (value.length == 0) return;
        var extensions = $("#file-upload-new").attr("data-extensions");
        var extensionsList = extensions.split(";");
        var ok = false;
        $.each(extensionsList, function (extIdx, extValue) {
            if (endsWith(value.toLowerCase(), extValue.toLowerCase())) {
                ok = true;
            }
        });
        if (ok) {
            $("#file-upload-new-error").html("");
        }
        else {
            $("#File").val("");
            $("#file-upload-new-error").html("Only the following file types are permitted: " + extensions);
        }
    });
    window.parent.registerFileUploadInit(fileUploadInit);
    window.parent.registerFileUploadSubmit(fileUploadSubmit);

    var postedFile = $("#PostedFile").val();
    if (postedFile.length > 0) {
        $("#PostedFile").val("");
        window.parent.filePosted(postedFile);
    }
});

function fileUploadInit(element, key, extensions, currentValue) {
    $("#Key").val(element);
    $("#Attribute").val(key);
    $("#file-upload-new").attr("data-extensions", extensions);
    $("#file-upload-new-error").html("");

    $("#existingFiles").empty();
    var extensionsList = extensions.split(";");
    var filesList = $("#AllFiles").val().split(":");
    var anyFiles = false;

    $("#existingFiles").append($("<option/>").attr("value", "<none>").text("None"));

    $.each(filesList, function (index, value) {
        $.each(extensionsList, function (extIdx, extValue) {
            if (endsWith(value.toLowerCase(), extValue.toLowerCase())) {
                $("#existingFiles").append($("<option/>").attr("value", value).text(value));
                anyFiles = true;
            }
        });
    });

    if (anyFiles) {
        if (currentValue == "") currentValue = "<none>";
        $("#existingFiles").val(currentValue);
        $("#file-upload-existing").show();
        $("#file-upload-new-header").show();
    }
}

function endsWith(str, suffix) {
    return str.indexOf(suffix, str.length - suffix.length) !== -1;
}

function fileUploadSubmit() {
    if ($("#File").val()) {
        $("#file-upload-form").submit();
    }
    else {
        var selection = $("#existingFiles").val();
        if (selection == "<none>") {
            window.parent.filePosted("")
        }
        else {
            window.parent.filePosted(selection)
        }
    }
}