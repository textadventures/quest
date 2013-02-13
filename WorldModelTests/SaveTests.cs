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
        public void RunWalkthrough()
        {
            string folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Substring(6).Replace("/", @"\");
            string templateFolder = System.IO.Path.Combine(folder, @"..\..\..\WorldModel\WorldModel\Core");
            WorldModel worldModel = new WorldModel(
                System.IO.Path.Combine(folder, @"..\..\savetest.aslx"),
                templateFolder,
                null);

            Mock<IPlayer> player = new Mock<IPlayer>();
            bool success = worldModel.Initialise(player.Object);
            Assert.IsTrue(success, "Initialisation failed");

            worldModel.Begin();

            worldModel.SendCommand("update");

            string tempFilename = System.IO.Path.GetTempFileName();
            worldModel.Save(tempFilename, null);

            WorldModel savedGameWorldModel = new WorldModel(tempFilename, null, null);
            success = savedGameWorldModel.Initialise(player.Object);
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
