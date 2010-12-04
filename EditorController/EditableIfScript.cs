using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public class EditableIfScript : EditableScriptBase, IEditableScript, IEditorData
    {
        private IfScript m_ifScript;
        private Element m_parent;
        private EditorController m_controller;
        private EditableScripts m_thenScript;

        internal EditableIfScript(EditorController controller, IfScript script, Element parent, UndoLogger undoLogger)
            : base(script, undoLogger)
        {
            m_controller = controller;
            m_ifScript = script;
            m_parent = parent;

            if (m_ifScript.ThenScript == null)
            {
                m_ifScript.ThenScript = new MultiScript();
            }

            m_thenScript = new EditableScripts(m_controller, m_ifScript.ThenScript, m_parent, null);
            m_thenScript.Updated += m_thenScript_Updated;
        }

        void m_thenScript_Updated(object sender, EditableScriptsUpdatedEventArgs e)
        {
            RaiseUpdated(new EditableScriptUpdatedEventArgs(1, e.UpdatedScript == null ? "" : e.UpdatedScript.DisplayString()));
        }

        public override string DisplayString(int index, string newValue)
        {
            string expression = (index == 0) ? newValue : IfExpression;
            string thenScript = (index == 1) ? newValue : ThenScript.DisplayString();

            string result = string.Format("If ({0}) Then '{1}'", expression, thenScript);

            // TO DO: Will then need to add elseif, else etc.

            return result;
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
                return m_thenScript;
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

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public object GetAttribute(string attribute)
        {
            if (attribute == "expression")
            {
                return m_ifScript.Expression;
            }
            throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'if' attribute");
        }

        public void SetAttribute(string attribute, object value)
        {
            if (attribute == "expression")
            {
                m_ifScript.Expression = (string)value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'if' attribute");
            }
        }

        public void AddElse()
        {
            m_ifScript.SetElse(null);
        }

        public void AddElseIf()
        {
            // expects an IfScript with an expression and just a "then" script
            //m_ifScript.AddElseIf(null);
        }

        public void RemoveElseIf()
        {

        }

        public void RemoveElse()
        {
            m_ifScript.SetElse(null);
        }
    }
}
