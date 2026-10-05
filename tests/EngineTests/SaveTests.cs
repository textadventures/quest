using Moq;
using QuestViva.Common;
using QuestViva.Engine.GameLoader;

namespace QuestViva.EngineTests;

[TestClass]
public class SaveTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameData = new GameData(File.ReadAllBytes("savetest.aslx"), "savetest.aslx");
        var worldModel = Helpers.CreateWorldModel(gameData);

        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        var success = await worldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");

        await worldModel.Begin();

        await worldModel.SendCommand("update");

        var tempFilename = Path.GetTempFileName();
        var saveData = worldModel.Save(SaveMode.SavedGame, html: null);
        await File.WriteAllTextAsync(tempFilename, saveData);

        var gameData2 = new GameData(File.ReadAllBytes(tempFilename), tempFilename);
        var savedGameWorldModel = Helpers.CreateWorldModel(gameData2);
        success = await savedGameWorldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");

        await savedGameWorldModel.Begin();

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

        File.Delete(tempFilename);
    }
}