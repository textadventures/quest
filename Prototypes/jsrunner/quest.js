/* global quest */

define(['attributes'], function (attributes) {
    window.quest = window.quest || {};
    
    var begin = function () {
        // Based on WorldModel.Begin
        
        // TODO: Init timer runner
        // TODO: Show Panes, Location, Command for ASL <= 540
        
        if (attributes.functionExists('InitInterface')) {
            quest.executeScript(attributes.getFunction('InitInterface'));
        }
        
        // TODO: Only call StartGame if not loaded from saved game
        quest.executeScript(attributes.getFunction('StartGame'));
        
        
        // TODO: Run on finally scripts
        // TODO: Update lists
        // TODO: If loaded from saved, load output etc.
        // TODO: Send next timer request
    };
    
    var sendCommand = function (command, elapsedTime, metadata) {
        // TODO: Increment time
        // TODO: Check if command override mode is on
        // TODO: Echo input for ASL <= 520
        
        quest.executeScript(attributes.getFunction('HandleCommand'), {
            command: command,
            metadata: metadata
        });
        
        // TODO: TryFinishTurn
        // TODO: UpdateLists
        // TODO: Send next timer request
    };
    
    var print = function (text, linebreak) {
        if (typeof linebreak === 'undefined') linebreak = true;
        
        // TODO: If ASL >= 540 and there is an OutputText function, use that
        if (linebreak) text += '<br/>';
        addTextAndScroll(text);
    };
    
    quest.begin = begin;
    quest.sendCommand = sendCommand;
    quest.print = print;
});