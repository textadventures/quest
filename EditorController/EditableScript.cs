using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;
using System.Text.RegularExpressions;
using System.Collections;

namespace AxeSoftware.Quest
{
    public class EditableScript : EditableScriptBase, IEditableScript
    {
        private string m_displayTemplate = null;
        private static Regex s_regex = new Regex("#(?<attribute>\\d+)");
        private List<EditableScripts> m_watchedNestedScripts = new List<EditableScripts>();
        private string m_editorName;

        internal EditableScript(EditorController controller, IScript script, UndoLogger undoLogger)
            : base(controller, script, undoLogger)
        {
            m_editorName = Script.Keyword;
        }

        internal string DisplayTemplate
        {
            get { return m_displayTemplate; }
            set { m_displayTemplate = value; }
        }

        public override string DisplayString(int index, object newValue)
        {
            // This version of DisplayString is used while we are editing an attribute value
            // as we don't want to save updates for every keypress

            if (string.IsNullOrEmpty(m_displayTemplate)) return (Script == null) ? null : Script.Save();

            string result = m_displayTemplate;

            while (s_regex.IsMatch(result))
            {
                Match m = s_regex.Match(result);
                int attributeNum = int.Parse(m.Groups["attribute"].Value);
                string attributeValue;

                object value;
                if (attributeNum == index)
                {
                    value = newValue;
                }
                else
                {
                    value = GetParameter(attributeNum);
                }

                if (value is IDataWrapper) value = ((IDataWrapper)value).GetUnderlyingValue();

                IScript scriptValue = value as IScript;
                string stringValue = value as string;
                ICollection collectionValue = value as ICollection;

                if (stringValue != null)
                {
                    attributeValue = (stringValue.Length == 0) ? "?" : stringValue;
                }
                else if (scriptValue != null)
                {
                    EditableScripts editableScripts = EditableScripts.GetInstance(Controller, scriptValue);
                    RegisterNestedScriptForUpdates(editableScripts);
                    attributeValue = (editableScripts.Count == 0) ? "(nothing)" : editableScripts.DisplayString();
                }
                else if (collectionValue != null)
                {
                    attributeValue = collectionValue.Count.ToString();
                }
                else if (value == null)
                {
                    attributeValue = "(nothing)";
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false, "Unknown attribute type for DisplayString");
                    attributeValue = "<unknown>";
                }

                result = s_regex.Replace(result, attributeValue, 1);
            }

            return result;
        }

        private void RegisterNestedScriptForUpdates(EditableScripts script)
        {
            if (!m_watchedNestedScripts.Contains(script))
            {
                script.Updated += nestedScript_Updated;
                m_watchedNestedScripts.Add(script);
            }
        }

        void nestedScript_Updated(object sender, EditableScriptsUpdatedEventArgs e)
        {
            RaiseUpdateForNestedScriptChange(e);
        }

        public override string EditorName
        {
            get { return m_editorName; }
            set { m_editorName = value; }
        }

        public override object GetParameter(int index)
        {
            if (EditorName.StartsWith("(function)"))
            {
                return FunctionCallScript.GetFunctionCallParameter(index);
            }
            return Script.GetParameter(index);
        }

        public override void SetParameter(int index, object value)
        {
            if (EditorName.StartsWith("(function)"))
            {
                FunctionCallScript.SetFunctionCallParameter(index, value);
                return;
            }
            Script.SetParameter(index, value);
        }

        public override ScriptType Type
        {
            get { return ScriptType.Normal; }
        }
    }
}
