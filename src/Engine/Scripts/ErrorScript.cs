﻿#nullable disable
using System;
using System.Collections.Generic;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts
{
    public class ErrorScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "error"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new ErrorScript(scriptContext, new ExpressionDynamic(parameters[0], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
        #endregion
    }

    public class ErrorScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private IFunctionDynamic m_function;
        private WorldModel m_worldModel;

        public ErrorScript(ScriptContext scriptContext, IFunctionDynamic function)
        {
            m_scriptContext = scriptContext;
            m_function = function;
            m_worldModel = scriptContext.WorldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new ErrorScript(m_scriptContext, m_function.Clone());
        }

        public override void Execute(Context c)
        {
            object result = m_function.Execute(c);
            throw new Exception(result.ToString());
        }

        public override string Save()
        {
            return SaveScript("error", m_function.Save());
        }

        public override string Keyword
        {
            get
            {
                return "error";
            }
        }

        public override object GetParameter(int index)
        {
            return m_function.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_function = new ExpressionDynamic((string)value, m_scriptContext);
        }
    }
}
