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
    private readonly IScript m_callbackScript;
    private readonly ScriptContext m_scriptContext;
    private readonly IScriptFactory m_scriptFactory;
    private readonly WorldModel m_worldModel;

    public GetInputScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IScript callbackScript)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_scriptFactory = scriptFactory;
        m_callbackScript = callbackScript;
    }

    public override string Keyword => "get input";

    protected override ScriptBase CloneScript()
    {
        return new GetInputScript(m_scriptContext, m_scriptFactory, (IScript) m_callbackScript.Clone());
    }

    public override Task ExecuteAsync(Context c)
    {
        m_worldModel._commandOverride = true;
        m_worldModel._commandInputTcs = new TaskCompletionSource<string>();
        m_worldModel.BeginPendingCallback();
        m_worldModel.SignalTurnSuspended();
        _ = AwaitResponseAndRunCallbackAsync(c);
        return Task.CompletedTask;
    }

    private async Task AwaitResponseAndRunCallbackAsync(Context c)
    {
        try
        {
            var result = await m_worldModel._commandInputTcs.Task;
            m_worldModel._commandOverride = false;
            c.Parameters["result"] = result;
            await m_worldModel.RunScriptAsync(m_callbackScript, c);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { m_worldModel.LogException(ex); }
        finally
        {
            await m_worldModel.EndPendingCallbackAsync();
            m_worldModel.SignalTurnSuspended();
        }
    }

    public override string Save()
    {
        return SaveScript("get input", m_callbackScript);
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_callbackScript;
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