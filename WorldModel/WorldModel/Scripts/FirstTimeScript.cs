using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    internal interface IFirstTimeScript
    {
        void SetOtherwiseScript(IScript script);
    }

    public class FirstTimeScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "firsttime"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            // Get script after "firsttime" keyword
            script = script.Substring(9).Trim();
            string firstTime = Utility.GetScript(script);
            IScript firstTimeScript = ScriptFactory.CreateScript(firstTime);

            return new FirstTimeScript(WorldModel, ScriptFactory, firstTimeScript);
        }

        public static void AddOtherwiseScript(IScript firstTimeScript, string script, IScriptFactory scriptFactory)
        {
            // Get script after "otherwise" keyword
            script = script.Substring(9).Trim();
            string otherwise = Utility.GetScript(script);
            IScript otherwiseScript = scriptFactory.CreateScript(otherwise);
            ((IFirstTimeScript)firstTimeScript).SetOtherwiseScript(otherwiseScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }
    }

    public class FirstTimeScript : ScriptBase, IFirstTimeScript
    {
        private IScript m_firstTimeScript;
        private IScript m_otherwiseScript;
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
            FirstTimeScript result = new FirstTimeScript(m_worldModel, m_scriptFactory, (IScript)m_firstTimeScript.Clone());
            if (m_otherwiseScript != null)
            {
                result.m_otherwiseScript = (IScript)m_otherwiseScript.Clone();
            }
            return result;
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
                m_worldModel.UndoLogger.AddUndoAction(new UndoFirstTime(this));
                m_firstTimeScript.Execute(c);
            }
            else
            {
                if (m_otherwiseScript != null)
                {
                    m_otherwiseScript.Execute(c);
                }
            }
        }

        private class UndoFirstTime : TextAdventures.Quest.UndoLogger.IUndoAction
        {
            private FirstTimeScript m_parent;
            
            public UndoFirstTime(FirstTimeScript parent)
            {
                m_parent = parent;
            }

            public void DoUndo(WorldModel worldModel)
            {
                m_parent.m_hasRun = false;
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_parent.m_hasRun = true;
            }
        }

        public override string Save()
        {
            if (m_worldModel.EditMode || !m_hasRun)
            {
                if (m_otherwiseScript == null)
                {
                    return SaveScript("firsttime", m_firstTimeScript);
                }
                else
                {
                    return SaveScript("firsttime", m_firstTimeScript) + Environment.NewLine + SaveScript("otherwise", m_otherwiseScript);
                }
            }
            else
            {
                if (m_otherwiseScript == null)
                {
                    return string.Empty;
                }
                else
                {
                    return m_otherwiseScript.Save();
                }
            }
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
                case 1:
                    return m_otherwiseScript;
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
                case 1:
                    m_otherwiseScript = (IScript)value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetOtherwiseScript(IScript script)
        {
            m_otherwiseScript = script;
        }
    }
}
