using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class ReturnScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "return";

    protected override int[] ExpectedParameters => [1];

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new ReturnScript(scriptContext, new ExpressionDynamic(parameters[0], scriptContext));
    }
}

public class ReturnScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunctionDynamic _returnValue;

    public ReturnScript(ScriptContext scriptContext, IFunctionDynamic returnValue)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _returnValue = returnValue;
    }

    public override string Keyword => "return";

    protected override ScriptBase CloneScript()
    {
        return new ReturnScript(_scriptContext, _returnValue.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        c.ReturnValue = await _returnValue.ExecuteAsync(c);
        // Leaving this set to v550 for backwards compatibility
        // Some things do not work in 550 games when this is changed to 580
        if (_worldModel.Version >= WorldModelVersion.v550)
        {
            c.IsReturned = true;
        }
    }

    public override string Save()
    {
        return SaveScript("return", _returnValue.Save());
    }

    public override object? GetParameter(int index)
    {
        return _returnValue.Save();
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        _returnValue = new ExpressionDynamic((string) value!, _scriptContext);
    }
}