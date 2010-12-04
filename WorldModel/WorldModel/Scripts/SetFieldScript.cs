using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class SetFieldScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "set"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new SetFieldScript(WorldModel, new Expression<Element>(parameters[0], WorldModel), new Expression<string>(parameters[1], WorldModel), new Expression<object>(parameters[2], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3 }; }
        }
        #endregion
    }

    public class SetFieldScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<Element> m_obj;
        private IFunction<string> m_field;
        private IFunction<object> m_value;

        public SetFieldScript(WorldModel worldModel, IFunction<Element> obj, IFunction<string> field, IFunction<object> value)
        {
            m_worldModel = worldModel;
            m_obj = obj;
            m_field = field;
            m_value = value;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            Element obj = m_obj.Execute(c);
            obj.Fields.Set(m_field.Execute(c), m_value.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("set", m_obj.Save(), m_field.Save(), m_value.Save());
        }

        #endregion
    }
}
