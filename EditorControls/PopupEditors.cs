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
            {ValidationMessage.InvalidElementNameMultipleSpaces, "Invalid element name. An element name cannot start or end with a space, and cannot contain multiple consecutive spaces."},
            {ValidationMessage.InvalidElementNameInvalidWord, "Invalid element name. Elements cannot contain these words: " + string.Join(", ", EditorController.ExpressionKeywords)},
            {ValidationMessage.CannotRenamePlayerElement, "The player object cannot be renamed"},
            {ValidationMessage.InvalidElementNameStartsWithNumber, "Invalid element name. An element name cannot start with a number."},
            {ValidationMessage.MismatchingBrackets, "The number of opening brackets \"(\" does not match the number of closing brackets \")\"."},
        };

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
            System.EventHandler<DataModifiedEventArgs> dirtyEventHandler = (object sender, DataModifiedEventArgs e) => dirtyAction.Invoke();
            popOut.ctlScriptEditor.Helper.Dirty += dirtyEventHandler;

            popOut.ShowDialog();
            scripts = popOut.ctlScriptEditor.Scripts;
            popOut.ctlScriptEditor.Save();
            popOut.ctlScriptEditor.Populate((IEditableScripts)null);
            popOut.ctlScriptEditor.Helper.DoUninitialise();
            popOut.ctlScriptEditor.Helper.Dirty -= dirtyEventHandler;
        }

    }
}