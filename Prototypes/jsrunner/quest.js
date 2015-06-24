/* global quest */
/* global jsep */

(function () {
    window.quest = window.quest || {};
    
    window.quest.load = function (data) {
        // TODO: Eventually this will be called with a full .aslx file
        // (Note - only a single file from a .quest, we don't need to worry ever about including
        // external libraries)

        quest.create('game');
        var script = quest.parseScript(data);
        quest.executeScript(script);
    };
})();