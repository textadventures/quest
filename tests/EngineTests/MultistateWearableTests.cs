using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

// Regression coverage for issue #2363: wearing a multi-state garment whose "Bonus to
// attributes" list is missing or empty failed with "Value cannot be null", because
// _SetMultistate read the list unconditionally. Most clothing gives no bonus, so a missing
// entry now means "*" - keep the garment's own bonusatts.
[TestClass]
public class MultistateWearableTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameData = new GameData(File.ReadAllBytes("multistatewearabletest.aslx"), "multistatewearabletest.aslx");
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
