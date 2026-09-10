#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class LazyLoadScript : IScript, IIfScript, IFirstTimeScript, IMultiScript
{
    private readonly IScriptConstructor _scriptConstructor;
    private readonly ScriptContext _scriptContext;
    private readonly ScriptFactory _scriptFactory;
    private readonly WorldModel _worldModel;
    private IScriptParent _parent;
    private IScript _script;
    private string _scriptString;

    public LazyLoadScript(ScriptFactory scriptFactory, string scriptString, ScriptContext scriptContext)
    {
        _scriptFactory = scriptFactory;
        _scriptString = scriptString;
        _scriptContext = scriptContext;
        _worldModel = scriptFactory.WorldModel;
    }

    public LazyLoadScript(ScriptFactory scriptFactory, IScriptConstructor scriptConstructor, string scriptString,
        ScriptContext scriptContext)
    {
        _scriptFactory = scriptFactory;
        _scriptConstructor = scriptConstructor;
        _scriptString = scriptString;
        _scriptContext = scriptContext;
        _worldModel = scriptConstructor.WorldModel;
    }

    public void SetOtherwiseScript(IScript script)
    {
        Initialise();
        ((FirstTimeScript) _script).SetOtherwiseScript(script);
    }

    public event EventHandler<IfScriptUpdatedEventArgs> IfScriptUpdated
    {
        add
        {
            Initialise();
            ((IfScript) _script).IfScriptUpdated += value;
        }
        remove
        {
            Initialise();
            ((IfScript) _script).IfScriptUpdated -= value;
        }
    }

    public void SetElse(IScript elseScript)
    {
        Initialise();
        ((IfScript) _script).SetElse(elseScript);
    }

    public IElseIfScript AddElseIf(string expression, IScript script)
    {
        Initialise();
        return ((IfScript) _script).AddElseIf(expression, script);
    }

    public IElseIfScript AddElseIf(IFunction<bool> expression, IScript script)
    {
        Initialise();
        return ((IfScript) _script).AddElseIf(expression, script);
    }

    public IFunction<bool> Expression
    {
        get
        {
            Initialise();
            return ((IfScript) _script).Expression;
        }
    }

    public IScript ThenScript
    {
        get
        {
            Initialise();
            return ((IfScript) _script).ThenScript;
        }
        set
        {
            Initialise();
            ((IfScript) _script).ThenScript = value;
        }
    }

    public IScript ElseScript
    {
        get
        {
            Initialise();
            return ((IfScript) _script).ElseScript;
        }
    }

    public IList<IElseIfScript> ElseIfScripts
    {
        get
        {
            Initialise();
            return ((IfScript) _script).ElseIfScripts;
        }
    }

    public string ExpressionString
    {
        get
        {
            Initialise();
            return ((IfScript) _script).ExpressionString;
        }
        set
        {
            Initialise();
            ((IfScript) _script).ExpressionString = value;
        }
    }

    public void RemoveElseIf(IElseIfScript elseIfScript)
    {
        Initialise();
        ((IfScript) _script).RemoveElseIf(elseIfScript);
    }

    public IEnumerable<IScript> Scripts
    {
        get
        {
            Initialise();
            return ((MultiScript) _script).Scripts;
        }
    }

    public void Add(params IScript[] scripts)
    {
        Initialise();
        ((MultiScript) _script).Add(scripts);
    }

    public void Remove(int index)
    {
        Initialise();
        ((MultiScript) _script).Remove(index);
    }

    public void Swap(int index1, int index2)
    {
        Initialise();
        ((MultiScript) _script).Swap(index1, index2);
    }

    public void Insert(int index, IScript script)
    {
        Initialise();
        ((MultiScript) _script).Insert(index, script);
    }

    public void LoadCode(string code)
    {
        Initialise();
        ((MultiScript) _script).LoadCode(code);
    }

    public IMutableField Clone()
    {
        if (_script != null)
        {
            return _script.Clone();
        }

        var result = new LazyLoadScript(_scriptFactory, _scriptString, _scriptContext);
        result.Line = _scriptString;
        result.Parent = _parent;
        return result;
    }

    public event EventHandler<ScriptUpdatedEventArgs> ScriptUpdated
    {
        add
        {
            Initialise();
            _script.ScriptUpdated += value;
        }
        remove
        {
            Initialise();
            _script.ScriptUpdated -= value;
        }
    }

    public Task ExecuteAsync(Context c)
    {
        Initialise();
        return _script.ExecuteAsync(c);
    }

    public string Line
    {
        get
        {
            if (_script == null)
            {
                return _scriptString;
            }

            return _script.Line;
        }
        set
        {
            if (_script == null)
            {
                _scriptString = value;
                return;
            }

            _script.Line = value;
        }
    }

    public string Save()
    {
        Initialise();
        return _script.Save();
    }

    public void SetParameter(int index, object value)
    {
        Initialise();
        _script.SetParameter(index, value);
    }

    public void SetParameterSilent(int index, object value)
    {
        Initialise();
        _script.SetParameterSilent(index, value);
    }

    public object GetParameter(int index)
    {
        Initialise();
        return _script.GetParameter(index);
    }

    public string Keyword
    {
        get
        {
            Initialise();
            return _script.Keyword;
        }
    }

    public IScriptParent Parent
    {
        get
        {
            if (_script == null)
            {
                return _parent;
            }

            return _script.Parent;
        }
        set
        {
            if (_script == null)
            {
                _parent = value;
                return;
            }

            _script.Parent = value;
        }
    }

    public IEnumerable<string> GetDefinedVariables()
    {
        Initialise();
        return _script.GetDefinedVariables();
    }

    public UndoLogger UndoLog
    {
        get
        {
            Initialise();
            return _script.UndoLog;
        }
        set
        {
            Initialise();
            _script.UndoLog = value;
        }
    }

    public Element Owner
    {
        get
        {
            Initialise();
            return _script.Owner;
        }
        set
        {
            Initialise();
            _script.Owner = value;
        }
    }

    public bool Locked
    {
        get
        {
            Initialise();
            return _script.Locked;
        }
        set
        {
            Initialise();
            _script.Locked = value;
        }
    }

    public bool RequiresCloning
    {
        get
        {
            Initialise();
            return _script.RequiresCloning;
        }
    }

    private void Initialise()
    {
        if (_script != null)
        {
            return;
        }

        try
        {
            if (_scriptConstructor == null)
            {
                _script = _scriptFactory.CreateScript(_scriptString, _scriptContext, false, false);
            }
            else
            {
                _script = _scriptConstructor.Create(_scriptString, _scriptContext);
                _script.Line = _scriptString;
            }
        }
        catch
        {
            if (!_worldModel.EditMode)
            {
                throw;
            }

            _script = new FailedScript(_scriptString);
            if (_scriptConstructor == null)
            {
                _script = new MultiScript(_scriptFactory.WorldModel, _script);
            }
        }

        _script.Parent = _parent;
        _scriptString = null;
    }

    public override string ToString()
    {
        if (_script != null)
        {
            return _script.ToString();
        }

        return _scriptString;
    }
}