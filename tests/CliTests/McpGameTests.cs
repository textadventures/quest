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
        Assert.AreEqual("before wait\nafter wait\n[Exits: south]\n", await game.SendAsync("testwait"));
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
    public async Task TextOnlyLeavesOutExits()
    {
        var game = new McpGame(new FileInfo("headless.aslx"), 1234, textOnly: true);
        Assert.DoesNotContain("[Exits:", await game.StartAsync());
    }
}
