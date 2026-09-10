#nullable disable
namespace QuestViva.Engine.Scripts;

public class GetInputScriptConstructor : IScriptConstructor
{
    public string Keyword => "get input";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        var callback = Utility.GetScript(script.Substring(Keyword.Length).Trim());
        var callbackScript = ScriptFactory.CreateScript(callback);
        return new GetInputScript(scriptContext, ScriptFactory, callbackScript);
    }

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }
}

public class GetInputScript : ScriptBase
{
    private readonly IScript _callbackScript;
    private readonly ScriptContext _scriptContext;
    private readonly IScriptFactory _scriptFactory;
    private readonly WorldModel _worldModel;

    public GetInputScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IScript callbackScript)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _scriptFactory = scriptFactory;
        _callbackScript = callbackScript;
    }

    public override string Keyword => "get input";

    protected override ScriptBase CloneScript()
    {
        return new GetInputScript(_scriptContext, _scriptFactory, (IScript) _callbackScript.Clone());
    }

    public override Task ExecuteAsync(Context c)
    {
        _worldModel._commandOverride = true;
        WorldModel.BeginPrompt(ref _worldModel._commandInputTcs);
        _worldModel.BeginDormantSuspension();
        _worldModel.SignalTurnSuspended();
        _ = AwaitResponseAndRunCallbackAsync(c);
        return Task.CompletedTask;
    }

    private async Task AwaitResponseAndRunCallbackAsync(Context c)
    {
        var resolved = false;
        try
        {
            var result = await _worldModel._commandInputTcs.Task;
            resolved = true;
            _worldModel.SignalCallbackResolving();
            _worldModel._commandOverride = false;
            c.Parameters["result"] = result;
            await _worldModel.RunScriptAsync(_callbackScript, c);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _worldModel.LogException(ex); }
        finally
        {
            if (!resolved) _worldModel.SignalCallbackResolving();
            await _worldModel.EndPendingCallbackAsync();
            _worldModel.SignalTurnSuspended();
        }
    }

    public override string Save()
    {
        return SaveScript("get input", _callbackScript);
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _callbackScript;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'get input' command");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}