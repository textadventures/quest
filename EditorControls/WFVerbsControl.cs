using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("verbs")]
    class WFVerbsControl : WFAttributesControl
    {
        private class VerbsSubEditorControlData : AttributeSubEditorControlData
        {
            private static Dictionary<string, string> s_allowedTypes = new Dictionary<string, string> {
                {"string", "Print a message"},
                {"script", "Run a script"},
                {"scriptdictionary", "Require another object"},
            };

            public VerbsSubEditorControlData(string attribute)
                : base(attribute)
            {
            }

            protected override Dictionary<string, string> AllowedTypes
            {
                get { return s_allowedTypes; }
            }

            public override string GetString(string tag)
            {
                switch (tag)
                {
                    case "keyname":
                        return "Object";
                    case "keyprompt":
                        return "Please enter the object name";
                    case "source":
                        return "object";
                }

                return base.GetString(tag);
            }
        }

        private static List<Type> s_validTypes = new List<Type> {
            typeof(string),
            typeof(IEditableScripts),
            typeof(IEditableDictionary<IEditableScripts>)
        };

        private IDictionary<string, string> m_clashMessages;
        private string m_defaultExpression;

        public WFVerbsControl()
        {
            ctlSplitContainerMain.Panel1Collapsed = true;
            ctlSplitContainer.SplitterDistance = 125;
            lblAttributesTitle.Text = "Verbs";
            lblAttributesTitle.Width = 60;
            cmdOnChange.Available = false;
        }

        public override void Initialise(EditorController controller, IEditorControl controlData)
        {
            base.Initialise(controller, controlData);
            m_clashMessages = controlData == null ? null : controlData.GetDictionary("clashmessages");
            m_defaultExpression = controlData == null ? null : controlData.GetString("defaultexpression");
        }

        protected override bool CanDisplayAttribute(string attribute, object value, bool isInherited)
        {
            if (!Controller.IsVerbAttribute(attribute)) return false;
            if (Controller.SimpleMode && isInherited) return false;
            Type valueType = value.GetType();
            return s_validTypes.Any(t => t.IsAssignableFrom(valueType));
        }

        protected override IEditorControl GetControlData(string attribute)
        {
            return new VerbsSubEditorControlData(attribute);
        }

        protected override void Add()
        {
            Save();

            // TO DO: This fetches all verbs in the game, but verbs can be defined in rooms, so we should
            // filter out any out-of-scope verbs.

            IDictionary<string, string> availableVerbs = Controller.GetVerbProperties();

            PopupEditors.EditStringResult result = PopupEditors.EditString(
                "Please enter a name for the new verb",
                string.Empty,
                availableVerbs.Values);

            if (result.Cancelled) return;

            string selectedPattern = result.Result.ToLower();
            string selectedAttribute = Controller.GetVerbAttributeForPattern(selectedPattern);

            AddVerb(selectedPattern, selectedAttribute);
        }

        private void AddVerb(string selectedPattern, string selectedAttribute)
        {
            bool setSelection = true;

            if (!lstAttributes.Items.ContainsKey(selectedAttribute))
            {
                if (!CanAddVerb(selectedPattern, selectedAttribute)) return;

                Controller.StartTransaction(string.Format("Add '{0}' verb", selectedPattern));

                if (!Controller.IsVerbAttribute(selectedAttribute))
                {
                    CreateNewVerb(selectedPattern, selectedAttribute);
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

        private bool CanAddVerb(string selectedPattern, string selectedAttribute)
        {
            if (!Controller.IsVerbAttribute(selectedAttribute))
            {
                TextAdventures.Quest.EditorController.CanAddVerbResult canAddResult = Controller.CanAddVerb(selectedPattern);
                if (!canAddResult.CanAdd)
                {
                    string clashMessage = "Verb would clash with command: " + canAddResult.ClashingCommandDisplay;
                    if (m_clashMessages.ContainsKey(canAddResult.ClashingCommand))
                    {
                        clashMessage += Environment.NewLine + Environment.NewLine + m_clashMessages[canAddResult.ClashingCommand];
                    }
                    MessageBox.Show(clashMessage, "Unable to add verb", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            return true;
        }

        private void CreateNewVerb(string selectedPattern, string selectedAttribute)
        {
            string newVerbId = Controller.CreateNewVerb(null, false);
            IEditorData verbData = Controller.GetEditorData(newVerbId);
            verbData.SetAttribute("property", selectedAttribute);
            EditableCommandPattern pattern = (EditableCommandPattern)verbData.GetAttribute("pattern");
            pattern.Pattern = selectedPattern;
            verbData.SetAttribute("defaultexpression", m_defaultExpression.Replace("#verb#", selectedPattern));
        }

        protected override string GetAttributeDisplayName(IEditorAttributeData attr)
        {
            string displayName = Controller.GetDisplayVerbPatternForAttribute(attr.AttributeName);
            return displayName ?? attr.AttributeName;
        }
    }
}
