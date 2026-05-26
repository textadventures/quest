#nullable disable
namespace QuestViva.Engine.Scripts;

public interface IMultiScript : IScript
{
    IEnumerable<IScript> Scripts { get; }
    void Add(params IScript[] scripts);
    void Remove(int index);
    void Swap(int index1, int index2);
    void Insert(int index, IScript script);
    void LoadCode(string code);
}

public class MultiScript : ScriptBase, IScriptParent, IMultiScript
{
    private readonly WorldModel m_worldModel;

    private ScriptFactory m_scriptFactory;
    private List<IScript> m_scripts;

    public MultiScript(WorldModel worldModel, params IScript[] scripts)
        : this(worldModel)
    {
        m_scripts = new List<IScript>(scripts);
    }

    private MultiScript(WorldModel worldModel)
    {
        m_worldModel = worldModel;
    }

    private ScriptFactory ScriptFactory
    {
        get
        {
            if (m_scriptFactory == null)
            {
                m_scriptFactory = new ScriptFactory(m_worldModel);
            }

            return m_scriptFactory;
        }
    }

    public override string Keyword => null;

    public void Add(params IScript[] scripts)
    {
        m_scripts.AddRange(scripts);
        if (UndoLog != null)
        {
            foreach (var script in scripts)
            {
                UndoLog.AddUndoAction(() => new UndoMultiScriptAddRemove(this, script, true, null));
            }
        }

        foreach (var script in scripts)
        {
            script.Parent = this;
            NotifyUpdate(script, null);
        }
    }

    public void Remove(int index)
    {
        if (UndoLog != null)
        {
            UndoLog.AddUndoAction(() => new UndoMultiScriptAddRemove(this, m_scripts[index], false, index));
        }

        RemoveSilent(m_scripts[index]);
    }

    public void Insert(int index, IScript script)
    {
        if (UndoLog != null)
        {
            UndoLog.AddUndoAction(() => new UndoMultiScriptAddRemove(this, script, true, index));
        }

        InsertSilent(index, script);
    }

    public void Swap(int index1, int index2)
    {
        if (index1 == index2)
        {
            return;
        }

        if (index1 > index2)
        {
            var temp = index1;
            index1 = index2;
            index2 = temp;
        }

        IScript script;

        // This swap assumes index1 < index2
        script = m_scripts[index1];
        Remove(index1);
        Insert(index2, script);

        // If the elements are consecutive then there's no need to
        // move the second script, as it's already in the correct place
        if (index1 != index2 - 1)
        {
            script = m_scripts[index2 - 1];
            Remove(index2 - 1);
            Insert(index1, script);
        }
    }

    public IEnumerable<IScript> Scripts => m_scripts.AsReadOnly();

    public override void Execute(Context c)
    {
        foreach (var script in m_scripts)
        {
            script.Execute(c);
            if (c.IsReturned)
            {
                break;
            }
        }
    }

    public override string Line
    {
        get
        {
            var result = string.Empty;
            foreach (var script in m_scripts)
            {
                result += script.Line + Environment.NewLine;
            }

            return result;
        }
        set => throw new Exception("Cannot set Line in MultiScript");
    }

    public override string Save()
    {
        var result = string.Empty;

        foreach (var script in m_scripts)
        {
            if (result.Length > 0)
            {
                result += Environment.NewLine;
            }

            result += script.Save();
        }

        return result;
    }

    public override object GetParameter(int index)
    {
        throw new NotImplementedException();
    }

    public void LoadCode(string code)
    {
        var newScript = (IMultiScript) ScriptFactory.CreateScript(code);
        var newScriptList = new List<IScript>(newScript.Scripts);
        if (UndoLog != null)
        {
            UndoLog.AddUndoAction(() => new UndoMultiScriptLoadCode(this, m_scripts, newScriptList));
        }

        ReplaceScripts(newScriptList);
    }

    public IEnumerable<string> GetVariablesInScope()
    {
        if (Parent == null)
        {
            var result = new List<string>();
            foreach (var script in m_scripts)
            {
                // add any variables defined by the child script to the list
                var definedVariables = script.GetDefinedVariables();
                if (definedVariables != null)
                {
                    result.AddRange(definedVariables);
                }
            }

            return result;
        }

        return Parent.GetVariablesInScope();
    }

    protected override ScriptBase CloneScript()
    {
        var clone = new MultiScript(m_worldModel);
        clone.m_scripts = new List<IScript>();
        foreach (var script in m_scripts)
        {
            var clonedScript = (IScript) script.Clone();
            clonedScript.Parent = clone;
            clone.m_scripts.Add(clonedScript);
        }

        return clone;
    }

    private void RemoveSilent(IScript script)
    {
        script.Parent = null;
        m_scripts.Remove(script);
        NotifyUpdate(null, script);
    }

    private void AddSilent(IScript script)
    {
        script.Parent = this;
        m_scripts.Add(script);
        NotifyUpdate(script, null);
    }

    private void InsertSilent(int index, IScript script)
    {
        script.Parent = this;
        m_scripts.Insert(index, script);
        NotifyUpdate(script, index);
    }

    protected override void SetParameterInternal(int index, object value)
    {
        throw new NotImplementedException();
    }

    private void ReplaceScripts(List<IScript> newScripts)
    {
        foreach (var script in m_scripts)
        {
            script.Parent = null;
        }

        m_scripts = newScripts;
        foreach (var script in m_scripts)
        {
            script.Parent = this;
        }

        NotifyUpdate(new ScriptUpdatedEventArgs {ScriptsReplaced = true});
    }

    private class UndoMultiScriptLoadCode : UndoLogger.IUndoAction
    {
        private readonly MultiScript m_appliesTo;
        private readonly List<IScript> m_newScripts;
        private readonly List<IScript> m_oldScripts;

        public UndoMultiScriptLoadCode(MultiScript appliesTo, List<IScript> oldScripts, List<IScript> newScripts)
        {
            m_appliesTo = appliesTo;
            m_oldScripts = oldScripts;
            m_newScripts = newScripts;
        }

        public void DoUndo(WorldModel worldModel)
        {
            m_appliesTo.ReplaceScripts(m_oldScripts);
        }

        public void DoRedo(WorldModel worldModel)
        {
            m_appliesTo.ReplaceScripts(m_newScripts);
        }
    }

    private class UndoMultiScriptAddRemove : UndoLogger.IUndoAction
    {
        private readonly MultiScript m_appliesTo;
        private readonly int? m_index;
        private readonly bool m_isAdd;
        private readonly IScript m_script;

        public UndoMultiScriptAddRemove(MultiScript appliesTo, IScript script, bool isAdd, int? index)
        {
            m_appliesTo = appliesTo;
            m_script = script;
            m_isAdd = isAdd;
            m_index = index;
        }

        private void DoAdd()
        {
            if (m_index.HasValue)
            {
                m_appliesTo.InsertSilent(m_index.Value, m_script);
            }
            else
            {
                m_appliesTo.AddSilent(m_script);
            }
        }

        private void DoRemove()
        {
            m_appliesTo.RemoveSilent(m_script);
        }

        #region IUndoAction Members

        public void DoUndo(WorldModel worldModel)
        {
            if (m_isAdd)
            {
                DoRemove();
            }
            else
            {
                DoAdd();
            }
        }

        public void DoRedo(WorldModel worldModel)
        {
            if (m_isAdd)
            {
                DoAdd();
            }
            else
            {
                DoRemove();
            }
        }

        #endregion
    }
}