using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class DoScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "do"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            switch (parameters.Count)
            {
                case 2:
                    return new DoActionScript(scriptContext, new Expression<Element>(parameters[0], scriptContext), new Expression<string>(parameters[1], scriptContext));
                case 3:
                    return new DoActionScript(scriptContext, new Expression<Element>(parameters[0], scriptContext), new Expression<string>(parameters[1], scriptContext), new Expression<IDictionary>(parameters[2], scriptContext));
            }
            return null;
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2, 3 }; }
        }
        #endregion
    }

    public class DoActionScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<Element> m_obj;
        private IFunction<string> m_action;
        private IFunction<IDictionary> m_parameters = null;

        public DoActionScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> action)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_obj = obj;
            m_action = action;
        }

        public DoActionScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> action, IFunction<IDictionary> parameters)
            : this(scriptContext, obj, action)
        {
            m_parameters = parameters;
        }

        protected override ScriptBase CloneScript()
        {
            return new DoActionScript(m_scriptContext, m_obj.Clone(), m_action.Clone(), m_parameters == null ? null : m_parameters.Clone());
        }

        public override void Execute(Context c)
        {
            Element obj = m_obj.Execute(c);
            IScript action = obj.GetAction(m_action.Execute(c));
            if (m_parameters == null)
            {
                m_worldModel.RunScript(action, obj);
            }
            else
            {
                m_worldModel.RunScript(action, new Parameters(m_parameters.Execute(c)), obj);
            }
        }

        public override string Save()
        {
            string parameters = (m_parameters == null) ? null : m_parameters.Save();
            if (!string.IsNullOrEmpty(parameters))
            {
                return SaveScript("do", m_obj.Save(), m_action.Save(), m_parameters.Save());
            }
            else
            {
                return SaveScript("do", m_obj.Save(), m_action.Save());
            }
        }

        public override string Keyword
        {
            get
            {
                return "do";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_obj.Save();
                case 1:
                    return m_action.Save();
                case 2:
                    return m_parameters == null ? null : m_parameters.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_obj = new Expression<Element>((string)value, m_scriptContext);
                    break;
                case 1:
                    m_action = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 2:
                    m_parameters = new Expression<IDictionary>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

   
}
