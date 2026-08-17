using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

// Regression coverage for the "Require all keys" toggle on lockable containers: by default
// (requireallkeys unset/true) a container needs every configured key present, matching the
// pre-existing AllKeysAvailable behaviour; with requireallkeys=false, any one of the configured
// keys is enough (AnyKeyAvailable), matching the old Quest 5.8 "Require all keys" checkbox.
[TestClass]
public class ContainerLockableTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameDataProvider = new FileGameDataProvider("containerlockabletest.aslx");
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
