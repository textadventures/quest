define(['state', 'loader', 'scripts'], function (state, loader, scripts) {
    var begin = function () {
        // Based on WorldModel.Begin
        
        // TODO: Init timer runner
        // TODO: Show Panes, Location, Command for ASL <= 540
        
        if (state.functionExists('InitInterface')) {
            scripts.executeScript(state.getFunction('InitInterface'));
        }
        
        // TODO: Only call StartGame if not loaded from saved game
        scripts.executeScript(state.getFunction('StartGame'));
        
        
        // TODO: Run on finally scripts
        // TODO: Update lists
        // TODO: If loaded from saved, load output etc.
        // TODO: Send next timer request
    };
    
    var sendCommand = function (command, elapsedTime, metadata) {
        // TODO: Increment time
        // TODO: Check if command override mode is on
        // TODO: Echo input for ASL <= 520
        
        scripts.executeScript(state.getFunction('HandleCommand'), {
            command: command,
            metadata: metadata
        });
        
        // TODO: TryFinishTurn
        // TODO: UpdateLists
        // TODO: Send next timer request
    };
    
    var load = function (data) {
        loader.load(data);
    };
    
    return {
        begin: begin,
        sendCommand: sendCommand,
        load: load
    };
});