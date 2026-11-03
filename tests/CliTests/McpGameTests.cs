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
        Assert.AreEqual("before wait\nafter wait\n", await game.SendAsync("testwait"));
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
}
