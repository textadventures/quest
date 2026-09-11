using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class ListAddScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "list add";

    protected override int[] ExpectedParameters => [2];

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new ListAddScript(scriptContext,
            new ExpressionDynamic(parameters[0], scriptContext),
            new Expression<object>(parameters[1], scriptContext));
    }
}

public class ListAddScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private IFunctionDynamic _list;
    private IFunction<object> _value;
    private WorldModel _worldModel;

    public ListAddScript(ScriptContext scriptContext, IFunctionDynamic list, IFunction<object> value)
    {
        _scriptContext = scriptContext;
        _list = list;
        _value = value;
        _worldModel = scriptContext.WorldModel;
    }

    public override string Keyword => "list add";

    protected override ScriptBase CloneScript()
    {
        return new ListAddScript(_scriptContext, _list.Clone(), _value.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (await _list.ExecuteAsync(c) is IQuestList result)
        {
            result.Add(await _value.ExecuteAsync(c));
        }
        else
        {
            throw new Exception("Unrecognised list type");
        }
    }

    public override string Save()
    {
        return SaveScript("list add", _list.Save(), _value.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _list.Save();
            case 1:
                return _value.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _list = new ExpressionDynamic((string) value!, _scriptContext);
                break;
            case 1:
                _value = new Expression<object>((string) value!, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class ListRemoveScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "list remove";

    protected override int[] ExpectedParameters => [2];

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new ListRemoveScript(scriptContext,
            new ExpressionDynamic(parameters[0], scriptContext),
            new Expression<object>(parameters[1], scriptContext));
    }
}

public class ListRemoveScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private IFunctionDynamic _list;
    private IFunction<object> _value;
    private WorldModel _worldModel;

    public ListRemoveScript(ScriptContext scriptContext, IFunctionDynamic list, IFunction<object> value)
    {
        _scriptContext = scriptContext;
        _list = list;
        _value = value;
        _worldModel = scriptContext.WorldModel;
    }

    public override string Keyword => "list remove";

    protected override ScriptBase CloneScript()
    {
        return new ListRemoveScript(_scriptContext, _list.Clone(), _value.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (await _list.ExecuteAsync(c) is IQuestList result)
        {
            result.Remove(await _value.ExecuteAsync(c));
        }
        else
        {
            throw new Exception("Unrecognised list type");
        }
    }

    public override string Save()
    {
        return SaveScript("list remove", _list.Save(), _value.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _list.Save();
            case 1:
                return _value.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _list = new ExpressionDynamic((string) value!, _scriptContext);
                break;
            case 1:
                _value = new Expression<object>((string) value!, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}