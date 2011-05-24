using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;
using System.Collections;

namespace AxeSoftware.Quest.Scripts
{
    public class InvokeScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "invoke"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            switch (parameters.Count)
            {
                case 1:
                    return new InvokeScript(WorldModel, new Expression<IScript>(parameters[0], WorldModel));
                case 2:
                    return new InvokeScript(WorldModel, new Expression<IScript>(parameters[0], WorldModel), new Expression<IDictionary>(parameters[2], WorldModel));
            }
            return null;
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1, 2 }; }
        }
    }

    public class InvokeScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<IScript> m_script;
        private IFunction<IDictionary> m_parameters = null;

        public InvokeScript(WorldModel worldModel, IFunction<IScript> script)
        {
            m_worldModel = worldModel;
            m_script = script;
        }

        public InvokeScript(WorldModel worldModel, IFunction<IScript> script, IFunction<IDictionary> parameters)
            : this(worldModel, script)
        {
            m_parameters = parameters;
        }

        protected override ScriptBase CloneScript()
        {
            return new InvokeScript(m_worldModel, m_script.Clone(), m_parameters == null ? null : m_parameters.Clone());
        }

        public override void Execute(Context c)
        {
            IScript script = m_script.Execute(c);
            if (m_parameters == null)
            {
                m_worldModel.RunScript(script);
            }
            else
            {
                m_worldModel.RunScript(script, new Parameters(m_parameters.Execute(c)));
            }
        }

        public override string Save()
        {
            string parameters = (m_parameters == null) ? null : m_parameters.Save();
            if (!string.IsNullOrEmpty(parameters))
            {
                return SaveScript("invoke", m_script.Save(), m_parameters.Save());
            }
            else
            {
                return SaveScript("invoke", m_script.Save(), m_parameters.Save());
            }
        }

        public override string Keyword
        {
            get
            {
                return "invoke";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_script.Save();
                case 1:
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
                    m_script = new Expression<IScript>((string)value, m_worldModel);
                    break;
                case 1:
                    m_parameters = new Expression<IDictionary>((string)value, m_worldModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }


}
