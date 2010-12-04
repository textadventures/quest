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
            return new ErrorScript(new ExpressionGeneric(parameters[0], WorldModel));
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

        public ErrorScript(IFunctionGeneric function)
        {
            m_function = function;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            object result = m_function.Execute(c);
            throw new Exception(result.ToString());
        }

        public override string Save()
        {
            return SaveScript("error", m_function.Save());
        }

        #endregion
    }
}
