using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class ForScriptConstructor : IScriptConstructor
    {
        #region IScriptConstructor Members

        public string Keyword
        {
            get { return "for"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string loop = Utility.GetScript(afterExpr);
            
            string[] parameters = Utility.SplitParameter(param).ToArray();
            if (parameters.Count() != 3)
            {
                throw new Exception(string.Format("'for' script should have 3 parameters: 'for ({0})'", param));
            }
            IScript loopScript = ScriptFactory.CreateScript(loop);

            return new ForScript(parameters[0], new Expression<int>(parameters[1], WorldModel), new Expression<int>(parameters[2], WorldModel), loopScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        #endregion
    }

    public class ForScript : ScriptBase
    {
        private IFunction<int> m_from;
        private IFunction<int> m_to;
        private IScript m_loopScript;
        private string m_variable;

        public ForScript(string variable, IFunction<int> from, IFunction<int> to, IScript loopScript)
        {
            m_variable = variable;
            m_from = from;
            m_to = to;
            m_loopScript = loopScript;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            int from = m_from.Execute(c);
            int to = m_to.Execute(c);
            int count;
            c.Parameters[m_variable] = 0;

            for (count = from; count <= to; count++)
            {
                c.Parameters[m_variable] = count;
                m_loopScript.Execute(c);
            }
        }

        public override string Save()
        {
            return SaveScript("for", m_loopScript, m_variable, m_from.Save(), m_to.Save());
        }

        #endregion
    }
}
