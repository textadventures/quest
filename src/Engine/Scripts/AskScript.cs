using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class AskScriptConstructor : IScriptConstructor
{
    public string Keyword => "ask";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        var param = Utility.GetParameter(script, out var afterExpr);
        var callback = Utility.GetScript(afterExpr);

        var parameters = Utility.SplitParameter(param).ToArray();
        if (parameters.Count() != 1)
        {
            throw new Exception($"'ask' script should have 1 parameter: 'ask ({param})'");
        }

        var callbackScript = ScriptFactory.CreateScript(callback);

        return new AskScript(scriptContext, ScriptFactory, new Expression<string>(parameters[0], scriptContext),
            callbackScript);
    }

    public required IScriptFactory ScriptFactory { get; set; }

    public required WorldModel WorldModel { get; set; }
}

public class AskScript(
    ScriptContext scriptContext,
    IScriptFactory scriptFactory,
    IFunction<string> caption,
    IScript callbackScript)
    : ScriptBase
{
    private readonly WorldModel _worldModel = scriptContext.WorldModel;
    private IFunction<string> _caption = caption;

    public override string Keyword => "ask";

    protected override ScriptBase CloneScript()
    {
        return new AskScript(scriptContext, scriptFactory, _caption.Clone(), (IScript) callbackScript.Clone());
    }

    public override void Execute(Context c)
    {
        _worldModel.ShowQuestionAsync(_caption.Execute(c), callbackScript, c);
    }

    public override string Save()
    {
        return SaveScript("ask", callbackScript, _caption.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _caption.Save();
            case 1:
                return callbackScript;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void SetParameterInternal(int index, object value)
    {
        _caption = index switch
        {
            0 => new Expression<string>((string) value, scriptContext),
            1 =>
                // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of an 'ask' command"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}