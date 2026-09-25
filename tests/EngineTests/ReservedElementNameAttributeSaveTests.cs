using Moq;
using QuestViva.Common;
using QuestViva.Engine.GameLoader;

namespace QuestViva.EngineTests;

// Regression test for https://github.com/textadventures/quest/issues/2350 - an attribute whose
// name matches a game-file element name (e.g. "object", "command", "exit", "verb") used to be
// saved as a bare element (e.g. <object>...</object>), which the loader then misread as a nested
// object/command/exit/verb element instead of an attribute, corrupting the saved game.
[TestClass]
public class ReservedElementNameAttributeSaveTests
{
    [TestMethod]
    public async Task AttributesNamedAfterReservedElementsSurviveSaveAndReload()
    {
        var gameDataProvider = new FileGameDataProvider("reservedelementnameattributesavetest.aslx");
        var gameData = await gameDataProvider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);

        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        var success = await worldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");

        await worldModel.Begin();
        await worldModel.SendCommand("addruntimeattributes");

        var tempFilename = Path.GetTempFileName();
        try
        {
            var saveData = worldModel.Save(SaveMode.SavedGame, html: null);
            await File.WriteAllTextAsync(tempFilename, saveData);

            var reloadedGameDataProvider = new FileGameDataProvider(tempFilename);
            var reloadedGameData = await reloadedGameDataProvider.GetData();
            var reloadedWorldModel = Helpers.CreateWorldModel(reloadedGameData);
            reloadedWorldModel.LogError += ex => throw ex;

            success = await reloadedWorldModel.Initialise(player.Object);
            Assert.IsTrue(success, "Initialisation failed after reload");

            await reloadedWorldModel.Begin();

            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "GetAttribute(player, \"object\") = \"a string called object\""));
            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "GetAttribute(player, \"exit\") = true"));
            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "GetAttribute(player, \"command\") = \"a runtime string called command\""));
            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "GetAttribute(player, \"verb\") = true"));
        }
        finally
        {
            File.Delete(tempFilename);
        }
    }
}
