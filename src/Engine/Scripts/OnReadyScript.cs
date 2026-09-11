namespace QuestViva.Engine.Scripts;

public class OnReadyScriptConstructor : IScriptConstructor
{
    public string Keyword => "on ready";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        var callback = Utility.GetScript(script.Substring(Keyword.Length).Trim());
        var callbackScript = ScriptFactory.CreateScript(callback);
        return new OnReadyScript(scriptContext, ScriptFactory, callbackScript);
    }

    public IScriptFactory ScriptFactory { get; set; } = null!;

    public WorldModel WorldModel { get; set; } = null!;
}

public class OnReadyScript : ScriptBase
{
    private readonly IScript _callbackScript;
    private readonly ScriptContext _scriptContext;
    private readonly IScriptFactory _scriptFactory;
    private readonly WorldModel _worldModel;

    public OnReadyScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IScript callbackScript)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _scriptFactory = scriptFactory;
        _callbackScript = callbackScript;
    }

    public override string Keyword => "on ready";

    protected override ScriptBase CloneScript()
    {
        return new OnReadyScript(_scriptContext, _scriptFactory, (IScript) _callbackScript.Clone());
    }

    public override Task ExecuteAsync(Context c)
    {
        return _worldModel.AddOnReady(_callbackScript, c);
    }

    public override string Save()
    {
        return SaveScript("on ready", _callbackScript);
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _callbackScript;
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
                    "Attempt to use SetParameter to change the script of an 'on ready' command");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}