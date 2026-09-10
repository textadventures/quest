using System.Collections;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class DictionaryAddScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "dictionary add";

    protected override int[] ExpectedParameters
    {
        get { return new[] {3}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new DictionaryAddScript(scriptContext,
            new ExpressionDynamic(parameters[0], scriptContext),
            new Expression<string>(parameters[1], scriptContext),
            new Expression<object>(parameters[2], scriptContext));
    }
}

public class DictionaryAddScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private IFunctionDynamic _dictionary;
    private IFunction<string> _key;
    private IFunction<object> _value;
    private WorldModel _worldModel;

    public DictionaryAddScript(ScriptContext scriptContext, IFunctionDynamic dictionary, IFunction<string> key,
        IFunction<object> value)
    {
        _scriptContext = scriptContext;
        _dictionary = dictionary;
        _key = key;
        _value = value;
        _worldModel = scriptContext.WorldModel;
    }

    public override string Keyword => "dictionary add";

    protected override ScriptBase CloneScript()
    {
        return new DictionaryAddScript(_scriptContext, _dictionary.Clone(), _key.Clone(), _value.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await _dictionary.ExecuteAsync(c) as IDictionary;

        if (result != null)
        {
            result.Add(await _key.ExecuteAsync(c), await _value.ExecuteAsync(c));
        }
        else
        {
            throw new Exception("Unrecognised dictionary type");
        }
    }

    public override string Save()
    {
        return SaveScript("dictionary add", _dictionary.Save(), _key.Save(), _value.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _dictionary.Save();
            case 1:
                return _key.Save();
            case 2:
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
                _dictionary = new ExpressionDynamic((string) value!, _scriptContext);
                break;
            case 1:
                _key = new Expression<string>((string) value!, _scriptContext);
                break;
            case 2:
                _value = new Expression<object>((string) value!, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class DictionaryRemoveScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "dictionary remove";

    protected override int[] ExpectedParameters
    {
        get { return new[] {2}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new DictionaryRemoveScript(scriptContext,
            new ExpressionDynamic(parameters[0], scriptContext),
            new Expression<string>(parameters[1], scriptContext));
    }
}

public class DictionaryRemoveScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private IFunctionDynamic _dictionary;
    private IFunction<string> _key;
    private WorldModel _worldModel;

    public DictionaryRemoveScript(ScriptContext scriptContext, IFunctionDynamic dictionary, IFunction<string> key)
    {
        _scriptContext = scriptContext;
        _dictionary = dictionary;
        _key = key;
        _worldModel = scriptContext.WorldModel;
    }

    public override string Keyword => "dictionary remove";

    protected override ScriptBase CloneScript()
    {
        return new DictionaryRemoveScript(_scriptContext, _dictionary.Clone(), _key.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await _dictionary.ExecuteAsync(c) as IDictionary;

        if (result != null)
        {
            result.Remove(await _key.ExecuteAsync(c));
        }
        else
        {
            throw new Exception("Unrecognised dictionary type");
        }
    }

    public override string Save()
    {
        return SaveScript("dictionary remove", _dictionary.Save(), _key.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _dictionary.Save();
            case 1:
                return _key.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _dictionary = new ExpressionDynamic((string) value!, _scriptContext);
                break;
            case 1:
                _key = new Expression<string>((string) value!, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}