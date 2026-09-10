using System.Diagnostics;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public interface IIfScript : IScript
{
    IFunction<bool> Expression { get; }
    IScript ThenScript { get; set; }
    IScript? ElseScript { get; }
    IList<IElseIfScript> ElseIfScripts { get; }
    string ExpressionString { get; set; }
    void SetElse(IScript? elseScript);
    IElseIfScript AddElseIf(string expression, IScript script);
    IElseIfScript AddElseIf(IFunction<bool> expression, IScript script);
    event EventHandler<IfScriptUpdatedEventArgs> IfScriptUpdated;
    void RemoveElseIf(IElseIfScript elseIfScript);
}

public interface IElseIfScript
{
    IScript Script { get; }
    string ExpressionString { get; set; }
    string Id { get; }
}

public class IfScriptConstructor : IScriptConstructor
{
    public void AddElse(IScript script, string elseScript, ScriptContext scriptContext)
    {
        var add = GetElse(elseScript, scriptContext);
        ((IIfScript) script).SetElse(add);
    }

    public void AddElseIf(IScript script, string elseIfScript, ScriptContext scriptContext)
    {
        var add = GetElse(elseIfScript, scriptContext);
        if (add.Line == "")
        {
            return;
        }

        // GetElse uses the ScriptFactory to parse the "else if" block, so it will return
        // a MultiScript containing an IfScript with one expression and one "then" script block.

        var elseIf = (IIfScript) ((IMultiScript) add).Scripts.First();

        ((IIfScript) script).AddElseIf(elseIf.Expression, elseIf.ThenScript);
    }

    private IScript GetElse(string elseScript, ScriptContext scriptContext)
    {
        elseScript = Utility.GetTextAfter(elseScript, "else");
        return ScriptFactory.CreateScript(elseScript, scriptContext);
    }

    #region IScriptConstructor Members

    public string Keyword => "if";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string afterExpr;
        var expr = Utility.GetParameter(script, out afterExpr);

        if (afterExpr.StartsWith(")"))
        {
            // We have a mismatch of brackets in the expression
            throw new Exception("Too many ')'");
        }

        var then = Utility.GetScript(afterExpr);

        var thenScript = ScriptFactory.CreateScript(then, scriptContext);

        return new IfScript(new Expression<bool>(expr, scriptContext), thenScript, scriptContext);
    }

    public IScriptFactory ScriptFactory { get; set; } = null!;

    public WorldModel WorldModel { get; set; } = null!;

    #endregion
}

public class IfScript : ScriptBase, IIfScript
{
    private readonly List<IElseIfScript> _elseIfScript = new();
    private readonly ScriptContext _scriptContext;

    private bool _hasElse;

    private int _lastElseIfId;
    private WorldModel _worldModel;

    public IfScript(IFunction<bool> expression, IScript thenScript, ScriptContext scriptContext)
        : this(expression, thenScript, null, scriptContext)
    {
    }

    public IfScript(IFunction<bool> expression, IScript thenScript, IScript? elseScript, ScriptContext scriptContext)
    {
        Expression = expression;
        ThenScript = thenScript;
        ElseScript = elseScript;
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
    }

    public event EventHandler<IfScriptUpdatedEventArgs>? IfScriptUpdated;

    public void SetElse(IScript? elseScript)
    {
        if (UndoLog != null)
        {
            Debug.Assert(elseScript == null || !_hasElse,
                "UndoSetElse assumes that we only ever set the Else script once");

            UndoLog.StartTransaction("Add Else script");
            UndoLog.AddUndoAction(() => new UndoSetElse(this, ElseScript, elseScript, _hasElse, true));
        }

        _hasElse = true;
        SetElseSilent(elseScript);

        if (UndoLog != null)
        {
            UndoLog.EndTransaction();
        }
    }

    public IElseIfScript AddElseIf(string expression, IScript script)
    {
        IFunction<bool> expr = new Expression<bool>(expression, _scriptContext);
        return AddElseIf(expr, script);
    }

    public IElseIfScript AddElseIf(IFunction<bool> expression, IScript script)
    {
        var elseIfScript = new ElseIfScript(expression, script, this, GetNewElseIfID());

        if (UndoLog != null)
        {
            UndoLog.StartTransaction("Add Else If script");
            UndoLog.AddUndoAction(() => new UndoAddElseIf(this, elseIfScript));
        }

        AddElseIfSilent(elseIfScript);

        if (UndoLog != null)
        {
            UndoLog.EndTransaction();
        }

        return elseIfScript;
    }

    public void RemoveElseIf(IElseIfScript elseIfScript)
    {
        if (UndoLog != null)
        {
            UndoLog.StartTransaction("Remove Else If script");
            UndoLog.AddUndoAction(() => new UndoRemoveElseIf(this, elseIfScript));
        }

        RemoveElseIfSilent(elseIfScript);

        if (UndoLog != null)
        {
            UndoLog.EndTransaction();
        }
    }

    public IList<IElseIfScript> ElseIfScripts => _elseIfScript.AsReadOnly();

    public string ExpressionString
    {
        get => Expression.Save();
        set
        {
            UndoLog!.AddUndoAction(() => new UndoChangeExpression(this, Expression.Save(), value));
            SetExpressionSilent(value);
        }
    }

    public IFunction<bool> Expression { get; private set; }

    public IScript ThenScript { get; set; }

    public IScript? ElseScript { get; private set; }

    protected override ScriptBase CloneScript()
    {
        var clone = new IfScript(Expression.Clone(), (IScript) ThenScript.Clone(),
            ElseScript == null ? null : (IScript) ElseScript.Clone(), _scriptContext);
        clone._hasElse = _hasElse;
        foreach (ElseIfScript elseif in _elseIfScript)
        {
            clone._elseIfScript.Add(elseif.Clone(clone));
        }

        clone._lastElseIfId = _lastElseIfId;
        return clone;
    }

    private void SetElseSilent(IScript? elseScript)
    {
        ElseScript = elseScript;

        if (IfScriptUpdated != null)
        {
            var eventType =
                elseScript == null
                    ? IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElse
                    : IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElse;
            IfScriptUpdated(this, new IfScriptUpdatedEventArgs(eventType));
        }
    }

    private string GetNewElseIfID()
    {
        _lastElseIfId++;
        return "elseif" + _lastElseIfId;
    }

    private void AddElseIfSilent(IElseIfScript elseIfScript)
    {
        _elseIfScript.Add(elseIfScript);

        if (IfScriptUpdated != null)
        {
            IfScriptUpdated(this,
                new IfScriptUpdatedEventArgs(IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElseIf,
                    elseIfScript));
        }
    }

    private void RemoveElseIfSilent(IElseIfScript elseIfScript)
    {
        _elseIfScript.Remove(elseIfScript);

        if (IfScriptUpdated != null)
        {
            IfScriptUpdated(this,
                new IfScriptUpdatedEventArgs(IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElseIf,
                    elseIfScript));
        }
    }

    private void SetExpressionSilent(string newValue)
    {
        Expression = new Expression<bool>(newValue, _scriptContext);
        NotifyUpdate(0, newValue);
    }

    public class ElseIfScript : IElseIfScript
    {
        private readonly IfScript _parent;

        public ElseIfScript(IFunction<bool> expression, IScript script, IfScript parent, string id)
        {
            Expression = expression;
            Script = script;
            _parent = parent;
            Id = id;
        }

        internal IFunction<bool> Expression { get; private set; }
        public IScript Script { get; }
        public string Id { get; }

        public string ExpressionString
        {
            get => Expression.Save();
            set
            {
                _parent.UndoLog!.AddUndoAction(() => new UndoChangeExpression(this, Expression.Save(), value));
                SetExpressionSilent(value);
            }
        }

        internal ElseIfScript Clone(IfScript newParent)
        {
            return new ElseIfScript(Expression.Clone(), (IScript) Script.Clone(), newParent, Id);
        }

        internal void SetExpressionSilent(string newValue)
        {
            Expression = new Expression<bool>(newValue, _parent._scriptContext);
            _parent.NotifyUpdate(Id, newValue);
        }
    }

    private class UndoChangeExpression : UndoLogger.IUndoAction
    {
        private readonly ElseIfScript? _elseIfScript;
        private readonly string _newValue;
        private readonly string _oldValue;
        private readonly IfScript? _script;

        private UndoChangeExpression(string oldValue, string newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public UndoChangeExpression(IfScript script, string oldValue, string newValue)
            : this(oldValue, newValue)
        {
            _script = script;
        }

        public UndoChangeExpression(ElseIfScript elseIfscript, string oldValue, string newValue)
            : this(oldValue, newValue)
        {
            _elseIfScript = elseIfscript;
        }

        public void DoUndo(WorldModel worldModel)
        {
            if (_script != null)
            {
                _script.SetExpressionSilent(_oldValue);
            }

            if (_elseIfScript != null)
            {
                _elseIfScript.SetExpressionSilent(_oldValue);
            }
        }

        public void DoRedo(WorldModel worldModel)
        {
            if (_script != null)
            {
                _script.SetExpressionSilent(_newValue);
            }

            if (_elseIfScript != null)
            {
                _elseIfScript.SetExpressionSilent(_newValue);
            }
        }
    }

    // We only need an UndoSetElse and not an UndoSetThen, because an "if" *always*
    // has a "then". So here we implictly assume that the old value was null, and that
    // _hasElse was false.

    private class UndoSetElse : UndoLogger.IUndoAction
    {
        private readonly bool _newHasElse;
        private readonly IScript? _newValue;
        private readonly bool _oldHasElse;
        private readonly IScript? _oldValue;
        private readonly IfScript _script;

        public UndoSetElse(IfScript script, IScript? oldValue, IScript? newValue, bool oldHasElse, bool newHasElse)
        {
            _script = script;
            _oldValue = oldValue;
            _newValue = newValue;
            _oldHasElse = oldHasElse;
            _newHasElse = newHasElse;
        }

        public void DoUndo(WorldModel worldModel)
        {
            _script._hasElse = _oldHasElse;
            _script.SetElseSilent(_oldValue);
        }

        public void DoRedo(WorldModel worldModel)
        {
            _script._hasElse = _newHasElse;
            _script.SetElseSilent(_newValue);
        }
    }

    private class UndoAddElseIf : UndoLogger.IUndoAction
    {
        private readonly IElseIfScript _elseIf;
        private readonly IfScript _script;

        public UndoAddElseIf(IfScript script, IElseIfScript elseIf)
        {
            _script = script;
            _elseIf = elseIf;
        }

        public void DoUndo(WorldModel worldModel)
        {
            _script.RemoveElseIfSilent(_elseIf);
        }

        public void DoRedo(WorldModel worldModel)
        {
            _script.AddElseIfSilent(_elseIf);
        }
    }

    private class UndoRemoveElseIf : UndoLogger.IUndoAction
    {
        private readonly IElseIfScript _elseIf;
        private readonly IfScript _script;

        public UndoRemoveElseIf(IfScript script, IElseIfScript elseIf)
        {
            _script = script;
            _elseIf = elseIf;
        }

        public void DoUndo(WorldModel worldModel)
        {
            _script.AddElseIfSilent(_elseIf);
        }

        public void DoRedo(WorldModel worldModel)
        {
            _script.RemoveElseIfSilent(_elseIf);
        }
    }

    #region IScript Members

    public override async Task ExecuteAsync(Context c)
    {
        if (await Expression.ExecuteAsync(c))
        {
            await ThenScript.ExecuteAsync(c);
            return;
        }

        if (_elseIfScript != null)
        {
            foreach (ElseIfScript elseIfScript in _elseIfScript)
            {
                if (await elseIfScript.Expression.ExecuteAsync(c))
                {
                    await elseIfScript.Script.ExecuteAsync(c);
                    return;
                }
            }
        }

        if (ElseScript != null)
        {
            await ElseScript.ExecuteAsync(c);
        }
    }

    public override string Save()
    {
        var result = SaveScript("if", ThenScript, Expression.Save());
        if (_elseIfScript != null)
        {
            foreach (ElseIfScript elseIf in _elseIfScript)
            {
                result += Environment.NewLine + SaveScript("else if", elseIf.Script, elseIf.Expression.Save());
            }
        }

        if (ElseScript != null)
        {
            result += "else {" + Environment.NewLine + ElseScript.Save() + Environment.NewLine + "}";
        }

        return result;
    }

    public override string Keyword => "if";

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                // expression
                return Expression.Save();
            case 1:
                // "then" script
                return ThenScript.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        throw new NotImplementedException();
    }

    #endregion
}

public class IfScriptUpdatedEventArgs : EventArgs
{
    public enum IfScriptUpdatedEventType
    {
        AddedElse,
        RemovedElse,
        AddedElseIf,
        RemovedElseIf
    }

    internal IfScriptUpdatedEventArgs(IfScriptUpdatedEventType eventType)
    {
        EventType = eventType;
    }

    internal IfScriptUpdatedEventArgs(IfScriptUpdatedEventType eventType, IElseIfScript data)
        : this(eventType)
    {
        Data = data;
    }

    public IfScriptUpdatedEventType EventType { get; private set; }
    public IElseIfScript? Data { get; private set; }
}