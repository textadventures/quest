using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public class EditableIfScript : EditableScriptBase, IEditableScript
    {
        IfScript m_ifScript;
        Element m_parent;
        EditorController m_controller;

        internal EditableIfScript(EditorController controller, IfScript script, Element parent)
        {
            m_controller = controller;
            m_ifScript = script;
            m_parent = parent;
        }

        public override string DisplayString()
        {
            return "If...";
        }

        public override string DisplayString(int index, string newValue)
        {
            return "If...";
        }

        public override string EditorName
        {
            get { return "if"; }
        }

        public string IfExpression
        {
            get { return m_ifScript.Expression; }
        }

        public IEditableScripts ThenScript
        {
            get
            {
                return new EditableScripts(m_controller, m_ifScript.ThenScript, m_parent, null);
            }
        }

        // these should probably not be on the interface...
        public override string GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public override void SetParameter(int index, string value)
        {
            throw new NotImplementedException();
        }

        public override ScriptType Type
        {
            get { return ScriptType.If; }
        }
    }
}
