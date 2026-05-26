#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class AskScriptConstructor : IScriptConstructor
{
    public string Keyword => "ask";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string afterExpr;
        var param = Utility.GetParameter(script, out afterExpr);
        var callback = Utility.GetScript(afterExpr);

        var parameters = Utility.SplitParameter(param).ToArray();
        if (parameters.Count() != 1)
        {
            throw new Exception(string.Format("'ask' script should have 1 parameter: 'ask ({0})'", param));
        }

        var callbackScript = ScriptFactory.CreateScript(callback);

        return new AskScript(scriptContext, ScriptFactory, new Expression<string>(parameters[0], scriptContext),
            callbackScript);
    }

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }
}

public class AskScript : ScriptBase
{
    private readonly IScript m_callbackScript;
    private readonly ScriptContext m_scriptContext;
    private readonly IScriptFactory m_scriptFactory;
    private readonly WorldModel m_worldModel;
    private IFunction<string> m_caption;

    public AskScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IFunction<string> caption,
        IScript callbackScript)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_scriptFactory = scriptFactory;
        m_caption = caption;
        m_callbackScript = callbackScript;
    }

    public override string Keyword => "ask";

    protected override ScriptBase CloneScript()
    {
        return new AskScript(m_scriptContext, m_scriptFactory, m_caption.Clone(), (IScript) m_callbackScript.Clone());
    }

    public override void Execute(Context c)
    {
        m_worldModel.ShowQuestionAsync(m_caption.Execute(c), m_callbackScript, c);
    }

    public override string Save()
    {
        return SaveScript("ask", m_callbackScript, m_caption.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_caption.Save();
            case 1:
                return m_callbackScript;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                m_caption = new Expression<string>((string) value, m_scriptContext);
                break;
            case 1:
                // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of an 'ask' command");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}