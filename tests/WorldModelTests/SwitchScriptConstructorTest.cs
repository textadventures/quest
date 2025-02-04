using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

using TextAdventures.Quest;
using TextAdventures.Quest.Scripts;

namespace WorldModelTests
{
    [TestClass()]
    public class SwitchScriptConstructorTest
    {
        WorldModel m_worldModel;
        ScriptFactory m_scriptFactory;
        SwitchScriptConstructor m_constructor;

        ScriptContext scriptContext;

        [TestInitialize]
        public void Setup()
        {
            m_worldModel = new WorldModel();
            m_scriptFactory = new ScriptFactory(m_worldModel);

            m_constructor = new SwitchScriptConstructor();
            m_constructor.WorldModel = m_worldModel;            
            m_constructor.ScriptFactory = m_scriptFactory;

            scriptContext = new ScriptContext(m_worldModel);
        }

        [TestMethod()]
        public void CreateTest()
        {
            string text = @"switch (""1"") {
            case (1) {
                msg (""!"")
            }
            }";
            var script = m_constructor.Create(text, scriptContext);
            QuestDictionary<IScript> actualCases = (QuestDictionary<IScript>) script.GetParameter(1);
            
            Assert.AreEqual(1, actualCases.Count);
            Assert.IsTrue(actualCases.Contains("1"));

            text = @"switch (""1"") {
            case (StringListItem(myStringList, 0), 1) {
                msg (""!"")
            }
            }";
            script = m_constructor.Create(text, scriptContext);
            actualCases = (QuestDictionary<IScript>)script.GetParameter(1);
            
            Assert.AreEqual(2, actualCases.Count);
            Assert.ReferenceEquals(actualCases["StringListItem(myStringList, 0)"],
                actualCases["1"]);
        }
    }
}
