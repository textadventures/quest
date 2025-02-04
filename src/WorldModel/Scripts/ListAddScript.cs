using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class ListAddScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "list add"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new ListAddScript(scriptContext,
                new ExpressionGeneric(parameters[0], scriptContext),
                new Expression<object>(parameters[1], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class ListAddScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private IFunctionGeneric m_list;
        private IFunction<object> m_value;
        private WorldModel m_worldModel;

        public ListAddScript(ScriptContext scriptContext, IFunctionGeneric list, IFunction<object> value)
        {
            m_scriptContext = scriptContext;
            m_list = list;
            m_value = value;
            m_worldModel = scriptContext.WorldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new ListAddScript(m_scriptContext, m_list.Clone(), m_value.Clone());
        }

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

        public override string Keyword
        {
            get
            {
                return "list add";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_list.Save();
                case 1:
                    return m_value.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_list = new ExpressionGeneric((string)value, m_scriptContext);
                    break;
                case 1:
                    m_value = new Expression<object>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class ListRemoveScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "list remove"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new ListRemoveScript(scriptContext,
                new ExpressionGeneric(parameters[0], scriptContext),
                new Expression<object>(parameters[1], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class ListRemoveScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private IFunctionGeneric m_list;
        private IFunction<object> m_value;
        private WorldModel m_worldModel;

        public ListRemoveScript(ScriptContext scriptContext, IFunctionGeneric list, IFunction<object> value)
        {
            m_scriptContext = scriptContext;
            m_list = list;
            m_value = value;
            m_worldModel = scriptContext.WorldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new ListRemoveScript(m_scriptContext, m_list.Clone(), m_value.Clone());
        }

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

        public override string Keyword
        {
            get
            {
                return "list remove";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_list.Save();
                case 1:
                    return m_value.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_list = new ExpressionGeneric((string)value, m_scriptContext);
                    break;
                case 1:
                    m_value = new Expression<object>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
