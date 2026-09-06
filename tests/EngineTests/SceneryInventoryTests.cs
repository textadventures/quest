using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

// Regression coverage for issue #905: objects flagged as scenery that the player is
// carrying. The objects pane (fed by ScopeInventory, which filters on ContainsVisible
// and has never looked at scenery) listed them; the INVENTORY command, via
// FormatObjectList -> RemoveSceneryObjects, did not - so a carried object could be
// invisible to the one command that lists carried objects. The pane is now treated as
// correct: FormatInventoryList includes carried scenery, at any depth, while room and
// container listings still exclude it.
[TestClass]
public class SceneryInventoryTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameDataProvider = new FileGameDataProvider("sceneryinventorytest.aslx");
        var gameData = await gameDataProvider.GetData();
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
