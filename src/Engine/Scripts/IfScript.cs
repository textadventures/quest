#nullable disable
using System.Diagnostics;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public interface IIfScript : IScript
{
    IFunction<bool> Expression { get; }
    IScript ThenScript { get; set; }
    IScript ElseScript { get; }
    IList<IElseIfScript> ElseIfScripts { get; }
    string ExpressionString { get; set; }
    void SetElse(IScript elseScript);
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

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }

    #endregion
}

public class IfScript : ScriptBase, IIfScript
{
    private readonly List<IElseIfScript> m_elseIfScript = new();
    private readonly ScriptContext m_scriptContext;

    private bool m_hasElse;

    private int m_lastElseIfId;
    private WorldModel m_worldModel;

    public IfScript(IFunction<bool> expression, IScript thenScript, ScriptContext scriptContext)
        : this(expression, thenScript, null, scriptContext)
    {
    }

    public IfScript(IFunction<bool> expression, IScript thenScript, IScript elseScript, ScriptContext scriptContext)
    {
        Expression = expression;
        ThenScript = thenScript;
        ElseScript = elseScript;
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
    }

    public event EventHandler<IfScriptUpdatedEventArgs> IfScriptUpdated;

    public void SetElse(IScript elseScript)
    {
        if (UndoLog != null)
        {
            Debug.Assert(elseScript == null || !m_hasElse,
                "UndoSetElse assumes that we only ever set the Else script once");

            UndoLog.StartTransaction("Add Else script");
            UndoLog.AddUndoAction(() => new UndoSetElse(this, ElseScript, elseScript, m_hasElse, true));
        }

        m_hasElse = true;
        SetElseSilent(elseScript);

        if (UndoLog != null)
        {
            UndoLog.EndTransaction();
        }
    }

    public IElseIfScript AddElseIf(string expression, IScript script)
    {
        IFunction<bool> expr = new Expression<bool>(expression, m_scriptContext);
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

    public IList<IElseIfScript> ElseIfScripts => m_elseIfScript.AsReadOnly();

    public string ExpressionString
    {
        get => Expression.Save();
        set
        {
            UndoLog.AddUndoAction(() => new UndoChangeExpression(this, Expression.Save(), value));
            SetExpressionSilent(value);
        }
    }

    public IFunction<bool> Expression { get; private set; }

    public IScript ThenScript { get; set; }

    public IScript ElseScript { get; private set; }

    protected override ScriptBase CloneScript()
    {
        var clone = new IfScript(Expression.Clone(), (IScript) ThenScript.Clone(),
            ElseScript == null ? null : (IScript) ElseScript.Clone(), m_scriptContext);
        clone.m_hasElse = m_hasElse;
        foreach (ElseIfScript elseif in m_elseIfScript)
        {
            clone.m_elseIfScript.Add(elseif.Clone(clone));
        }

        clone.m_lastElseIfId = m_lastElseIfId;
        return clone;
    }

    private void SetElseSilent(IScript elseScript)
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
        m_lastElseIfId++;
        return "elseif" + m_lastElseIfId;
    }

    private void AddElseIfSilent(IElseIfScript elseIfScript)
    {
        m_elseIfScript.Add(elseIfScript);

        if (IfScriptUpdated != null)
        {
            IfScriptUpdated(this,
                new IfScriptUpdatedEventArgs(IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElseIf,
                    elseIfScript));
        }
    }

    private void RemoveElseIfSilent(IElseIfScript elseIfScript)
    {
        m_elseIfScript.Remove(elseIfScript);

        if (IfScriptUpdated != null)
        {
            IfScriptUpdated(this,
                new IfScriptUpdatedEventArgs(IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElseIf,
                    elseIfScript));
        }
    }

    private void SetExpressionSilent(string newValue)
    {
        Expression = new Expression<bool>(newValue, m_scriptContext);
        NotifyUpdate(0, newValue);
    }

    public class ElseIfScript : IElseIfScript
    {
        private readonly IfScript m_parent;

        public ElseIfScript(IFunction<bool> expression, IScript script, IfScript parent, string id)
        {
            Expression = expression;
            Script = script;
            m_parent = parent;
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
                m_parent.UndoLog.AddUndoAction(() => new UndoChangeExpression(this, Expression.Save(), value));
                SetExpressionSilent(value);
            }
        }

        internal ElseIfScript Clone(IfScript newParent)
        {
            return new ElseIfScript(Expression.Clone(), (IScript) Script.Clone(), newParent, Id);
        }

        internal void SetExpressionSilent(string newValue)
        {
            Expression = new Expression<bool>(newValue, m_parent.m_scriptContext);
            m_parent.NotifyUpdate(Id, newValue);
        }
    }

    private class UndoChangeExpression : UndoLogger.IUndoAction
    {
        private readonly ElseIfScript m_elseIfScript;
        private readonly string m_newValue;
        private readonly string m_oldValue;
        private readonly IfScript m_script;

        private UndoChangeExpression(string oldValue, string newValue)
        {
            m_oldValue = oldValue;
            m_newValue = newValue;
        }

        public UndoChangeExpression(IfScript script, string oldValue, string newValue)
            : this(oldValue, newValue)
        {
            m_script = script;
        }

        public UndoChangeExpression(ElseIfScript elseIfscript, string oldValue, string newValue)
            : this(oldValue, newValue)
        {
            m_elseIfScript = elseIfscript;
        }

        public void DoUndo(WorldModel worldModel)
        {
            if (m_script != null)
            {
                m_script.SetExpressionSilent(m_oldValue);
            }

            if (m_elseIfScript != null)
            {
                m_elseIfScript.SetExpressionSilent(m_oldValue);
            }
        }

        public void DoRedo(WorldModel worldModel)
        {
            if (m_script != null)
            {
                m_script.SetExpressionSilent(m_newValue);
            }

            if (m_elseIfScript != null)
            {
                m_elseIfScript.SetExpressionSilent(m_newValue);
            }
        }
    }

    // We only need an UndoSetElse and not an UndoSetThen, because an "if" *always*
    // has a "then". So here we implictly assume that the old value was null, and that
    // m_hasElse was false.

    private class UndoSetElse : UndoLogger.IUndoAction
    {
        private readonly bool m_newHasElse;
        private readonly IScript m_newValue;
        private readonly bool m_oldHasElse;
        private readonly IScript m_oldValue;
        private readonly IfScript m_script;

        public UndoSetElse(IfScript script, IScript oldValue, IScript newValue, bool oldHasElse, bool newHasElse)
        {
            m_script = script;
            m_oldValue = oldValue;
            m_newValue = newValue;
            m_oldHasElse = oldHasElse;
            m_newHasElse = newHasElse;
        }

        public void DoUndo(WorldModel worldModel)
        {
            m_script.m_hasElse = m_oldHasElse;
            m_script.SetElseSilent(m_oldValue);
        }

        public void DoRedo(WorldModel worldModel)
        {
            m_script.m_hasElse = m_newHasElse;
            m_script.SetElseSilent(m_newValue);
        }
    }

    private class UndoAddElseIf : UndoLogger.IUndoAction
    {
        private readonly IElseIfScript m_elseIf;
        private readonly IfScript m_script;

        public UndoAddElseIf(IfScript script, IElseIfScript elseIf)
        {
            m_script = script;
            m_elseIf = elseIf;
        }

        public void DoUndo(WorldModel worldModel)
        {
            m_script.RemoveElseIfSilent(m_elseIf);
        }

        public void DoRedo(WorldModel worldModel)
        {
            m_script.AddElseIfSilent(m_elseIf);
        }
    }

    private class UndoRemoveElseIf : UndoLogger.IUndoAction
    {
        private readonly IElseIfScript m_elseIf;
        private readonly IfScript m_script;

        public UndoRemoveElseIf(IfScript script, IElseIfScript elseIf)
        {
            m_script = script;
            m_elseIf = elseIf;
        }

        public void DoUndo(WorldModel worldModel)
        {
            m_script.AddElseIfSilent(m_elseIf);
        }

        public void DoRedo(WorldModel worldModel)
        {
            m_script.RemoveElseIfSilent(m_elseIf);
        }
    }

    #region IScript Members

    public override void Execute(Context c)
    {
        if (Expression.Execute(c))
        {
            ThenScript.Execute(c);
            return;
        }

        if (m_elseIfScript != null)
        {
            foreach (ElseIfScript elseIfScript in m_elseIfScript)
            {
                if (elseIfScript.Expression.Execute(c))
                {
                    elseIfScript.Script.Execute(c);
                    return;
                }
            }
        }

        if (ElseScript != null)
        {
            ElseScript.Execute(c);
        }
    }

    public override string Save()
    {
        var result = SaveScript("if", ThenScript, Expression.Save());
        if (m_elseIfScript != null)
        {
            foreach (ElseIfScript elseIf in m_elseIfScript)
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

    public override object GetParameter(int index)
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

    protected override void SetParameterInternal(int index, object value)
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
    public IElseIfScript Data { get; private set; }
}