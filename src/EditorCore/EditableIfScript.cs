using QuestViva.Engine;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

public class EditableIfScript : EditableScriptBase, IEditableScript, IEditorData
{
    private readonly Dictionary<IScript, EditableElseIf> m_elseIfScripts = new();

    private readonly IIfScript m_ifScript;
    private readonly EditableScripts m_thenScript;
    private EditableScripts m_elseScript;

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
            var newEditableElseIf = new EditableElseIf(elseIfScript, this);
            m_elseIfScripts.Add(elseIfScript.Script, newEditableElseIf);
            newEditableElseIf.EditableScripts.Updated += nestedScript_Updated;
        }

        if (m_ifScript.ElseScript != null)
        {
            m_elseScript = EditableScripts.GetInstance(Controller, m_ifScript.ElseScript);
            m_elseScript.Updated += nestedScript_Updated;
        }
    }

    public string IfExpression => m_ifScript.ExpressionString;

    public IEditableScripts ThenScript => m_thenScript;

    public IEditableScripts ElseScript => m_elseScript;

    public IEnumerable<EditableElseIf> ElseIfScripts => m_elseIfScripts.Values;

    public override string DisplayString(int index, object newValue)
    {
        // if index is 0 then we're editing, and newValue is the entire new script, so just return it straight away.
        if (index == 0)
        {
            return (string) newValue;
        }

        // if index is not 0, then this is a request for the DisplayString based on the stored data, so generate it.
        return DisplayString(null, -1, string.Empty);
    }

    public override string EditorName
    {
        get => "if";
        set { } // do nothing, EditorController won't need to change the EditorName for an "if"
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

    public override ScriptType Type => ScriptType.If;

    public event EventHandler Changed
    {
        add { }
        remove { }
    }

    public string Name => null;

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
            m_ifScript.ExpressionString = (string) value;
        }
        else
        {
            throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'if' attribute");
        }

        return new ValidationResult {Valid = true};
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

    public bool ReadOnly { get; set; }

    public bool IsDirectlySaveable => true;

    public event EventHandler AddedElse;
    public event EventHandler RemovedElse;
    public event EventHandler<ElseIfEventArgs> AddedElseIf;
    public event EventHandler<ElseIfEventArgs> RemovedElseIf;

    private void m_ifScript_IfScriptUpdated(object sender, IfScriptUpdatedEventArgs e)
    {
        switch (e.EventType)
        {
            case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElse:
                m_elseScript = EditableScripts.GetInstance(Controller, m_ifScript.ElseScript);
                m_elseScript.Updated += nestedScript_Updated;
                if (AddedElse != null)
                {
                    AddedElse(this, new EventArgs());
                }

                break;
            case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElse:
                m_elseScript.Updated -= nestedScript_Updated;
                m_elseScript = null;
                if (RemovedElse != null)
                {
                    RemovedElse(this, new EventArgs());
                }

                break;
            case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElseIf:
                var editableNewScript = EditableScripts.GetInstance(Controller, e.Data.Script);
                editableNewScript.Updated += nestedScript_Updated;

                // Wrap the newly created elseif in an EditableElseIf and add it to our internal dictionary
                var newEditableElseIf = new EditableElseIf(e.Data, this);
                m_elseIfScripts.Add(e.Data.Script, newEditableElseIf);

                // Raise the update to display in the UI
                if (AddedElseIf != null)
                {
                    AddedElseIf(this, new ElseIfEventArgs(newEditableElseIf));
                }

                break;
            case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElseIf:
                EditableScripts.GetInstance(Controller, e.Data.Script).Updated -= nestedScript_Updated;
                if (RemovedElseIf != null)
                {
                    RemovedElseIf(this, new ElseIfEventArgs(m_elseIfScripts[e.Data.Script]));
                }

                m_elseIfScripts.Remove(e.Data.Script);
                break;
            default:
                throw new Exception("Unhandled event");
        }

        RaiseUpdated(new EditableScriptUpdatedEventArgs(DisplayString()));
    }

    private void nestedScript_Updated(object sender, EditableScriptsUpdatedEventArgs e)
    {
        RaiseUpdateForNestedScriptChange(e);
    }

    public string DisplayString(IEditableScripts modifiedSection, int index, string newValue)
    {
        // If we've updated the "then" script, then we need to get an updated "then" script where attribute "index" has been updated to "newValue"
        var result = modifiedSection == ThenScript
            ? ThenDisplayStringFragment(index, newValue)
            : ThenDisplayStringFragment(-1, string.Empty);

        foreach (var elseIf in m_elseIfScripts.Values)
        {
            result += modifiedSection == elseIf.EditableScripts
                ? ElseIfDisplayStringFragment(elseIf, index, newValue)
                : ElseIfDisplayStringFragment(elseIf, -1, string.Empty);
        }

        if (ElseScript != null)
        {
            result += modifiedSection == ElseScript
                ? ElseDisplayStringFragment(index, newValue)
                : ElseDisplayStringFragment(-1, string.Empty);
        }

        return result;
    }

    private string ThenDisplayStringFragment(int index, string newValue)
    {
        var expression = index == 0 ? newValue : IfExpression;
        var thenScript = index == 1 ? newValue : ThenScript.DisplayString();
        return string.Format("If ({0}) Then ({1})", expression, thenScript);
    }

    private string ElseIfDisplayStringFragment(EditableElseIf elseIf, int index, string newValue)
    {
        var expression = index == 0 ? newValue : elseIf.Expression;
        var thenScript = index == 1 ? newValue : elseIf.EditableScripts.DisplayString();
        return string.Format(", Else If ({0}) Then ({1})", expression, thenScript);
    }

    private string ElseDisplayStringFragment(int index, string newValue)
    {
        return string.Format(", Else ({0})", index == 1 ? newValue : ElseScript.DisplayString());
    }

    public void AddElse()
    {
        IScript newScript = new MultiScript(Controller.WorldModel);
        m_ifScript.SetElse(newScript);
    }

    public void AddElseIf()
    {
        IScript newScript = new MultiScript(Controller.WorldModel);
        var newElseIf = m_ifScript.AddElseIf(string.Empty, newScript);
    }

    public void RemoveElseIf(EditableElseIf removeElseIf)
    {
        m_ifScript.RemoveElseIf(removeElseIf.ElseIfScript);
    }

    public void RemoveElse()
    {
        m_ifScript.SetElse(null);
    }

    public class ElseIfEventArgs : EventArgs
    {
        public ElseIfEventArgs(EditableElseIf script)
        {
            Script = script;
        }

        public EditableElseIf Script { get; private set; }
    }

    public class EditableElseIf : IEditorData
    {
        private readonly EditableIfScript m_parent;

        internal EditableElseIf(IElseIfScript elseIfScript, EditableIfScript parent)
        {
            ElseIfScript = elseIfScript;
            m_parent = parent;
            EditableScripts = EditorCore.EditableScripts.GetInstance(parent.Controller, elseIfScript.Script);
        }

        public IEditableScripts EditableScripts { get; }

        public string Expression
        {
            get => ElseIfScript.ExpressionString;
            set => ElseIfScript.ExpressionString = value;
        }

        public string Id => ElseIfScript.Id;

        internal IElseIfScript ElseIfScript { get; }

        public event EventHandler Changed
        {
            add { }
            remove { }
        }

        public string Name => null;

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
                Expression = (string) value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'else if' attribute");
            }

            return new ValidationResult {Valid = true};
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

        public bool ReadOnly { get; set; }

        public IEnumerable<string> GetVariablesInScope()
        {
            return m_parent.GetVariablesInScope();
        }

        public bool IsDirectlySaveable => true;
    }
}

public class IfExpressionControlDefinition : IEditorControl
{
    public static IfExpressionControlDefinition Instance => new();

    public string ControlType => "textbox";

    public string Caption => null;

    public int? Height => null;

    public int? Width => null;

    public string Attribute => "expression";

    public bool Expand => false;

    public string GetString(string tag)
    {
        if (tag == "usetemplates")
        {
            return "if";
        }

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
        if (tag == "nullable")
        {
            return false;
        }

        throw new NotImplementedException();
    }

    public Task<bool> IsControlVisible(IEditorData data)
    {
        return Task.FromResult(true);
    }

    public IEditorDefinition Parent => null;

    public bool IsControlVisibleInSimpleMode => true;

    public string Id => null;
}