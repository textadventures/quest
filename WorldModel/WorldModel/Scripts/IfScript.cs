using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class IfScriptConstructor : IScriptConstructor
    {
        #region IScriptConstructor Members

        public string Keyword
        {
            get { return "if"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string expr = Utility.GetParameter(script, out afterExpr);
            string then = Utility.GetScript(afterExpr);

            IScript thenScript = ScriptFactory.CreateScript(then, proc);

            return new IfScript(new Expression<bool>(expr), thenScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        #endregion

        public void AddElse(IScript script, string elseScript, Element proc)
        {
            IScript add = GetElse(elseScript, proc);
            ((IfScript)script).SetElse(add);
        }

        public void AddElseIf(IScript script, string elseIfScript, Element proc)
        {
            IScript add = GetElse(elseIfScript, proc);
            ((IfScript)script).AddElseIf(add);
        }

        private IScript GetElse(string elseScript, Element proc)
        {
            elseScript = Utility.GetTextAfter(elseScript, "else");
            return ScriptFactory.CreateScript(elseScript, proc);
        }
    }

    public class IfScript : ScriptBase
    {
        private IFunction<bool> m_expression;
        private IScript m_thenScript;
        private IScript m_elseScript;
        private List<IScript> m_elseIfScript;
        
        public IfScript(IFunction<bool> expression, IScript thenScript)
            : this(expression, thenScript, null)
        {
        }

        public IfScript(IFunction<bool> expression, IScript thenScript, IScript elseScript)
        {
            m_expression = expression;
            m_thenScript = thenScript;
            m_elseScript = elseScript;
        }

        public void SetElse(IScript elseScript)
        {
            m_elseScript = elseScript;
        }

        public void AddElseIf(IScript elseIfScript)
        {
            if (m_elseIfScript == null) m_elseIfScript = new List<IScript>();
            m_elseIfScript.Add(elseIfScript);
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            if (m_expression.Execute(c))
            {
                m_thenScript.Execute(c);
            }
            else
            {
                if (m_elseIfScript != null)
                {
                    foreach (IScript elseIfScript in m_elseIfScript)
                    {
                        // All IScripts in m_elseIfScript are just simple
                        // IfScript objects with only one "if". If any of them run
                        // successfully then we're finished with this if block.
                        if (((IfScript)elseIfScript).ExecuteWithResult(c)) return;
                    }
                }

                if (m_elseScript != null) m_elseScript.Execute(c);
            }
        }

        public override string Save()
        {
            string result = SaveScript("if", m_thenScript, m_expression.Save());
            if (m_elseIfScript != null)
            {
                foreach (IScript elseIf in m_elseIfScript)
                {
                    result += Environment.NewLine + "else " + elseIf.Save();
                }
            }
            if (m_elseScript != null) result += "else {" + Environment.NewLine + m_elseScript.Save() + Environment.NewLine + "}";
            return result;
        }

        public override string Keyword
        {
            get
            {
                return "if";
            }
        }

        public override string GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    // expression
                    return m_expression.Save();
                case 1:
                    // "then" script
                    return m_thenScript.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, string value)
        {
            base.SetParameterInternal(index, value);
        }

        #endregion

        private bool ExecuteWithResult(Context c)
        {
            if (m_expression.Execute(c))
            {
                m_thenScript.Execute(c);
                return true;
            }
            return false;
        }
    }
}
