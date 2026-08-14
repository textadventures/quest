window.__restartTestScriptRuns = (window.__restartTestScriptRuns || 0) + 1;

function addSidebarPane() {
    var sidebar = document.getElementById('sidebar');
    if (!sidebar) return;
    var div = document.createElement('div');
    div.className = 'newwindow-test-pane';
    div.textContent = 'pane';
    sidebar.appendChild(div);
}
