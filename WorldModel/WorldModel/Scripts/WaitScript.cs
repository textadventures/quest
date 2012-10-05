using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class WaitScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "wait"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            string callback = Utility.GetScript(script.Substring(Keyword.Length).Trim());

            IScript callbackScript = ScriptFactory.CreateScript(callback);

            return new WaitScript(WorldModel, ScriptFactory, callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }
        public WorldModel WorldModel { get; set; }
    }

    public class WaitScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IScript m_callbackScript;
        private IScriptFactory m_scriptFactory;

        public WaitScript(WorldModel worldModel, IScriptFactory scriptFactory, IScript callbackScript)
        {
            m_worldModel = worldModel;
            m_scriptFactory = scriptFactory;
            m_callbackScript = callbackScript;
        }

        protected override ScriptBase CloneScript()
        {
            return new WaitScript(m_worldModel, m_scriptFactory, (IScript)m_callbackScript.Clone());
        }

        public override void Execute(Context c)
        {
            m_worldModel.StartWaitAsync(m_callbackScript, c);
        }

        public override string Save()
        {
            return SaveScript("wait", m_callbackScript);
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_callbackScript;
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
                    throw new InvalidOperationException("Attempt to use SetParameter to change the script of a 'wait' command");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string Keyword
        {
            get
            {
                return "wait";
            }
        }
    }
}
