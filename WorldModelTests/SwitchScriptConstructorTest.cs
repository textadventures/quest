using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

using AxeSoftware.Quest;
using AxeSoftware.Quest.Scripts;

namespace WorldModelTests
{
    [TestClass()]
    public class SwitchScriptConstructorTest
    {
        WorldModel m_worldModel;
        ScriptFactory m_scriptFactory;
        SwitchScriptConstructor m_constructor;

        Element a;

        [TestInitialize]
        public void Setup()
        {
            m_worldModel = new WorldModel();
            m_scriptFactory = new ScriptFactory(m_worldModel);

            m_constructor = new SwitchScriptConstructor();
            m_constructor.WorldModel = m_worldModel;            
            m_constructor.ScriptFactory = m_scriptFactory;

            a = m_worldModel.GetElementFactory(ElementType.Object).Create("a");
        }

        [TestMethod()]
        public void CreateTest()
        {
            string text = @"switch (""1"") {
            case (1) {
                msg (""!"")
            }
            }";
            var script = m_constructor.Create(text, a);
            QuestDictionary<IScript> actualCases = (QuestDictionary<IScript>) script.GetParameter(1);
            
            Assert.AreEqual(1, actualCases.Count);
            Assert.IsTrue(actualCases.Contains("1"));

            text = @"switch (""1"") {
            case (StringListItem(myStringList, 0), 1) {
                msg (""!"")
            }
            }";
            script = m_constructor.Create(text, a);
            actualCases = (QuestDictionary<IScript>)script.GetParameter(1);
            
            Assert.AreEqual(2, actualCases.Count);
            Assert.ReferenceEquals(actualCases["StringListItem(myStringList, 0)"],
                actualCases["1"]);
        }
    }
}
