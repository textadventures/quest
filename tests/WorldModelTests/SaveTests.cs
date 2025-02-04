using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;
using Moq;

namespace WorldModelTests
{
    [TestClass]
    public class SaveTests
    {
        [TestMethod]
        public async Task RunWalkthrough()
        {
            var gameDataProvider = new FileGameDataProvider(Path.Combine("..", "..", "..", "savetest.aslx"), "test");
            var gameData = await gameDataProvider.GetData();
            WorldModel worldModel = new WorldModel(
                gameData,
                Path.Combine("..", "..", ".."));

            Mock<IPlayer> player = new Mock<IPlayer>();
            bool success = await worldModel.Initialise(player.Object);
            Assert.IsTrue(success, "Initialisation failed");

            worldModel.Begin();

            worldModel.SendCommand("update");

            string tempFilename = System.IO.Path.GetTempFileName();
            worldModel.Save(tempFilename, null);

            var gameDataProvider2 = new FileGameDataProvider(tempFilename, "test");
            var gameData2 = await gameDataProvider2.GetData();
            WorldModel savedGameWorldModel = new WorldModel(gameData2, null);
            success = await savedGameWorldModel.Initialise(player.Object);
            Assert.IsTrue(success, "Initialisation failed");

            savedGameWorldModel.Begin();

            foreach (string cmd in worldModel.Walkthroughs.Walkthroughs["verify"].Steps)
            {
                if (cmd.StartsWith("assert:"))
                {
                    string expr = cmd.Substring(7);
                    Assert.AreEqual(true, worldModel.Assert(expr), expr);
                }
                else
                {
                    worldModel.SendCommand(cmd);
                }
            }

            System.IO.File.Delete(tempFilename);
        }
    }
}
