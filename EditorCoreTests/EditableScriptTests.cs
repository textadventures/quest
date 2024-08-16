using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;

namespace EditorControllerTests
{
    [TestClass]
    public class EditableScriptTests : EditorControllerTestBase
    {
        private void TestUndoRedo(string initialScript, string initialDisplayString, string transactionDescription, Action<IEditableScript> changeValue, string newDisplayString)
        {
            EditableScriptsUpdatedEventArgs updatedEventArgs = null;

            // Nothing in the undo/redo lists to start with
            Assert.AreEqual(0, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);

            // Create a new script and apply it to game.somescript
            EditableScripts newScripts = Controller.CreateNewEditableScripts("game", "somescript", initialScript, true);
            
            // When the Updated event fires, store the event arguments locally
            newScripts.Updated += delegate(object sender, EditableScriptsUpdatedEventArgs e)
            {
                updatedEventArgs = e;
            };

            // We should now have one script, and the undo list should have one item
            Assert.AreEqual(1, newScripts.Scripts.Count());
            Assert.AreEqual(1, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            IEditableScript script = newScripts[0];
            Assert.AreEqual(initialDisplayString, script.DisplayString());

            // Now make a change to the script
            Controller.StartTransaction(transactionDescription);
            changeValue(script);

            // We expect the Updated event to have been triggered
            Assert.IsTrue(updatedEventArgs != null, "Expected Updated event to be triggered");

            // TO DO: Test the updated event args are correct

            Controller.EndTransaction();

            // We now have an additional undo list item
            Assert.AreEqual(2, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            Assert.AreEqual(transactionDescription, UndoList[0]);

            // Display string should be correct
            Assert.AreEqual(newDisplayString, script.DisplayString());

            // Now undo, and check the undo/redo lists are correct
            Controller.Undo();
            Assert.AreEqual(1, UndoList.Count);
            Assert.AreEqual(1, RedoList.Count);
            Assert.AreEqual(transactionDescription, RedoList[0]);

            // The display string should be back to what it was before the change
            Assert.AreEqual(initialDisplayString, script.DisplayString());

            // Now redo, and check the undo/redo lists are correct
            Controller.Redo();
            Assert.AreEqual(2, UndoList.Count);
            Assert.AreEqual(0, RedoList.Count);
            Assert.AreEqual(transactionDescription, UndoList[0]);

            // Display string should be back to the "changed" version
            Assert.AreEqual(newDisplayString, script.DisplayString());
        }

        [TestMethod]
        public void TestSimpleUndoRedo()
        {
            TestUndoRedo(
                "msg (\"hello\")",
                "Print \"hello\"",
                "Change msg script value",
                (script) => script.SetParameter("0", "\"new value\""),
                "Print \"new value\"");
        }

        [TestMethod]
        public void TestIfExpressionUndoRedo()
        {
            TestUndoRedo(
                "if (someExpression) { msg (\"Then script\") }",
                "If (someExpression) Then (Print \"Then script\")",
                "Change expression",
                (script) => ((EditableIfScript)script).SetAttribute("expression", "newExpression"),
                "If (newExpression) Then (Print \"Then script\")");
        }

        [TestMethod]
        public void TestIfThenUndoRedo()
        {
            TestUndoRedo(
                "if (someExpression) { msg (\"Then script\") }",
                "If (someExpression) Then (Print \"Then script\")",
                "Change then script",
                (script) => ((EditableIfScript)script).ThenScript[0].SetParameter("0", "\"new value\""),
                "If (someExpression) Then (Print \"new value\")");
        }

        [TestMethod]
        public void TestIfThenElseIf()
        {
            // Create an "if (...) { } else if (...) { }" script
            EditableScripts newScripts = Controller.CreateNewEditableScripts("game", "somescript", "if (someExpression) { msg (\"Then script\") }", true);
            ((EditableIfScript)newScripts[0]).AddElseIf();
            EditableIfScript.EditableElseIf newElseIf = ((EditableIfScript)newScripts[0]).ElseIfScripts.First();
            newElseIf.Expression = "elseIfExpression";
            newElseIf.EditableScripts.AddNew("msg (\"test\")", "game");

            // Capture update events
            EditableScriptsUpdatedEventArgs lastArgs = null;
            newScripts.Updated += (object sender, EditableScriptsUpdatedEventArgs e) => { lastArgs = e; };

            // Check the initial display string is correct
            string initialExpectedDisplayString = "If (someExpression) Then (Print \"Then script\"), Else If (elseIfExpression) Then (Print \"test\")";
            Assert.AreEqual(initialExpectedDisplayString, newScripts.DisplayString());

            // Now change the expression
            Controller.StartTransaction("Change elseif expression");
            newElseIf.Expression = "newElseIfExpression";
            Controller.EndTransaction();

            // Check the new display string is correct, and that we received the update event
            string newExpectedDisplayString = "If (someExpression) Then (Print \"Then script\"), Else If (newElseIfExpression) Then (Print \"test\")";
            Assert.AreEqual(newExpectedDisplayString, newScripts.DisplayString());
            Assert.AreEqual(lastArgs.UpdatedScriptEventArgs.NewValue, "newElseIfExpression");

            // Now undo and redo, and check the display strings update correctly
            Controller.Undo();
            Assert.AreEqual(initialExpectedDisplayString, newScripts.DisplayString());
            Controller.Redo();
            Assert.AreEqual(newExpectedDisplayString, newScripts.DisplayString());

            // Now change the script. This automatically creates a transaction.
            newElseIf.EditableScripts.AddNew("msg (\"test2\")", "game");

            // Check the new display string is correct
            string newerExpectedDisplayString = "If (someExpression) Then (Print \"Then script\"), Else If (newElseIfExpression) Then (Print \"test\" / Print \"test2\")".Replace(" / ", Environment.NewLine);
            Assert.AreEqual(newerExpectedDisplayString, newScripts.DisplayString());

            // Now undo and redo, and check the display strings update correctly
            Controller.Undo();
            Assert.AreEqual(newExpectedDisplayString, newScripts.DisplayString());
            Controller.Redo();
            Assert.AreEqual(newerExpectedDisplayString, newScripts.DisplayString());
        }

    }
}
