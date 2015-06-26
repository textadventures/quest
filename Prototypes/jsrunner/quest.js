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
    
    var sendCommand = function (command, elapsedTime, metadata) {
        console.log(command);
        // TODO: Increment time
        // TODO: Check if command override mode is on
        // TODO: Echo input for ASL <= 520
        
        quest.executeScript(quest.getFunction('HandleCommand'), {
            command: command,
            metadata: metadata
        });
        
        // TODO: TryFinishTurn
        // TODO: UpdateLists
        // TODO: Send next timer request
    };
    
    quest.begin = begin;
    quest.sendCommand = sendCommand;
})();