#nullable disable
using QuestViva.Engine.Functions;

/*
 * This script command is an alternative to request (Speak, "some text"), and is added as part of deprecating
 * request. It is called requestspeak to replace RequestSpeak (an earlier step to deprecation), and
 * to hopefully avoid name clashes in existing games.
 */

namespace QuestViva.Engine.Scripts;

public class RequestSpeakScriptConstructor : ScriptConstructorBase
{
    #region ScriptConstructorBase Members

    public override string Keyword => "requestspeak";

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new RequestSpeakScript(scriptContext, new ExpressionDynamic(parameters[0], scriptContext));
    }

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    #endregion
}

public class RequestSpeakScript : ScriptBase
{
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunctionDynamic m_function;

    public RequestSpeakScript(ScriptContext scriptContext, IFunctionDynamic function)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_function = function;
    }

    public override string Keyword => "requestspeak";

    protected override ScriptBase CloneScript()
    {
        return new RequestSpeakScript(m_scriptContext, m_function.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await m_function.ExecuteAsync(c);
        m_worldModel.PlayerUi.Speak(result.ToString());
    }

    public override string Save()
    {
        return SaveScript("requestspeak", m_function.Save());
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