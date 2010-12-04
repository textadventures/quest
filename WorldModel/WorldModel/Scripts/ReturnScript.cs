using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class ReturnScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "return"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            throw new Exception("Invalid constructor for 'return' script");
        }

        protected override IScript CreateInt(List<string> parameters, Element proc)
        {
            return new ReturnScript(WorldModel, new ExpressionGeneric(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }

        protected override bool RequireProcedure
        {
            get
            {
                return true;
            }
        }
        #endregion
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

        #region IScript Members

        public override void Execute(Context c)
        {
            c.ReturnValue = m_returnValue.Execute(c);
        }

        public override string Save()
        {
            return SaveScript("return", m_returnValue.Save());
        }

        #endregion
    }
}
