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
        private void TestUndoRedo(string initialScript, string initialDisplayString, string transactionDescription, Action<IEditableScript> changeValue, string newDisplayString)
        {
            Assert.AreEqual(0, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            EditableScripts newScripts = Controller.CreateNewEditableScripts("game", "somescript", initialScript);
            Assert.AreEqual(1, newScripts.Scripts.Count());
            Assert.AreEqual(1, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            IEditableScript script = newScripts[0];
            Assert.AreEqual(initialDisplayString, script.DisplayString());
            Controller.StartTransaction(transactionDescription);
            changeValue(script);
            Controller.EndTransaction();
            Assert.AreEqual(2, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            Assert.AreEqual(transactionDescription, UndoList[0]);
            Assert.AreEqual(newDisplayString, script.DisplayString());
            Controller.Undo();
            Assert.AreEqual(1, UndoList.Count);
            Assert.AreEqual(1, RedoList.Count);
            Assert.AreEqual(transactionDescription, RedoList[0]);
            Assert.AreEqual(initialDisplayString, script.DisplayString());
            Controller.Redo();
            Assert.AreEqual(2, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            Assert.AreEqual(transactionDescription, UndoList[0]);
            Assert.AreEqual(newDisplayString, script.DisplayString());
        }

        [TestMethod]
        public void TestSimpleUndoRedo()
        {
            TestUndoRedo(
                "msg (\"hello\")",
                "Print \"hello\"",
                "Change msg script value",
                (script) => script.SetParameter(0, "\"new value\""),
                "Print \"new value\"");
        }

        [TestMethod]
        public void TestIfExpressionUndoRedo()
        {
            TestUndoRedo(
                "if (someExpression) { msg (\"Then script\") }",
                "If (someExpression) Then 'Print \"Then script\"'",
                "Change expression",
                (script) => ((EditableIfScript)script).SetAttribute("expression", "newExpression"),
                "If (newExpression) Then 'Print \"Then script\"'");
        }

        [TestMethod]
        public void TestIfThenUndoRedo()
        {
            TestUndoRedo(
                "if (someExpression) { msg (\"Then script\") }",
                "If (someExpression) Then 'Print \"Then script\"'",
                "Change then script",
                (script) => ((EditableIfScript)script).ThenScript[0].SetParameter(0, "\"new value\""),
                "If (someExpression) Then 'Print \"new value\"'");
        }

    }
}
