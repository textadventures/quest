using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest.EditorControls
{
    [ControlType("verbs")]
    class WFVerbsControl : WFAttributesControl
    {
        private static Dictionary<string, string> s_allowedTypes = new Dictionary<string, string> {
            {"string", "Print a message"},
            {"script", "Run a script"}
        };

        public WFVerbsControl()
        {
            ctlSplitContainerMain.Panel1Collapsed = true;
            lblAttributesTitle.Text = "Verbs";
            lblAttributesTitle.Width = 60;
        }

        protected override bool CanDisplayAttribute(string attribute, object value)
        {
            if (!Controller.IsVerbAttribute(attribute)) return false;
            return typeof(string).IsAssignableFrom(value.GetType()) || typeof(IEditableScripts).IsAssignableFrom(value.GetType());
        }

        protected override Dictionary<string, string> AllowedTypes
        {
            get { return s_allowedTypes; }
        }

        protected override void Add()
        {
            // TO DO: This fetches all verbs in the game, but verbs can be defined in rooms, so we should
            // filter out any out-of-scope verbs.

            PopupEditors.EditStringResult result = PopupEditors.EditString(
                "Please enter a name for the new verb",
                string.Empty,
                Controller.GetVerbProperties());

            if (result.Cancelled) return;

            bool setSelection = true;

            if (!lstAttributes.Items.ContainsKey(result.Result))
            {
                Controller.StartTransaction(string.Format("Add '{0}' verb", result.Result));

                if (!Controller.IsVerbAttribute(result.Result))
                {
                    string newVerbId = Controller.CreateNewVerb(null, false);
                    IEditorData verbData = Controller.GetEditorData(newVerbId);
                    verbData.SetAttribute("property", result.Result);
                    verbData.SetAttribute("pattern", result.Result);
                    verbData.SetAttribute("defaulttext", "You can't " + result.Result + " that.");
                }

                ValidationResult setAttrResult = Data.SetAttribute(result.Result, String.Empty);
                if (!setAttrResult.Valid)
                {
                    PopupEditors.DisplayValidationError(setAttrResult, result.Result, "Unable to add verb");
                    setSelection = false;
                }
                Controller.EndTransaction();
            }

            if (setSelection)
            {
                lstAttributes.Items[result.Result].Selected = true;
                lstAttributes.SelectedItems[0].EnsureVisible();
            }
        }
    }
}
