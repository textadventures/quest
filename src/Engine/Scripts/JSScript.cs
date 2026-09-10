#nullable disable
using System.Text.RegularExpressions;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class JSScriptConstructor : IScriptConstructor
{
    private static readonly Regex JsFunctionName = new(@"^JS\.([\w\.\@]*)");
    public string Keyword => "JS.";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        var param = Utility.GetParameter(script);

        List<IFunctionDynamic> expressions = null;

        if (param != null)
        {
            var parameters = Utility.SplitParameter(param);
            if (parameters.Count != 1 || parameters[0].Trim().Length != 0)
            {
                expressions =
                    new List<IFunctionDynamic>(parameters.Select(p => new ExpressionDynamic(p, scriptContext)));
            }
        }

        if (!JsFunctionName.IsMatch(script))
        {
            throw new Exception(string.Format("Invalid JS function name in '{0}'", script));
        }

        var functionName = JsFunctionName.Match(script).Groups[1].Value;

        return new JSScript(scriptContext, functionName, expressions);
    }

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }
}

public class JSScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private string _function;
    private List<IFunctionDynamic> _parameters;

    public JSScript(ScriptContext scriptContext, string function, List<IFunctionDynamic> parameters)
    {
        _scriptContext = scriptContext;
        _function = function;
        _parameters = parameters;
    }

    public override string Keyword => "JS.";

    protected override ScriptBase CloneScript()
    {
        return new JSScript(_scriptContext, _function,
            _parameters == null ? null : new List<IFunctionDynamic>(_parameters));
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (string.IsNullOrEmpty(_function))
        {
            return;
        }

        if (_parameters != null)
        {
            var paramValues = new object[_parameters.Count];
            for (var i = 0; i < _parameters.Count; i++)
                paramValues[i] = await _parameters[i].ExecuteAsync(c);
            await _scriptContext.WorldModel.PlayerUi.RunScriptAsync(_function, paramValues);
        }
        else
        {
            await _scriptContext.WorldModel.PlayerUi.RunScriptAsync(_function, null);
        }
    }

    public override string Save()
    {
        if (string.IsNullOrEmpty(_function))
        {
            return "JS.";
        }

        return SaveScript("JS." + _function,
            _parameters == null ? new[] {string.Empty} : _parameters.Select(p => p.Save()).ToArray());
    }

    protected override void SetParameterInternal(int index, object value)
    {
        var constuctor = new JSScriptConstructor();
        try
        {
            var newScript = (JSScript) constuctor.Create("JS." + (string) value, _scriptContext);
            _function = newScript._function;
            _parameters = newScript._parameters;
        }
        catch
        {
            // simply ignore any invalid input
        }
    }

    public override object GetParameter(int index)
    {
        return Save().Substring(3);
    }
}