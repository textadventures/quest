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
        public override string Keyword
        {
            get { return "list add"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new ListAddScript(WorldModel,
                new ExpressionGeneric(parameters[0], WorldModel),
                new Expression<object>(parameters[1], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class ListAddScript : ScriptBase
    {
        private IFunctionGeneric m_list;
        private IFunction<object> m_value;
        private WorldModel m_worldModel;

        public ListAddScript(WorldModel worldModel, IFunctionGeneric list, IFunction<object> value)
        {
            m_list = list;
            m_value = value;
            m_worldModel = worldModel;
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
                    m_list = new ExpressionGeneric((string)value, m_worldModel);
                    break;
                case 1:
                    m_value = new Expression<object>((string)value, m_worldModel);
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

        protected override IScript CreateInt(List<string> parameters)
        {
            return new ListRemoveScript(WorldModel,
                new ExpressionGeneric(parameters[0], WorldModel),
                new Expression<object>(parameters[1], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class ListRemoveScript : ScriptBase
    {
        private IFunctionGeneric m_list;
        private IFunction<object> m_value;
        private WorldModel m_worldModel;

        public ListRemoveScript(WorldModel worldModel, IFunctionGeneric list, IFunction<object> value)
        {
            m_list = list;
            m_value = value;
            m_worldModel = worldModel;
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
                    m_list = new ExpressionGeneric((string)value, m_worldModel);
                    break;
                case 1:
                    m_value = new Expression<object>((string)value, m_worldModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
