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
            string folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Substring(6).Replace("/", @"\");
            string templateFolder = System.IO.Path.Combine(folder, @"..\..\..\WorldModel\WorldModel\Core");
            WorldModel worldModel = new WorldModel(
                System.IO.Path.Combine(folder, @"..\..\walkthrough.aslx"),
                templateFolder,
                null);

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
