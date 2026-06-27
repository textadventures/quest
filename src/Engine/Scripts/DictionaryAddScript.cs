#nullable disable
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
    private readonly ScriptContext m_scriptContext;
    private IFunctionDynamic m_dictionary;
    private IFunction<string> m_key;
    private IFunction<object> m_value;
    private WorldModel m_worldModel;

    public DictionaryAddScript(ScriptContext scriptContext, IFunctionDynamic dictionary, IFunction<string> key,
        IFunction<object> value)
    {
        m_scriptContext = scriptContext;
        m_dictionary = dictionary;
        m_key = key;
        m_value = value;
        m_worldModel = scriptContext.WorldModel;
    }

    public override string Keyword => "dictionary add";

    protected override ScriptBase CloneScript()
    {
        return new DictionaryAddScript(m_scriptContext, m_dictionary.Clone(), m_key.Clone(), m_value.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await m_dictionary.ExecuteAsync(c) as IDictionary;

        if (result != null)
        {
            result.Add(await m_key.ExecuteAsync(c), await m_value.ExecuteAsync(c));
        }
        else
        {
            throw new Exception("Unrecognised dictionary type");
        }
    }

    public override string Save()
    {
        return SaveScript("dictionary add", m_dictionary.Save(), m_key.Save(), m_value.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_dictionary.Save();
            case 1:
                return m_key.Save();
            case 2:
                return m_value.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                m_dictionary = new ExpressionDynamic((string) value, m_scriptContext);
                break;
            case 1:
                m_key = new Expression<string>((string) value, m_scriptContext);
                break;
            case 2:
                m_value = new Expression<object>((string) value, m_scriptContext);
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
    private readonly ScriptContext m_scriptContext;
    private IFunctionDynamic m_dictionary;
    private IFunction<string> m_key;
    private WorldModel m_worldModel;

    public DictionaryRemoveScript(ScriptContext scriptContext, IFunctionDynamic dictionary, IFunction<string> key)
    {
        m_scriptContext = scriptContext;
        m_dictionary = dictionary;
        m_key = key;
        m_worldModel = scriptContext.WorldModel;
    }

    public override string Keyword => "dictionary remove";

    protected override ScriptBase CloneScript()
    {
        return new DictionaryRemoveScript(m_scriptContext, m_dictionary.Clone(), m_key.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await m_dictionary.ExecuteAsync(c) as IDictionary;

        if (result != null)
        {
            result.Remove(await m_key.ExecuteAsync(c));
        }
        else
        {
            throw new Exception("Unrecognised dictionary type");
        }
    }

    public override string Save()
    {
        return SaveScript("dictionary remove", m_dictionary.Save(), m_key.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_dictionary.Save();
            case 1:
                return m_key.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                m_dictionary = new ExpressionDynamic((string) value, m_scriptContext);
                break;
            case 1:
                m_key = new Expression<string>((string) value, m_scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}