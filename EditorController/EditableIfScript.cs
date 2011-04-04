using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public class EditableIfScriptUpdatedEventArgs : EventArgs
    {
        public enum UpdateEventType
        {
            AddedElse
        }

        internal EditableIfScriptUpdatedEventArgs(UpdateEventType eventType)
        {
            EventType = eventType;
        }

        public UpdateEventType EventType { get; private set; }
    }

    public class EditableIfScript : EditableScriptBase, IEditableScript, IEditorData
    {
        private IfScript m_ifScript;
        private Element m_parent;
        private EditorController m_controller;
        private EditableScripts m_thenScript;
        private EditableScripts m_elseScript;

        public event EventHandler<EditableIfScriptUpdatedEventArgs> IfUpdated;

        internal EditableIfScript(EditorController controller, IfScript script, Element parent, UndoLogger undoLogger)
            : base(script, undoLogger)
        {
            m_controller = controller;
            m_ifScript = script;
            m_parent = parent;

            m_ifScript.IfScriptUpdated += m_ifScript_IfScriptUpdated;

            if (m_ifScript.ThenScript == null)
            {
                m_ifScript.ThenScript = new MultiScript();
            }

            m_thenScript = new EditableScripts(m_controller, m_ifScript.ThenScript, m_parent, null);
            m_thenScript.Updated += m_thenScript_Updated;

            if (m_ifScript.ElseScript != null)
            {
                m_elseScript = new EditableScripts(m_controller, m_ifScript.ElseScript, m_parent, null);
                m_elseScript.Updated += m_elseScript_Updated;
            }
        }

        void m_ifScript_IfScriptUpdated(object sender, IfScript.IfScriptUpdatedEventArgs e)
        {
            if (IfUpdated != null)
            {
                if (e.EventType == IfScript.IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElse)
                {
                    IfUpdated(this, new EditableIfScriptUpdatedEventArgs(EditableIfScriptUpdatedEventArgs.UpdateEventType.AddedElse));
                }
            }
        }

        void m_thenScript_Updated(object sender, EditableScriptsUpdatedEventArgs e)
        {
            RaiseUpdated(new EditableScriptUpdatedEventArgs(1, e.UpdatedScript == null ? "" : e.UpdatedScript.DisplayString()));
        }

        void m_elseScript_Updated(object sender, EditableScriptsUpdatedEventArgs e)
        {
            // This doesn't look correct to me - the whole area of indexes doesn't really apply to "if" scripts. Yet this
            // appears to work at the moment...
            RaiseUpdated(new EditableScriptUpdatedEventArgs(1, e.UpdatedScript == null ? "" : e.UpdatedScript.DisplayString()));
        }

        public override string DisplayString(int index, string newValue)
        {
            // if index is 0 then we're editing, and newValue is the entire new script, so just return it straight away.
            if (index == 0) return newValue;

            // if index is not 0, then this is a request for the DisplayString based on the stored data, so generate it.
            return DisplayString("", -1, string.Empty);
        }

        public string DisplayString(string section, int index, string newValue)
        {
            // If we've updated the "then" script, then we need to get an updated "then" script where attribute "index" has been updated to "newValue"
            string result = (section == "then") ? DisplayStringFragment("then", index, newValue) : DisplayStringFragment("then", -1, string.Empty);

            // TO DO: Will then need to add elseif

            if (ElseScript != null)
            {
                result += (section == "else") ? DisplayStringFragment("else", index, newValue) : DisplayStringFragment("else", -1, string.Empty);
            }
            return result;

        }

        public string DisplayStringFragment(string section, int index, string newValue)
        {
            if (section == "then")
            {
                string expression = (index == 0) ? newValue : IfExpression;
                string thenScript = (index == 1) ? newValue : ThenScript.DisplayString();
                return string.Format("If ({0}) Then '{1}'", expression, thenScript);
            }

            if (section == "else")
            {
                return string.Format(", Else '{0}'", (index == 1) ? newValue : ElseScript.DisplayString());
            }

            // TO DO: elseif
            throw new NotImplementedException();
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
            get { return m_thenScript; }
        }

        public IEditableScripts ElseScript
        {
            get { return m_elseScript; }
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
            m_ifScript.SetElse(new MultiScript());
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
