using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using TextAdventures.Quest;
using System.Windows.Forms;

namespace TextAdventures.Quest.EditorControls
{
    public class PopupEditors
    {
        public struct EditStringResult
        {
            public bool Cancelled;
            public string Result;
            public string ListResult;
            public string AliasResult;
        }

        public static EditStringResult EditString(string prompt, string defaultResult, IEnumerable<string> autoCompleteList = null, bool allowEmptyString = false)
        {
            return EditStringWithDropdown(prompt, defaultResult, null, null, null, autoCompleteList, allowEmptyString);
        }

        public static EditStringResult EditStringWithDropdown(string prompt, string defaultResult, string listCaption, IEnumerable<string> listItems, string defaultListSelection, IEnumerable<string> autoCompleteList = null, bool allowEmptyString = false)
        {
            EditStringResult result = new EditStringResult();

            InputWindow inputWindow = new InputWindow();
            inputWindow.lblPrompt.Text = prompt + ":";
            if (autoCompleteList != null)
            {
                inputWindow.SetAutoComplete(autoCompleteList);
            }
            inputWindow.ActiveInputControl.Text = defaultResult;
            inputWindow.ActiveInputControl.Focus();
            inputWindow.txtInput.SelectAll();

            if (listItems != null)
            {
                inputWindow.SetDropdown(listCaption, listItems, defaultListSelection);
            }

            inputWindow.ShowDialog();

            string inputResult = inputWindow.ActiveInputControl.Text;
            result.Cancelled = (inputWindow.Cancelled || (!allowEmptyString && inputResult.Length == 0));
            result.Result = inputResult;

            if (listItems != null)
            {
                result.ListResult = inputWindow.lstDropdown.Text;
            }

            return result;
        }

        public static void DisplayValidationError(ValidationResult result, string input, string title)
        {
            string message = EditorController.GetValidationError(result, input);
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static void EditScript(EditorController controller, ref IEditableScripts scripts, string attribute, string element, bool isReadOnly, Action dirtyAction)
        {
            ScriptEditorPopOut popOut = new ScriptEditorPopOut();

            popOut.ctlScriptEditor.HidePopOutButton();
            popOut.ctlScriptEditor.Helper.DoInitialise(controller, null);
            popOut.ctlScriptEditor.Populate(scripts);
            System.EventHandler<DataModifiedEventArgs> dirtyEventHandler = (object sender, DataModifiedEventArgs e) => dirtyAction.Invoke();
            popOut.ctlScriptEditor.Helper.Dirty += dirtyEventHandler;

            popOut.ShowDialog();
            scripts = popOut.ctlScriptEditor.Scripts;
            popOut.ctlScriptEditor.Save();
            popOut.ctlScriptEditor.Populate((IEditableScripts)null);
            popOut.ctlScriptEditor.Helper.DoUninitialise();
            popOut.ctlScriptEditor.Helper.Dirty -= dirtyEventHandler;
        }

        private static EditorController s_popOutScriptAdderController;
        private static ScriptAdderPopOut s_popOutScriptAdder;

        public static string AddScript(EditorController controller)
        {
            if (controller != s_popOutScriptAdderController)
            {
                if (s_popOutScriptAdder != null)
                {
                    s_popOutScriptAdder.ctlScriptAdder.Uninitialise();
                }

                s_popOutScriptAdder = new ScriptAdderPopOut();
                s_popOutScriptAdder.ctlScriptAdder.Initialise(controller);

                s_popOutScriptAdderController = controller;
            }

            s_popOutScriptAdder.ctlScriptAdder.PopulateTree();

            s_popOutScriptAdder.SelectedScript = null;
            s_popOutScriptAdder.ShowDialog();
            return s_popOutScriptAdder.SelectedScript;
        }
    }
}