$(function () {
    $("#file-upload-submit").button().click(function () {
    });
    window.parent.registerFileUploadInit(fileUploadInit);
});

function fileUploadInit(element, key) {
    $("#Key").val(element);
    $("#Attribute").val(key);
}