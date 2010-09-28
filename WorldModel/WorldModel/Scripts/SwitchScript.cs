using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class SwitchScriptConstructor : IScriptConstructor
    {
        #region IScriptConstructor Members

        public string Keyword
        {
            get { return "switch"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            IScript defaultScript;
            Dictionary<IFunctionGeneric, IScript> cases = ProcessCases(Utility.GetScript(afterExpr), out defaultScript, proc);

            return new SwitchScript(new ExpressionGeneric(param), cases, defaultScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        #endregion

        private Dictionary<IFunctionGeneric, IScript> ProcessCases(string cases, out IScript defaultScript, Element proc)
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
                        IScript script = ScriptFactory.CreateScript(caseScript, proc);

                        result.Add(new ExpressionGeneric(expr), script);
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
        private IFunctionGeneric m_expr;
        private Dictionary<IFunctionGeneric, IScript> m_cases;
        private IScript m_default;

        public SwitchScript(IFunctionGeneric expression, Dictionary<IFunctionGeneric, IScript> cases, IScript defaultScript)
        {
            m_expr = expression;
            m_cases = cases;
            m_default = defaultScript;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            object result = m_expr.Execute(c);
            bool success = false;

            foreach (IFunctionGeneric expr in m_cases.Keys)
            {
                // using .ToString() here as an object comparison of ints won't work
                if (result.ToString() == expr.Execute(c).ToString())
                {
                    m_cases[expr].Execute(c);
                    success = true;
                    break;
                }
            }

            if (!success && m_default != null)
            {
                m_default.Execute(c);
            }
        }

        public override string Save()
        {
            string result = SaveScript("switch", m_expr.Save()) + " {" + Environment.NewLine;
            foreach (KeyValuePair<IFunctionGeneric, IScript> caseItem in m_cases)
            {
                result += SaveScript("case", caseItem.Value, caseItem.Key.Save());
            }
            if (m_default != null) result += SaveScript("default", m_default);
            result += Environment.NewLine + "}";
            return result;
        }

        #endregion
    }

}
