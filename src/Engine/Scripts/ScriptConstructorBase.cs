#nullable disable
namespace QuestViva.Engine.Scripts;

public abstract class ScriptConstructorBase : IScriptConstructor
{
    protected abstract int[] ExpectedParameters { get; }

    protected abstract IScript CreateInt(List<string> parameters, ScriptContext scriptContext);

    private string FormatExpectedParameters()
    {
        var result = "";
        foreach (var i in ExpectedParameters)
        {
            result += i + ",";
        }

        return result.Substring(0, result.Length - 1);
    }

    #region IScriptConstructor Members

    public abstract string Keyword { get; }

    public IScript Create(string script, ScriptContext scriptContext)
    {
        List<string> parameters = null;
        var param = Utility.GetParameter(script);

        int numParams;
        if (param == null)
        {
            numParams = 0;
        }
        else
        {
            parameters = Utility.SplitParameter(param);
            numParams = parameters.Count;
        }

        if (ExpectedParameters.Count() > 0)
        {
            if (!ExpectedParameters.Contains(numParams))
            {
                throw new Exception(string.Format("Expected {0} parameter(s) in script '{1}'",
                    FormatExpectedParameters(), script));
            }
        }

        return CreateInt(parameters, scriptContext);
    }

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }

    #endregion
}