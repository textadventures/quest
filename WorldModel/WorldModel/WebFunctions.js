function SetBackground(color) {
    document.body.style.background = color;
}

function ASLEvent(event, parameter) {
    window.external.Trigger(event, parameter);
}
