using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class DestroyScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "destroy";

    protected override int[] ExpectedParameters => [1];

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new DestroyScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class DestroyScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _expr;

    public DestroyScript(ScriptContext scriptContext, IFunction<string> expr)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _expr = expr;
    }

    public override string Keyword => "destroy";

    protected override ScriptBase CloneScript()
    {
        return new DestroyScript(_scriptContext, _expr.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var elementName = await _expr.ExecuteAsync(c);
        var element = _worldModel.Elements.Get(elementName);
        if (element.ElemType == ElementType.Object || element.ElemType == ElementType.Timer)
        {
            _worldModel.GetElementFactory(element.ElemType).DestroyElement(elementName);
        }
        else
        {
            throw new InvalidOperationException(
                string.Format("Unable to destroy element of type {0}", element.ElemType));
        }
    }

    public override string Save()
    {
        return SaveScript("destroy", _expr.Save());
    }

    public override object? GetParameter(int index)
    {
        return _expr.Save();
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        _expr = new Expression<string>((string) value!, _scriptContext);
    }
}