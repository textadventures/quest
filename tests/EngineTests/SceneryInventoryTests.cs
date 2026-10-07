using Moq;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// Regression coverage for issue #905: objects flagged as scenery that the player is
// carrying. The objects pane (fed by ScopeInventory, which filters on ContainsVisible
// and has never looked at scenery) listed them; the INVENTORY command, via
// FormatObjectList -> RemoveSceneryObjects, did not - so a carried object could be
// invisible to the one command that lists carried objects. The pane is now treated as
// correct: FormatInventoryList includes carried scenery, at any depth, while room and
// container listings still exclude it.
//
// Issue #2492 adds the opt-out authors were using scenery for: "hidefrominventory"
// keeps a carried object out of both the INVENTORY command and the pane, while
// leaving it in ScopeInventory so the parser (DROP, Got()) still treats it as held.
[TestClass]
public class SceneryInventoryTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameData = new GameData(File.ReadAllBytes("sceneryinventorytest.aslx"), "sceneryinventorytest.aslx");
        var worldModel = Helpers.CreateWorldModel(gameData);

        worldModel.LogError += ex => throw ex;

        List<ListData>? pane = null;
        worldModel.UpdateList += (listType, items) =>
        {
            if (listType == ListType.InventoryList)
            {
                pane = items;
            }
        };

        var player = new Mock<IPlayer>();
        var success = await worldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");

        await worldModel.Begin();

        pane.ShouldNotBeNull();
        var paneIds = pane.Select(i => i.ElementId).ToList();
        paneIds.ShouldContain("lamp");
        paneIds.ShouldContain("poster");
        paneIds.ShouldNotContain("bladder");
        paneIds.ShouldNotContain("pouch");
        paneIds.ShouldNotContain("coin");

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
