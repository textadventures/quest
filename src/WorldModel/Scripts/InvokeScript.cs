using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class InvokeScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "invoke"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            switch (parameters.Count)
            {
                case 1:
                    return new InvokeScript(scriptContext, new Expression<IScript>(parameters[0], scriptContext));
                case 2:
                    return new InvokeScript(scriptContext, new Expression<IScript>(parameters[0], scriptContext), new Expression<IDictionary>(parameters[1], scriptContext));
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
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<IScript> m_script;
        private IFunction<IDictionary> m_parameters = null;

        public InvokeScript(ScriptContext scriptContext, IFunction<IScript> script)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_script = script;
        }

        public InvokeScript(ScriptContext scriptContext, IFunction<IScript> script, IFunction<IDictionary> parameters)
            : this(scriptContext, script)
        {
            m_parameters = parameters;
        }

        protected override ScriptBase CloneScript()
        {
            return new InvokeScript(m_scriptContext, m_script.Clone(), m_parameters == null ? null : m_parameters.Clone());
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
            if (string.IsNullOrEmpty(parameters))
            {
                return SaveScript("invoke", m_script.Save());
            }
            else
            {
                return SaveScript("invoke", m_script.Save(), parameters);
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
                    m_script = new Expression<IScript>((string)value, m_scriptContext);
                    break;
                case 1:
                    m_parameters = new Expression<IDictionary>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }


}
