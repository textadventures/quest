using System.ComponentModel;
using ModelContextProtocol.Server;

namespace QuestViva.Cli;

[McpServerToolType]
internal sealed class PlayTools(McpGame game)
{
    [McpServerTool(Name = "start_game"),
     Description("Start the text adventure game from the beginning, or restart it, and return its opening text.")]
    public Task<string> StartGame() => game.StartAsync();

    [McpServerTool(Name = "send_command"),
     Description("Type something into the game, as a player would: a command such as \"look\", " +
                 "\"take lamp\" or \"go north\", or the answer to a menu or yes/no question the game is " +
                 "waiting on. Returns what the game prints in response.")]
    public Task<string> SendCommand([Description("What to type")] string input) => game.SendAsync(input);
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
}
