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
    private readonly WorldModel _worldModel;

    private ScriptFactory? _scriptFactory;
    private List<IScript> _scripts;

    public MultiScript(WorldModel worldModel, params IScript[] scripts)
        : this(worldModel)
    {
        _scripts = new List<IScript>(scripts);
    }

    private MultiScript(WorldModel worldModel)
    {
        _worldModel = worldModel;
        _scripts = [];
    }

    private ScriptFactory ScriptFactory
    {
        get
        {
            if (_scriptFactory == null)
            {
                _scriptFactory = new ScriptFactory(_worldModel);
            }

            return _scriptFactory;
        }
    }

    public override string? Keyword => null;

    public void Add(params IScript[] scripts)
    {
        _scripts.AddRange(scripts);
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
            UndoLog.AddUndoAction(() => new UndoMultiScriptAddRemove(this, _scripts[index], false, index));
        }

        RemoveSilent(_scripts[index]);
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
        script = _scripts[index1];
        Remove(index1);
        Insert(index2, script);

        // If the elements are consecutive then there's no need to
        // move the second script, as it's already in the correct place
        if (index1 != index2 - 1)
        {
            script = _scripts[index2 - 1];
            Remove(index2 - 1);
            Insert(index1, script);
        }
    }

    public IEnumerable<IScript> Scripts => _scripts.AsReadOnly();

    public override async Task ExecuteAsync(Context c)
    {
        foreach (var script in _scripts)
        {
            await script.ExecuteAsync(c);
            if (c.IsReturned)
            {
                break;
            }
        }
    }

    public override string? Line
    {
        get
        {
            var result = string.Empty;
            foreach (var script in _scripts)
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

        foreach (var script in _scripts)
        {
            if (result.Length > 0)
            {
                result += Environment.NewLine;
            }

            result += script.Save();
        }

        return result;
    }

    public override object? GetParameter(int index)
    {
        throw new NotImplementedException();
    }

    public void LoadCode(string code)
    {
        var newScript = (IMultiScript) ScriptFactory.CreateScript(code);
        var newScriptList = new List<IScript>(newScript.Scripts);
        if (UndoLog != null)
        {
            UndoLog.AddUndoAction(() => new UndoMultiScriptLoadCode(this, _scripts, newScriptList));
        }

        ReplaceScripts(newScriptList);
    }

    public IEnumerable<string> GetVariablesInScope()
    {
        if (Parent == null)
        {
            var result = new List<string>();
            foreach (var script in _scripts)
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
        var clone = new MultiScript(_worldModel);
        clone._scripts = new List<IScript>();
        foreach (var script in _scripts)
        {
            var clonedScript = (IScript) script.Clone();
            clonedScript.Parent = clone;
            clone._scripts.Add(clonedScript);
        }

        return clone;
    }

    private void RemoveSilent(IScript script)
    {
        script.Parent = null;
        _scripts.Remove(script);
        NotifyUpdate(null, script);
    }

    private void AddSilent(IScript script)
    {
        script.Parent = this;
        _scripts.Add(script);
        NotifyUpdate(script, null);
    }

    private void InsertSilent(int index, IScript script)
    {
        script.Parent = this;
        _scripts.Insert(index, script);
        NotifyUpdate(script, index);
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        throw new NotImplementedException();
    }

    private void ReplaceScripts(List<IScript> newScripts)
    {
        foreach (var script in _scripts)
        {
            script.Parent = null;
        }

        _scripts = newScripts;
        foreach (var script in _scripts)
        {
            script.Parent = this;
        }

        NotifyUpdate(new ScriptUpdatedEventArgs {ScriptsReplaced = true});
    }

    private class UndoMultiScriptLoadCode : UndoLogger.IUndoAction
    {
        private readonly MultiScript _appliesTo;
        private readonly List<IScript> _newScripts;
        private readonly List<IScript> _oldScripts;

        public UndoMultiScriptLoadCode(MultiScript appliesTo, List<IScript> oldScripts, List<IScript> newScripts)
        {
            _appliesTo = appliesTo;
            _oldScripts = oldScripts;
            _newScripts = newScripts;
        }

        public void DoUndo(WorldModel worldModel)
        {
            _appliesTo.ReplaceScripts(_oldScripts);
        }

        public void DoRedo(WorldModel worldModel)
        {
            _appliesTo.ReplaceScripts(_newScripts);
        }
    }

    private class UndoMultiScriptAddRemove : UndoLogger.IUndoAction
    {
        private readonly MultiScript _appliesTo;
        private readonly int? _index;
        private readonly bool _isAdd;
        private readonly IScript _script;

        public UndoMultiScriptAddRemove(MultiScript appliesTo, IScript script, bool isAdd, int? index)
        {
            _appliesTo = appliesTo;
            _script = script;
            _isAdd = isAdd;
            _index = index;
        }

        private void DoAdd()
        {
            if (_index.HasValue)
            {
                _appliesTo.InsertSilent(_index.Value, _script);
            }
            else
            {
                _appliesTo.AddSilent(_script);
            }
        }

        private void DoRemove()
        {
            _appliesTo.RemoveSilent(_script);
        }

        #region IUndoAction Members

        public void DoUndo(WorldModel worldModel)
        {
            if (_isAdd)
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
            if (_isAdd)
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