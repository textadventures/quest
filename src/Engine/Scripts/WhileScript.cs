#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class WhileScriptConstructor : IScriptConstructor
{
    public string Keyword => "while";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string afterExpr;
        var param = Utility.GetParameter(script, out afterExpr);
        var loop = Utility.GetScript(afterExpr);
        var loopScript = ScriptFactory.CreateScript(loop);

        return new WhileScript(scriptContext, ScriptFactory, new Expression<bool>(param, scriptContext), loopScript);
    }

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }
}

public class WhileScript : ScriptBase
{
    private readonly IScript m_loopScript;
    private readonly ScriptContext m_scriptContext;
    private readonly IScriptFactory m_scriptFactory;
    private IFunction<bool> m_expression;
    private WorldModel m_worldModel;

    public WhileScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IFunction<bool> expression,
        IScript loopScript)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_scriptFactory = scriptFactory;
        m_expression = expression;
        m_loopScript = loopScript;
    }

    public override string Keyword => "while";

    protected override ScriptBase CloneScript()
    {
        return new WhileScript(m_scriptContext, m_scriptFactory, m_expression.Clone(), (IScript) m_loopScript.Clone());
    }

    protected override void ParentUpdated()
    {
        m_loopScript.Parent = Parent;
    }

    public override void Execute(Context c)
    {
        while (m_expression.Execute(c))
        {
            m_loopScript.Execute(c);
            if (c.IsReturned)
            {
                break;
            }
        }
    }

    public override async Task ExecuteAsync(Context c)
    {
        while (m_expression.Execute(c))
        {
            await m_loopScript.ExecuteAsync(c);
            if (c.IsReturned)
            {
                break;
            }
        }
    }

    public override string Save()
    {
        return SaveScript("while", m_loopScript, m_expression.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_expression.Save();
            case 1:
                return m_loopScript;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                m_expression = new Expression<bool>((string) value, m_scriptContext);
                break;
            case 1:
                // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'while' loop");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}