using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class CreateScriptConstructor : ScriptConstructorBase
    {
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
    }

    public class CreateExitScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create exit"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            switch (parameters.Count)
            {
                case 3:
                    return new CreateExitScript(WorldModel, new Expression<string>(parameters[0], WorldModel), new Expression<Element>(parameters[1], WorldModel), new Expression<Element>(parameters[2], WorldModel));
                case 4:
                    return new CreateExitScript(WorldModel, new Expression<string>(parameters[0], WorldModel), new Expression<Element>(parameters[1], WorldModel), new Expression<Element>(parameters[2], WorldModel), new Expression<string>(parameters[3], WorldModel));
                case 5:
                    return new CreateExitScript(WorldModel, new Expression<string>(parameters[1], WorldModel), new Expression<Element>(parameters[2], WorldModel), new Expression<Element>(parameters[3], WorldModel), new Expression<string>(parameters[4], WorldModel), new Expression<string>(parameters[0], WorldModel));
            }
            return null;
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3, 4, 5 }; }
        }
    }

    public class CreateExitScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_id;
        private IFunction<string> m_name;
        private IFunction<Element> m_from;
        private IFunction<Element> m_to;
        private IFunction<string> m_initialType;

        public CreateExitScript(WorldModel worldModel, IFunction<string> name, IFunction<Element> from, IFunction<Element> to)
        {
            m_worldModel = worldModel;
            m_name = name;
            m_from = from;
            m_to = to;
        }

        public CreateExitScript(WorldModel worldModel, IFunction<string> name, IFunction<Element> from, IFunction<Element> to, IFunction<string> initialType)
            : this(worldModel, name, from, to)
        {
            m_initialType = initialType;
        }

        public CreateExitScript(WorldModel worldModel, IFunction<string> name, IFunction<Element> from, IFunction<Element> to, IFunction<string> initialType, IFunction<string> id)
            : this(worldModel, name, from, to, initialType)
        {
            m_id = id;
        }

        protected override ScriptBase CloneScript()
        {
            if (m_initialType == null)
            {
                return new CreateExitScript(m_worldModel, m_name.Clone(), m_from.Clone(), m_to.Clone());
            }
            if (m_id == null)
            {
                return new CreateExitScript(m_worldModel, m_name.Clone(), m_from.Clone(), m_to.Clone(), m_initialType.Clone());
            }
            return new CreateExitScript(m_worldModel, m_name.Clone(), m_from.Clone(), m_to.Clone(), m_initialType.Clone(), m_id.Clone());
        }

        public override void Execute(Context c)
        {
            m_worldModel.ObjectFactory.CreateExit(m_id == null ? null : m_id.Execute(c), m_name.Execute(c), m_from.Execute(c), m_to.Execute(c), m_initialType == null ? null : m_initialType.Execute(c));
        }

        public override string Save()
        {
            if (m_initialType == null)
            {
                return SaveScript("create exit", m_name.Save(), m_from.Save(), m_to.Save());
            }
            if (m_id == null)
            {
                return SaveScript("create exit", m_name.Save(), m_from.Save(), m_to.Save(), m_initialType.Save());
            }
            return SaveScript("create exit", m_id.Save(), m_name.Save(), m_from.Save(), m_to.Save(), m_initialType.Save());
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
                    return m_id.Save();
                case 1:
                    return m_name.Save();
                case 2:
                    return m_from.Save();
                case 3:
                    return m_to.Save();
                case 4:
                    return m_initialType == null ? null : m_initialType.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_id = new Expression<string>((string)value, m_worldModel);
                    break;
                case 1:
                    m_name = new Expression<string>((string)value, m_worldModel);
                    break;
                case 2:
                    m_from = new Expression<Element>((string)value, m_worldModel);
                    break;
                case 3:
                    m_to = new Expression<Element>((string)value, m_worldModel);
                    break;
                case 4:
                    m_initialType = new Expression<string>((string)value, m_worldModel);
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

    public class CreateTurnScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create turnscript"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new CreateTurnScript(WorldModel, new Expression<string>(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class CreateTurnScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_expr;

        public CreateTurnScript(WorldModel worldModel, IFunction<string> expr)
        {
            m_worldModel = worldModel;
            m_expr = expr;
        }

        protected override ScriptBase CloneScript()
        {
            return new CreateTurnScript(m_worldModel, m_expr.Clone());
        }

        public override void Execute(Context c)
        {
            m_worldModel.ObjectFactory.CreateTurnScript(m_expr.Execute(c), null);
        }

        public override string Save()
        {
            return SaveScript("create turnscript", m_expr.Save());
        }

        public override string Keyword
        {
            get
            {
                return "create turnscript";
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
