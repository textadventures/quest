#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class MsgScriptConstructor : ScriptConstructorBase
{
    #region ScriptConstructorBase Members

    public override string Keyword => "msg";

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new MsgScript(scriptContext, new ExpressionDynamic(parameters[0], scriptContext));
    }

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    #endregion
}

public class MsgScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunctionDynamic _function;

    public MsgScript(ScriptContext scriptContext, IFunctionDynamic function)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _function = function;
    }

    public override string Keyword => "msg";

    protected override ScriptBase CloneScript()
    {
        return new MsgScript(_scriptContext, _function.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await _function.ExecuteAsync(c);
        await _worldModel.PrintAsync(Utility.ExpressionResultToString(result));
    }

    public override string Save()
    {
        return SaveScript("msg", _function.Save());
    }

    public override object GetParameter(int index)
    {
        return _function.Save();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        _function = new ExpressionDynamic((string) value, _scriptContext);
    }
}