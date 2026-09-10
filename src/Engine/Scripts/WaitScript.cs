#nullable disable
namespace QuestViva.Engine.Scripts;

public class WaitScriptConstructor : IScriptConstructor
{
    public string Keyword => "wait";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        var callback = Utility.GetScript(script.Substring(Keyword.Length).Trim());

        var callbackScript = ScriptFactory.CreateScript(callback);

        return new WaitScript(WorldModel, ScriptFactory, callbackScript);
    }

    public IScriptFactory ScriptFactory { get; set; }
    public WorldModel WorldModel { get; set; }
}

public class WaitScript : ScriptBase
{
    private readonly IScript _callbackScript;
    private readonly IScriptFactory _scriptFactory;
    private readonly WorldModel _worldModel;

    public WaitScript(WorldModel worldModel, IScriptFactory scriptFactory, IScript callbackScript)
    {
        _worldModel = worldModel;
        _scriptFactory = scriptFactory;
        _callbackScript = callbackScript;
    }

    public override string Keyword => "wait";

    protected override ScriptBase CloneScript()
    {
        return new WaitScript(_worldModel, _scriptFactory, (IScript) _callbackScript.Clone());
    }

    public override Task ExecuteAsync(Context c)
    {
        _worldModel.PlayerUi.DoWait();
        WorldModel.BeginPrompt(ref _worldModel._waitTcs);
        _worldModel.BeginDormantSuspension();
        _worldModel.SignalTurnSuspended();
        _ = AwaitWaitAndRunCallbackAsync(c);
        return Task.CompletedTask;
    }

    private async Task AwaitWaitAndRunCallbackAsync(Context c)
    {
        var resolved = false;
        try
        {
            await _worldModel._waitTcs.Task;
            resolved = true;
            _worldModel.SignalCallbackResolving();
            if (_callbackScript != null)
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
        return SaveScript("wait", _callbackScript);
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
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'wait' command");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
