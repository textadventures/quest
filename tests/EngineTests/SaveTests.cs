using Moq;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;

namespace QuestViva.EngineTests
{
    [TestClass]
    public class SaveTests
    {
        [TestMethod]
        public async Task RunWalkthrough()
        {
            var gameDataProvider = new FileGameDataProvider("savetest.aslx");
            var gameData = await gameDataProvider.GetData();
            var worldModel = Helpers.CreateWorldModel(gameData);

            worldModel.LogError += ex => throw ex;

            var player = new Mock<IPlayer>();
            var success = await worldModel.Initialise(player.Object);
            Assert.IsTrue(success, "Initialisation failed");

            worldModel.Begin();

            worldModel.SendCommand("update");

            var tempFilename = Path.GetTempFileName();
            var saveData = worldModel.Save(SaveMode.SavedGame, html: null);
            await File.WriteAllTextAsync(tempFilename, saveData);

            var gameDataProvider2 = new FileGameDataProvider(tempFilename);
            var gameData2 = await gameDataProvider2.GetData();
            var savedGameWorldModel = Helpers.CreateWorldModel(gameData2);
            success = await savedGameWorldModel.Initialise(player.Object);
            Assert.IsTrue(success, "Initialisation failed");

            savedGameWorldModel.Begin();

            foreach (var cmd in worldModel.Walkthroughs.Walkthroughs["verify"].Steps)
            {
                if (cmd.StartsWith("assert:"))
                {
                    var expr = cmd.Substring(7);
                    Assert.IsTrue(worldModel.Assert(expr), expr);
                }
                else
                {
                    worldModel.SendCommand(cmd);
                }
            }

            File.Delete(tempFilename);
        }
    }
}
