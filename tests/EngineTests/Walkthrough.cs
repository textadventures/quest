using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

[TestClass]
public class Walkthrough
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameDataProvider = new FileGameDataProvider(Path.Combine("..", "..", "..", "walkthrough.aslx"));
        var gameData = await gameDataProvider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);

        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        await worldModel.Initialise(player.Object);
        await worldModel.Begin();

        foreach (var cmd in worldModel.Walkthroughs.Walkthroughs["debug"].Steps)
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
