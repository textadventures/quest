using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class SwitchScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "switch"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            IScript defaultScript;
            Dictionary<IFunctionGeneric, IScript> cases = ProcessCases(Utility.GetScript(afterExpr), out defaultScript, scriptContext);

            return new SwitchScript(scriptContext, new ExpressionGeneric(param, scriptContext), cases, defaultScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        private Dictionary<IFunctionGeneric, IScript> ProcessCases(string cases, out IScript defaultScript, ScriptContext scriptContext)
        {
            bool finished = false;
            string remainingCases;
            string afterExpr;
            Dictionary<IFunctionGeneric, IScript> result = new Dictionary<IFunctionGeneric, IScript>();
            defaultScript = null;

            cases = Utility.RemoveSurroundingBraces(cases);

            while (!finished)
            {
                cases = Utility.GetScript(cases, out remainingCases);
                if (cases != null) cases = cases.Trim();

                if (!string.IsNullOrEmpty(cases))
                {
                    if (cases.StartsWith("case"))
                    {
                        string expr = Utility.GetParameter(cases, out afterExpr);
                        string caseScript = Utility.GetScript(afterExpr);
                        IScript script = ScriptFactory.CreateScript(caseScript, scriptContext);

                        // Case expression can have multiple values separated by commas. In Edit mode,
                        // just load this as one expression for editing.

                        if (!scriptContext.WorldModel.EditMode)
                        {
                            var matchList = Utility.SplitParameter(expr);
                            foreach (var match in matchList)
                            {
                                result.Add(new ExpressionGeneric(match, scriptContext), script);
                            }
                        }
                        else
                        {
                            result.Add(new ExpressionGeneric(expr, scriptContext), script);
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
                if (string.IsNullOrEmpty(cases)) finished = true;
            }

            return result;
        }
    }

    public class SwitchScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private IFunctionGeneric m_expr;
        private SwitchCases m_cases;
        private IScript m_default;
        private WorldModel m_worldModel;

        public SwitchScript(ScriptContext scriptContext, IFunctionGeneric expression, Dictionary<IFunctionGeneric, IScript> cases, IScript defaultScript)
            : this(scriptContext, expression, defaultScript)
        {
            m_cases = new SwitchCases(this, cases);
        }

        private SwitchScript(ScriptContext scriptContext, IFunctionGeneric expression, IScript defaultScript)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_expr = expression;
            m_default = defaultScript ?? new MultiScript(m_worldModel);
        }

        protected override ScriptBase CloneScript()
        {
            SwitchScript clone = new SwitchScript(m_scriptContext, m_expr.Clone(), (IScript)m_default.Clone());
            clone.m_cases = m_cases.Clone(clone);
            return clone;
        }

        public override void Execute(Context c)
        {
            object result = m_expr.Execute(c);
            bool success = false;

            // using .ToString() here as an object comparison of ints won't work
            success = m_cases.Execute(c, result.ToString());

            if (!success && m_default != null)
            {
                m_default.Execute(c);
            }
        }

        public override string Save()
        {
            string result = SaveScript("switch", m_expr.Save()) + " {" + Environment.NewLine;
            result += m_cases.Save();
            if (m_default != null && ((IMultiScript)m_default).Scripts.Count() > 0) result += SaveScript("default", m_default);
            result += Environment.NewLine + "}";
            return result;
        }

        public override string Keyword
        {
            get
            {
                return "switch";
            }
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

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_expr = new ExpressionGeneric((string)value, m_scriptContext);
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
            private QuestDictionary<IScript> m_cases = new QuestDictionary<IScript>();
            private Dictionary<string, IFunctionGeneric> m_compiledExpressions = new Dictionary<string, IFunctionGeneric>();
            private SwitchScript m_parent;

            public SwitchCases(SwitchScript parent, Dictionary<IFunctionGeneric, IScript> cases)
                : this(parent)
            {

                foreach (var switchCase in cases)
                {
                    IFunctionGeneric compiledExpression = switchCase.Key;
                    string caseString = compiledExpression.Save();
                    IScript script = switchCase.Value;

                    if (m_cases.ContainsKey(caseString))
                    {
                        throw new Exception(string.Format("'switch' block contains duplicate case '{0}'", caseString));
                    }
                    m_cases.Add(caseString, script);
                    m_compiledExpressions.Add(caseString, compiledExpression);
                }
            }

            private SwitchCases(SwitchScript parent)
            {
                m_parent = parent;
                if (parent.m_worldModel.EditMode)
                {
                    m_cases.UndoLog = parent.m_worldModel.UndoLogger;
                }
            }

            internal SwitchCases Clone(SwitchScript newParent)
            {
                SwitchCases clone = new SwitchCases(newParent);
                clone.m_cases = (QuestDictionary<IScript>)m_cases.Clone();
                clone.m_compiledExpressions = new Dictionary<string, IFunctionGeneric>();
                foreach (var compiledExpression in m_compiledExpressions)
                {
                    clone.m_compiledExpressions.Add(compiledExpression.Key, compiledExpression.Value);
                }
                return clone;
            }

            public QuestDictionary<IScript> CasesAsQuestDictionary
            {
                get { return m_cases; }
            }

            public string Save()
            {
                string result = string.Empty;
                foreach (KeyValuePair<string, IScript> caseItem in m_cases)
                {
                    result += m_parent.SaveScript("case", caseItem.Value, caseItem.Key);
                }
                return result;
            }

            public bool Execute(Context c, string result)
            {
                foreach (var switchCase in m_cases)
                {
                    IFunctionGeneric expr = m_compiledExpressions[switchCase.Key];

                    if (result == expr.Execute(c).ToString())
                    {
                        switchCase.Value.Execute(c);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}