using QuestViva.Engine;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

public class EditableIfScript : EditableScriptBase, IEditableScript, IEditorData
{
    private readonly Dictionary<IScript, EditableElseIf> _elseIfScripts = [];

    private readonly IIfScript _ifScript;
    private readonly EditableScripts _thenScript;
    private EditableScripts? _elseScript;

    internal EditableIfScript(EditorController controller, IIfScript script, UndoLogger undoLogger)
        : base(controller, script, undoLogger)
    {
        _ifScript = script;

        _ifScript.IfScriptUpdated += OnIfScriptUpdated;

        if (_ifScript.ThenScript == null)
        {
            _ifScript.ThenScript = new MultiScript(Controller.WorldModel);
        }

        _thenScript = EditableScripts.GetInstance(Controller, _ifScript.ThenScript);
        _thenScript.Updated += nestedScript_Updated;

        foreach (var elseIfScript in _ifScript.ElseIfScripts)
        {
            var newEditableElseIf = new EditableElseIf(elseIfScript, this);
            _elseIfScripts.Add(elseIfScript.Script, newEditableElseIf);
            newEditableElseIf.EditableScripts.Updated += nestedScript_Updated;
        }

        if (_ifScript.ElseScript != null)
        {
            _elseScript = EditableScripts.GetInstance(Controller, _ifScript.ElseScript);
            _elseScript.Updated += nestedScript_Updated;
        }
    }

    public string IfExpression => _ifScript.ExpressionString;

    public IEditableScripts ThenScript => _thenScript;

    public IEditableScripts? ElseScript => _elseScript;

    public IEnumerable<EditableElseIf> ElseIfScripts => _elseIfScripts.Values;

    public override string DisplayString(int index, object? newValue)
    {
        // if index is 0 then we're editing, and newValue is the entire new script, so just return it straight away.
        if (index == 0)
        {
            return (string) newValue!;
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
    public override object? GetParameter(string index)
    {
        throw new NotImplementedException();
    }

    public override void SetParameter(string index, object? value)
    {
        throw new NotImplementedException();
    }

    public override ScriptType Type => ScriptType.If;

    public event EventHandler Changed
    {
        add { }
        remove { }
    }

    public string? Name => null;

    public object? GetAttribute(string attribute)
    {
        if (attribute == "expression")
        {
            return _ifScript.ExpressionString;
        }

        throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'if' attribute");
    }

    public ValidationResult SetAttribute(string attribute, object? value)
    {
        if (attribute == "expression")
        {
            _ifScript.ExpressionString = (string) value!;
        }
        else
        {
            throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'if' attribute");
        }

        return new ValidationResult {Valid = true};
    }

    public string? GetSelectedFilter(string filterGroup)
    {
        return null;
    }

    public void SetSelectedFilter(string filterGroup, string filter)
    {
    }

    public event EventHandler? AddedElse;
    public event EventHandler? RemovedElse;
    public event EventHandler<ElseIfEventArgs>? AddedElseIf;
    public event EventHandler<ElseIfEventArgs>? RemovedElseIf;

    private void OnIfScriptUpdated(object? sender, IfScriptUpdatedEventArgs e)
    {
        switch (e.EventType)
        {
            case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElse:
                _elseScript = EditableScripts.GetInstance(Controller, _ifScript.ElseScript!);
                _elseScript.Updated += nestedScript_Updated;
                AddedElse?.Invoke(this, new EventArgs());

                break;
            case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElse:
                _elseScript!.Updated -= nestedScript_Updated;
                _elseScript = null;
                RemovedElse?.Invoke(this, new EventArgs());

                break;
            case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElseIf:
                var editableNewScript = EditableScripts.GetInstance(Controller, e.Data!.Script);
                editableNewScript.Updated += nestedScript_Updated;

                // Wrap the newly created elseif in an EditableElseIf and add it to our internal dictionary
                var newEditableElseIf = new EditableElseIf(e.Data, this);
                _elseIfScripts.Add(e.Data.Script, newEditableElseIf);

                // Raise the update to display in the UI
                AddedElseIf?.Invoke(this, new ElseIfEventArgs(newEditableElseIf));

                break;
            case IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElseIf:
                EditableScripts.GetInstance(Controller, e.Data!.Script).Updated -= nestedScript_Updated;
                RemovedElseIf?.Invoke(this, new ElseIfEventArgs(_elseIfScripts[e.Data.Script]));

                _elseIfScripts.Remove(e.Data.Script);
                break;
            default:
                throw new Exception("Unhandled event");
        }

        RaiseUpdated(new EditableScriptUpdatedEventArgs(DisplayString()));
    }

    private void nestedScript_Updated(object? sender, EditableScriptsUpdatedEventArgs e)
    {
        RaiseUpdateForNestedScriptChange(e);
    }

    public string DisplayString(IEditableScripts? modifiedSection, int index, string newValue)
    {
        // If we've updated the "then" script, then we need to get an updated "then" script where attribute "index" has been updated to "newValue"
        var result = modifiedSection == ThenScript
            ? ThenDisplayStringFragment(index, newValue)
            : ThenDisplayStringFragment(-1, string.Empty);

        foreach (var elseIf in _elseIfScripts.Values)
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
        return string.Format(", Else ({0})", index == 1 ? newValue : ElseScript!.DisplayString());
    }

    public void AddElse()
    {
        IScript newScript = new MultiScript(Controller.WorldModel);
        _ifScript.SetElse(newScript);
    }

    public void AddElseIf()
    {
        IScript newScript = new MultiScript(Controller.WorldModel);
        var newElseIf = _ifScript.AddElseIf(string.Empty, newScript);
    }

    public void RemoveElseIf(EditableElseIf removeElseIf)
    {
        _ifScript.RemoveElseIf(removeElseIf.ElseIfScript);
    }

    public void RemoveElse()
    {
        _ifScript.SetElse(null);
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
        private readonly EditableIfScript _parent;

        internal EditableElseIf(IElseIfScript elseIfScript, EditableIfScript parent)
        {
            ElseIfScript = elseIfScript;
            _parent = parent;
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

        public string? Name => null;

        public object? GetAttribute(string attribute)
        {
            if (attribute == "expression")
            {
                return Expression;
            }

            throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'else if' attribute");
        }

        public ValidationResult SetAttribute(string attribute, object? value)
        {
            if (attribute == "expression")
            {
                Expression = (string) value!;
            }
            else
            {
                throw new ArgumentOutOfRangeException("attribute", "Unrecognised 'else if' attribute");
            }

            return new ValidationResult {Valid = true};
        }

        public string? GetSelectedFilter(string filterGroup)
        {
            return null;
        }

        public void SetSelectedFilter(string filterGroup, string filter)
        {
        }

        public IEnumerable<string> GetVariablesInScope()
        {
            return _parent.GetVariablesInScope();
        }
    }
}
