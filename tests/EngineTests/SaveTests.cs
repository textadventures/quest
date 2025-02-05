using Moq;
using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.EngineTests
{
    [TestClass]
    public class SaveTests
    {
        [TestMethod]
        public async Task RunWalkthrough()
        {
            var gameDataProvider = new FileGameDataProvider("savetest.aslx", "test");
            var gameData = await gameDataProvider.GetData();
            var worldModel = new WorldModel(gameData);

            var player = new Mock<IPlayer>();
            var success = await worldModel.Initialise(player.Object);
            Assert.IsTrue(success, "Initialisation failed");

            worldModel.Begin();

            worldModel.SendCommand("update");

            var tempFilename = System.IO.Path.GetTempFileName();
            worldModel.Save(tempFilename, null);

            var gameDataProvider2 = new FileGameDataProvider(tempFilename, "test");
            var gameData2 = await gameDataProvider2.GetData();
            var savedGameWorldModel = new WorldModel(gameData2);
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
