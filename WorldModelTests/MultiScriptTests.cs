using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;
using TextAdventures.Quest.Scripts;
using Moq;

namespace WorldModelTests
{
    [TestClass]
    public class MultiScriptTests
    {
        private WorldModel m_worldModel;

        [TestInitialize]
        public void Setup()
        {
            m_worldModel = new WorldModel();
        }

        private MultiScript CreateMultiScript(params string[] lines)
        {
            // Creates a new MultiScript with mock IScripts, with .Line returning each
            // line passed in as a parameter.

            List<IScript> source = new List<IScript>();
            foreach (string line in lines)
            {
                var script = new Mock<IScript>();
                script.Setup(s => s.Line).Returns(line);
                source.Add(script.Object);
            }

            MultiScript result = new MultiScript(m_worldModel, source.ToArray());
            result.UndoLog = m_worldModel.UndoLogger;

            return result;
        }

        private string GetLinesString(MultiScript multiScript)
        {
            return string.Join(";", multiScript.Scripts.Select(s => s.Line));
        }

        [TestMethod]
        public void TestMultiScriptCreation()
        {
            MultiScript multiScript = CreateMultiScript("line 1", "line 2", "line 3", "line 4");
            Assert.AreEqual(4, multiScript.Scripts.Count());
            Assert.AreEqual("line 2", multiScript.Scripts.ElementAt(1).Line);
            Assert.AreEqual("line 1;line 2;line 3;line 4", GetLinesString(multiScript));
        }

        [TestMethod]
        public void TestMultiScriptSwap()
        {
            MultiScript multiScript = CreateMultiScript("line 1", "line 2", "line 3", "line 4");

            // Swap lines 2 and 3

            m_worldModel.UndoLogger.StartTransaction("Swap lines 2 and 3");
            multiScript.Swap(1, 2);
            m_worldModel.UndoLogger.EndTransaction();

            // Check they are swapped correctly

            Assert.AreEqual("line 1;line 3;line 2;line 4", GetLinesString(multiScript));

            // Undo - should be back to original

            m_worldModel.UndoLogger.Undo();
            Assert.AreEqual("line 1;line 2;line 3;line 4", GetLinesString(multiScript));

            // Redo - lines 2 and 3 swapped again

            m_worldModel.UndoLogger.Redo();
            Assert.AreEqual("line 1;line 3;line 2;line 4", GetLinesString(multiScript));

            // Undo - should be back to original

            m_worldModel.UndoLogger.Undo();
            Assert.AreEqual("line 1;line 2;line 3;line 4", GetLinesString(multiScript));

            // Now swap two non-consecutive elements, lines 1 and 4

            m_worldModel.UndoLogger.StartTransaction("Swap lines 1 and 4");
            multiScript.Swap(0, 3);
            m_worldModel.UndoLogger.EndTransaction();

            // Check they are swapped correctly

            Assert.AreEqual("line 4;line 2;line 3;line 1", GetLinesString(multiScript));

            // Undo - should be back to original

            m_worldModel.UndoLogger.Undo();
            Assert.AreEqual("line 1;line 2;line 3;line 4", GetLinesString(multiScript));
        }
    }
}
