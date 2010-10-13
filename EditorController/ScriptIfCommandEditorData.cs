using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    class ScriptIfCommandEditorData : IEditorData
    {
        private IfScript m_script;

        public ScriptIfCommandEditorData(IEditableScript script)
        {
            //m_script = script;
        }

        public string Name
        {
            get { return null; }
        }

        public object GetAttribute(string attribute)
        {
            // attributes might be "expression", "thenscript", "elseif1expression", "elseif1script" etc.
            return string.Empty;
        }

        public void SetAttribute(string attribute, object value)
        {

        }
    }
}
