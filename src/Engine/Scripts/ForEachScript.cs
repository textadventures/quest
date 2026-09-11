using System.Collections;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class ForEachScriptConstructor : IScriptConstructor
{
    public string Keyword => "foreach";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string? afterExpr;
        var param = Utility.GetParameter(script, out afterExpr);
        var loop = Utility.GetScript(afterExpr!);

        var parameters = Utility.SplitParameter(param!).ToArray();
        if (parameters.Count() != 2)
        {
            throw new Exception(string.Format("'foreach' script should have 2 parameters: 'foreach ({0})'", param));
        }

        var loopScript = ScriptFactory.CreateScript(loop);

        return new ForEachScript(scriptContext, parameters[0], new ExpressionDynamic(parameters[1], scriptContext),
            loopScript);
    }

    public IScriptFactory ScriptFactory { get; set; } = null!;

    public WorldModel WorldModel { get; set; } = null!;
}

public class ForEachScript : ScriptBase
{
    private readonly IScript _loopScript;
    private readonly ScriptContext _scriptContext;
    private IFunctionDynamic _list;
    private string _variable;

    public ForEachScript(ScriptContext scriptContext, string variable, IFunctionDynamic list, IScript loopScript)
    {
        _scriptContext = scriptContext;
        _variable = variable;
        _list = list;
        _loopScript = loopScript;
    }

    public override string Keyword => "foreach";

    protected override ScriptBase CloneScript()
    {
        return new ForEachScript(_scriptContext, _variable, _list.Clone(), (IScript) _loopScript.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await _list.ExecuteAsync(c);
        IEnumerable? resultList = null;

        // Cannot foreach over strings as of Quest 5.3, as the Char data type is not supported (retained functionality
        // for pre-5.3 to prevent breaking existing scripts)

        if (_scriptContext.WorldModel.Version < WorldModelVersion.v530 || !(result is string))
        {
            if (result is IDictionary resultDictionary)
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
            c.Parameters![_variable] = variable;
            await _loopScript.ExecuteAsync(c);
            if (c.IsReturned)
            {
                break;
            }
        }
    }

    public override string Save()
    {
        return SaveScript("foreach", _loopScript, _variable, _list.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _variable;
            case 1:
                return _list.Save();
            case 2:
                return _loopScript;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _variable = (string) value!;
                break;
            case 1:
                _list = new ExpressionDynamic((string) value!, _scriptContext);
                break;
            case 2:
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'foreach' loop");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}