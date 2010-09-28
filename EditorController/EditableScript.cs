using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;
using System.Text.RegularExpressions;

namespace AxeSoftware.Quest
{
    public class EditableScript : IEditableScript
    {
        // TO DO: Adding and removing multiscripts needs to come through here, this
        // should wrap everything to do with editing scripts.

        private string m_displayTemplate = null;
        private static Regex s_regex = new Regex("#(?<attribute>\\d+)");
        private IScript m_script;

        internal EditableScript(IScript script, Element parent)
        {
            Script = script;
            ((IMutableField)Script).Parent = parent.Fields;
        }

        internal string DisplayTemplate
        {
            get { return m_displayTemplate; }
            set { m_displayTemplate = value; }
        }

        public string DisplayString()
        {
            return DisplayString(-1, "");
        }

        public string DisplayString(int index, string newValue)
        {
            // This version of DisplayString is used while we are editing an attribute value
            // as we don't want to save updates for every keypress

            if (string.IsNullOrEmpty(m_displayTemplate)) return Script.Save();

            string result = m_displayTemplate;

            while (s_regex.IsMatch(result))
            {
                Match m = s_regex.Match(result);
                int attributeNum = int.Parse(m.Groups["attribute"].Value);
                string attributeValue;
                if (attributeNum == index)
                {
                    attributeValue = newValue;
                }
                else
                {
                    attributeValue = Script.GetParameter(attributeNum);
                }
                result = s_regex.Replace(result, attributeValue);
            }

            return result;
        }

        public string EditorName
        {
            get { return Script.Keyword; }
        }

        public string GetParameter(int index)
        {
            return Script.GetParameter(index);
        }

        public void SetParameter(int index, string value)
        {
            Script.SetParameter(index, value);
        }

        public event EventHandler<EditableScriptUpdatedEventArgs> Updated;

        internal IScript Script
        {
            get
            {
                return m_script;
            }
            set
            {
                if (m_script != null)
                {
                    m_script.ScriptUpdated -= ScriptUpdated;
                }
                m_script = value;
                m_script.ScriptUpdated += ScriptUpdated;
            }
        }

        void ScriptUpdated(object sender, ScriptUpdatedEventArgs e)
        {
            if (Updated != null)
            {
                if (e.IsParameterUpdate)
                {
                    Updated(this, new EditableScriptUpdatedEventArgs(e.Index, e.NewValue));
                }
                else
                {
                    Updated(this, new EditableScriptUpdatedEventArgs());
                }
            }
        }
    }
}
