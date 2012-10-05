using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class FinishScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "finish"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new FinishScript(WorldModel);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
    }

    public class FinishScript : ScriptBase
    {
        private WorldModel m_worldModel;

        public FinishScript(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new FinishScript(m_worldModel);
        }

        public override void Execute(Context c)
        {
            m_worldModel.FinishGame();
        }

        public override string Save()
        {
            return "finish";
        }

        public override string Keyword
        {
            get
            {
                return "finish";
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
