#nullable disable
using System.Collections;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class DoScriptConstructor : ScriptConstructorBase
{
    #region ScriptConstructorBase Members

    public override string Keyword => "do";

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        switch (parameters.Count)
        {
            case 2:
                return new DoActionScript(scriptContext, new Expression<Element>(parameters[0], scriptContext),
                    new Expression<string>(parameters[1], scriptContext));
            case 3:
                return new DoActionScript(scriptContext, new Expression<Element>(parameters[0], scriptContext),
                    new Expression<string>(parameters[1], scriptContext),
                    new Expression<IDictionary>(parameters[2], scriptContext));
        }

        return null;
    }

    protected override int[] ExpectedParameters
    {
        get { return new[] {2, 3}; }
    }

    #endregion
}

public class DoActionScript : ScriptBase
{
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunction<string> m_action;
    private IFunction<Element> m_obj;
    private IFunction<IDictionary> m_parameters;

    public DoActionScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> action)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_obj = obj;
        m_action = action;
    }

    public DoActionScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> action,
        IFunction<IDictionary> parameters)
        : this(scriptContext, obj, action)
    {
        m_parameters = parameters;
    }

    public override string Keyword => "do";

    protected override ScriptBase CloneScript()
    {
        return new DoActionScript(m_scriptContext, m_obj.Clone(), m_action.Clone(),
            m_parameters == null ? null : m_parameters.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var obj = await m_obj.ExecuteAsync(c);
        var action = obj.GetAction(await m_action.ExecuteAsync(c));
        if (m_parameters == null)
        {
            await m_worldModel.RunScriptAsync(action, obj);
        }
        else
        {
            await m_worldModel.RunScriptAsync(action, new Parameters(await m_parameters.ExecuteAsync(c)), obj);
        }
    }

    public override string Save()
    {
        var parameters = m_parameters == null ? null : m_parameters.Save();
        if (!string.IsNullOrEmpty(parameters))
        {
            return SaveScript("do", m_obj.Save(), m_action.Save(), m_parameters.Save());
        }

        return SaveScript("do", m_obj.Save(), m_action.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_obj.Save();
            case 1:
                return m_action.Save();
            case 2:
                return m_parameters == null ? null : m_parameters.Save();
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
                m_action = new Expression<string>((string) value, m_scriptContext);
                break;
            case 2:
                m_parameters = new Expression<IDictionary>((string) value, m_scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}