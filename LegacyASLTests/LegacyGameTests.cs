using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;
using AxeSoftware.Quest.LegacyASL;

namespace LegacyASLTests
{
    [TestClass]
    public class LegacyGameTests
    {
        private IASL m_game;
        private TestPlayer m_player = new TestPlayer();

        [TestInitialize]
        public void Init()
        {
            m_game = new LegacyGame(@"..\..\..\LegacyASLTests\test1.asl");
            m_game.PrintText += m_player.PrintText;
            m_game.RequestRaised += m_player.RequestRaised;
            m_game.Initialise(m_player);
            m_game.Begin();
        }

        [TestMethod]
        public void TestLookAt()
        {
            m_player.ClearBuffer();
            m_game.SendCommand("look at object");
            Assert.AreEqual("&gt; look at object", m_player.Buffer(0));
            Assert.AreEqual("object look desc", m_player.Buffer(1));
        }

        [TestMethod]
        public void TestWait()
        {
            m_player.ClearBuffer();
            m_game.SendCommand("wait");
            Assert.AreEqual("Start wait", m_player.Buffer(1));
            Assert.AreEqual(true, m_player.IsWaiting);
            Assert.AreEqual(2, m_player.BufferLength, "Expected nothing else in the output buffer after the wait command");
            m_player.IsWaiting = false;
            m_game.FinishWait();
            Assert.AreEqual("Done wait", m_player.Buffer(2));
        }

        [TestMethod]
        public void TestEnter()
        {
            m_player.ClearBuffer();
            m_game.SendCommand("enter");
            Assert.AreEqual("Enter text", m_player.Buffer(1));
            Assert.AreEqual(2, m_player.BufferLength, "Expected nothing else in the output buffer after the enter command");
            m_game.SendCommand("response");
            Assert.AreEqual("You entered: response", m_player.Buffer(2));
        }

        [TestMethod]
        public void TestMenu()
        {
            m_player.ClearBuffer();
            m_player.LatestMenu = null;
            m_game.SendCommand("x twin");
            Assert.AreEqual("- <i>Please select which twin you mean:</i>", m_player.Buffer(1));
            Assert.AreEqual(2, m_player.BufferLength, "Expected nothing else in the output buffer after menu displayed");
            Assert.AreNotEqual(null, m_player.LatestMenu);
            Assert.AreEqual(2, m_player.LatestMenu.Options.Count);
            Assert.AreEqual("Twin 1", m_player.LatestMenu.Options.ElementAt(0).Value);
            Assert.AreEqual("Twin 2", m_player.LatestMenu.Options.ElementAt(1).Value);
            m_game.SetMenuResponse(m_player.LatestMenu.Options.ElementAt(0).Key);
            Assert.AreEqual("It's twin 1", m_player.Buffer(3));
        }
    }
}
