using Moq;
using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.EngineTests
{
    [TestClass]
    public class Walkthrough
    {
        [TestMethod]
        public async Task RunWalkthrough()
        {
            var gameDataProvider = new FileGameDataProvider(Path.Combine("..", "..", "..", "walkthrough.aslx"), new DummyResourceProvider());
            var gameData = await gameDataProvider.GetData();
            var worldModel = Helpers.CreateWorldModel(gameData);

            worldModel.LogError += ex => throw ex;

            var player = new Mock<IPlayer>();
            await worldModel.Initialise(player.Object);
            worldModel.Begin();

            foreach (var cmd in worldModel.Walkthroughs.Walkthroughs["debug"].Steps)
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
        }
    }
}
