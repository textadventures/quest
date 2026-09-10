using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class RunDelegateScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "rundelegate";

    protected override int[] ExpectedParameters
    {
        get { return new int[] { }; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        if (parameters.Count < 2)
        {
            throw new Exception("Expected at least 2 parameters in rundelegate call");
        }

        var obj = new Expression<Element>(parameters[0], scriptContext);
        var delegateName = new Expression<string>(parameters[1], scriptContext);
        var paramExpressions = new List<IFunction<object>>();
        foreach (var param in parameters.Skip(2))
        {
            paramExpressions.Add(new Expression<object>(param, scriptContext));
        }

        return new RunDelegateScript(scriptContext, obj, delegateName, paramExpressions);
    }
}

public class RunDelegateScript : ScriptBase
{
    private readonly FunctionCallParameters _parameters;
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<Element> _appliesTo;
    private IFunction<string> _delegate;

    public RunDelegateScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> del,
        IList<IFunction<object>> parameters)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _delegate = del;
        _parameters = new FunctionCallParameters(_worldModel, parameters);
        _appliesTo = obj;
    }

    public override string Keyword => "rundelegate";

    protected override ScriptBase CloneScript()
    {
        return new RunDelegateScript(_scriptContext, _appliesTo.Clone(), _delegate.Clone(), _parameters.Parameters!);
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (_parameters == null)
        {
            throw new NotImplementedException();
        }

        var obj = await _appliesTo.ExecuteAsync(c);
        var delName = await _delegate.ExecuteAsync(c);
        var impl = obj.Fields.Get(delName) as DelegateImplementation;

        if (impl == null)
        {
            throw new Exception(
                string.Format("Object '{0}' has no delegate implementation '{1}'", obj.Name, _delegate));
        }

        var paramValues = new Parameters();

        var cnt = 0;
        foreach (var f in _parameters.Parameters!)
        {
            paramValues.Add((string) impl.Definition.Fields[FieldDefinitions.ParamNames]![cnt]!, await f.ExecuteAsync(c));
            cnt++;
        }

        await _worldModel.RunScriptAsync(impl.Implementation.Fields[FieldDefinitions.Script]!, paramValues, obj);
    }

    public override string Save()
    {
        var saveParameters = new List<string>();
        saveParameters.Add(_appliesTo.Save());
        saveParameters.Add(_delegate.Save());
        foreach (var p in _parameters.ParametersAsQuestList)
        {
            saveParameters.Add(p);
        }

        return SaveScript("rundelegate", saveParameters.ToArray());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _appliesTo.Save();
            case 1:
                return _delegate.Save();
            case 2:
                return _parameters.ParametersAsQuestList;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _appliesTo = new Expression<Element>((string) value!, _scriptContext);
                break;
            case 1:
                _delegate = new Expression<string>((string) value!, _scriptContext);
                break;
            case 2:
                // any updates to the parameters should change the list itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the parameters of a 'rundelegate'");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}