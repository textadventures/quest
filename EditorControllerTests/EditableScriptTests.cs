using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

namespace EditorControllerTests
{
    [TestClass]
    public class EditableScriptTests : EditorControllerTestBase
    {
        [TestMethod]
        public void TestUndoRedo()
        {
            Assert.AreEqual(0, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            EditableScripts newScripts = Controller.CreateNewEditableScripts("game", "somescript", "msg (\"hello\")");
            Assert.AreEqual(1, newScripts.Scripts.Count());
            Assert.AreEqual(1, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            IEditableScript script = newScripts[0];
            Assert.AreEqual("Print \"hello\"", script.DisplayString());
            Controller.StartTransaction("Change msg script value");
            script.SetParameter(0, "\"new value\"");
            Controller.EndTransaction();
            Assert.AreEqual(2, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            Assert.AreEqual("Change msg script value", UndoList[0]);
            Assert.AreEqual("Print \"new value\"", script.DisplayString());
            Controller.Undo();
            Assert.AreEqual(1, UndoList.Count);
            Assert.AreEqual(1, RedoList.Count);
            Assert.AreEqual("Change msg script value", RedoList[0]);
            Assert.AreEqual("Print \"hello\"", script.DisplayString());
            Controller.Redo();
            Assert.AreEqual(2, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            Assert.AreEqual("Change msg script value", UndoList[0]);
            Assert.AreEqual("Print \"new value\"", script.DisplayString());
        }
    }
}
