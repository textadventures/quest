using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;
using Moq;
using QuestViva.Common;

namespace WorldModelTests
{
    [TestClass]
    public class Walkthrough
    {
        [TestMethod]
        public async Task RunWalkthrough()
        {
            var gameDataProvider = new FileGameDataProvider(Path.Combine("..", "..", "..", "walkthrough.aslx"), "test");
            var gameData = await gameDataProvider.GetData();
            WorldModel worldModel = new WorldModel(
                gameData,
                Path.Combine("..", "..", ".."));

            Mock<IPlayer> player = new Mock<IPlayer>();
            await worldModel.Initialise(player.Object);
            worldModel.Begin();

            foreach (string cmd in worldModel.Walkthroughs.Walkthroughs["debug"].Steps)
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
        }
    }
}
