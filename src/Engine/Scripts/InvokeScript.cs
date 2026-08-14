#nullable disable
using System.Collections;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class InvokeScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "invoke";

    protected override int[] ExpectedParameters
    {
        get { return new[] {1, 2}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        switch (parameters.Count)
        {
            case 1:
                return new InvokeScript(scriptContext, new Expression<IScript>(parameters[0], scriptContext));
            case 2:
                return new InvokeScript(scriptContext, new Expression<IScript>(parameters[0], scriptContext),
                    new Expression<IDictionary>(parameters[1], scriptContext));
        }

        return null;
    }
}

public class InvokeScript : ScriptBase
{
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunction<IDictionary> m_parameters;
    private IFunction<IScript> m_script;

    public InvokeScript(ScriptContext scriptContext, IFunction<IScript> script)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_script = script;
    }

    public InvokeScript(ScriptContext scriptContext, IFunction<IScript> script, IFunction<IDictionary> parameters)
        : this(scriptContext, script)
    {
        m_parameters = parameters;
    }

    public override string Keyword => "invoke";

    protected override ScriptBase CloneScript()
    {
        return new InvokeScript(m_scriptContext, m_script.Clone(), m_parameters == null ? null : m_parameters.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var script = await m_script.ExecuteAsync(c);
        if (m_parameters == null)
        {
            await m_worldModel.RunScriptAsync(script);
        }
        else
        {
            await m_worldModel.RunScriptAsync(script, new Parameters(await m_parameters.ExecuteAsync(c)));
        }
    }

    public override string Save()
    {
        var parameters = m_parameters == null ? null : m_parameters.Save();
        if (string.IsNullOrEmpty(parameters))
        {
            return SaveScript("invoke", m_script.Save());
        }

        return SaveScript("invoke", m_script.Save(), parameters);
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_script.Save();
            case 1:
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
                m_script = new Expression<IScript>((string) value, m_scriptContext);
                break;
            case 1:
                m_parameters = new Expression<IDictionary>((string) value, m_scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}