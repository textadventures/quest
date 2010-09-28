using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest.Scripts
{
    public abstract class ScriptConstructorBase : IScriptConstructor
    {
        #region IScriptConstructor Members

        public abstract string Keyword { get; }

        public IScript Create(string script, Element proc)
        {
            List<string> parameters = null;
            string param = Utility.GetParameter(script);

            int numParams;
            if (param == null)
            {
                numParams = 0;
            }
            else
            {
                parameters = Utility.SplitParameter(param);
                numParams = parameters.Count;
            }

            if (ExpectedParameters.Count() > 0)
            {
                if (!ExpectedParameters.Contains(numParams))
                {
                    throw new Exception(string.Format("Expected {0} parameter(s)", FormatExpectedParameters(), script));
                }
            }

            if (!RequireProcedure)
            {
                return CreateInt(parameters);
            }
            else
            {
                return CreateInt(parameters, proc);
            }
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        #endregion

        protected abstract IScript CreateInt(List<string> parameters);

        protected virtual IScript CreateInt(List<string> parameters, Element proc)
        {
            // only overridden for "return" script
            return null;
        }

        protected virtual bool RequireProcedure
        {
            get { return false; }
        }

        protected abstract int[] ExpectedParameters { get; }

        private string FormatExpectedParameters()
        {
            string result = "";
            foreach (int i in ExpectedParameters)
            {
                result += i.ToString() + ",";
            }
            return result.Substring(0, result.Length - 1);
        }

    }
}
