using QuestViva.Engine.Functions;

/*
 * This script command is an alternative to request (Speak, "some text"), and is added as part of deprecating
 * request. It is called requestspeak to replace RequestSpeak (an earlier step to deprecation), and
 * to hopefully avoid name clashes in existing games.
 */

namespace QuestViva.Engine.Scripts;

public class RequestSpeakScriptConstructor : ScriptConstructorBase
{
    #region ScriptConstructorBase Members

    public override string Keyword => "requestspeak";

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new RequestSpeakScript(scriptContext, new ExpressionDynamic(parameters[0], scriptContext));
    }

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    #endregion
}

public class RequestSpeakScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunctionDynamic _function;

    public RequestSpeakScript(ScriptContext scriptContext, IFunctionDynamic function)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _function = function;
    }

    public override string Keyword => "requestspeak";

    protected override ScriptBase CloneScript()
    {
        return new RequestSpeakScript(_scriptContext, _function.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await _function.ExecuteAsync(c);
        _worldModel.PlayerUi.Speak(Utility.ExpressionResultToString(result));
    }

    public override string Save()
    {
        return SaveScript("requestspeak", _function.Save());
    }

    public override object? GetParameter(int index)
    {
        return _function.Save();
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        _function = new ExpressionDynamic((string) value!, _scriptContext);
    }
}