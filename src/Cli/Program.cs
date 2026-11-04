using System.CommandLine;
using QuestViva.Cli;

var gameArgument = new Argument<FileInfo>("game")
{
    Description = "The game to play: a .quest or .aslx file, or an .asl or .cas file from Quest 4 or earlier"
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
var textOnlyOption = new Option<bool>("--text-only")
{
    Description = "Give the agent only the game's text, without the exits a browser player sees in the compass"
};
var debugToolsOption = new Option<bool>("--debug-tools")
{
    Description = "Also offer tools that reach past what a player can do: evaluate, tick, send_event and run_walkthrough"
};
var mcp = new Command("mcp", "Run an MCP server over stdio whose tools play a game, for AI agents")
{
    mcpGameArgument, seedOption, textOnlyOption, debugToolsOption
};
mcp.SetAction((parseResult, _) => McpCommand.RunAsync(parseResult.GetValue(mcpGameArgument),
    parseResult.GetValue(seedOption), parseResult.GetValue(textOnlyOption), parseResult.GetValue(debugToolsOption)));

var walkthroughNameArgument = new Argument<string?>("name")
{
    Description = "The walkthrough to run. Without one, every walkthrough runs, each from a fresh start",
    Arity = ArgumentArity.ZeroOrOne
};
var quietOption = new Option<bool>("--quiet")
{
    Description = "Print only whether each walkthrough passed, not its transcript"
};
var walkthroughSeedOption = new Option<int?>("--seed")
{
    Description = $"Seed the game's random numbers (default {QuestViva.PlayerCore.HeadlessSession.DefaultSeed}, so every run gives the same results)"
};
var walkthrough = new Command("walkthrough",
    "Run a game's built-in walkthroughs, exiting non-zero if any fails, for example in CI")
{
    gameArgument, walkthroughNameArgument, walkthroughSeedOption, quietOption
};
walkthrough.SetAction((parseResult, _) => WalkthroughCommand.RunAsync(parseResult.GetValue(gameArgument)!,
    parseResult.GetValue(walkthroughNameArgument), parseResult.GetValue(walkthroughSeedOption),
    parseResult.GetValue(quietOption)));

var root = new RootCommand("Quest Viva command-line tools") { play, serve, mcp, walkthrough };
return await root.Parse(args).InvokeAsync();
