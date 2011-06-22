using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using AxeSoftware.Quest;
using System.Windows.Forms;

namespace AxeSoftware.Quest.EditorControls
{
    public class PopupEditors
    {
        public struct EditStringResult
        {
            public bool Cancelled;
            public string Result;
            public string ListResult;
        }

        private static Dictionary<ValidationMessage, string> s_validationMessages = new Dictionary<ValidationMessage, string> {
		    {ValidationMessage.OK,"No error"},
		    {ValidationMessage.ItemAlreadyExists,"Item '{0}' already exists in the list"},
		    {ValidationMessage.ElementAlreadyExists,"An element called '{0}' already exists in this game"},
            {ValidationMessage.InvalidAttributeName, "Invalid attribute name"},
            {ValidationMessage.ExceptionOccurred, "An error occurred: {1}"},
            {ValidationMessage.InvalidElementName, "Invalid element name"},
            {ValidationMessage.CircularTypeReference, "Circular type reference"},
        };

        public static EditStringResult EditString(string prompt, string defaultResult, IEnumerable<string> autoCompleteList = null)
        {
            return EditStringWithDropdown(prompt, defaultResult, null, null, null, autoCompleteList);
        }

        public static EditStringResult EditStringWithDropdown(string prompt, string defaultResult, string listCaption, IEnumerable<string> listItems, string defaultListSelection, IEnumerable<string> autoCompleteList = null)
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
            result.Cancelled = (inputResult.Length == 0);
            result.Result = inputResult;

            if (listItems != null)
            {
                result.ListResult = inputWindow.lstDropdown.Text;
            }

            return result;
        }

        private static string GetError(ValidationMessage validationMessage, string item, string data)
        {
            return string.Format(s_validationMessages[validationMessage], item, data);
        }

        public static void DisplayValidationError(ValidationResult result, string input, string title)
        {
            MessageBox.Show(GetError(result.Message, input, result.MessageData), title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static void EditScript(EditorController controller, ref IEditableScripts scripts, string attribute, string element, bool isReadOnly, Action dirtyAction)
        {
            ScriptEditorPopOut popOut = new ScriptEditorPopOut();

            popOut.ctlScriptEditor.HidePopOutButton();
            popOut.ctlScriptEditor.Helper.DoInitialise(controller, null);
            popOut.ctlScriptEditor.Populate(scripts);
            popOut.ctlScriptEditor.Helper.Dirty += (object sender, DataModifiedEventArgs e) => dirtyAction.Invoke();

            popOut.ShowDialog();
            scripts = popOut.ctlScriptEditor.Scripts;
            popOut.ctlScriptEditor.Save();
        }

    }
}