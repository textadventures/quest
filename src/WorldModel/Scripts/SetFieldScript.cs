using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class SetFieldScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "set"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new SetFieldScript(scriptContext, new Expression<Element>(parameters[0], scriptContext), new Expression<string>(parameters[1], scriptContext), new Expression<object>(parameters[2], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3 }; }
        }
    }

    public class SetFieldScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<Element> m_obj;
        private IFunction<string> m_field;
        private IFunction<object> m_value;

        public SetFieldScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> field, IFunction<object> value)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_obj = obj;
            m_field = field;
            m_value = value;
        }

        protected override ScriptBase CloneScript()
        {
            return new SetFieldScript(m_scriptContext, m_obj.Clone(), m_field.Clone(), m_value.Clone());
        }

        public override void Execute(Context c)
        {
            Element obj = m_obj.Execute(c);
            obj.Fields.Set(m_field.Execute(c), m_value.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("set", m_obj.Save(), m_field.Save(), m_value.Save());
        }

        public override string Keyword
        {
            get
            {
                return "set";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_obj.Save();
                case 1:
                    return m_field.Save();
                case 2:
                    return m_value.Save();
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
                    m_field = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 2:
                    m_value = new Expression<object>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
