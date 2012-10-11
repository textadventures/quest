using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class ApiScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "api"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new ApiScript(scriptContext, new ExpressionGeneric(parameters[0], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class ApiScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunctionGeneric m_expression;

        public ApiScript(ScriptContext scriptContext, IFunctionGeneric expression)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_expression = expression;
        }

        protected override ScriptBase CloneScript()
        {
            return new ApiScript(m_scriptContext, m_expression.Clone());
        }

        public override void Execute(Context c)
        {
            m_worldModel.API.Test();
        }

        public override string Save()
        {
            return SaveScript("api", m_expression.Save());
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_expression.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_expression = new ExpressionGeneric((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string Keyword
        {
            get
            {
                return "api";
            }
        }
    }
}
