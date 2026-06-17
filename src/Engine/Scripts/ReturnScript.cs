#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class ReturnScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "return";

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new ReturnScript(scriptContext, new ExpressionDynamic(parameters[0], scriptContext));
    }
}

public class ReturnScript : ScriptBase
{
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunctionDynamic m_returnValue;

    public ReturnScript(ScriptContext scriptContext, IFunctionDynamic returnValue)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_returnValue = returnValue;
    }

    public override string Keyword => "return";

    protected override ScriptBase CloneScript()
    {
        return new ReturnScript(m_scriptContext, m_returnValue.Clone());
    }

    public override void Execute(Context c)
    {
        c.ReturnValue = m_returnValue.Execute(c);
        // Leaving this set to v550 for backwards compatibility
        // Some things do not work in 550 games when this is changed to 580
        if (m_worldModel.Version >= WorldModelVersion.v550)
        {
            c.IsReturned = true;
        }
    }

    public override string Save()
    {
        return SaveScript("return", m_returnValue.Save());
    }

    public override object GetParameter(int index)
    {
        return m_returnValue.Save();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        m_returnValue = new ExpressionDynamic((string) value, m_scriptContext);
    }
}