#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class PictureScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "picture";

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new PictureScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class PictureScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _filename;

    public PictureScript(ScriptContext scriptContext, IFunction<string> function)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _filename = function;
    }

    public override string Keyword => "picture";

    protected override ScriptBase CloneScript()
    {
        return new PictureScript(_scriptContext, _filename.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var filename = await _filename.ExecuteAsync(c);

        if (_worldModel.Version >= WorldModelVersion.v540)
        {
            await _worldModel.PrintAsync("<img src=\"" + await _worldModel.GetExternalUrlAsync(filename) + "\" />");
        }
        else
        {
            await _worldModel.PlayerUi.ShowPictureAsync(filename);
            ((LegacyOutputLogger) _worldModel.OutputLogger).AddPicture(filename);
        }
    }

    public override string Save()
    {
        return SaveScript("picture", _filename.Save());
    }

    public override object GetParameter(int index)
    {
        return _filename.Save();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        _filename = new Expression<string>((string) value, _scriptContext);
    }
}