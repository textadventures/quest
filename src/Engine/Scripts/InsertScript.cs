using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class InsertScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "insert";

    protected override int[] ExpectedParameters => [1];

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new InsertScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class InsertScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _filename;

    public InsertScript(ScriptContext scriptContext, IFunction<string> filename)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _filename = filename;
    }

    public override string Keyword => "insert";

    protected override ScriptBase CloneScript()
    {
        return new InsertScript(_scriptContext, _filename.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (_worldModel.Version >= WorldModelVersion.v540)
        {
            throw new Exception(
                "The 'insert' script command is not supported for games with WorldModel version 540 or later. You can output HTML directly using the 'msg' command instead.");
        }

        var filename = await _filename.ExecuteAsync(c);
        if (_worldModel.Version == WorldModelVersion.v500)
        {
            // v500 games used Frame.htm for static panel feature. This is now implemented natively
            // in Player and WebPlayer.
            if (filename.ToLower() == "frame.htm")
            {
                return;
            }
        }

        var stream = _worldModel.GetResourceStream(filename);
        if (stream == null)
        {
            return;
        }

        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();
        _worldModel.PlayerUi.WriteHTML(html);
    }

    public override string Save()
    {
        return SaveScript("insert", _filename.Save());
    }

    public override object? GetParameter(int index)
    {
        return _filename.Save();
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        _filename = new Expression<string>((string) value!, _scriptContext);
    }
}