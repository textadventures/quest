// The "phone" recipe's JavaScript, from the panes docs (customise/panes.md), for
// v2-phone-pane-test.aslx. Keep it in step with the docs.

function phoneAttribute(text) {
  return String(text).replace(/&/g, "&amp;").replace(/"/g, "&quot;");
}

function drawPhone() {
  var lists = QuestUI.lists();
  var html = '<div class="phone-screen"><h3>Carrying</h3><ul>';
  if (lists.inventory.length === 0) {
    html += "<li>Nothing</li>";
  }
  lists.inventory.forEach(function (item) {
    html += '<li><a class="cmdlink elementmenu" data-elementid="' + phoneAttribute(item.elementId) +
      '" data-verbs="' + phoneAttribute(item.verbs.join("/")) + '">' + item.text + "</a></li>";
  });
  html += '</ul><h3>Exits</h3><ul class="phone-exits">';
  lists.exits.forEach(function (exit) {
    html += '<li><a class="cmdlink commandlink" data-elementid="" data-command="' + phoneAttribute(exit) +
      '">' + exit + "</a></li>";
  });
  html += "</ul></div>";
  QuestUI.setPaneContent("phone", html);
}

QuestUI.on("inventoryChanged", drawPhone);
QuestUI.on("exitsChanged", drawPhone);
