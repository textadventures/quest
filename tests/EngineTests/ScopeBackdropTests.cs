using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

// Regression coverage for issue #2670: calling a scope function such as ScopeReachable
// from the game's backdrop scope script ran the script again from inside itself, forever.
// A nested scope call now gets the room's objects without backdrops. The script is also
// told which room is being scoped, in a `room` variable, so it works for
// ScopeReachableForRoom(otherRoom) and not just the player's room.
[TestClass]
public class ScopeBackdropTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameData = new GameData(File.ReadAllBytes("scopebackdroptest.aslx"), "scopebackdroptest.aslx");
        var worldModel = Helpers.CreateWorldModel(gameData);

        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        var success = await worldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");

        await worldModel.Begin();

        foreach (var cmd in worldModel.Walkthroughs.Walkthroughs["verify"].Steps)
        {
            if (cmd.StartsWith("assert:"))
            {
                var expr = cmd.Substring(7);
                Assert.IsTrue(await worldModel.AssertAsync(expr), expr);
            }
            else
            {
                await worldModel.SendCommand(cmd);
            }
        }
    }
}
