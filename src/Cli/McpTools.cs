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
     Description("Start the text adventure game from the beginning, or restart it, and return its opening text. " +
                 "Restarting reads the game's file again, so it picks up any edits to it.")]
    public Task<string> StartGame() => game.StartAsync();
}

// start_game when the server wasn't given a game: the agent loads one.
[McpServerToolType]
internal sealed class LoadGameTools(McpGame mcpGame)
{
    [McpServerTool(Name = "start_game"),
     Description("Load a Quest text adventure game (a .quest, .aslx, .asl or .cas file) and return its opening text. " +
                 "With no game, restart the current one from the beginning. Either way the file is read again, so " +
                 "restarting picks up any edits to it.")]
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

    [McpServerTool(Name = "list_functions", ReadOnly = true),
     Description("List the functions the game's scripts can call, read from its file as it is now, with their " +
                 "parameters and where each is defined: the game file, a library the game includes, or, with " +
                 "include_built_in, the libraries built into Quest such as Core.aslx. A game function with the same " +
                 "name as a library one replaces it.")]
    public Task<string> ListFunctions(
        [Description("Only list functions whose names contain this")] string? filter = null,
        [Description("Also list the functions of Quest's built-in libraries")] bool include_built_in = false) =>
        game.ListFunctionsAsync(filter, include_built_in);

    [McpServerTool(Name = "describe_function", ReadOnly = true),
     Description("Show one function: its parameters, where it's defined, its body, and its entry in the docs. " +
                 "Works for functions built into the engine too, from the docs.")]
    public Task<string> DescribeFunction([Description("The function's name")] string name) =>
        game.DescribeFunctionAsync(name);

    [McpServerTool(Name = "docs", ReadOnly = true),
     Description("Look things up in Quest Viva's documentation, for the version of Quest that qv runs: the " +
                 "reference for script commands, functions, attributes and the .aslx file format, pages explaining " +
                 "how values, types and scripts work, and how-to guides. query searches it, and a command or function " +
                 "name such as \"foreach\" or \"GetBoolean\" finds its entry first. page reads a whole page, or one " +
                 "section with page#section; links in the docs, such as /reference/functions/string#template, work " +
                 "as page ids. With neither, lists the pages.")]
    public static string Docs(
        [Description("Words to search for, or a command or function name")] string? query = null,
        [Description("A page id such as reference/script-commands, optionally with #section")] string? page = null) =>
        page != null ? QuestDocs.Read(page) : query != null ? QuestDocs.Search(query) : QuestDocs.Contents();
}
