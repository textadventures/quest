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

            IDictionary<string, string> availableVerbs = Controller.GetVerbProperties();

            PopupEditors.EditStringResult result = PopupEditors.EditString(
                "Please enter a name for the new verb",
                string.Empty,
                availableVerbs.Values);

            if (result.Cancelled) return;

            string selectedPattern = result.Result;

            var attributeForSelectedPattern = from verb in availableVerbs.Keys
                                              where availableVerbs[verb] == selectedPattern
                                              select verb;

            string selectedAttribute = attributeForSelectedPattern.FirstOrDefault();

            if (selectedAttribute == null)
            {
                selectedAttribute = selectedPattern.Replace(" ", "");
            }

            bool setSelection = true;

            if (!lstAttributes.Items.ContainsKey(selectedAttribute))
            {
                Controller.StartTransaction(string.Format("Add '{0}' verb", selectedPattern));

                if (!Controller.IsVerbAttribute(selectedAttribute))
                {
                    string newVerbId = Controller.CreateNewVerb(null, false);
                    IEditorData verbData = Controller.GetEditorData(newVerbId);
                    verbData.SetAttribute("property", selectedAttribute);
                    verbData.SetAttribute("pattern", selectedPattern);
                    verbData.SetAttribute("defaulttext", "You can't " + selectedPattern + " that.");
                }

                ValidationResult setAttrResult = Data.SetAttribute(selectedAttribute, String.Empty);
                if (!setAttrResult.Valid)
                {
                    PopupEditors.DisplayValidationError(setAttrResult, selectedAttribute, "Unable to add verb");
                    setSelection = false;
                }
                Controller.EndTransaction();
            }

            if (setSelection)
            {
                lstAttributes.Items[selectedAttribute].Selected = true;
                lstAttributes.SelectedItems[0].EnsureVisible();
            }
        }

        protected override string GetAttributeDisplayName(IEditorAttributeData attr)
        {
            string displayName = Controller.GetVerbPatternForAttribute(attr.AttributeName);
            return displayName ?? attr.AttributeName;
        }
    }
}
