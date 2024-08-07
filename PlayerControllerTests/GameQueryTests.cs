using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;

namespace PlayerControllerTests
{
    [TestClass]
    public class GameQueryTests
    {
        private string GetPath(string filename)
        {
            return @"..\..\..\" + filename;
        }

        [TestMethod]
        public void TestValidASL()
        {
            GameQuery query = new GameQuery(GetPath("test1.asl"));
            Assert.IsTrue(query.Initialise());
            Assert.AreEqual("Test ASL Game", query.GameName);
            Assert.AreEqual(410, query.ASLVersion);
            Assert.AreEqual("ACAB148143981E8F7F9A95151F4CB9F3", query.GameID);
            Assert.AreEqual(null, query.Category);
            Assert.AreEqual(null, query.Description);
        }

        [TestMethod]
        public void TestInvalidASL()
        {
            GameQuery query = new GameQuery(GetPath("test2.asl"));
            Assert.IsFalse(query.Initialise());
        }

        [TestMethod]
        public void TestValidQuest()
        {
            GameQuery query = new GameQuery(GetPath("test1.quest"));
            Assert.IsTrue(query.Initialise());
            Assert.AreEqual("Test ASLX Game", query.GameName);
            Assert.AreEqual(520, query.ASLVersion);
            Assert.AreEqual("33cb328d-bf80-42f7-a136-e916e7b45ed8", query.GameID);
            Assert.AreEqual("Test Category", query.Category);
            Assert.AreEqual("Test Description", query.Description);
        }

        [TestMethod]
        public void TestInvalidQuest()
        {
            GameQuery query = new GameQuery(GetPath("test2.quest"));
            Assert.IsFalse(query.Initialise());
        }
    }
}
