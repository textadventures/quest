using System.Diagnostics.CodeAnalysis;

namespace QuestViva.Engine.Scripts;

public class ScriptUpdatedEventArgs : EventArgs
{
    internal ScriptUpdatedEventArgs()
    {
    }

    internal ScriptUpdatedEventArgs(IScript added, IScript removed)
    {
        AddedScript = added;
        RemovedScript = removed;
    }

    internal ScriptUpdatedEventArgs(IScript inserted, int index)
    {
        InsertedScript = inserted;
        Index = index;
    }

    internal ScriptUpdatedEventArgs(int index, object newValue)
    {
        Index = index;
        NewValue = newValue;
        IsParameterUpdate = true;
    }

    internal ScriptUpdatedEventArgs(string id, object newValue)
    {
        Id = id;
        NewValue = newValue;
        IsNamedParameterUpdate = true;
    }

    public IScript? RemovedScript { get; private set; }
    public IScript? AddedScript { get; private set; }
    public IScript? InsertedScript { get; private set; }
    public int Index { get; private set; }
    public object? NewValue { get; private set; }
    public bool IsParameterUpdate { get; private set; }
    public bool IsNamedParameterUpdate { get; private set; }
    public string? Id { get; private set; }
    public bool ScriptsReplaced { get; set; }
}

public interface IScriptParent
{
    IEnumerable<string> GetVariablesInScope();
}

public interface IScript : IMutableField
{
    string? Line { get; set; }
    string Keyword { get; }
    IScriptParent Parent { get; set; }
    void Execute(Context c);
    Task ExecuteAsync(Context c);
    string Save();
    void SetParameter(int index, object value);
    void SetParameterSilent(int index, object value);
    object GetParameter(int index);
    event EventHandler<ScriptUpdatedEventArgs> ScriptUpdated;
    IEnumerable<string>? GetDefinedVariables();
}

public interface IFunctionCallScript : IScript
{
    object GetFunctionCallParameter(int index);
    void SetFunctionCallParameter(int index, object value);
    IScript GetFunctionCallParameterScript();
    void SetFunctionCallParameterScript(IScript script);
    event EventHandler<ScriptUpdatedEventArgs> FunctionCallParametersUpdated;
}

public interface IScriptConstructor
{
    string Keyword { get; }
    IScriptFactory ScriptFactory { set; }
    WorldModel WorldModel { get; set; }
    IScript Create(string script, ScriptContext scriptContext);
}

public abstract class ScriptBase : IScript, IMutableField
{
    private string? _line;

    public Element? Owner { get; set; }

    public abstract void Execute(Context c);

    public virtual Task ExecuteAsync(Context c) { Execute(c); return Task.CompletedTask; }

    public abstract string Save();

    public virtual string? Line
    {
        get => _line;
        set => _line = value;
    }

    public void SetParameter(int index, object value)
    {
        var oldValue = GetParameter(index);
        SetParameterSilent(index, value);
        UndoLog?.AddUndoAction(() => new UndoScriptChange(this, index, oldValue, value));
    }

    public void SetParameterSilent(int index, object value)
    {
        SetParameterInternal(index, value);
        NotifyUpdate(index, value);
    }

    public abstract object GetParameter(int index);

    public abstract string Keyword { get; }

    public event EventHandler<ScriptUpdatedEventArgs>? ScriptUpdated;

    [field: AllowNull, MaybeNull]
    public IScriptParent Parent
    {
        get;
        set
        {
            var changed = field != value;
            field = value;
            if (changed)
            {
                ParentUpdated();
            }
        }
    }

    public virtual IEnumerable<string>? GetDefinedVariables()
    {
        return null;
    }

    public override string ToString()
    {
        return "Script: " + Line;
    }

    protected string SaveScript(string keyword, params string[] args)
    {
        return keyword + " (" + string.Join(", ", args) + ")";
    }

    protected string SaveScript(string keyword, IScript? script, params string[] args)
    {
        var result = args.Length == 0 ? keyword : SaveScript(keyword, args);

        var scriptString = script != null ? script.Save() : string.Empty;
        return result + " {" + Environment.NewLine + scriptString + Environment.NewLine + "}";
    }

    protected void NotifyUpdate(int index, object newValue)
    {
        NotifyUpdate(new ScriptUpdatedEventArgs(index, newValue));
    }

    protected void NotifyUpdate(IScript added, IScript removed)
    {
        NotifyUpdate(new ScriptUpdatedEventArgs(added, removed));
    }

    protected void NotifyUpdate(IScript inserted, int index)
    {
        NotifyUpdate(new ScriptUpdatedEventArgs(inserted, index));
    }

    protected void NotifyUpdate(string id, object newValue)
    {
        NotifyUpdate(new ScriptUpdatedEventArgs(id, newValue));
    }

    protected void NotifyUpdate(ScriptUpdatedEventArgs args)
    {
        if (ScriptUpdated != null)
        {
            ScriptUpdated(this, args);
        }
    }

    protected abstract ScriptBase CloneScript();

    protected abstract void SetParameterInternal(int index, object value);

    protected virtual void ParentUpdated()
    {
    }

    private class UndoScriptChange(IScript appliesTo, int index, object oldValue, object newValue)
        : UndoLogger.IUndoAction
    {
        public void DoUndo(WorldModel worldModel)
        {
            appliesTo.SetParameterSilent(index, oldValue);
        }

        public void DoRedo(WorldModel worldModel)
        {
            appliesTo.SetParameterSilent(index, newValue);
        }
    }

    #region IMutableField Members

    public UndoLogger? UndoLog { get; set; }

    public bool Locked
    {
        get => false;
        set { }
    }

    public virtual IMutableField Clone()
    {
        var clone = CloneScript();
        clone._line = _line;
        return clone;
    }

    // Scripts only require cloning in the editor, as they cannot be modified when the game is running

    public bool RequiresCloning => false;

    #endregion
}