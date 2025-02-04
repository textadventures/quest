using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class ShowMenuScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "show menu"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string callback = Utility.GetScript(afterExpr);

            string[] parameters = Utility.SplitParameter(param).ToArray();
            if (parameters.Count() != 3)
            {
                throw new Exception(string.Format("'show menu' script should have 3 parameters: 'show menu ({0})'", param));
            }
            IScript callbackScript = ScriptFactory.CreateScript(callback);

            return new ShowMenuScript(scriptContext, ScriptFactory, new Expression<string>(parameters[0], scriptContext), new ExpressionGeneric(parameters[1], scriptContext), new Expression<bool>(parameters[2], scriptContext), callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }
    }

    public class ShowMenuScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_caption;
        private IFunctionGeneric m_options;
        private IFunction<bool> m_allowCancel;
        private IScript m_callbackScript;
        private IScriptFactory m_scriptFactory;

        public ShowMenuScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IFunction<string> caption, IFunctionGeneric options, IFunction<bool> allowCancel, IScript callbackScript)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_scriptFactory = scriptFactory;
            m_caption = caption;
            m_options = options;
            m_allowCancel = allowCancel;
            m_callbackScript = callbackScript;
        }

        protected override ScriptBase CloneScript()
        {
            return new ShowMenuScript(m_scriptContext, m_scriptFactory, m_caption.Clone(), m_options.Clone(), m_allowCancel.Clone(), (IScript)m_callbackScript.Clone());
        }

        public override void Execute(Context c)
        {
            object options = m_options.Execute(c);
            IList<string> stringListOptions = options as IList<string>;
            IDictionary<string, string> stringDictionaryOptions = options as IDictionary<string, string>;

            if (stringListOptions != null)
            {
                if (stringListOptions.Count == 0)
                {
                    throw new Exception("No menu options specified");
                }
                m_worldModel.DisplayMenuAsync(m_caption.Execute(c), stringListOptions, m_allowCancel.Execute(c), m_callbackScript, c);
            }
            else if (stringDictionaryOptions != null)
            {
                if (stringDictionaryOptions.Count == 0)
                {
                    throw new Exception("No menu options specified");
                }
                m_worldModel.DisplayMenuAsync(m_caption.Execute(c), stringDictionaryOptions, m_allowCancel.Execute(c), m_callbackScript, c);
            }
            else
            {
                throw new Exception("Unknown menu options type");
            }
        }

        public override string Save()
        {
            return SaveScript("show menu", m_callbackScript, m_caption.Save(), m_options.Save(), m_allowCancel.Save());
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_caption.Save();
                case 1:
                    return m_options.Save();
                case 2:
                    return m_allowCancel.Save();
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
                    m_caption = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 1:
                    m_options = new ExpressionGeneric((string)value, m_scriptContext);
                    break;
                case 2:
                    m_allowCancel = new Expression<bool>((string)value, m_scriptContext);
                    break;
                case 3:
                    // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                    throw new InvalidOperationException("Attempt to use SetParameter to change the script of a 'show menu' command");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string Keyword
        {
            get
            {
                return "show menu";
            }
        }
    }
}
