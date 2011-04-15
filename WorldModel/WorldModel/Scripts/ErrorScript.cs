using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class ErrorScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "error"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new ErrorScript(WorldModel, new ExpressionGeneric(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
        #endregion
    }

    public class ErrorScript : ScriptBase
    {
        private IFunctionGeneric m_function;
        private WorldModel m_worldModel;

        public ErrorScript(WorldModel worldModel, IFunctionGeneric function)
        {
            m_function = function;
            m_worldModel = worldModel;
        }

        public override void Execute(Context c)
        {
            object result = m_function.Execute(c);
            throw new Exception(result.ToString());
        }

        public override string Save()
        {
            return SaveScript("error", m_function.Save());
        }

        public override string Keyword
        {
            get
            {
                return "error";
            }
        }

        public override object GetParameter(int index)
        {
            return m_function.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_function = new ExpressionGeneric((string)value, m_worldModel);
        }
    }
}
