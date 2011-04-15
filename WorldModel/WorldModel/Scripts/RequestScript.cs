using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class RequestScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "request"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new RequestScript(WorldModel, parameters[0], new Expression<string>(parameters[1], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
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

        public override void Execute(Context c)
        {
            m_worldModel.RaiseRequest(m_request, m_data.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("request", m_request.ToString(), m_data.Save());
        }

        public override string Keyword
        {
            get
            {
                return "request";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_request.ToString();
                case 1:
                    return m_data.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_request = (Request)(Enum.Parse(typeof(Request), (string)value));
                    break;
                case 1:
                    m_data = new Expression<string>((string)value, m_worldModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
