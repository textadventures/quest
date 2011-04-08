using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;
using System.Text.RegularExpressions;

namespace AxeSoftware.Quest
{
    public class EditableScript : EditableScriptBase, IEditableScript
    {
        // TO DO: Adding and removing multiscripts needs to come through here, this
        // should wrap everything to do with editing scripts.

        private string m_displayTemplate = null;
        private static Regex s_regex = new Regex("#(?<attribute>\\d+)");

        internal EditableScript(IScript script, UndoLogger undoLogger)
            : base(script, undoLogger)
        {
        }

        internal string DisplayTemplate
        {
            get { return m_displayTemplate; }
            set { m_displayTemplate = value; }
        }

        public override string DisplayString(int index, string newValue)
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
                if (attributeNum == index)
                {
                    attributeValue = newValue;
                }
                else
                {
                    //attributeValue = (string)Script.GetParameter(attributeNum);
                    attributeValue = Script.GetParameter(attributeNum) as string;

                    // TO DO: This shouldn't be necessary
                    if (attributeValue == null) attributeValue = "{NOTSTRING}";
                }
                
                // TO DO: This shouldn't be necessary
                if (attributeValue == null) attributeValue = "{NULL}";
                
                result = s_regex.Replace(result, attributeValue, 1);
            }

            return result;
        }

        public override string EditorName
        {
            get { return Script.Keyword; }
        }

        public override object GetParameter(int index)
        {
            return Script.GetParameter(index);
        }

        public override void SetParameter(int index, object value)
        {
            Script.SetParameter(index, value);
        }

        public override ScriptType Type
        {
            get { return ScriptType.Normal; }
        }
    }
}
