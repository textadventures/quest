using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class MsgScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "msg"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new MsgScript(WorldModel, new ExpressionGeneric(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
        #endregion
    }

    public class MsgScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunctionGeneric m_function;

        public MsgScript(WorldModel worldModel, IFunctionGeneric function)
        {
            m_worldModel = worldModel;
            m_function = function;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            object result = m_function.Execute(c);
            m_worldModel.Print(result.ToString());
        }

        #endregion

        public override string Save()
        {
            return SaveScript("msg", m_function.Save());
        }

        public override object GetParameter(int index)
        {
            return m_function.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_function = new ExpressionGeneric((string)value, m_worldModel);
        }

        public override string Keyword
        {
            get
            {
                return "msg";
            }
        }
    }
}
