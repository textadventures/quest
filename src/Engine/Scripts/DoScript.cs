using System.Collections;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class DoScriptConstructor : ScriptConstructorBase
{
    #region ScriptConstructorBase Members

    public override string Keyword => "do";

    protected override IScript? CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        switch (parameters.Count)
        {
            case 2:
                return new DoActionScript(scriptContext, new Expression<Element>(parameters[0], scriptContext),
                    new Expression<string>(parameters[1], scriptContext));
            case 3:
                return new DoActionScript(scriptContext, new Expression<Element>(parameters[0], scriptContext),
                    new Expression<string>(parameters[1], scriptContext),
                    new Expression<IDictionary>(parameters[2], scriptContext));
        }

        return null;
    }

    protected override int[] ExpectedParameters => [2, 3];

    #endregion
}

public class DoActionScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _action;
    private IFunction<Element> _obj;
    private IFunction<IDictionary>? _parameters;

    public DoActionScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> action)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _obj = obj;
        _action = action;
    }

    public DoActionScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> action,
        IFunction<IDictionary>? parameters)
        : this(scriptContext, obj, action)
    {
        _parameters = parameters;
    }

    public override string Keyword => "do";

    protected override ScriptBase CloneScript()
    {
        return new DoActionScript(_scriptContext, _obj.Clone(), _action.Clone(),
            _parameters == null ? null : _parameters.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var obj = await _obj.ExecuteAsync(c);
        var action = obj.GetAction(await _action.ExecuteAsync(c))!;
        if (_parameters == null)
        {
            await _worldModel.RunScriptAsync(action, obj);
        }
        else
        {
            await _worldModel.RunScriptAsync(action, new Parameters(await _parameters.ExecuteAsync(c)), obj);
        }
    }

    public override string Save()
    {
        var parameters = _parameters == null ? null : _parameters.Save();
        if (!string.IsNullOrEmpty(parameters))
        {
            return SaveScript("do", _obj.Save(), _action.Save(), parameters);
        }

        return SaveScript("do", _obj.Save(), _action.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _obj.Save();
            case 1:
                return _action.Save();
            case 2:
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
                _obj = new Expression<Element>((string) value!, _scriptContext);
                break;
            case 1:
                _action = new Expression<string>((string) value!, _scriptContext);
                break;
            case 2:
                _parameters = new Expression<IDictionary>((string) value!, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}