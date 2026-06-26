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
    private readonly IScript m_callbackScript;
    private readonly IScriptFactory m_scriptFactory;
    private readonly WorldModel m_worldModel;

    public WaitScript(WorldModel worldModel, IScriptFactory scriptFactory, IScript callbackScript)
    {
        m_worldModel = worldModel;
        m_scriptFactory = scriptFactory;
        m_callbackScript = callbackScript;
    }

    public override string Keyword => "wait";

    protected override ScriptBase CloneScript()
    {
        return new WaitScript(m_worldModel, m_scriptFactory, (IScript) m_callbackScript.Clone());
    }

    public override void Execute(Context c)
    {
        ExecuteAsync(c).GetAwaiter().GetResult();
    }

    public override async Task ExecuteAsync(Context c)
    {
        m_worldModel.PlayerUi.DoWait();
        m_worldModel._waitTcs = new TaskCompletionSource();
        m_worldModel.SignalTurnSuspended();
        await m_worldModel._waitTcs.Task;
        if (m_callbackScript != null)
            await m_worldModel.RunScriptAsync(m_callbackScript, c);
    }

    public override string Save()
    {
        return SaveScript("wait", m_callbackScript);
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
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'wait' command");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
