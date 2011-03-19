function SetBackground(color) {
    document.body.style.background = color;
}

function ASLEvent(event, parameter) {
    var elem = $("#_ASLEvent");
    elem.html(event + ";" + parameter);
    elem.click();
}