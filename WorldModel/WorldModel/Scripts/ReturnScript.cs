using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class ReturnScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "return"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new ReturnScript(scriptContext, new ExpressionGeneric(parameters[0], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class ReturnScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunctionGeneric m_returnValue;

        public ReturnScript(ScriptContext scriptContext, IFunctionGeneric returnValue)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_returnValue = returnValue;
        }

        protected override ScriptBase CloneScript()
        {
            return new ReturnScript(m_scriptContext, m_returnValue.Clone());
        }

        public override void Execute(Context c)
        {
            c.ReturnValue = m_returnValue.Execute(c);
            if (m_worldModel.Version >= WorldModelVersion.v550) c.IsReturned = true;
        }

        public override string Save()
        {
            return SaveScript("return", m_returnValue.Save());
        }

        public override string Keyword
        {
            get
            {
                return "return";
            }
        }

        public override object GetParameter(int index)
        {
            return m_returnValue.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_returnValue = new ExpressionGeneric((string)value, m_scriptContext);
        }
    }
}
