namespace QuestViva.Engine.Scripts;

internal interface IFirstTimeScript
{
    void SetOtherwiseScript(IScript script);
}

public class FirstTimeScriptConstructor : IScriptConstructor
{
    public string Keyword => "firsttime";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        // Get script after "firsttime" keyword
        script = script.Substring(9).Trim();
        var firstTime = Utility.GetScript(script);
        var firstTimeScript = ScriptFactory.CreateScript(firstTime);

        return new FirstTimeScript(WorldModel, ScriptFactory, firstTimeScript);
    }

    public IScriptFactory ScriptFactory { get; set; } = null!;

    public WorldModel WorldModel { get; set; } = null!;

    public static void AddOtherwiseScript(IScript firstTimeScript, string script, IScriptFactory scriptFactory)
    {
        // Get script after "otherwise" keyword
        script = script.Substring(9).Trim();
        var otherwise = Utility.GetScript(script);
        var otherwiseScript = scriptFactory.CreateScript(otherwise);
        ((IFirstTimeScript) firstTimeScript).SetOtherwiseScript(otherwiseScript);
    }
}

public class FirstTimeScript : ScriptBase, IFirstTimeScript
{
    private readonly IScript _firstTimeScript;
    private readonly IScriptFactory _scriptFactory;
    private readonly WorldModel _worldModel;
    private bool _hasRun;
    private IScript? _otherwiseScript;

    public FirstTimeScript(WorldModel worldModel, IScriptFactory scriptFactory, IScript firstTimeScript)
    {
        _worldModel = worldModel;
        _scriptFactory = scriptFactory;
        _firstTimeScript = firstTimeScript;
    }

    public override string Keyword => "firsttime";

    public void SetOtherwiseScript(IScript script)
    {
        _otherwiseScript = script;
    }

    protected override ScriptBase CloneScript()
    {
        var result = new FirstTimeScript(_worldModel, _scriptFactory, (IScript) _firstTimeScript.Clone());
        if (_otherwiseScript != null)
        {
            result._otherwiseScript = (IScript) _otherwiseScript.Clone();
        }

        return result;
    }

    protected override void ParentUpdated()
    {
        _firstTimeScript.Parent = Parent;
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (!_hasRun)
        {
            _hasRun = true;
            _worldModel.UndoLogger.AddUndoAction(() => new UndoFirstTime(this));
            await _firstTimeScript.ExecuteAsync(c);
        }
        else
        {
            if (_otherwiseScript != null)
            {
                await _otherwiseScript.ExecuteAsync(c);
            }
        }
    }

    public override string Save()
    {
        if (_worldModel.EditMode || !_hasRun)
        {
            if (_otherwiseScript == null)
            {
                return SaveScript("firsttime", _firstTimeScript);
            }

            return SaveScript("firsttime", _firstTimeScript) + Environment.NewLine +
                   SaveScript("otherwise", _otherwiseScript);
        }

        if (_otherwiseScript == null)
        {
            return string.Empty;
        }

        return _otherwiseScript.Save();
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _firstTimeScript;
            case 1:
                return _otherwiseScript;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'firsttime' script");
            case 1:
                _otherwiseScript = (IScript) value!;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private class UndoFirstTime : UndoLogger.IUndoAction
    {
        private readonly FirstTimeScript _parent;

        public UndoFirstTime(FirstTimeScript parent)
        {
            _parent = parent;
        }

        public void DoUndo(WorldModel worldModel)
        {
            _parent._hasRun = false;
        }

        public void DoRedo(WorldModel worldModel)
        {
            _parent._hasRun = true;
        }
    }
}