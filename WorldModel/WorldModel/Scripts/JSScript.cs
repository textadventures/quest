using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class JSScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "JS."; }
        }

        private static Regex s_jsFunctionName = new Regex(@"^JS\.([\w\.\@]*)");

        public IScript Create(string script, ScriptContext scriptContext)
        {
            var param = Utility.GetParameter(script);

            List<IFunctionGeneric> expressions = null;

            if (param != null)
            {
                var parameters = Utility.SplitParameter(param);
                if (parameters.Count != 1 || parameters[0].Trim().Length != 0)
                {
                    expressions = new List<IFunctionGeneric>(parameters.Select(p => new ExpressionGeneric(p, scriptContext)));
                }
            }

            if (!s_jsFunctionName.IsMatch(script))
            {
                throw new Exception(string.Format("Invalid JS function name in '{0}'", script));
            }

            var functionName = s_jsFunctionName.Match(script).Groups[1].Value;

            bool outputLog = false;

            if (functionName.StartsWith("output@"))
            {
                outputLog = true;
                functionName = functionName.Substring(7);
            }

            return new JSScript(scriptContext, functionName, outputLog, expressions);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }
    }

    public class JSScript : ScriptBase
    {
        private readonly ScriptContext m_scriptContext;
        private readonly string m_function;
        private readonly bool m_outputLog;
        private readonly List<IFunctionGeneric> m_parameters;

        public JSScript(ScriptContext scriptContext, string function, bool outputLog, List<IFunctionGeneric> parameters)
        {
            m_scriptContext = scriptContext;
            m_function = function;
            m_outputLog = outputLog;
            m_parameters = parameters;
        }

        protected override ScriptBase CloneScript()
        {
            throw new NotImplementedException();
        }

        public override void Execute(Context c)
        {
            if (m_parameters != null)
            {
                var paramValues = m_parameters.Select(p => p.Execute(c));
                m_scriptContext.WorldModel.PlayerUI.RunScript(m_function, paramValues.ToArray());
                if (m_outputLog)
                {
                    m_scriptContext.WorldModel.OutputLogger.RunJavaScript(m_function, paramValues.ToArray());
                }
            }
            else
            {
                m_scriptContext.WorldModel.PlayerUI.RunScript(m_function, null);
                if (m_outputLog)
                {
                    m_scriptContext.WorldModel.OutputLogger.RunJavaScript(m_function, null);
                }
            }
        }

        public override string Save()
        {
            string functionName = (m_outputLog ? "output@" : "") + m_function;
            return SaveScript("JS." + functionName, m_parameters == null ? new[] { string.Empty } : m_parameters.Select(p => p.Save()).ToArray());
        }

        public override void SetParameterInternal(int index, object value)
        {
            throw new NotImplementedException();
        }

        public override object GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public override string Keyword
        {
            get { throw new NotImplementedException(); }
        }
    }
}
