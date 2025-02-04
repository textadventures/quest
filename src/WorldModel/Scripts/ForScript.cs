using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class ForScriptConstructor : IScriptConstructor
    {
        #region IScriptConstructor Members

        public string Keyword
        {
            get { return "for"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string loop = Utility.GetScript(afterExpr);

            string[] parameters = Utility.SplitParameter(param).ToArray();
            IScript loopScript = ScriptFactory.CreateScript(loop);

            if (parameters.Count() == 3)
            {
                return new ForScript(scriptContext, ScriptFactory, parameters[0], new Expression<int>(parameters[1], scriptContext), new Expression<int>(parameters[2], scriptContext), loopScript);
            }
            else if (parameters.Count() == 4)
            {
                return new ForScript(scriptContext, ScriptFactory, parameters[0], new Expression<int>(parameters[1], scriptContext), new Expression<int>(parameters[2], scriptContext), new Expression<int>(parameters[3], scriptContext), loopScript);
            }
            else
            {
                throw new Exception(string.Format("'for' script should have 3 or 4 parameters: 'for ({0})'", param));
            }
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        #endregion
    }

    public class ForScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private IFunction<int> m_from;
        private IFunction<int> m_to;
        private IFunction<int> m_step;
        private IScript m_loopScript;
        private string m_variable;
        private WorldModel m_worldModel;
        private IScriptFactory m_scriptFactory;

        public ForScript(ScriptContext scriptContext, IScriptFactory scriptFactory, string variable, IFunction<int> from, IFunction<int> to, IScript loopScript)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_scriptFactory = scriptFactory;
            m_variable = variable;
            m_from = from;
            m_to = to;
            m_loopScript = loopScript;
        }

        public ForScript(ScriptContext scriptContext, IScriptFactory scriptFactory, string variable, IFunction<int> from, IFunction<int> to, IFunction<int> step, IScript loopScript)
            : this(scriptContext, scriptFactory, variable, from, to, loopScript)
        {
            m_step = step;
        }

        protected override ScriptBase CloneScript()
        {
            return new ForScript(m_scriptContext, m_scriptFactory, m_variable, m_from.Clone(), m_to.Clone(), m_step == null ? null : m_step.Clone(), (IScript)m_loopScript.Clone());
        }

        protected override void ParentUpdated()
        {
            m_loopScript.Parent = Parent;
        }

        public override void Execute(Context c)
        {
            int from = m_from.Execute(c);
            int to = m_to.Execute(c);
            int step = m_step == null ? 1 : m_step.Execute(c);
            int count;
            c.Parameters[m_variable] = 0;

            for (count = from; (step > 0 && count <= to) || (step < 0 && count >= to); count += step)
            {
                c.Parameters[m_variable] = count;
                m_loopScript.Execute(c);
                if (c.IsReturned) break;

                object newCount = c.Parameters[m_variable];
                if (newCount is int)
                {
                    count = (int)newCount;
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
            if (m_step == null)
            {
                return SaveScript("for", m_loopScript, m_variable, m_from.Save(), m_to.Save());
            }
            else
            {
                return SaveScript("for", m_loopScript, m_variable, m_from.Save(), m_to.Save(), m_step.Save());
            }
        }

        public override string Keyword
        {
            get
            {
                return "for";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_variable;
                case 1:
                    return m_from.Save();
                case 2:
                    return m_to.Save();
                case 3:
                    return m_step == null ? null : m_step.Save();
                case 4:
                    return m_loopScript;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_variable = (string)value;
                    break;
                case 1:
                    m_from = new Expression<int>((string)value, m_scriptContext);
                    break;
                case 2:
                    m_to = new Expression<int>((string)value, m_scriptContext);
                    break;
                case 3:
                    m_step = new Expression<int>((string)value, m_scriptContext);
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
            return new List<string> { m_variable };
        }
    }
}
