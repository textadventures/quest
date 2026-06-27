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
    private readonly ScriptContext m_scriptContext;
    private IFunction<string> m_field;
    private IFunction<Element> m_obj;
    private IFunction<object> m_value;
    private WorldModel m_worldModel;

    public SetFieldScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> field,
        IFunction<object> value)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_obj = obj;
        m_field = field;
        m_value = value;
    }

    public override string Keyword => "set";

    protected override ScriptBase CloneScript()
    {
        return new SetFieldScript(m_scriptContext, m_obj.Clone(), m_field.Clone(), m_value.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var obj = m_obj.Execute(c);
        await obj.SetFieldAsync(m_field.Execute(c), m_value.Execute(c));
    }

    public override string Save()
    {
        return SaveScript("set", m_obj.Save(), m_field.Save(), m_value.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_obj.Save();
            case 1:
                return m_field.Save();
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
                m_obj = new Expression<Element>((string) value, m_scriptContext);
                break;
            case 1:
                m_field = new Expression<string>((string) value, m_scriptContext);
                break;
            case 2:
                m_value = new Expression<object>((string) value, m_scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}