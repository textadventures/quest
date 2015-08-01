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
    setTimeout(function() {
        window.parent.registerFileUploadInit(fileUploadInit);
        window.parent.registerFileUploadSubmit(fileUploadSubmit);
    }, 1000);
    
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
    $("#existingFiles").change(function () {
        $("#file-upload-imgPreview").empty();
        if ($("#existingFiles").val() != "<none>") {
            $("#file-upload-imgPreview").append($("<img />")
                .attr("src", "/ImageProcessor.ashx?image=" + $("#existingFiles").val() + "&gameId=" + $("#GameId").val() + "&w=100&h=100")
                .attr("style", "max-width: 100px; max-height: 100px"));
        }
    });

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
        $("#existingFiles").trigger('change');
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
            window.parent.filePosted("");
        }
        else {
            window.parent.filePosted(selection);
        }
    }
}