using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class ForEachScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "foreach"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string loop = Utility.GetScript(afterExpr);

            string[] parameters = Utility.SplitParameter(param).ToArray();
            if (parameters.Count() != 2)
            {
                throw new Exception(string.Format("'foreach' script should have 2 parameters: 'foreach ({0})'", param));
            }
            IScript loopScript = ScriptFactory.CreateScript(loop);

            return new ForEachScript(scriptContext, parameters[0], new ExpressionGeneric(parameters[1], scriptContext), loopScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }
    }

    public class ForEachScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private IFunctionGeneric m_list;
        private IScript m_loopScript;
        private string m_variable;

        public ForEachScript(ScriptContext scriptContext, string variable, IFunctionGeneric list, IScript loopScript)
        {
            m_scriptContext = scriptContext;
            m_variable = variable;
            m_list = list;
            m_loopScript = loopScript;
        }

        protected override ScriptBase CloneScript()
        {
            return new ForEachScript(m_scriptContext, m_variable, m_list.Clone(), (IScript)m_loopScript.Clone());
        }

        public override void Execute(Context c)
        {
            object result = m_list.Execute(c);
            IEnumerable resultList = null;

            // Cannot foreach over strings as of Quest 5.3, as the Char data type is not supported (retained functionality
            // for pre-5.3 to prevent breaking existing scripts)

            if (m_scriptContext.WorldModel.Version < WorldModelVersion.v530 || !(result is string))
            {
                var resultDictionary = result as IDictionary;
                if (resultDictionary != null)
                {
                    resultList = resultDictionary.Keys;
                }
                else
                {
                    resultList = result as IEnumerable;
                }
            }

            if (resultList == null) throw new Exception(string.Format("Cannot foreach over '{0}' as it is not a list", result));

            foreach (object variable in resultList)
            {
                c.Parameters[m_variable] = variable;
                m_loopScript.Execute(c);
                if (c.IsReturned) break;
            }
        }

        public override string Save()
        {
            return SaveScript("foreach", m_loopScript, m_variable, m_list.Save());
        }

        public override string Keyword
        {
            get
            {
                return "foreach";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_variable;
                case 1:
                    return m_list.Save();
                case 2:
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
                    m_list = new ExpressionGeneric((string)value, m_scriptContext);
                    break;
                case 2:
                    throw new InvalidOperationException("Attempt to use SetParameter to change the script of a 'foreach' loop");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
