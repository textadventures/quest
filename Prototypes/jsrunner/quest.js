/* global quest */

(function () {
    window.quest = window.quest || {};
    
    var begin = function () {
        // Based on WorldModel.Begin
        
        // TODO: Init timer runner
        // TODO: Show Panes, Location, Command for ASL <= 540
        
        if (quest.functionExists('InitInterface')) {
            quest.executeScript(quest.getFunction('InitInterface'));
        }
        
        // InitInterface
        
        // TODO: StartGame
        // TODO: Run on finally scripts
        // TODO: Update lists
        // TODO: If loaded from saved, load output etc.
        // TODO: Send next timer request
    };
    
    quest.begin = begin;
})();