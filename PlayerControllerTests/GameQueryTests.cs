using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

namespace PlayerControllerTests
{
    [TestClass]
    public class GameQueryTests
    {
        private string GetPath(string filename)
        {
            string folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Substring(6).Replace("/", @"\");
            return System.IO.Path.Combine(folder, @"..\..\" + filename);
        }

        [TestMethod]
        public void TestValidASL()
        {
            GameQuery query = new GameQuery(GetPath("test1.asl"));
            Assert.IsTrue(query.Initialise());
            Assert.AreEqual("Test ASL Game", query.GameName);
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
        }

        [TestMethod]
        public void TestInvalidQuest()
        {
            GameQuery query = new GameQuery(GetPath("test2.quest"));
            Assert.IsFalse(query.Initialise());
        }

    }
}
