using System;
using System.Collections.Generic;

/*
 * This script command is an alternative to request (Save, ""), and is added as part of deprecating
 * request. It is called requestsave to replace RequestSave (an earlier step to deprecation), and
 * to hopefully avoid name clashes in existing games.
 */

namespace QuestViva.Engine.Scripts
{
    public class RequestSaveScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "requestsave"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new RequestSaveScript(WorldModel);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
    }

    public class RequestSaveScript : ScriptBase
    {
        private WorldModel m_worldModel;

        public RequestSaveScript(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new RequestSaveScript(m_worldModel);
        }

        public override void Execute(Context c)
        {
            m_worldModel.PlayerUi.RequestSave(null);
        }

        public override string Save()
        {
            return "requestsave";
        }

        public override string Keyword
        {
            get
            {
                return "requestsave";
            }
        }

        public override object GetParameter(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override void SetParameterInternal(int index, object value)
        {
            throw new ArgumentOutOfRangeException();
        }
    }
}
