using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
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
            public event EventHandler Changed { add { } remove { } }

            private IElseIfScript m_elseIfScript;
            private EditableIfScript m_parent;

            internal EditableElseIf(IElseIfScript elseIfScript, EditableIfScript parent)
            {
                m_elseIfScript = elseIfScript;
                m_parent = parent;
                EditableScripts = TextAdventures.Quest.EditableScripts.GetInstance(parent.Controller, elseIfScript.Script);
            }

            public IEditableScripts EditableScripts { get; private set; }

            public string Expression
            {
                get { return m_elseIfScript.ExpressionString; }
                set { m_elseIfScript.ExpressionString = (string)value; }
            }

            public string Name
            {
                get { return null; }
            }

            public object GetAttribute(string attribute)
            {
                if (attribute == "expression")
                {
                    return Expression;
                }
                throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'else if' attribute");
            }

            public ValidationResult SetAttribute(string attribute, object value)
            {
                if (attribute == "expression")
                {
                    Expression = (string)value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'else if' attribute");
                }

                return new ValidationResult { Valid = true };
            }

            public IEnumerable<string> GetAffectedRelatedAttributes(string attribute)
            {
                return null;
            }

            public string GetSelectedFilter(string filterGroup)
            {
                return null;
            }

            public void SetSelectedFilter(string filterGroup, string filter)
            {
            }

            public string Id
            {
                get { return m_elseIfScript.Id; }
            }

            internal IElseIfScript ElseIfScript
            {
                get { return m_elseIfScript; }
            }

            public bool ReadOnly
            {
                get;
                set;
            }

            public IEnumerable<string> GetVariablesInScope()
            {
                return m_parent.GetVariablesInScope();
            }

            public bool IsDirectlySaveable { get { return true; } }
        }

        private IIfScript m_ifScript;
        private EditableScripts m_thenScript;
        private EditableScripts m_elseScript;
        private Dictionary<IScript, EditableElseIf> m_elseIfScripts = new Dictionary<IScript, EditableElseIf>();

        public event EventHandler AddedElse;
        public event EventHandler RemovedElse;
        public event EventHandler<ElseIfEventArgs> AddedElseIf;
        public event EventHandler<ElseIfEventArgs> RemovedElseIf;
        public event EventHandler Changed { add { } remove { } }

        internal EditableIfScript(EditorController controller, IIfScript script, UndoLogger undoLogger)
            : base(controller, script, undoLogger)
        {
            m_ifScript = script;

            m_ifScript.IfScriptUpdated += m_ifScript_IfScriptUpdated;

            if (m_ifScript.ThenScript == null)
            {
                m_ifScript.ThenScript = new MultiScript(Controller.WorldModel);
            }

            m_thenScript = EditableScripts.GetInstance(Controller, m_ifScript.ThenScript);
            m_thenScript.Updated += nestedScript_Updated;

            foreach (var elseIfScript in m_ifScript.ElseIfScripts)
            {
                EditableElseIf newEditableElseIf = new EditableElseIf(elseIfScript, this);
                m_elseIfScripts.Add(elseIfScript.Script, newEditableElseIf);
                newEditableElseIf.EditableScripts.Updated += nestedScript_Updated;
            }

            if (m_ifScript.ElseScript != null)
            {
                m_elseScript = EditableScripts.GetInstance(Controller, m_ifScript.ElseScript);
                m_elseScript.Updated += nestedScript_Updated;
            }
        }

        void m_ifScript_IfScriptUpdated(object sender, IfScriptUpdatedEventArgs e)
        {
            switch (e.EventType)
            {
                case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElse:
                    m_elseScript = EditableScripts.GetInstance(Controller, m_ifScript.ElseScript);
                    m_elseScript.Updated += nestedScript_Updated;
                    if (AddedElse != null) AddedElse(this, new EventArgs());
                    break;
                case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElse:
                    m_elseScript.Updated -= nestedScript_Updated;
                    m_elseScript = null;
                    if (RemovedElse != null) RemovedElse(this, new EventArgs());
                    break;
                case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElseIf:
                    EditableScripts editableNewScript = EditableScripts.GetInstance(Controller, e.Data.Script);
                    editableNewScript.Updated += nestedScript_Updated;

                    // Wrap the newly created elseif in an EditableElseIf and add it to our internal dictionary
                    EditableElseIf newEditableElseIf = new EditableElseIf(e.Data, this);
                    m_elseIfScripts.Add(e.Data.Script, newEditableElseIf);

                    // Raise the update to display in the UI
                    if (AddedElseIf != null)
                    {
                        AddedElseIf(this, new ElseIfEventArgs(newEditableElseIf));
                    }
                    break;
                case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElseIf:
                    EditableScripts.GetInstance(Controller, e.Data.Script).Updated -= nestedScript_Updated;
                    if (RemovedElseIf != null) RemovedElseIf(this, new ElseIfEventArgs(m_elseIfScripts[e.Data.Script]));
                    m_elseIfScripts.Remove(e.Data.Script);
                    break;
                default:
                    throw new Exception("Unhandled event");
            }

            RaiseUpdated(new EditableScriptUpdatedEventArgs(DisplayString()));
        }

        void nestedScript_Updated(object sender, EditableScriptsUpdatedEventArgs e)
        {
            RaiseUpdateForNestedScriptChange(e);
        }

        public override string DisplayString(int index, object newValue)
        {
            // if index is 0 then we're editing, and newValue is the entire new script, so just return it straight away.
            if (index == 0) return (string)newValue;

            // if index is not 0, then this is a request for the DisplayString based on the stored data, so generate it.
            return DisplayString(null, -1, string.Empty);
        }

        public string DisplayString(IEditableScripts modifiedSection, int index, string newValue)
        {
            // If we've updated the "then" script, then we need to get an updated "then" script where attribute "index" has been updated to "newValue"
            string result = (modifiedSection == ThenScript) ? ThenDisplayStringFragment(index, newValue) : ThenDisplayStringFragment(-1, string.Empty);

            foreach (EditableElseIf elseIf in m_elseIfScripts.Values)
            {
                result += (modifiedSection == elseIf.EditableScripts) ? ElseIfDisplayStringFragment(elseIf, index, newValue) : ElseIfDisplayStringFragment(elseIf, -1, string.Empty);
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
            set { } // do nothing, EditorController won't need to change the EditorName for an "if"
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
        public override object GetParameter(string index)
        {
            throw new NotImplementedException();
        }

        public override void SetParameter(string index, object value)
        {
            throw new NotImplementedException();
        }

        public override ScriptType Type
        {
            get { return ScriptType.If; }
        }

        public string Name
        {
            get { return null; }
        }

        public object GetAttribute(string attribute)
        {
            if (attribute == "expression")
            {
                return m_ifScript.ExpressionString;
            }
            throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'if' attribute");
        }

        public ValidationResult SetAttribute(string attribute, object value)
        {
            if (attribute == "expression")
            {
                m_ifScript.ExpressionString = (string)value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'if' attribute");
            }

            return new ValidationResult { Valid = true };
        }

        public IEnumerable<string> GetAffectedRelatedAttributes(string attribute)
        {
            return null;
        }

        public string GetSelectedFilter(string filterGroup)
        {
            return null;
        }

        public void SetSelectedFilter(string filterGroup, string filter)
        {
        }

        public void AddElse()
        {
            IScript newScript = new MultiScript(Controller.WorldModel);
            m_ifScript.SetElse(newScript);
        }

        public void AddElseIf()
        {
            IScript newScript = new MultiScript(Controller.WorldModel);
            IElseIfScript newElseIf = m_ifScript.AddElseIf(string.Empty, newScript);
        }

        public void RemoveElseIf(EditableElseIf removeElseIf)
        {
            m_ifScript.RemoveElseIf(removeElseIf.ElseIfScript);
        }

        public void RemoveElse()
        {
            m_ifScript.SetElse(null);
        }

        public bool ReadOnly
        {
            get;
            set;
        }

        public bool IsDirectlySaveable { get { return true; } }
    }

    public class IfExpressionControlDefinition : IEditorControl
    {
        public static IfExpressionControlDefinition Instance
        {
            get { return new IfExpressionControlDefinition(); }
        }

        public string ControlType
        {
            get { return "textbox"; }
        }

        public string Caption
        {
            get { return null; }
        }

        public int? Height
        {
            get { return null; }
        }

        public int? Width
        {
            get { return null; }
        }

        public string Attribute
        {
            get { return "expression"; }
        }

        public bool Expand
        {
            get { return false; }
        }

        public string GetString(string tag)
        {
            if (tag == "usetemplates") return "if";
            return null;
        }

        public IEnumerable<string> GetListString(string tag)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetDictionary(string tag)
        {
            throw new NotImplementedException();
        }

        public int? GetInt(string tag)
        {
            throw new NotImplementedException();
        }

        public double? GetDouble(string tag)
        {
            throw new NotImplementedException();
        }

        public bool GetBool(string tag)
        {
            if (tag == "nullable") return false;
            throw new NotImplementedException();
        }

        public bool IsControlVisible(IEditorData data)
        {
            return true;
        }

        public IEditorDefinition Parent
        {
            get { return null; }
        }

        public bool IsControlVisibleInSimpleMode
        {
            get { return true; }
        }

        public string Id
        {
            get { return null; }
        }
    }
}
