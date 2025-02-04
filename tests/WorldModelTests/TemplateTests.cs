using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;

namespace WorldModelTests
{
    [TestClass]
    public class TemplateTests
    {
        private WorldModel m_worldModel;

        [TestInitialize]
        public void Init()
        {
            m_worldModel = new WorldModel();
            m_worldModel.Template.AddTemplate("Test1", "my text", false);
            m_worldModel.Template.AddTemplate("Test2", "other text", false);
        }

        [TestMethod]
        public void TestGetText()
        {
            Assert.AreEqual("my text", m_worldModel.Template.GetText("Test1"));
            Assert.AreEqual("other text", m_worldModel.Template.GetText("Test2"));
        }

        [TestMethod]
        public void TestReplaceTemplateText()
        {
            Assert.AreEqual("this is my text", m_worldModel.Template.ReplaceTemplateText("this is [Test1]"));
            Assert.AreEqual("this is other text", m_worldModel.Template.ReplaceTemplateText("this is [Test2]"));
            Assert.AreEqual("my text is my other text", m_worldModel.Template.ReplaceTemplateText("[Test1] is my [Test2]"));
        }

        [TestMethod]
        public void TestReplaceTemplateText_InvalidTemplateNames()
        {
            Assert.AreEqual("[unknown]", m_worldModel.Template.ReplaceTemplateText("[unknown]"));
            Assert.AreEqual("[unknown] and my text", m_worldModel.Template.ReplaceTemplateText("[unknown] and [Test1]"));
            Assert.AreEqual("other text and [unknown]", m_worldModel.Template.ReplaceTemplateText("[Test2] and [unknown]"));
            Assert.AreEqual("[unknown1], my text, [unknown2], other text", m_worldModel.Template.ReplaceTemplateText("[unknown1], [Test1], [unknown2], [Test2]"));
        }
    }
}
