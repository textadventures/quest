using System.CommandLine;
using QuestViva.Cli;

var gameArgument = new Argument<FileInfo>("game")
{
    Description = "The game to play: a .quest package or an .aslx file"
};
var seedOption = new Option<int?>("--seed")
{
    Description = "Seed the game's random numbers, so the same commands give the same results"
};

var play = new Command("play", "Play a game in the terminal") { gameArgument, seedOption };
play.SetAction((parseResult, _) =>
    PlayCommand.RunAsync(parseResult.GetValue(gameArgument)!, parseResult.GetValue(seedOption)));

var serve = new Command("serve", "Play a game over a JSON-lines protocol on stdin/stdout, for programs to drive");
serve.SetAction((_, _) => ServeCommand.RunAsync());

var mcpGameArgument = new Argument<FileInfo?>("game")
{
    Description = "The game to serve. Without one, the agent loads games with start_game",
    Arity = ArgumentArity.ZeroOrOne
};
var debugToolsOption = new Option<bool>("--debug-tools")
{
    Description = "Also offer tools that reach past what a player can do: evaluate, tick and send_event"
};
var mcp = new Command("mcp", "Run an MCP server over stdio whose tools play a game, for AI agents")
{
    mcpGameArgument, seedOption, debugToolsOption
};
mcp.SetAction((parseResult, _) => McpCommand.RunAsync(parseResult.GetValue(mcpGameArgument),
    parseResult.GetValue(seedOption), parseResult.GetValue(debugToolsOption)));

var root = new RootCommand("Quest Viva command-line tools") { play, serve, mcp };
return await root.Parse(args).InvokeAsync();
