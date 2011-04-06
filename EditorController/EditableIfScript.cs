using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public class EditableIfScript : EditableScriptBase, IEditableScript, IEditorData
    {
        public class ElseIfEventArgs : EventArgs
        {
            public EditableElseIf Script { get; private set; }

            public ElseIfEventArgs(EditableElseIf script)
            {
                Script = script;
            }
        }

        public class EditableElseIf : IEditorData
        {
            private IfScript.ElseIfScript m_elseIfScript;
            private EditableIfScript m_parent;

            internal EditableElseIf(IfScript.ElseIfScript elseIfScript, EditableIfScript parent)
            {
                m_elseIfScript = elseIfScript;
                m_parent = parent;
                EditableScripts = new EditableScripts(parent.m_controller, elseIfScript.Script, parent.m_parent, null);
            }

            public IEditableScripts EditableScripts { get; private set; }

            public string Expression
            {
                get { return m_elseIfScript.ExpressionString; }
                set { m_elseIfScript.ExpressionString = (string)value; }
            }

            public string Name
            {
                get { throw new NotImplementedException(); }
            }

            public object GetAttribute(string attribute)
            {
                if (attribute == "expression")
                {
                    return Expression;
                }
                throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'else if' attribute");
            }

            public void SetAttribute(string attribute, object value)
            {
                if (attribute == "expression")
                {
                    Expression = (string)value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'else if' attribute");
                }
            }

            public string Id
            {
                get { return m_elseIfScript.Id; }
            }
        }

        private IfScript m_ifScript;
        private Element m_parent;
        private EditorController m_controller;
        private EditableScripts m_thenScript;
        private EditableScripts m_elseScript;
        private Dictionary<IScript, EditableElseIf> m_elseIfScripts = new Dictionary<IScript, EditableElseIf>();

        public event EventHandler AddedElse;
        public event EventHandler RemovedElse;
        public event EventHandler<ElseIfEventArgs> AddedElseIf;
        public event EventHandler<ElseIfEventArgs> RemovedElseIf;

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

            foreach (var elseIfScript in m_ifScript.ElseIfScripts)
            {
                EditableElseIf newEditableElseIf = new EditableElseIf(elseIfScript, this);
                m_elseIfScripts.Add(elseIfScript.Script, newEditableElseIf);
                newEditableElseIf.EditableScripts.Updated += ElseIfUpdated;
            }

            if (m_ifScript.ElseScript != null)
            {
                m_elseScript = new EditableScripts(m_controller, m_ifScript.ElseScript, m_parent, null);
                m_elseScript.Updated += m_elseScript_Updated;
            }
        }

        void m_ifScript_IfScriptUpdated(object sender, IfScript.IfScriptUpdatedEventArgs e)
        {
            switch (e.EventType)
            {
                case IfScript.IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElse:
                    if (AddedElse != null) AddedElse(this, new EventArgs());
                    break;
                case IfScript.IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElse:
                    if (RemovedElse != null) RemovedElse(this, new EventArgs());
                    break;
                case IfScript.IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElseIf:
                    if (AddedElseIf != null)
                    {
                        // our dictionary of existing elseIfScripts won't contain this one if we're in the middle of adding it
                        // via the AddElseIf function below, so we just ignore it for now and will raise an update ourselves later.
                        if (m_elseIfScripts.ContainsKey(e.Data.Script))
                        {
                            AddedElseIf(this, new ElseIfEventArgs(m_elseIfScripts[e.Data.Script]));
                        }
                    }
                    break;
                case IfScript.IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElseIf:
                    if (RemovedElseIf != null) RemovedElseIf(this, new ElseIfEventArgs(m_elseIfScripts[e.Data.Script]));
                    break;
                default:
                    throw new Exception("Unhandled event");
            }
        }

        // TO DO: FIX *************************************************************************************************************
        // This is flawed. We shouldn't have to listen to the Updated event on the then/else/elseif script at all. This is only used for
        // updating the scripts list after an undo/redo, and what should be happening instead is that there should be an event
        // on IEditableScripts which will tell a listbox that is bound to it to update. The current approach is hacky and relies
        // on the individual ScriptEditor not being destroyed when you click off to edit a different script.
        void m_thenScript_Updated(object sender, EditableScriptsUpdatedEventArgs e)
        {
            RaiseUpdated(new EditableScriptUpdatedEventArgs(1, e.UpdatedScript == null ? "" : e.UpdatedScript.DisplayString()));
        }

        void m_elseScript_Updated(object sender, EditableScriptsUpdatedEventArgs e)
        {
            RaiseUpdated(new EditableScriptUpdatedEventArgs(1, e.UpdatedScript == null ? "" : e.UpdatedScript.DisplayString()));
        }

        void ElseIfUpdated(object sender, EditableScriptsUpdatedEventArgs e)
        {
            RaiseUpdated(new EditableScriptUpdatedEventArgs(1, e.UpdatedScript == null ? "" : e.UpdatedScript.DisplayString()));
        }
        // ************************************************************************************************************************

        public override string DisplayString(int index, string newValue)
        {
            // if index is 0 then we're editing, and newValue is the entire new script, so just return it straight away.
            if (index == 0) return newValue;

            // if index is not 0, then this is a request for the DisplayString based on the stored data, so generate it.
            return DisplayString(null, -1, string.Empty);
        }

        public string DisplayString(IEditableScripts modifiedSection, int index, string newValue)
        {
            // If we've updated the "then" script, then we need to get an updated "then" script where attribute "index" has been updated to "newValue"
            string result = (modifiedSection == ThenScript) ? ThenDisplayStringFragment(index, newValue) : ThenDisplayStringFragment(-1, string.Empty);

            foreach (EditableElseIf elseIf in m_elseIfScripts.Values)
            {
                result += (modifiedSection == elseIf.EditableScripts) ? ElseIfDisplayStringFragment(elseIf, index, newValue) : ElseIfDisplayStringFragment(elseIf, - 1, string.Empty);
            }

            if (ElseScript != null)
            {
                result += (modifiedSection == ElseScript) ? ElseDisplayStringFragment(index, newValue) : ElseDisplayStringFragment(-1, string.Empty);
            }
            return result;

        }

        private string ThenDisplayStringFragment(int index, string newValue)
        {
            string expression = (index == 0) ? newValue : IfExpression;
            string thenScript = (index == 1) ? newValue : ThenScript.DisplayString();
            return string.Format("If ({0}) Then ({1})", expression, thenScript);
        }

        private string ElseIfDisplayStringFragment(EditableElseIf elseIf, int index, string newValue)
        {
            string expression = (index == 0) ? newValue : elseIf.Expression;
            string thenScript = (index == 1) ? newValue : elseIf.EditableScripts.DisplayString();
            return string.Format(", Else If ({0}) Then ({1})", expression, thenScript);
        }

        private string ElseDisplayStringFragment(int index, string newValue)
        {
            return string.Format(", Else ({0})", (index == 1) ? newValue : ElseScript.DisplayString());
        }

        public override string EditorName
        {
            get { return "if"; }
        }

        public string IfExpression
        {
            get { return m_ifScript.ExpressionString; }
        }

        public IEditableScripts ThenScript
        {
            get { return m_thenScript; }
        }

        public IEditableScripts ElseScript
        {
            get { return m_elseScript; }
        }

        public IEnumerable<EditableElseIf> ElseIfScripts
        {
            get { return m_elseIfScripts.Values; }
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
                return m_ifScript.ExpressionString;
            }
            throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'if' attribute");
        }

        public void SetAttribute(string attribute, object value)
        {
            if (attribute == "expression")
            {
                m_ifScript.ExpressionString = (string)value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'if' attribute");
            }
        }

        public void AddElse()
        {
            IScript newScript = new MultiScript();
            // we have to set m_elseScript here before setting it on m_ifScript, because setting it on
            // m_ifScript will trigger the screen update, and we therefore need to have m_elseScript
            // set here before that happens
            m_elseScript = new EditableScripts(m_controller, newScript, m_parent, null);
            m_elseScript.Updated += m_elseScript_Updated;
            m_ifScript.SetElse(newScript);
        }

        public EditableElseIf AddElseIf()
        {
            // Create a blank "then" script for this new elseif, and wrap it in an EditableScripts
            IScript newScript = new MultiScript();
            EditableScripts editableNewScript = new EditableScripts(m_controller, newScript, m_parent, null);
            editableNewScript.Updated += ElseIfUpdated;

            // Add it to the "if" with an empty expression
            IfScript.ElseIfScript newElseIf = m_ifScript.AddElseIf(string.Empty, newScript);

            // Wrap the newly created elseif in an EditableElseIf and add it to our internal dictionary
            EditableElseIf newEditableElseIf = new EditableElseIf(newElseIf, this);
            m_elseIfScripts.Add(newElseIf.Script, newEditableElseIf);

            // Raise the update to display in the UI
            if (AddedElseIf != null)
            {
                AddedElseIf(this, new ElseIfEventArgs(newEditableElseIf));
            }

            return newEditableElseIf;
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
