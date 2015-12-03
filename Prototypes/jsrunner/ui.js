/* global uiHide */
/* global uiShow */

// UI functions based on the IPlayer interface in WorldModel and implementation in PlayerHandler

define(function () {
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
    
    var updateCompassDirections = function (listData) {
        var directions = listData.map(function (item) {
            return item.Text;
        });
        updateCompass(directions);
    }
    
    var playSound = function (filename, synchronous, looped) {
        if (filename.toLowerCase().substr(-4) == ".mp3") {
            playMp3(filename, synchronous, looped);
        }
        else if (filename.toLowerCase().substr(-4) == ".wav") {
            playWav(filename, synchronous, looped);
        }
    }
    
    var print = function (text, linebreak) {
        if (typeof linebreak === 'undefined') linebreak = true;
        
        // TODO: If ASL >= 540 and there is an OutputText function, use that
        if (linebreak) text += '<br/>';
        addTextAndScroll(text);
    };
    
    quest.ui = quest.ui || {};
    quest.ui.show = show;
    quest.ui.hide = hide;
    quest.ui.locationUpdated = updateLocation;
    quest.ui.updateList = updateList;
    quest.ui.updateCompass = updateCompassDirections;
    quest.ui.beginWait = beginWait;
    quest.ui.showQuestion = showQuestion;
    quest.ui.showMenu = showMenu;
    quest.ui.requestNextTimerTick = requestNextTimerTick;
    quest.ui.clearScreen = clearScreen;
    quest.ui.updateStatus = updateStatus;
    quest.ui.setGameName = setGameName;
    quest.ui.setPanelContents = setPanelContents;
    quest.ui.playSound = playSound;
    quest.ui.stopSound = stopAudio;
    quest.ui.setBackground = setBackground;
    quest.ui.panesVisible = panesVisible;
    quest.print = print;
});