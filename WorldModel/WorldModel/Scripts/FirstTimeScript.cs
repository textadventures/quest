using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class FirstTimeScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "firsttime"; }
        }

        public IScript Create(string script, Element proc)
        {
            // Get script after "firsttime" keyword
            script = script.Substring(9).Trim();
            string firstTime = Utility.GetScript(script);
            IScript firstTimeScript = ScriptFactory.CreateScript(firstTime);

            return new FirstTimeScript(WorldModel, ScriptFactory, firstTimeScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }
    }

    public class FirstTimeScript : ScriptBase
    {
        private IScript m_firstTimeScript;
        private WorldModel m_worldModel;
        private IScriptFactory m_scriptFactory;
        private bool m_hasRun = false;

        public FirstTimeScript(WorldModel worldModel, IScriptFactory scriptFactory, IScript firstTimeScript)
        {
            m_worldModel = worldModel;
            m_scriptFactory = scriptFactory;
            m_firstTimeScript = firstTimeScript;
        }

        protected override ScriptBase CloneScript()
        {
            return new FirstTimeScript(m_worldModel, m_scriptFactory, (IScript)m_firstTimeScript.Clone());
        }

        protected override void ParentUpdated()
        {
            m_firstTimeScript.Parent = Parent;
        }

        public override void Execute(Context c)
        {
            if (!m_hasRun)
            {
                m_hasRun = true;
                m_firstTimeScript.Execute(c);
            }
        }

        public override string Save()
        {
            return SaveScript("firsttime", m_firstTimeScript);
        }

        public override string Keyword
        {
            get
            {
                return "firsttime";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_firstTimeScript;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                    throw new InvalidOperationException("Attempt to use SetParameter to change the script of a 'firsttime' script");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
