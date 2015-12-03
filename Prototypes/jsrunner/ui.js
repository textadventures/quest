/* global panesVisible */
/* global setBackground */
/* global stopAudio */
/* global setPanelContents */
/* global setGameName */
/* global updateStatus */
/* global clearScreen */
/* global requestNextTimerTick */
/* global showMenu */
/* global showQuestion */
/* global beginWait */
/* global updateList */
/* global updateLocation */
/* global quest */
/* global addTextAndScroll */
/* global playWav */
/* global playMp3 */
/* global updateCompass */
/* global uiHide */
/* global uiShow */

// Globals are all in ui/player.js
// TODO: Remove globals, move player.js stuff in here
// For Quest 5 games, may need to put existing globals back so games can call them

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
    };
    
    var playSound = function (filename, synchronous, looped) {
        if (filename.toLowerCase().substr(-4) == ".mp3") {
            playMp3(filename, synchronous, looped);
        }
        else if (filename.toLowerCase().substr(-4) == ".wav") {
            playWav(filename, synchronous, looped);
        }
    };
    
    var print = function (text, linebreak) {
        if (typeof linebreak === 'undefined') linebreak = true;
        
        // TODO: If ASL >= 540 and there is an OutputText function, use that
        // (should be within msg script probably, not here)
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
    
    return {
        show: show,
        hide: hide,
        locationUpdated: updateLocation,
        updateList: updateList,
        updateCompass: updateCompassDirections,
        beginWait: beginWait,
        showQuestion: showQuestion,
        showMenu: showMenu,
        requestNextTimerTick: requestNextTimerTick,
        clearScreen: clearScreen,
        updateStatus: updateStatus,
        setGameName: setGameName,
        setPanelContents: setPanelContents,
        playSound: playSound,
        stopSound: stopAudio,
        setBackground: setBackground,
        panesVisible: panesVisible,
        print: print
    };
});