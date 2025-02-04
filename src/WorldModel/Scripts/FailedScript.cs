using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    internal class FailedScript : ScriptBase
    {
        private string m_script;

        public FailedScript(string script)
        {
            m_script = script;
        }

        protected override ScriptBase CloneScript()
        {
            return new FailedScript(m_script);
        }

        public override void Execute(Context c)
        {
            throw new NotImplementedException();
        }

        public override string Save()
        {
            return m_script;
        }

        public override void SetParameterInternal(int index, object value)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            m_script = (string)value;
        }

        public override object GetParameter(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return m_script;
        }

        public override string Keyword
        {
            get { return "@failed"; }
        }
    }
}
