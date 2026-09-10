using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class FunctionCallScriptConstructor : IScriptConstructor
{
    public IScript Create(string script, ScriptContext scriptContext)
    {
        List<IFunction<object>>? paramExpressions = null;
        string procName, afterParameter;

        var param = Utility.GetParameter(script, out afterParameter);
        IScript? paramScript = null;

        // Handle functions of the form
        //    SomeFunction (parameter) { script }
        if (afterParameter != null)
        {
            afterParameter = afterParameter.Trim();
            if (afterParameter.Length > 0)
            {
                var paramScriptString = Utility.GetScript(afterParameter);
                paramScript = ScriptFactory.CreateScript(paramScriptString);
            }
        }

        if (param == null && paramScript == null)
        {
            procName = script;
        }
        else
        {
            if (param != null)
            {
                var parameters = Utility.SplitParameter(param);
                procName = script.Substring(0, script.IndexOf('(')).Trim();
                paramExpressions = new List<IFunction<object>>();
                if (param.Trim().Length > 0)
                {
                    foreach (var s in parameters)
                    {
                        paramExpressions.Add(new Expression<object>(s, scriptContext));
                    }
                }
            }
            else
            {
                procName = script.Substring(0, script.IndexOfAny(new[] {'{', ' '}));
            }
        }

        if (!WorldModel.EditMode && WorldModel.Procedure(procName) == null)
        {
            throw new Exception(string.Format("Function not found: '{0}'", procName));
        }

        return new FunctionCallScript(WorldModel, procName, paramExpressions, paramScript);
    }

    public IScriptFactory ScriptFactory { get; set; } = null!;

    public WorldModel WorldModel { get; set; } = null!;

    public string? Keyword => null;
}

public class FunctionCallScript : ScriptBase, IFunctionCallScript
{
    private readonly FunctionCallParameters _parameters;
    private readonly WorldModel _worldModel;
    private IScript? _paramFunction;
    private string _procedure;

    public FunctionCallScript(WorldModel worldModel, string procedure)
        : this(worldModel, procedure, null, null)
    {
    }

    public FunctionCallScript(WorldModel worldModel, string procedure, IList<IFunction<object>>? parameters,
        IScript? paramFunction)
    {
        _worldModel = worldModel;
        _procedure = procedure;
        _parameters = new FunctionCallParameters(worldModel, parameters);
        _paramFunction = paramFunction;

        _parameters.ParametersAsQuestList.Added += Parameters_Added;
        _parameters.ParametersAsQuestList.Removed += Parameters_Removed;
    }

    public event EventHandler<ScriptUpdatedEventArgs>? FunctionCallParametersUpdated;

    public override async Task ExecuteAsync(Context c)
    {
        if ((_parameters.Parameters == null || _parameters.Parameters.Count == 0) && _paramFunction == null)
        {
            await _worldModel.RunProcedureAsync(_procedure);
        }
        else
        {
            var paramValues = new Parameters();
            var proc = _worldModel.Procedure(_procedure)!;

            var paramNames = proc.Fields[FieldDefinitions.ParamNames]!;

            var parameters = _parameters.Parameters!;
            var paramCount = parameters.Count;
            if (_paramFunction != null)
            {
                paramCount++;
            }

            if (paramCount > paramNames.Count)
            {
                throw new Exception(string.Format(
                    "Too many parameters passed to {0} function - {1} passed, but only {2} expected",
                    _procedure,
                    paramCount,
                    paramNames.Count));
            }

            if (_worldModel.Version >= WorldModelVersion.v520)
            {
                if (paramCount < paramNames.Count)
                {
                    throw new Exception(string.Format(
                        "Too few parameters passed to {0} function - only {1} passed, but {2} expected",
                        _procedure,
                        paramCount,
                        paramNames.Count));
                }
            }

            var cnt = 0;
            foreach (var f in parameters)
            {
                paramValues.Add((string) paramNames[cnt]!, await f.ExecuteAsync(c));
                cnt++;
            }

            if (_paramFunction != null)
            {
                paramValues.Add((string) paramNames[cnt]!, _paramFunction);
            }

            await _worldModel.RunProcedureAsync(_procedure, paramValues, false);
        }
    }

    public override string Keyword => "(function)" + _procedure;

    public override string Save()
    {
        if (_worldModel.Procedure(_procedure) == null)
        {
            // TO DO: this is the wrong place to be throwing an exception, because Save may be called while editing a script,
            // and maybe the user simply hasn't created their function yet. Maybe instead we should append to a list of warnings
            // when doing an actual File Save, then we can display any warnings after saving.
            //throw new Exception(string.Format("Unable to save call to function '{0}' - function does not exist", _procedure));
        }

        if (_parameters.ParametersAsQuestList.Count == 0 && _paramFunction == null)
        {
            return _procedure;
        }

        var saveParameters = new List<string>();
        foreach (var p in _parameters.ParametersAsQuestList)
        {
            saveParameters.Add(p);
        }

        if (_paramFunction == null)
        {
            return SaveScript(_procedure, saveParameters.ToArray());
        }

        if (saveParameters.Count > 0)
        {
            return SaveScript(_procedure, _paramFunction, saveParameters.ToArray());
        }

        return SaveScript(_procedure + "()", _paramFunction);
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _procedure;
            case 1:
                return _parameters.ParametersAsQuestList;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public object? GetFunctionCallParameter(int index)
    {
        if (index >= _parameters.ParametersAsQuestList.Count)
        {
            // In the editor, when a blank function call is created, it will have no parameters, but
            // if the editor requests a first parameter then we want to return a blank default instead
            // of throwing an error.
            return "";
        }

        return _parameters.ParametersAsQuestList[index];
    }

    public void SetFunctionCallParameter(int index, object? value)
    {
        if (index < _parameters.ParametersAsQuestList.Count)
        {
            // In the editor, when a blank function call is created, it will have no parameters
            _parameters.ParametersAsQuestList.Remove(_parameters.ParametersAsQuestList[index], UpdateSource.User,
                index);
        }

        _parameters.ParametersAsQuestList.Add(value, UpdateSource.User, index);
    }

    public IScript? GetFunctionCallParameterScript()
    {
        return _paramFunction;
    }

    public void SetFunctionCallParameterScript(IScript? script)
    {
        _paramFunction = script;
    }

    // Only an EditableScript wrapper - i.e. a script currently open in the editor - subscribes
    // to FunctionCallParametersUpdated, so both handlers below have to cope with there being no
    // subscriber at all.
    private void Parameters_Added(object? sender, QuestListUpdatedEventArgs<string> e)
    {
        // the number of parameters in a function call cannot change. So, as QuestList doesn't
        // provide an Updated event (we simulate Updates with a Remove and an Add at the same
        // index), we assume that any Added event is really an update.

        FunctionCallParametersUpdated?.Invoke(this, new ScriptUpdatedEventArgs(e.Index, e.UpdatedItem));
    }

    private void Parameters_Removed(object? sender, QuestListUpdatedEventArgs<string> e)
    {
        // the only time we care about a parameter being removed is if it's the first parameter being
        // deleted. Everything else should simply be a Remove followed by an Add, and we handle the
        // Add above.
        if (e.Index == 0)
        {
            FunctionCallParametersUpdated?.Invoke(this, new ScriptUpdatedEventArgs(e.Index, string.Empty));
        }
    }

    protected override ScriptBase CloneScript()
    {
        return new FunctionCallScript(_worldModel, _procedure, _parameters.Parameters,
            _paramFunction);
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _procedure = (string) value!;
                break;
            case 1:
                // any updates to the parameters should change the list itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the parameters of a function call");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}