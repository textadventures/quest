/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class PopulateScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public PopulateScriptConstructor()
        {
        }

        public override string Keyword
        {
            get { return "populate"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new PopulateScript(new Expression<string>(parameters[0]), new Expression<string>(parameters[1]));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
        #endregion
    }

    public class PopulateScript : ScriptBase
    {
        private IFunction<string> m_regex;
        private IFunction<string> m_input;

        public PopulateScript(IFunction<string> regex, IFunction<string> input)
        {
            m_regex = regex;
            m_input = input;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            // actually this is premature, we don't want to set "object" and "exit" vars to what the player typed
            // in, we need to disambiguate first! "object" and "exit" are direct references!


            string command = m_input.Execute(c);
            string regexPattern = m_regex.Execute(c);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern);

            if (!regex.IsMatch(command))
            {
                throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", command, regexPattern));
            }

            foreach (string groupName in regex.GetGroupNames())
            {
                string groupMatch = regex.Match(command).Groups[groupName].Value;
                c.Parameters.Add(groupName, groupMatch);
            }
        }

        #endregion
    }
}
*/