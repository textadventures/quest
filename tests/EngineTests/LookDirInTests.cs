using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

// A look exit can be created for "in" on the Exits tab, so LOOK IN has to reach it through the
// lookdir command rather than falling through to LOOK AT (#2365). LOOK IN <object> must not match it.
[TestClass]
public class LookDirInTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameData = new GameData(File.ReadAllBytes("lookdirintest.aslx"), "lookdirintest.aslx");
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
