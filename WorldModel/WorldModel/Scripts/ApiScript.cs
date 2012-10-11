using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class ApiScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "api"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string callback = Utility.GetScript(afterExpr);

            string[] parameters = Utility.SplitParameter(param).ToArray();
            if (parameters.Count() != 3)
            {
                throw new Exception(string.Format("'api' script should have 3 parameters: 'ask ({0})'", param));
            }
            IScript callbackScript = ScriptFactory.CreateScript(callback);

            return new ApiScript(scriptContext, new Expression<string>(parameters[0], scriptContext), new Expression<string>(parameters[1], scriptContext), new ExpressionGeneric(parameters[2], scriptContext), callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }
        public WorldModel WorldModel { get; set; }
    }

    public class ApiScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_api;
        private IFunction<string> m_method;
        private IFunctionGeneric m_parameters;
        private IScript m_callbackScript;

        public ApiScript(ScriptContext scriptContext, IFunction<string> api, IFunction<string> method, IFunctionGeneric parameters, IScript callbackScript)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_api = api;
            m_method = method;
            m_parameters = parameters;
            m_callbackScript = callbackScript;
        }

        protected override ScriptBase CloneScript()
        {
            return new ApiScript(m_scriptContext, m_api.Clone(), m_method.Clone(), m_parameters.Clone(), (IScript)m_callbackScript.Clone());
        }

        public override void Execute(Context c)
        {
            QuestList<string> parameters = m_parameters.Execute(c) as QuestList<string>;
            m_worldModel.API.Execute(m_api.Execute(c), m_method.Execute(c), parameters, m_callbackScript);
        }

        public override string Save()
        {
            return SaveScript("api", m_callbackScript, m_api.Save(), m_method.Save(), m_parameters.Save());
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_api.Save();
                case 1:
                    return m_method.Save();
                case 2:
                    return m_parameters.Save();
                case 3:
                    return m_callbackScript;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_api = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 1:
                    m_method = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 2:
                    m_parameters = new ExpressionGeneric((string)value, m_scriptContext);
                    break;
                case 3:
                    // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                    throw new InvalidOperationException("Attempt to use SetParameter to change the script of an 'api' command");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string Keyword
        {
            get
            {
                return "api";
            }
        }
    }
}
