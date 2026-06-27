#nullable disable
using System.Collections;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class ForEachScriptConstructor : IScriptConstructor
{
    public string Keyword => "foreach";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string afterExpr;
        var param = Utility.GetParameter(script, out afterExpr);
        var loop = Utility.GetScript(afterExpr);

        var parameters = Utility.SplitParameter(param).ToArray();
        if (parameters.Count() != 2)
        {
            throw new Exception(string.Format("'foreach' script should have 2 parameters: 'foreach ({0})'", param));
        }

        var loopScript = ScriptFactory.CreateScript(loop);

        return new ForEachScript(scriptContext, parameters[0], new ExpressionDynamic(parameters[1], scriptContext),
            loopScript);
    }

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }
}

public class ForEachScript : ScriptBase
{
    private readonly IScript m_loopScript;
    private readonly ScriptContext m_scriptContext;
    private IFunctionDynamic m_list;
    private string m_variable;

    public ForEachScript(ScriptContext scriptContext, string variable, IFunctionDynamic list, IScript loopScript)
    {
        m_scriptContext = scriptContext;
        m_variable = variable;
        m_list = list;
        m_loopScript = loopScript;
    }

    public override string Keyword => "foreach";

    protected override ScriptBase CloneScript()
    {
        return new ForEachScript(m_scriptContext, m_variable, m_list.Clone(), (IScript) m_loopScript.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await m_list.ExecuteAsync(c);
        IEnumerable resultList = null;

        if (m_scriptContext.WorldModel.Version < WorldModelVersion.v530 || !(result is string))
        {
            var resultDictionary = result as IDictionary;
            if (resultDictionary != null)
            {
                resultList = resultDictionary.Keys;
            }
            else
            {
                resultList = result as IEnumerable;
            }
        }

        if (resultList == null)
        {
            throw new Exception(string.Format("Cannot foreach over '{0}' as it is not a list", result));
        }

        foreach (var variable in resultList)
        {
            c.Parameters[m_variable] = variable;
            await m_loopScript.ExecuteAsync(c);
            if (c.IsReturned)
            {
                break;
            }
        }
    }

    public override string Save()
    {
        return SaveScript("foreach", m_loopScript, m_variable, m_list.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_variable;
            case 1:
                return m_list.Save();
            case 2:
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
                m_variable = (string) value;
                break;
            case 1:
                m_list = new ExpressionDynamic((string) value, m_scriptContext);
                break;
            case 2:
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'foreach' loop");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}