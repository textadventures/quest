#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class PlaySoundScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "play sound";

    protected override int[] ExpectedParameters
    {
        get { return new[] {3}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new PlaySoundScript(scriptContext,
            new Expression<string>(parameters[0], scriptContext),
            new Expression<bool>(parameters[1], scriptContext),
            new Expression<bool>(parameters[2], scriptContext));
    }
}

public class PlaySoundScript : ScriptBase
{
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunction<string> m_filename;
    private IFunction<bool> m_loop;
    private IFunction<bool> m_synchronous;

    public PlaySoundScript(ScriptContext scriptContext, IFunction<string> function, IFunction<bool> synchronous,
        IFunction<bool> loop)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_filename = function;
        m_synchronous = synchronous;
        m_loop = loop;
    }

    public override string Keyword => "play sound";

    protected override ScriptBase CloneScript()
    {
        return new PlaySoundScript(m_scriptContext, m_filename.Clone(), m_synchronous.Clone(), m_loop.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var filename = await m_filename.ExecuteAsync(c);
        var synchronous = await m_synchronous.ExecuteAsync(c);
        var loop = await m_loop.ExecuteAsync(c);

        if (synchronous)
        {
            m_worldModel._waitTcs = new TaskCompletionSource();
            await m_worldModel.PlayerUi.PlaySoundAsync(filename, true, loop);
            m_worldModel.SignalTurnSuspended();
            await m_worldModel._waitTcs.Task;
        }
        else
        {
            await m_worldModel.PlayerUi.PlaySoundAsync(filename, false, loop);
        }
    }

    public override string Save()
    {
        return SaveScript("play sound", m_filename.Save(), m_synchronous.Save(), m_loop.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_filename.Save();
            case 1:
                return m_synchronous.Save();
            case 2:
                return m_loop.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                m_filename = new Expression<string>((string) value, m_scriptContext);
                break;
            case 1:
                m_synchronous = new Expression<bool>((string) value, m_scriptContext);
                break;
            case 2:
                m_loop = new Expression<bool>((string) value, m_scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class StopSoundScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "stop sound";

    protected override int[] ExpectedParameters
    {
        get { return new[] {0}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new StopSoundScript(WorldModel);
    }
}

public class StopSoundScript : ScriptBase
{
    private readonly WorldModel m_worldModel;

    public StopSoundScript(WorldModel worldModel)
    {
        m_worldModel = worldModel;
    }

    public override string Keyword => "stop sound";

    protected override ScriptBase CloneScript()
    {
        return new StopSoundScript(m_worldModel);
    }

    public override Task ExecuteAsync(Context c)
    {
        m_worldModel.PlayerUi.StopSound();
        return Task.CompletedTask;
    }

    public override string Save()
    {
        return "stop sound";
    }

    public override object GetParameter(int index)
    {
        throw new ArgumentOutOfRangeException();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        throw new ArgumentOutOfRangeException();
    }
}