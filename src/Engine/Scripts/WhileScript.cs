using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class WhileScriptConstructor : IScriptConstructor
{
    public string Keyword => "while";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string? afterExpr;
        var param = Utility.GetRequiredParameter(script, out afterExpr);
        var loop = Utility.GetScript(afterExpr!);
        var loopScript = ScriptFactory.CreateScript(loop);

        return new WhileScript(scriptContext, ScriptFactory, new Expression<bool>(param, scriptContext), loopScript);
    }

    public IScriptFactory ScriptFactory { get; set; } = null!;

    public WorldModel WorldModel { get; set; } = null!;
}

public class WhileScript : ScriptBase
{
    private readonly IScript _loopScript;
    private readonly ScriptContext _scriptContext;
    private readonly IScriptFactory _scriptFactory;
    private IFunction<bool> _expression;
    private WorldModel _worldModel;

    public WhileScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IFunction<bool> expression,
        IScript loopScript)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _scriptFactory = scriptFactory;
        _expression = expression;
        _loopScript = loopScript;
    }

    public override string Keyword => "while";

    protected override ScriptBase CloneScript()
    {
        return new WhileScript(_scriptContext, _scriptFactory, _expression.Clone(), (IScript) _loopScript.Clone());
    }

    protected override void ParentUpdated()
    {
        _loopScript.Parent = Parent;
    }

    public override async Task ExecuteAsync(Context c)
    {
        while (await _expression.ExecuteAsync(c))
        {
            await _loopScript.ExecuteAsync(c);
            if (c.IsReturned)
            {
                break;
            }
        }
    }

    public override string Save()
    {
        return SaveScript("while", _loopScript, _expression.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _expression.Save();
            case 1:
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
                _expression = new Expression<bool>((string) value!, _scriptContext);
                break;
            case 1:
                // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'while' loop");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}