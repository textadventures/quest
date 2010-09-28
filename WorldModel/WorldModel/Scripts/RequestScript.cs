using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class RequestScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "request"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new RequestScript(WorldModel, parameters[0], new Expression<string>(parameters[1]));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
        #endregion
    }

    public class RequestScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private Request m_request;
        private IFunction<string> m_data;

        public RequestScript(WorldModel worldModel, string request, IFunction<string> data)
        {
            m_worldModel = worldModel;
            m_data = data;
            m_request = (Request)(Enum.Parse(typeof(Request), request));
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            m_worldModel.RaiseRequest(m_request, m_data.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("request", m_request.ToString(), m_data.Save());
        }

        #endregion
    }
}
