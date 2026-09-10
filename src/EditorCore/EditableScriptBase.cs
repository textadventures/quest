using QuestViva.Engine;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

public abstract class EditableScriptBase : IEditableScript
{
    private static int _count;

    // Assigned through Script in the constructor
    private IScript _script = null!;

    public EditableScriptBase(EditorController controller, IScript script, UndoLogger undoLogger)
    {
        Script = script;
        Controller = controller;
        if (script != null)
        {
            script.UndoLog = undoLogger;
        }

        _count++;
        Id = "script" + _count;
    }

    internal IScript Script
    {
        get => _script;
        set
        {
            if (_script != null)
            {
                _script.ScriptUpdated -= ScriptUpdated;
                if (FunctionCallScript != null)
                {
                    FunctionCallScript.FunctionCallParametersUpdated -= FunctionCallParametersUpdated;
                }
            }

            _script = value;
            if (_script != null)
            {
                _script.ScriptUpdated += ScriptUpdated;
                if (FunctionCallScript != null)
                {
                    FunctionCallScript.FunctionCallParametersUpdated += FunctionCallParametersUpdated;
                }
            }
        }
    }

    internal IFunctionCallScript? FunctionCallScript => _script as IFunctionCallScript;

    protected EditorController Controller { get; }

    public event EventHandler<EditableScriptUpdatedEventArgs>? Updated;

    public virtual string DisplayString()
    {
        return DisplayString(-1, string.Empty);
    }

    public abstract string DisplayString(int index, object? newValue);
    public abstract string EditorName { get; set; }
    public abstract object? GetParameter(string index);
    public abstract void SetParameter(string index, object? value);
    public abstract ScriptType Type { get; }

    public string Id { get; }

    public IEnumerable<string> GetVariablesInScope()
    {
        // Editable scripts always live inside a MultiScript
        return Script.Parent!.GetVariablesInScope();
    }

    protected void ScriptUpdated(object? sender, ScriptUpdatedEventArgs e)
    {
        if (Updated != null)
        {
            if (e != null && e.IsParameterUpdate)
            {
                Updated(this, new EditableScriptUpdatedEventArgs(e.Index, Controller.WrapValue(e.NewValue)));
            }
            else if (e != null && e.IsNamedParameterUpdate)
            {
                Updated(this, new EditableScriptUpdatedEventArgs(e.Id, (string?) e.NewValue));
            }
            else
            {
                Updated(this, new EditableScriptUpdatedEventArgs());
            }
        }
    }

    private void FunctionCallParametersUpdated(object? sender, ScriptUpdatedEventArgs e)
    {
        if (EditorName.StartsWith("(function)"))
        {
            Updated!(this, new EditableScriptUpdatedEventArgs(e.Index, Controller.WrapValue(e.NewValue)));
        }
    }

    protected void RaiseUpdated(EditableScriptUpdatedEventArgs e)
    {
        if (Updated != null)
        {
            Updated(this, e);
        }
    }

    protected void RaiseUpdateForNestedScriptChange(EditableScriptsUpdatedEventArgs e)
    {
        RaiseUpdated(new EditableScriptUpdatedEventArgs {IsNestedScriptUpdate = true});
    }
}