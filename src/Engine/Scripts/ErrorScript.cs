using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class ErrorScriptConstructor : ScriptConstructorBase
{
    #region ScriptConstructorBase Members

    public override string Keyword => "error";

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new ErrorScript(scriptContext, new ExpressionDynamic(parameters[0], scriptContext));
    }

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    #endregion
}

public class ErrorScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private IFunctionDynamic _function;
    private WorldModel _worldModel;

    public ErrorScript(ScriptContext scriptContext, IFunctionDynamic function)
    {
        _scriptContext = scriptContext;
        _function = function;
        _worldModel = scriptContext.WorldModel;
    }

    public override string Keyword => "error";

    protected override ScriptBase CloneScript()
    {
        return new ErrorScript(_scriptContext, _function.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await _function.ExecuteAsync(c);
        throw new Exception(Utility.ExpressionResultToString(result));
    }

    public override string Save()
    {
        return SaveScript("error", _function.Save());
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