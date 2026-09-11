using System.Collections;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class InvokeScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "invoke";

    protected override int[] ExpectedParameters => [1, 2];

    protected override IScript? CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        switch (parameters.Count)
        {
            case 1:
                return new InvokeScript(scriptContext, new Expression<IScript>(parameters[0], scriptContext));
            case 2:
                return new InvokeScript(scriptContext, new Expression<IScript>(parameters[0], scriptContext),
                    new Expression<IDictionary>(parameters[1], scriptContext));
        }

        return null;
    }
}

public class InvokeScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<IDictionary>? _parameters;
    private IFunction<IScript> _script;

    public InvokeScript(ScriptContext scriptContext, IFunction<IScript> script)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _script = script;
    }

    public InvokeScript(ScriptContext scriptContext, IFunction<IScript> script, IFunction<IDictionary>? parameters)
        : this(scriptContext, script)
    {
        _parameters = parameters;
    }

    public override string Keyword => "invoke";

    protected override ScriptBase CloneScript()
    {
        return new InvokeScript(_scriptContext, _script.Clone(), _parameters == null ? null : _parameters.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var script = await _script.ExecuteAsync(c);
        if (_parameters == null)
        {
            await _worldModel.RunScriptAsync(script);
        }
        else
        {
            await _worldModel.RunScriptAsync(script, new Parameters(await _parameters.ExecuteAsync(c)));
        }
    }

    public override string Save()
    {
        var parameters = _parameters == null ? null : _parameters.Save();
        if (string.IsNullOrEmpty(parameters))
        {
            return SaveScript("invoke", _script.Save());
        }

        return SaveScript("invoke", _script.Save(), parameters);
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _script.Save();
            case 1:
                return _parameters == null ? null : _parameters.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _script = new Expression<IScript>((string) value!, _scriptContext);
                break;
            case 1:
                _parameters = new Expression<IDictionary>((string) value!, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}