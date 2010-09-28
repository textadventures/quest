using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class FinishScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "finish"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new FinishScript(WorldModel);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
        #endregion
    }

    public class FinishScript : ScriptBase
    {
        private WorldModel m_worldModel;

        public FinishScript(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            m_worldModel.FinishGame();
        }

        public override string Save()
        {
            return "finish";
        }

        #endregion
    }
}
