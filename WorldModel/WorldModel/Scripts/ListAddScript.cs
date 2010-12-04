using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;
using System.Collections;

namespace AxeSoftware.Quest.Scripts
{
    public class ListAddScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public ListAddScriptConstructor()
        {
        }

        public override string Keyword
        {
            get { return "list add"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new ListAddScript(new ExpressionGeneric(parameters[0], WorldModel), new Expression<object>(parameters[1], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
        #endregion
    }

    public class ListAddScript : ScriptBase
    {
        private IFunctionGeneric m_list;
        private IFunction<object> m_value;

        public ListAddScript(IFunctionGeneric list, IFunction<object> value)
        {
            m_list = list;
            m_value = value;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            IQuestList result = m_list.Execute(c) as IQuestList;

            if (result != null)
            {
                result.Add(m_value.Execute(c));
            }
            else
            {
                throw new Exception("Unrecognised list type");
            }
        }

        public override string Save()
        {
            return SaveScript("list add", m_list.Save(), m_value.Save());
        }

        #endregion
    }

    public class ListRemoveScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public ListRemoveScriptConstructor()
        {
        }

        public override string Keyword
        {
            get { return "list remove"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new ListRemoveScript(new ExpressionGeneric(parameters[0], WorldModel), new Expression<object>(parameters[1], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
        #endregion
    }

    public class ListRemoveScript : ScriptBase
    {
        private IFunctionGeneric m_list;
        private IFunction<object> m_value;

        public ListRemoveScript(IFunctionGeneric list, IFunction<object> value)
        {
            m_list = list;
            m_value = value;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            IQuestList result = m_list.Execute(c) as IQuestList;

            if (result != null)
            {
                result.Remove(m_value.Execute(c));
            }
            else
            {
                throw new Exception("Unrecognised list type");
            }
        }

        public override string Save()
        {
            return SaveScript("list remove", m_list.Save(), m_value.Save());
        }

        #endregion
    }
}
