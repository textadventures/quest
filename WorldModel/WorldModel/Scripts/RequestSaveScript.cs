using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

/*
 * This script command is an alternative to request (Save, ""), and is added as part of deprecating
 * request. It is called requestsave to replace RequestSave (an earlier step to deprecation), and
 * to hopefully avoid name clashes in existing games.
 */

namespace TextAdventures.Quest.Scripts
{
    public class RequestSaveScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "requestsave"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new RequestSaveScript(scriptContext);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
        #endregion
    }

    public class RequestSaveScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;

        public RequestSaveScript(ScriptContext scriptContext)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new RequestSaveScript(m_scriptContext);
        }

        public override void Execute(Context c)
        {
            m_worldModel.PlayerUI.RequestSave(null);
        }

        public override string Save()
        {
            return SaveScript("requestsave");
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
                return "requestsave";
            }
        }
    }
}
