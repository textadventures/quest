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
    public class Walkthrough
    {
        [TestMethod]
        public void RunWalkthrough()
        {
            WorldModel worldModel = new WorldModel(
                new FileGameDataProvider(Path.Combine("..", "..", "..", "walkthrough.aslx")),
                Path.Combine("..", "..", ".."));

            Mock<IPlayer> player = new Mock<IPlayer>();
            worldModel.Initialise(player.Object);
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
