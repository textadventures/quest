using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class QuitScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "quit"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new QuitScript(scriptContext);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
        #endregion
    }

    public class QuitScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;

        public QuitScript(ScriptContext scriptContext)
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
            m_worldModel.PlayerUI.Quit();
            m_worldModel.Finish();
        }

        public override string Save()
        {
            return SaveScript("quit");
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
                return "quit";
            }
        }
    }
}
