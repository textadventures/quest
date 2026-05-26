using QuestViva.Engine;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

public abstract class EditableScriptBase : IEditableScript
{
    private static int s_count;

    private IScript m_script;

    public EditableScriptBase(EditorController controller, IScript script, UndoLogger undoLogger)
    {
        Script = script;
        Controller = controller;
        if (script != null)
        {
            script.UndoLog = undoLogger;
        }

        s_count++;
        Id = "script" + s_count;
    }

    internal IScript Script
    {
        get => m_script;
        set
        {
            if (m_script != null)
            {
                m_script.ScriptUpdated -= ScriptUpdated;
                if (FunctionCallScript != null)
                {
                    FunctionCallScript.FunctionCallParametersUpdated -= FunctionCallParametersUpdated;
                }
            }

            m_script = value;
            if (m_script != null)
            {
                m_script.ScriptUpdated += ScriptUpdated;
                if (FunctionCallScript != null)
                {
                    FunctionCallScript.FunctionCallParametersUpdated += FunctionCallParametersUpdated;
                }
            }
        }
    }

    internal IFunctionCallScript FunctionCallScript => m_script as IFunctionCallScript;

    protected EditorController Controller { get; }

    public event EventHandler<EditableScriptUpdatedEventArgs> Updated;

    public virtual string DisplayString()
    {
        return DisplayString(-1, string.Empty);
    }

    public abstract string DisplayString(int index, object newValue);
    public abstract string EditorName { get; set; }
    public abstract object GetParameter(string index);
    public abstract void SetParameter(string index, object value);
    public abstract ScriptType Type { get; }

    public string Id { get; }

    public IEnumerable<string> GetVariablesInScope()
    {
        return Script.Parent.GetVariablesInScope();
    }

    protected void ScriptUpdated(object sender, ScriptUpdatedEventArgs e)
    {
        if (Updated != null)
        {
            if (e != null && e.IsParameterUpdate)
            {
                Updated(this, new EditableScriptUpdatedEventArgs(e.Index, Controller.WrapValue(e.NewValue)));
            }
            else if (e != null && e.IsNamedParameterUpdate)
            {
                Updated(this, new EditableScriptUpdatedEventArgs(e.Id, (string) e.NewValue));
            }
            else
            {
                Updated(this, new EditableScriptUpdatedEventArgs());
            }
        }
    }

    private void FunctionCallParametersUpdated(object sender, ScriptUpdatedEventArgs e)
    {
        if (EditorName.StartsWith("(function)"))
        {
            Updated(this, new EditableScriptUpdatedEventArgs(e.Index, Controller.WrapValue(e.NewValue)));
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