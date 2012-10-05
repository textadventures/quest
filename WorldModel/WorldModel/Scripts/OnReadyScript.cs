using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class OnReadyScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "on ready"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            string callback = Utility.GetScript(script.Substring(Keyword.Length).Trim());
            IScript callbackScript = ScriptFactory.CreateScript(callback);
            return new OnReadyScript(scriptContext, ScriptFactory, callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }
    }

    public class OnReadyScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IScript m_callbackScript;
        private IScriptFactory m_scriptFactory;

        public OnReadyScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IScript callbackScript)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_scriptFactory = scriptFactory;
            m_callbackScript = callbackScript;
        }

        protected override ScriptBase CloneScript()
        {
            return new OnReadyScript(m_scriptContext, m_scriptFactory, (IScript)m_callbackScript.Clone());
        }

        public override void Execute(Context c)
        {
            m_worldModel.AddOnReady(m_callbackScript, c);
        }

        public override string Save()
        {
            return SaveScript("on ready", m_callbackScript);
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
                    throw new InvalidOperationException("Attempt to use SetParameter to change the script of an 'on ready' command");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string Keyword
        {
            get
            {
                return "on ready";
            }
        }
    }
}
