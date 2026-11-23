// For questui-lists-test.aslx: records every list event, and every turn, in the order they came.
// The file runs again when a saved game is loaded, which starts the record afresh.
window.listEvents = [];
["inventoryChanged", "objectsChanged", "exitsChanged", "statusChanged", "turnStarted"].forEach(function (name) {
  QuestUI.on(name, function (detail) {
    window.listEvents.push({ name: name, detail: detail });
  });
});
