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
    Description = "Also offer tools for debugging and fixing a game, which reach past what a player can do, such as evaluate, check and docs"
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

var checkGameArgument = new Argument<FileInfo>("game")
{
    Description = "The game to check: an .aslx or .quest file"
};
var jsonOption = new Option<bool>("--json")
{
    Description = "Print the problems as JSON, for programs"
};
var check = new Command("check",
    "Find problems in a game's scripts without playing it, exiting non-zero if there are any, for example in CI")
{
    checkGameArgument, jsonOption
};
check.SetAction((parseResult, _) =>
    CheckCommand.RunAsync(parseResult.GetValue(checkGameArgument)!, parseResult.GetValue(jsonOption)));

var publishGameArgument = new Argument<FileInfo>("game")
{
    Description = "The game to publish: an .aslx file"
};
var formatOption = new Option<string>("--format")
{
    Description = "What to make, as the Editor's Publish does: quest, a .quest package; html, one web page that " +
                  "loads the player from jsDelivr (a released qv only); or zip, the page and the player for any web host",
    DefaultValueFactory = _ => "quest"
};
var outputOption = new Option<FileInfo?>("--output", "-o")
{
    Description = "Where to write it. By default, beside the game: game.aslx makes game.quest, game.html or game-html.zip"
};
var forceOption = new Option<bool>("--force")
{
    Description = "Publish even if qv check finds problems in the game"
};
var publishPlayerOption = new Option<DirectoryInfo?>("--player")
{
    Description = "For --format zip, use the WasmPlayer bundle in this folder instead of the one built into qv"
};
var cdnVersionOption = new Option<string?>("--cdn-version")
{
    Description = "For --format html, load the player from jsDelivr at this published version instead of qv's own"
};
var publish = new Command("publish",
    "Publish a game, with the files in its folder, after checking it for problems")
{
    publishGameArgument, formatOption, outputOption, forceOption, publishPlayerOption, cdnVersionOption
};
publish.SetAction((parseResult, _) => PublishCommand.RunAsync(parseResult.GetValue(publishGameArgument)!,
    parseResult.GetValue(formatOption)!, parseResult.GetValue(outputOption), parseResult.GetValue(forceOption),
    parseResult.GetValue(publishPlayerOption), parseResult.GetValue(cdnVersionOption)));

var previewGameArgument = new Argument<FileInfo>("game")
{
    Description = "The game to preview: usually an .aslx file, but any game the player can play"
};
var portOption = new Option<int>("--port")
{
    Description = "The port to serve on. By default, a free one"
};
var playerOption = new Option<DirectoryInfo?>("--player")
{
    Description = "Serve the WasmPlayer bundle in this folder instead of the one built into qv"
};
var preview = new Command("preview",
    "Play a game in your browser, served by qv on this computer only, reloading it when its files change")
{
    previewGameArgument, portOption, seedOption, playerOption
};
preview.SetAction((parseResult, _) => PreviewCommand.RunAsync(parseResult.GetValue(previewGameArgument)!,
    parseResult.GetValue(portOption), parseResult.GetValue(seedOption), parseResult.GetValue(playerOption)));

var root = new RootCommand("Quest Viva command-line tools") { play, serve, mcp, walkthrough, check, publish, preview };
return await root.Parse(args).InvokeAsync();
