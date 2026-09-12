using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class ForScriptConstructor : IScriptConstructor
{
    #region IScriptConstructor Members

    public string Keyword => "for";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string? afterExpr;
        var param = Utility.GetRequiredParameter(script, out afterExpr);
        var loop = Utility.GetScript(afterExpr!);

        var parameters = Utility.SplitParameter(param).ToArray();
        var loopScript = ScriptFactory.CreateScript(loop);

        if (parameters.Count() == 3)
        {
            return new ForScript(scriptContext, ScriptFactory, parameters[0],
                new Expression<int>(parameters[1], scriptContext), new Expression<int>(parameters[2], scriptContext),
                loopScript);
        }

        if (parameters.Count() == 4)
        {
            return new ForScript(scriptContext, ScriptFactory, parameters[0],
                new Expression<int>(parameters[1], scriptContext), new Expression<int>(parameters[2], scriptContext),
                new Expression<int>(parameters[3], scriptContext), loopScript);
        }

        throw new Exception(string.Format("'for' script should have 3 or 4 parameters: 'for ({0})'", param));
    }

    public IScriptFactory ScriptFactory { get; set; } = null!;

    public WorldModel WorldModel { get; set; } = null!;

    #endregion
}

public class ForScript : ScriptBase
{
    private readonly IScript _loopScript;
    private readonly ScriptContext _scriptContext;
    private readonly IScriptFactory _scriptFactory;
    private IFunction<int> _from;
    private IFunction<int>? _step;
    private IFunction<int> _to;
    private string _variable;
    private WorldModel _worldModel;

    public ForScript(ScriptContext scriptContext, IScriptFactory scriptFactory, string variable, IFunction<int> from,
        IFunction<int> to, IScript loopScript)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _scriptFactory = scriptFactory;
        _variable = variable;
        _from = from;
        _to = to;
        _loopScript = loopScript;
    }

    public ForScript(ScriptContext scriptContext, IScriptFactory scriptFactory, string variable, IFunction<int> from,
        IFunction<int> to, IFunction<int>? step, IScript loopScript)
        : this(scriptContext, scriptFactory, variable, from, to, loopScript)
    {
        _step = step;
    }

    public override string Keyword => "for";

    protected override ScriptBase CloneScript()
    {
        return new ForScript(_scriptContext, _scriptFactory, _variable, _from.Clone(), _to.Clone(),
            _step == null ? null : _step.Clone(), (IScript) _loopScript.Clone());
    }

    protected override void ParentUpdated()
    {
        _loopScript.Parent = Parent;
    }

    public override async Task ExecuteAsync(Context c)
    {
        var from = await _from.ExecuteAsync(c);
        var to = await _to.ExecuteAsync(c);
        var step = _step == null ? 1 : await _step.ExecuteAsync(c);
        int count;
        c.Parameters![_variable] = 0;

        for (count = from; (step > 0 && count <= to) || (step < 0 && count >= to); count += step)
        {
            c.Parameters![_variable] = count;
            await _loopScript.ExecuteAsync(c);
            if (c.IsReturned)
            {
                break;
            }

            var newCount = c.Parameters![_variable];
            if (newCount is int)
            {
                count = (int) newCount;
            }
            else
            {
                // The type of the count variable has changed, so abort the loop
                break;
            }
        }
    }

    public override string Save()
    {
        if (_step == null)
        {
            return SaveScript("for", _loopScript, _variable, _from.Save(), _to.Save());
        }

        return SaveScript("for", _loopScript, _variable, _from.Save(), _to.Save(), _step.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _variable;
            case 1:
                return _from.Save();
            case 2:
                return _to.Save();
            case 3:
                return _step == null ? null : _step.Save();
            case 4:
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
                _from = new Expression<int>((string) value!, _scriptContext);
                break;
            case 2:
                _to = new Expression<int>((string) value!, _scriptContext);
                break;
            case 3:
                _step = new Expression<int>((string) value!, _scriptContext);
                break;
            case 4:
                // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException("Attempt to use SetParameter to change the script of a 'for' loop");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override IEnumerable<string> GetDefinedVariables()
    {
        return new List<string> {_variable};
    }
}