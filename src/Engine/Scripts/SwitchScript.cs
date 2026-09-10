using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class SwitchScriptConstructor : IScriptConstructor
{
    public string Keyword => "switch";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string? afterExpr;
        var param = Utility.GetParameter(script, out afterExpr);
        IScript? defaultScript;
        var cases = ProcessCases(Utility.GetScript(afterExpr!), out defaultScript, scriptContext);

        return new SwitchScript(scriptContext, new ExpressionDynamic(param!, scriptContext), cases, defaultScript);
    }

    public IScriptFactory ScriptFactory { get; set; } = null!;

    public WorldModel WorldModel { get; set; } = null!;

    private Dictionary<IFunctionDynamic, IScript> ProcessCases(string cases, out IScript? defaultScript,
        ScriptContext scriptContext)
    {
        var finished = false;
        string? remainingCases;
        string? afterExpr;
        var result = new Dictionary<IFunctionDynamic, IScript>();
        defaultScript = null;

        cases = Utility.RemoveSurroundingBraces(cases);

        while (!finished)
        {
            cases = Utility.GetScript(cases, out remainingCases).Trim();

            if (!string.IsNullOrEmpty(cases))
            {
                if (cases.StartsWith("case"))
                {
                    var expr = Utility.GetParameter(cases, out afterExpr)!;
                    var caseScript = Utility.GetScript(afterExpr!);
                    var script = ScriptFactory.CreateScript(caseScript, scriptContext);

                    // Case expression can have multiple values separated by commas. In Edit mode,
                    // just load this as one expression for editing.

                    if (!scriptContext.WorldModel.EditMode)
                    {
                        var matchList = Utility.SplitParameter(expr);
                        foreach (var match in matchList)
                        {
                            result.Add(new ExpressionDynamic(match, scriptContext), script);
                        }
                    }
                    else
                    {
                        result.Add(new ExpressionDynamic(expr, scriptContext), script);
                    }
                }
                else if (cases.StartsWith("default"))
                {
                    defaultScript = ScriptFactory.CreateScript(cases.Substring(8).Trim());
                }
                else
                {
                    throw new Exception(string.Format("Invalid inside switch block: '{0}'", cases));
                }
            }

            if (string.IsNullOrEmpty(remainingCases))
            {
                finished = true;
            }
            else
            {
                cases = remainingCases;
            }
        }

        return result;
    }
}

public class SwitchScript : ScriptBase
{
    private readonly IScript _default;
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    // Assigned straight after construction, by the public constructor and by CloneScript
    private SwitchCases _cases = null!;
    private IFunctionDynamic _expr;

    public SwitchScript(ScriptContext scriptContext, IFunctionDynamic expression,
        Dictionary<IFunctionDynamic, IScript> cases, IScript? defaultScript)
        : this(scriptContext, expression, defaultScript)
    {
        _cases = new SwitchCases(this, cases);
    }

    private SwitchScript(ScriptContext scriptContext, IFunctionDynamic expression, IScript? defaultScript)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _expr = expression;
        _default = defaultScript ?? new MultiScript(_worldModel);
    }

    public override string Keyword => "switch";

    protected override ScriptBase CloneScript()
    {
        var clone = new SwitchScript(_scriptContext, _expr.Clone(), (IScript) _default.Clone());
        clone._cases = _cases.Clone(clone);
        return clone;
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await _expr.ExecuteAsync(c);
        // using .ToString() here as an object comparison of ints won't work
        var success = await _cases.ExecuteAsync(c, Utility.ExpressionResultToString(result));

        if (!success && _default != null)
        {
            await _default.ExecuteAsync(c);
        }
    }

    public override string Save()
    {
        var result = SaveScript("switch", _expr.Save()) + " {" + Environment.NewLine;
        result += _cases.Save();
        if (_default != null && ((IMultiScript) _default).Scripts.Count() > 0)
        {
            result += SaveScript("default", _default);
        }

        result += Environment.NewLine + "}";
        return result;
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _expr.Save();
            case 1:
                return _cases.CasesAsQuestDictionary;
            case 2:
                return _default;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _expr = new ExpressionDynamic((string) value!, _scriptContext);
                break;
            case 1:
                // any updates to the cases should change the scriptdictionary itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException("Attempt to use SetParameter to change the cases of a 'switch'");
            case 2:
                // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException("Attempt to use SetParameter to change the script of a 'switch'");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // We store the switch cases internally as a QuestDictionary<IScript>, so we can edit them in the Editor
    // using the standard scriptdictionary editor control.
    private class SwitchCases
    {
        private readonly SwitchScript _parent;
        private Dictionary<string, IFunctionDynamic> _compiledExpressions = new();

        public SwitchCases(SwitchScript parent, Dictionary<IFunctionDynamic, IScript> cases)
            : this(parent)
        {
            foreach (var switchCase in cases)
            {
                var compiledExpression = switchCase.Key;
                var caseString = compiledExpression.Save();
                var script = switchCase.Value;

                if (CasesAsQuestDictionary.ContainsKey(caseString))
                {
                    throw new Exception(string.Format("'switch' block contains duplicate case '{0}'", caseString));
                }

                CasesAsQuestDictionary.Add(caseString, script);
                _compiledExpressions.Add(caseString, compiledExpression);
            }
        }

        private SwitchCases(SwitchScript parent)
        {
            _parent = parent;
            if (parent._worldModel.EditMode)
            {
                CasesAsQuestDictionary.UndoLog = parent._worldModel.UndoLogger;
            }
        }

        public QuestDictionary<IScript> CasesAsQuestDictionary { get; private set; } = new();

        internal SwitchCases Clone(SwitchScript newParent)
        {
            var clone = new SwitchCases(newParent);
            clone.CasesAsQuestDictionary = (QuestDictionary<IScript>) CasesAsQuestDictionary.Clone();
            clone._compiledExpressions = new Dictionary<string, IFunctionDynamic>();
            foreach (var compiledExpression in _compiledExpressions)
            {
                clone._compiledExpressions.Add(compiledExpression.Key, compiledExpression.Value);
            }

            return clone;
        }

        public string Save()
        {
            var result = string.Empty;
            foreach (var caseItem in CasesAsQuestDictionary)
            {
                result += _parent.SaveScript("case", caseItem.Value, caseItem.Key);
            }

            return result;
        }

        public async Task<bool> ExecuteAsync(Context c, string result)
        {
            foreach (var switchCase in CasesAsQuestDictionary)
            {
                var expr = _compiledExpressions[switchCase.Key];

                if (result == Utility.ExpressionResultToString(await expr.ExecuteAsync(c)))
                {
                    await switchCase.Value.ExecuteAsync(c);
                    return true;
                }
            }

            return false;
        }
    }
}