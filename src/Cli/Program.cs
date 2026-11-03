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

var root = new RootCommand("Quest Viva command-line tools") { play };
return await root.Parse(args).InvokeAsync();
