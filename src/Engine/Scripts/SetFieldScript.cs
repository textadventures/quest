#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class SetFieldScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "set";

    protected override int[] ExpectedParameters
    {
        get { return new[] {3}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new SetFieldScript(scriptContext, new Expression<Element>(parameters[0], scriptContext),
            new Expression<string>(parameters[1], scriptContext), new Expression<object>(parameters[2], scriptContext));
    }
}

public class SetFieldScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private IFunction<string> _field;
    private IFunction<Element> _obj;
    private IFunction<object> _value;
    private WorldModel _worldModel;

    public SetFieldScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> field,
        IFunction<object> value)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _obj = obj;
        _field = field;
        _value = value;
    }

    public override string Keyword => "set";

    protected override ScriptBase CloneScript()
    {
        return new SetFieldScript(_scriptContext, _obj.Clone(), _field.Clone(), _value.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var obj = await _obj.ExecuteAsync(c);
        await obj.SetFieldAsync(await _field.ExecuteAsync(c), await _value.ExecuteAsync(c));
    }

    public override string Save()
    {
        return SaveScript("set", _obj.Save(), _field.Save(), _value.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _obj.Save();
            case 1:
                return _field.Save();
            case 2:
                return _value.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                _obj = new Expression<Element>((string) value, _scriptContext);
                break;
            case 1:
                _field = new Expression<string>((string) value, _scriptContext);
                break;
            case 2:
                _value = new Expression<object>((string) value, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}