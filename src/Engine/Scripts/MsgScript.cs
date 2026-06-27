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
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunctionDynamic m_function;

    public MsgScript(ScriptContext scriptContext, IFunctionDynamic function)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_function = function;
    }

    public override string Keyword => "msg";

    protected override ScriptBase CloneScript()
    {
        return new MsgScript(m_scriptContext, m_function.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await m_function.ExecuteAsync(c);
        m_worldModel.Print(result.ToString());
    }

    public override string Save()
    {
        return SaveScript("msg", m_function.Save());
    }

    public override object GetParameter(int index)
    {
        return m_function.Save();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        m_function = new ExpressionDynamic((string) value, m_scriptContext);
    }
}