using ModelContextProtocol;
using QuestViva.Cli;

namespace QuestViva.CliTests;

[TestClass]
public class McpGameTests
{
    private static McpGame Game() => new(new FileInfo("headless.aslx"), 1234);

    [TestMethod]
    public async Task CommandBeforeStartSaysToStart()
    {
        var ex = await Assert.ThrowsAsync<McpException>(() => Game().SendAsync("look"));
        StringAssert.Contains(ex.Message, "start_game");
    }

    [TestMethod]
    public async Task StartReturnsOpening()
    {
        StringAssert.Contains(await Game().StartAsync(), "Welcome.");
    }

    [TestMethod]
    public async Task CommandReturnsTextWithoutEcho()
    {
        var game = Game();
        await game.StartAsync();
        Assert.AreEqual("before wait\nafter wait\n[Exits: south]\n[Status: Score: 0]\n", await game.SendAsync("testwait"));
    }

    [TestMethod]
    public async Task IgnoredMenuAnswerSaysHowToChoose()
    {
        var game = Game();
        await game.StartAsync();
        await game.SendAsync("testmenunocancel");
        StringAssert.Contains(await game.SendAsync("blue"), "Choose an option by its number");
    }

    [TestMethod]
    public async Task EndOfGameIsAnnounced()
    {
        var game = Game();
        await game.StartAsync();
        StringAssert.Contains(await game.SendAsync("testend"), "The game has ended");
    }

    [TestMethod]
    public async Task StartAgainRestarts()
    {
        var game = Game();
        await game.StartAsync();
        await game.SendAsync("testend");
        StringAssert.Contains(await game.StartAsync(), "Welcome.");
        StringAssert.Contains(await game.SendAsync("look"), "A plain room.");
    }

    [TestMethod]
    public async Task EvaluateErrorsAreReported()
    {
        var game = Game();
        await game.StartAsync();
        Assert.AreEqual("room", await game.EvaluateAsync("player.parent.name"));
        var ex = await Assert.ThrowsAsync<McpException>(() => game.EvaluateAsync("nosuch.attr"));
        StringAssert.Contains(ex.Message, "nosuch");
    }

    [TestMethod]
    public async Task EvaluateShowsNullAndScriptErrors()
    {
        var game = Game();
        await game.StartAsync();
        var result = await game.EvaluateAsync("Broken()");
        StringAssert.StartsWith(result, "null\n[The game printed:]\nabout to fail\nError running script: ");
        StringAssert.Contains(result, "nosuchobject");
        Assert.DoesNotContain("[error]", result);
        Assert.AreEqual("\"\" (an empty string)", await game.EvaluateAsync("\"\""));
    }

    [TestMethod]
    public async Task CheckTakesAGameBeforeStartGame()
    {
        var game = new McpGame(null, null);
        Assert.AreEqual("No problems found.\n", await game.CheckAsync("headless.aslx"));
        // That game is now the current one.
        StringAssert.Contains(await game.StartAsync(), "Welcome.");
    }

    [TestMethod]
    public async Task AServerGivenAGameOnlyTakesThatOne()
    {
        var game = Game();
        Assert.AreEqual("No problems found.\n", await game.CheckAsync("headless.aslx"));
        var ex = await Assert.ThrowsAsync<McpException>(() => game.CheckAsync("other.aslx"));
        StringAssert.Contains(ex.Message, "only plays");
    }

    [TestMethod]
    public async Task MissingGameIsReported()
    {
        var ex = await Assert.ThrowsAsync<McpException>(() => new McpGame(new FileInfo("nosuch.quest"), 1).StartAsync());
        StringAssert.Contains(ex.Message, "File not found");
    }

    [TestMethod]
    public async Task WithNoGameStartSaysToPassOne()
    {
        var ex = await Assert.ThrowsAsync<McpException>(() => new McpGame(null, null).StartAsync());
        StringAssert.Contains(ex.Message, "Pass the path");
    }

    [TestMethod]
    public async Task StartLoadsAGameByPath()
    {
        var game = new McpGame(null, null);
        StringAssert.Contains(await game.StartAsync(Path.GetFullPath("headless.aslx")), "Welcome.");
        await game.SendAsync("testend");
        StringAssert.Contains(await game.StartAsync(), "Welcome.");
    }

    [TestMethod]
    public async Task BadPathKeepsTheCurrentGame()
    {
        var game = new McpGame(null, null);
        await game.StartAsync("headless.aslx");
        await Assert.ThrowsAsync<McpException>(() => game.StartAsync("nosuch.quest"));
        StringAssert.Contains(await game.SendAsync("look"), "A plain room.");
    }

    [TestMethod]
    public async Task RestartKeepsTheSeed()
    {
        var game = new McpGame(null, null);
        await game.StartAsync("headless.aslx");
        var first = await game.EvaluateAsync("GetRandomInt(1, 1000000)");
        await game.StartAsync();
        Assert.AreEqual(first, await game.EvaluateAsync("GetRandomInt(1, 1000000)"));
    }

    [TestMethod]
    public async Task ExitsAreListedWhenTheCompassIsVisible()
    {
        var game = Game();
        StringAssert.Contains(await game.StartAsync(), "[Exits: south]");
        Assert.DoesNotContain("[Exits:", await game.SendAsync("testnocompass"));
    }

    [TestMethod]
    public async Task TextOnlyLeavesOutExitsAndStatus()
    {
        var game = new McpGame(new FileInfo("headless.aslx"), 1234, textOnly: true);
        var opening = await game.StartAsync();
        Assert.DoesNotContain("[Exits:", opening);
        Assert.DoesNotContain("[Status:", opening);
    }

    [TestMethod]
    public async Task StatusIsShownWhenThePanesAreVisible()
    {
        var game = Game();
        StringAssert.Contains(await game.StartAsync(), "[Status: Score: 0]");
        StringAssert.Contains(await game.SendAsync("testscore"), "[Status: Score: 5]");
        Assert.DoesNotContain("[Status:", await game.SendAsync("testnopanes"));
    }

    [TestMethod]
    public async Task WalkthroughWithNoNameListsThem()
    {
        StringAssert.Contains(await Game().RunWalkthroughAsync(null, false), "passes");
    }

    [TestMethod]
    public async Task WalkthroughLeavesTheGameWhereItStopped()
    {
        var game = Game();
        var result = await game.RunWalkthroughAsync("passes", false);
        StringAssert.Contains(result, "[answer:yes]\nanswer True");
        StringAssert.EndsWith(result, "PASS passes (7 steps)\n");
        StringAssert.Contains(await game.SendAsync("inventory"), "lamp");
    }

    [TestMethod]
    public async Task WalkthroughTranscriptHasEveryStep()
    {
        var result = await Game().RunWalkthroughAsync("passes", true);
        StringAssert.Contains(result, "> take lamp\n");
        StringAssert.Contains(result, "[assert:lamp.parent = player: passed]");
        StringAssert.Contains(result, "chose g");
    }

    [TestMethod]
    public async Task FailedWalkthroughSaysWhere()
    {
        StringAssert.Contains(await Game().RunWalkthroughAsync("failsassert", false),
            "FAIL failsassert at step 1 of 2: Assert failed: lamp.parent = player");
    }

    [TestMethod]
    public async Task UnknownWalkthroughListsTheOthers()
    {
        var ex = await Assert.ThrowsAsync<McpException>(() => Game().RunWalkthroughAsync("nosuch", false));
        StringAssert.Contains(ex.Message, "passes");
    }

    [TestMethod]
    public async Task CheckFindsNoProblemsInAWorkingGame()
    {
        Assert.AreEqual("No problems found.\n", await Game().CheckAsync());
    }

    [TestMethod]
    public async Task CheckReportsWhereEachProblemIs()
    {
        var result = await new McpGame(new FileInfo("check.aslx"), 1234).CheckAsync();
        StringAssert.Contains(result, "game, attribute 'start': Unknown script command or function 'Prnt'\n");
        StringAssert.Contains(result, "checklib.aslx, function 'LibraryFunction', attribute 'script': Unknown function 'AlsoNoSuchFunction'");
        StringAssert.EndsWith(result, "11 problems found.\n");
    }

    [TestMethod]
    public async Task CheckWithNoGameSaysToLoadOne()
    {
        var ex = await Assert.ThrowsAsync<McpException>(() => new McpGame(null, null).CheckAsync());
        StringAssert.Contains(ex.Message, "as game, or to start_game");
    }

    private static McpGame FunctionsGame() => new(new FileInfo("functions.aslx"), 1234);

    [TestMethod]
    public async Task ListFunctionsShowsWhereEachComesFrom()
    {
        var result = await FunctionsGame().ListFunctionsAsync(null, false);
        StringAssert.Contains(result, "TwoParams (a, b) returns int  [game]\n");
        StringAssert.Contains(result, "GetDisplayAlias (obj) returns string  [game, replacing CoreDescriptions.aslx's]\n");
        StringAssert.Contains(result, "LibraryFunction ()  [checklib.aslx]\n");
        StringAssert.Contains(result, "[game; an expression calls the engine's own CapFirst instead]");
        Assert.DoesNotContain("GetDisplayName", result);
    }

    [TestMethod]
    public async Task ListFunctionsCanIncludeBuiltInLibraries()
    {
        StringAssert.Contains(await FunctionsGame().ListFunctionsAsync("GetDisplayName", true),
            "GetDisplayName (obj) returns string  [CoreDescriptions.aslx]\n");
    }

    [TestMethod]
    public async Task DescribeFunctionGivesItsBodyAndDocs()
    {
        var result = await FunctionsGame().DescribeFunctionAsync("GetDisplayAlias");
        StringAssert.Contains(result, "replaces the function of the same name in CoreDescriptions.aslx");
        StringAssert.Contains(result, "my own alias");
        StringAssert.Contains(result, "[reference/functions/core#getdisplayalias]");
    }

    [TestMethod]
    public async Task DescribeFunctionCoversEngineFunctionsFromTheDocs()
    {
        StringAssert.StartsWith(await FunctionsGame().DescribeFunctionAsync("GetBoolean"),
            "GetBoolean is built into the engine");
    }

    [TestMethod]
    public async Task DescribeFunctionSuggestsSimilarNames()
    {
        StringAssert.Contains(await FunctionsGame().DescribeFunctionAsync("TwoParam"), "Similar names: TwoParams.");
    }

    [TestMethod]
    public void InstructionsTeachASLOnlyWithDebugTools()
    {
        Assert.DoesNotContain("ASL", McpInstructions.For(false));
        StringAssert.Contains(McpInstructions.For(true), "= is both assignment and comparison");
    }
}
