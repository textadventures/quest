using QuestViva.Common;
using QuestViva.Legacy;

namespace QuestViva.LegacyTests;

[TestClass]
public class V4GameStartTests
{
    // An "enter" in the start script means Begin() doesn't return until the player answers,
    // so a buffering IPlayer only learns there's output to show from TurnSuspended.
    [TestMethod]
    public async Task TestEnterInStartScriptRaisesTurnSuspended()
    {
        var player = new TestPlayer();
        var filename = Path.Combine(["..", "..", "..", "starttest-enter.asl"]);
        var gameData = await new FileGameDataProvider(filename).GetData();
        IGame game = new V4Game(gameData, null);
        game.PrintText += player.PrintText;
        var suspended = new TaskCompletionSource();
        game.TurnSuspended += () => suspended.TrySetResult();
        await game.Initialise(player);

        var begin = game.Begin();

        await suspended.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsFalse(begin.IsCompleted, "Begin() should still be waiting for the answer to the enter prompt");
        Assert.IsTrue(Enumerable.Range(0, player.BufferLength).Select(player.Buffer).Contains("What is your name?"));

        await game.SendCommand("Alex");
        await begin.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsTrue(Enumerable.Range(0, player.BufferLength).Select(player.Buffer).Contains("Hello Alex"));
    }
}
