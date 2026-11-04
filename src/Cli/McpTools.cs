using System.ComponentModel;
using ModelContextProtocol.Server;

namespace QuestViva.Cli;

[McpServerToolType]
internal sealed class PlayTools(McpGame game)
{
    [McpServerTool(Name = "send_command"),
     Description("Type something into the game, as a player would: a command such as \"look\", " +
                 "\"take lamp\" or \"go north\", or the answer to a menu or yes/no question the game is " +
                 "waiting on. Returns what the game prints in response.")]
    public Task<string> SendCommand([Description("What to type")] string input) => game.SendAsync(input);
}

// start_game when the server was given a game: it can only be restarted.
[McpServerToolType]
internal sealed class FixedGameTools(McpGame game)
{
    [McpServerTool(Name = "start_game"),
     Description("Start the text adventure game from the beginning, or restart it, and return its opening text.")]
    public Task<string> StartGame() => game.StartAsync();
}

// start_game when the server wasn't given a game: the agent loads one.
[McpServerToolType]
internal sealed class LoadGameTools(McpGame mcpGame)
{
    [McpServerTool(Name = "start_game"),
     Description("Load a Quest text adventure game (a .quest, .aslx, .asl or .cas file) and return its opening text. " +
                 "With no game, restart the current one from the beginning.")]
    public Task<string> StartGame(
        [Description("Path to the game file. An absolute path is safest; a relative one is resolved " +
                     "against the server's working directory.")]
        string? game = null) =>
        mcpGame.StartAsync(game);
}

// Only registered with --debug-tools: these reach past what a player can do, and evaluate can read
// the solution out of the game.
[McpServerToolType]
internal sealed class DebugTools(McpGame game)
{
    [McpServerTool(Name = "evaluate"),
     Description("Evaluate a Quest ASL expression against the game's current state, e.g. " +
                 "\"player.parent.name\" or \"GetBoolean(door, \\\"locked\\\")\". For debugging; not something a player could do.")]
    public Task<string> Evaluate([Description("The ASL expression")] string expression) =>
        game.EvaluateAsync(expression);

    [McpServerTool(Name = "tick"),
     Description("Advance the game clock, firing any timers that come due. For games that do things in real time.")]
    public Task<string> Tick([Description("How many seconds to advance")] int seconds) =>
        game.TickAsync(seconds);

    [McpServerTool(Name = "send_event"),
     Description("Fire an event that the game's own JavaScript would normally send with ASLEvent.")]
    public Task<string> SendEvent([Description("The event name")] string name,
        [Description("The event's parameter")] string param = "") =>
        game.SendEventAsync(name, param);

    [McpServerTool(Name = "run_walkthrough"),
     Description("Restart the game, re-reading its file, and run one of its built-in walkthroughs: a list of " +
                 "steps saved in the game, such as commands and assert:<expression> checks. Reports whether it " +
                 "passed, or the step it failed at and why, and leaves the game where the walkthrough stopped. " +
                 "With no name, lists the game's walkthroughs.")]
    public Task<string> RunWalkthrough(
        [Description("The walkthrough to run. Omit it to list them")] string? name = null,
        [Description("Return every step's response, not just the last")] bool transcript = false) =>
        game.RunWalkthroughAsync(name, transcript);

    [McpServerTool(Name = "check"),
     Description("Check the game's file for problems without playing it: scripts the engine can't parse, unknown " +
                 "script commands and functions, functions given too many arguments, and elements with the same " +
                 "name. Reads the file as it is now, so it sees edits; restart the game with start_game to play them.")]
    public Task<string> Check() => game.CheckAsync();
}
