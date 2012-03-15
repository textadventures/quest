using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class ReturnScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "return"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new ReturnScript(WorldModel, new ExpressionGeneric(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class ReturnScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunctionGeneric m_returnValue;

        public ReturnScript(WorldModel worldModel, IFunctionGeneric returnValue)
        {
            m_worldModel = worldModel;
            m_returnValue = returnValue;
        }

        protected override ScriptBase CloneScript()
        {
            return new ReturnScript(m_worldModel, m_returnValue.Clone());
        }

        public override void Execute(Context c)
        {
            c.ReturnValue = m_returnValue.Execute(c);
        }

        public override string Save()
        {
            return SaveScript("return", m_returnValue.Save());
        }

        public override string Keyword
        {
            get
            {
                return "return";
            }
        }

        public override object GetParameter(int index)
        {
            return m_returnValue.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_returnValue = new ExpressionGeneric((string)value, m_worldModel);
        }
    }
}
