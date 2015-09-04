/* global uiHide */
/* global uiShow */

// UI functions based on the IPlayer interface in WorldModel and implementation in PlayerHandler

(function () {
    window.quest = window.quest || {};
    
    var elementMap = {
        'Panes': '#gamePanes',
        'Location': '#location',
        'Command': '#txtCommandDiv'
    };
    
    var showHide = function (element, show) {
        var jsElement = elementMap[element];
        if (!jsElement) return;
        var uiFunction = show ? uiShow : uiHide;
        uiFunction(jsElement);
    };
        
    var show = function (element) {
        showHide(element, true);
    };
    
    var hide = function (element) {
        showHide(element, false);
    };
    
    quest.ui = quest.ui || {};
    quest.ui.show = show;
    quest.ui.hide = hide;
    quest.ui.locationUpdated = updateLocation;
    quest.ui.updateList = updateList;
    quest.ui.updateCompass = updateCompass;
})();