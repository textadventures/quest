using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class MsgScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "msg"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new MsgScript(scriptContext, new ExpressionGeneric(parameters[0], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
        #endregion
    }

    public class MsgScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunctionGeneric m_function;

        public MsgScript(ScriptContext scriptContext, IFunctionGeneric function)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_function = function;
        }

        protected override ScriptBase CloneScript()
        {
            return new MsgScript(m_scriptContext, m_function.Clone());
        }

        public override void Execute(Context c)
        {
            object result = m_function.Execute(c);
            m_worldModel.Print(result.ToString());
        }

        public override string Save()
        {
            return SaveScript("msg", m_function.Save());
        }

        public override object GetParameter(int index)
        {
            return m_function.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_function = new ExpressionGeneric((string)value, m_scriptContext);
        }

        public override string Keyword
        {
            get
            {
                return "msg";
            }
        }
    }
}
