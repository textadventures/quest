#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class DestroyScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "destroy";

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new DestroyScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class DestroyScript : ScriptBase
{
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunction<string> m_expr;

    public DestroyScript(ScriptContext scriptContext, IFunction<string> expr)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_expr = expr;
    }

    public override string Keyword => "destroy";

    protected override ScriptBase CloneScript()
    {
        return new DestroyScript(m_scriptContext, m_expr.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var elementName = await m_expr.ExecuteAsync(c);
        var element = m_worldModel.Elements.Get(elementName);
        if (element.ElemType == ElementType.Object || element.ElemType == ElementType.Timer)
        {
            m_worldModel.GetElementFactory(element.ElemType).DestroyElement(elementName);
        }
        else
        {
            throw new InvalidOperationException(
                string.Format("Unable to destroy element of type {0}", element.ElemType));
        }
    }

    public override string Save()
    {
        return SaveScript("destroy", m_expr.Save());
    }

    public override object GetParameter(int index)
    {
        return m_expr.Save();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        m_expr = new Expression<string>((string) value, m_scriptContext);
    }
}