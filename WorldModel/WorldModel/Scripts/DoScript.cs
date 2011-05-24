using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;
using System.Collections;

namespace AxeSoftware.Quest.Scripts
{
    public class DoScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "do"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            switch (parameters.Count)
            {
                case 2:
                    return new DoActionScript(WorldModel, new Expression<Element>(parameters[0], WorldModel), new Expression<string>(parameters[1], WorldModel));
                case 3:
                    return new DoActionScript(WorldModel, new Expression<Element>(parameters[0], WorldModel), new Expression<string>(parameters[1], WorldModel), new Expression<IDictionary>(parameters[2], WorldModel));
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
        private WorldModel m_worldModel;
        private IFunction<Element> m_obj;
        private IFunction<string> m_action;
        private IFunction<IDictionary> m_parameters = null;

        public DoActionScript(WorldModel worldModel, IFunction<Element> obj, IFunction<string> action)
        {
            m_worldModel = worldModel;
            m_obj = obj;
            m_action = action;
        }

        public DoActionScript(WorldModel worldModel, IFunction<Element> obj, IFunction<string> action, IFunction<IDictionary> parameters)
            : this(worldModel, obj, action)
        {
            m_parameters = parameters;
        }

        protected override ScriptBase CloneScript()
        {
            return new DoActionScript(m_worldModel, m_obj.Clone(), m_action.Clone(), m_parameters == null ? null : m_parameters.Clone());
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
                    m_obj = new Expression<Element>((string)value, m_worldModel);
                    break;
                case 1:
                    m_action = new Expression<string>((string)value, m_worldModel);
                    break;
                case 2:
                    m_parameters = new Expression<IDictionary>((string)value, m_worldModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

   
}
