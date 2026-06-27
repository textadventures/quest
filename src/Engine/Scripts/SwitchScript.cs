#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class SwitchScriptConstructor : IScriptConstructor
{
    public string Keyword => "switch";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string afterExpr;
        var param = Utility.GetParameter(script, out afterExpr);
        IScript defaultScript;
        var cases = ProcessCases(Utility.GetScript(afterExpr), out defaultScript, scriptContext);

        return new SwitchScript(scriptContext, new ExpressionDynamic(param, scriptContext), cases, defaultScript);
    }

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }

    private Dictionary<IFunctionDynamic, IScript> ProcessCases(string cases, out IScript defaultScript,
        ScriptContext scriptContext)
    {
        var finished = false;
        string remainingCases;
        string afterExpr;
        var result = new Dictionary<IFunctionDynamic, IScript>();
        defaultScript = null;

        cases = Utility.RemoveSurroundingBraces(cases);

        while (!finished)
        {
            cases = Utility.GetScript(cases, out remainingCases);
            if (cases != null)
            {
                cases = cases.Trim();
            }

            if (!string.IsNullOrEmpty(cases))
            {
                if (cases.StartsWith("case"))
                {
                    var expr = Utility.GetParameter(cases, out afterExpr);
                    var caseScript = Utility.GetScript(afterExpr);
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

            cases = remainingCases;
            if (string.IsNullOrEmpty(cases))
            {
                finished = true;
            }
        }

        return result;
    }
}

public class SwitchScript : ScriptBase
{
    private readonly IScript m_default;
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private SwitchCases m_cases;
    private IFunctionDynamic m_expr;

    public SwitchScript(ScriptContext scriptContext, IFunctionDynamic expression,
        Dictionary<IFunctionDynamic, IScript> cases, IScript defaultScript)
        : this(scriptContext, expression, defaultScript)
    {
        m_cases = new SwitchCases(this, cases);
    }

    private SwitchScript(ScriptContext scriptContext, IFunctionDynamic expression, IScript defaultScript)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_expr = expression;
        m_default = defaultScript ?? new MultiScript(m_worldModel);
    }

    public override string Keyword => "switch";

    protected override ScriptBase CloneScript()
    {
        var clone = new SwitchScript(m_scriptContext, m_expr.Clone(), (IScript) m_default.Clone());
        clone.m_cases = m_cases.Clone(clone);
        return clone;
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await m_expr.ExecuteAsync(c);
        var success = await m_cases.ExecuteAsync(c, result.ToString());

        if (!success && m_default != null)
        {
            await m_default.ExecuteAsync(c);
        }
    }

    public override string Save()
    {
        var result = SaveScript("switch", m_expr.Save()) + " {" + Environment.NewLine;
        result += m_cases.Save();
        if (m_default != null && ((IMultiScript) m_default).Scripts.Count() > 0)
        {
            result += SaveScript("default", m_default);
        }

        result += Environment.NewLine + "}";
        return result;
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_expr.Save();
            case 1:
                return m_cases.CasesAsQuestDictionary;
            case 2:
                return m_default;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                m_expr = new ExpressionDynamic((string) value, m_scriptContext);
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
        private readonly SwitchScript m_parent;
        private Dictionary<string, IFunctionDynamic> m_compiledExpressions = new();

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
                m_compiledExpressions.Add(caseString, compiledExpression);
            }
        }

        private SwitchCases(SwitchScript parent)
        {
            m_parent = parent;
            if (parent.m_worldModel.EditMode)
            {
                CasesAsQuestDictionary.UndoLog = parent.m_worldModel.UndoLogger;
            }
        }

        public QuestDictionary<IScript> CasesAsQuestDictionary { get; private set; } = new();

        internal SwitchCases Clone(SwitchScript newParent)
        {
            var clone = new SwitchCases(newParent);
            clone.CasesAsQuestDictionary = (QuestDictionary<IScript>) CasesAsQuestDictionary.Clone();
            clone.m_compiledExpressions = new Dictionary<string, IFunctionDynamic>();
            foreach (var compiledExpression in m_compiledExpressions)
            {
                clone.m_compiledExpressions.Add(compiledExpression.Key, compiledExpression.Value);
            }

            return clone;
        }

        public string Save()
        {
            var result = string.Empty;
            foreach (var caseItem in CasesAsQuestDictionary)
            {
                result += m_parent.SaveScript("case", caseItem.Value, caseItem.Key);
            }

            return result;
        }

        public async Task<bool> ExecuteAsync(Context c, string result)
        {
            foreach (var switchCase in CasesAsQuestDictionary)
            {
                var expr = m_compiledExpressions[switchCase.Key];

                if (result == (await expr.ExecuteAsync(c)).ToString())
                {
                    await switchCase.Value.ExecuteAsync(c);
                    return true;
                }
            }

            return false;
        }
    }
}