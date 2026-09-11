using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class PlaySoundScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "play sound";

    protected override int[] ExpectedParameters => [3];

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
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _filename;
    private IFunction<bool> _loop;
    private IFunction<bool> _synchronous;

    public PlaySoundScript(ScriptContext scriptContext, IFunction<string> function, IFunction<bool> synchronous,
        IFunction<bool> loop)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _filename = function;
        _synchronous = synchronous;
        _loop = loop;
    }

    public override string Keyword => "play sound";

    protected override ScriptBase CloneScript()
    {
        return new PlaySoundScript(_scriptContext, _filename.Clone(), _synchronous.Clone(), _loop.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var filename = await _filename.ExecuteAsync(c);
        var synchronous = await _synchronous.ExecuteAsync(c);
        var loop = await _loop.ExecuteAsync(c);

        if (synchronous && loop)
        {
            throw new Exception("play sound: cannot use 'wait' and 'loop' together - this would wait forever for a sound that never finishes");
        }

        if (synchronous)
        {
            var tcs = WorldModel.BeginPrompt(ref _worldModel._waitTcs);
            await _worldModel.PlayerUi.PlaySoundAsync(filename, true, loop);
            _worldModel.BeginPendingCallback();
            _worldModel.SignalTurnSuspended();
            try
            {
                await tcs.Task;
            }
            catch (OperationCanceledException) { }
            finally
            {
                await _worldModel.EndPendingCallbackAsync();
                _worldModel.SignalTurnSuspended();
            }
        }
        else
        {
            await _worldModel.PlayerUi.PlaySoundAsync(filename, false, loop);
        }
    }

    public override string Save()
    {
        return SaveScript("play sound", _filename.Save(), _synchronous.Save(), _loop.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _filename.Save();
            case 1:
                return _synchronous.Save();
            case 2:
                return _loop.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _filename = new Expression<string>((string) value!, _scriptContext);
                break;
            case 1:
                _synchronous = new Expression<bool>((string) value!, _scriptContext);
                break;
            case 2:
                _loop = new Expression<bool>((string) value!, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class StopSoundScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "stop sound";

    protected override int[] ExpectedParameters => [0];

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new StopSoundScript(WorldModel);
    }
}

public class StopSoundScript : ScriptBase
{
    private readonly WorldModel _worldModel;

    public StopSoundScript(WorldModel worldModel)
    {
        _worldModel = worldModel;
    }

    public override string Keyword => "stop sound";

    protected override ScriptBase CloneScript()
    {
        return new StopSoundScript(_worldModel);
    }

    public override Task ExecuteAsync(Context c)
    {
        _worldModel.PlayerUi.StopSound();
        return Task.CompletedTask;
    }

    public override string Save()
    {
        return "stop sound";
    }

    public override object? GetParameter(int index)
    {
        throw new ArgumentOutOfRangeException();
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        throw new ArgumentOutOfRangeException();
    }
}