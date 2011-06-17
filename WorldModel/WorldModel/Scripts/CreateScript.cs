using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class CreateScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "create"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new CreateScript(WorldModel, new Expression<string>(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
        #endregion
    }

    public class CreateScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_expr;

        public CreateScript(WorldModel worldModel, IFunction<string> expr)
        {
            m_worldModel = worldModel;
            m_expr = expr;
        }

        protected override ScriptBase CloneScript()
        {
            return new CreateScript(m_worldModel, m_expr.Clone());
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            m_worldModel.ObjectFactory.CreateObject(m_expr.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("create", m_expr.Save());
        }

        public override string Keyword
        {
            get
            {
                return "create";
            }
        }

        public override object GetParameter(int index)
        {
            return m_expr.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_expr = new Expression<string>((string)value, m_worldModel);
        }

        #endregion
    }

    public class CreateExitScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "create exit"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new CreateExitScript(WorldModel, new Expression<string>(parameters[0], WorldModel), new Expression<Element>(parameters[1], WorldModel), new Expression<Element>(parameters[2], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3 }; }
        }
        #endregion
    }

    public class CreateExitScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_name;
        private IFunction<Element> m_from;
        private IFunction<Element> m_to;

        public CreateExitScript(WorldModel worldModel, IFunction<string> name, IFunction<Element> from, IFunction<Element> to)
        {
            m_worldModel = worldModel;
            m_name = name;
            m_from = from;
            m_to = to;
        }

        protected override ScriptBase CloneScript()
        {
            return new CreateExitScript(m_worldModel, m_name.Clone(), m_from.Clone(), m_to.Clone());
        }

        public override void Execute(Context c)
        {
            m_worldModel.ObjectFactory.CreateExit(m_name.Execute(c), m_from.Execute(c), m_to.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("create exit", m_name.Save(), m_from.Save(), m_to.Save());
        }

        public override string Keyword
        {
            get
            {
                return "create exit";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_name.Save();
                case 1:
                    return m_from.Save();
                case 2:
                    return m_to.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_name = new Expression<string>((string)value, m_worldModel);
                    break;
                case 1:
                    m_from = new Expression<Element>((string)value, m_worldModel);
                    break;
                case 2:
                    m_to = new Expression<Element>((string)value, m_worldModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class CreateTimerScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create timer"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new CreateTimerScript(WorldModel, new Expression<string>(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class CreateTimerScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_expr;

        public CreateTimerScript(WorldModel worldModel, IFunction<string> expr)
        {
            m_worldModel = worldModel;
            m_expr = expr;
        }

        protected override ScriptBase CloneScript()
        {
            return new CreateTimerScript(m_worldModel, m_expr.Clone());
        }


        public override void Execute(Context c)
        {
            m_worldModel.GetElementFactory(ElementType.Timer).Create(m_expr.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("create timer", m_expr.Save());
        }

        public override string Keyword
        {
            get
            {
                return "create timer";
            }
        }

        public override object GetParameter(int index)
        {
            return m_expr.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_expr = new Expression<string>((string)value, m_worldModel);
        }
    }
}
