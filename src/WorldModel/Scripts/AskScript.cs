using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class AskScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "ask"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string callback = Utility.GetScript(afterExpr);

            string[] parameters = Utility.SplitParameter(param).ToArray();
            if (parameters.Count() != 1)
            {
                throw new Exception(string.Format("'ask' script should have 1 parameter: 'ask ({0})'", param));
            }
            IScript callbackScript = ScriptFactory.CreateScript(callback);

            return new AskScript(scriptContext, ScriptFactory, new Expression<string>(parameters[0], scriptContext), callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }
    }

    public class AskScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_caption;
        private IScript m_callbackScript;
        private IScriptFactory m_scriptFactory;

        public AskScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IFunction<string> caption, IScript callbackScript)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_scriptFactory = scriptFactory;
            m_caption = caption;
            m_callbackScript = callbackScript;
        }

        protected override ScriptBase CloneScript()
        {
            return new AskScript(m_scriptContext, m_scriptFactory, m_caption.Clone(), (IScript)m_callbackScript.Clone());
        }

        public override void Execute(Context c)
        {
            m_worldModel.ShowQuestionAsync(m_caption.Execute(c), m_callbackScript, c);
        }

        public override string Save()
        {
            return SaveScript("ask", m_callbackScript, m_caption.Save());
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_caption.Save();
                case 1:
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
                    m_caption = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 1:
                    // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                    throw new InvalidOperationException("Attempt to use SetParameter to change the script of an 'ask' command");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string Keyword
        {
            get
            {
                return "ask";
            }
        }
    }
}
