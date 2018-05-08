using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class SaveScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "save"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new SaveScript(scriptContext);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
        #endregion
    }

    public class SaveScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;

        public SaveScript(ScriptContext scriptContext)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new QuitScript(m_scriptContext);
        }

        public override void Execute(Context c)
        {
            m_worldModel.PlayerUI.RequestSave(null);
        }

        public override string Save()
        {
            return SaveScript("save");
        }

        public override object GetParameter(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override void SetParameterInternal(int index, object value)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override string Keyword
        {
            get
            {
                return "save";
            }
        }
    }
}
