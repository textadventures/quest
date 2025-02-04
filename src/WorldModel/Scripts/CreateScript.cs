using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class CreateScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            switch (parameters.Count)
            {
                case 1:
                    return new CreateScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
                case 2:
                    return new CreateScript(scriptContext, new Expression<string>(parameters[0], scriptContext), new Expression<string>(parameters[1], scriptContext));
            }
            return null;
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1, 2 }; }
        }
    }

    public class CreateScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_expr;
        private IFunction<string> m_type;

        public CreateScript(ScriptContext scriptContext, IFunction<string> expr)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_expr = expr;
        }

        public CreateScript(ScriptContext scriptContext, IFunction<string> expr, IFunction<string> type)
            : this(scriptContext, expr)
        {
            m_type = type;
        }

        protected override ScriptBase CloneScript()
        {
            if (m_type == null)
            {
                return new CreateScript(m_scriptContext, m_expr.Clone());
            }
            else
            {
                return new CreateScript(m_scriptContext, m_expr.Clone(), m_type.Clone());
            }
        }

        public override void Execute(Context c)
        {
            if (m_type == null)
            {
                m_worldModel.ObjectFactory.CreateObject(m_expr.Execute(c));
            }
            else
            {
                m_worldModel.ObjectFactory.CreateObject(m_expr.Execute(c), ObjectType.Object, true, new List<string> { m_type.Execute(c) }, null);
            }
        }

        public override string Save()
        {
            if (m_type == null)
            {
                return SaveScript("create", m_expr.Save());
            }
            else
            {
                return SaveScript("create", m_expr.Save(), m_type.Save());
            }
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
            switch (index)
            {
                case 0:
                    return m_expr.Save();
                case 1:
                    return m_type == null ? null : m_type.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_expr = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 1:
                    m_type = (value == null) ? null : new Expression<string>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class CreateExitScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create exit"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            switch (parameters.Count)
            {
                case 3:
                    return new CreateExitScript(scriptContext, new Expression<string>(parameters[0], scriptContext), new Expression<Element>(parameters[1], scriptContext), new Expression<Element>(parameters[2], scriptContext));
                case 4:
                    return new CreateExitScript(scriptContext, new Expression<string>(parameters[0], scriptContext), new Expression<Element>(parameters[1], scriptContext), new Expression<Element>(parameters[2], scriptContext), new Expression<string>(parameters[3], scriptContext));
                case 5:
                    return new CreateExitScript(scriptContext, new Expression<string>(parameters[1], scriptContext), new Expression<Element>(parameters[2], scriptContext), new Expression<Element>(parameters[3], scriptContext), new Expression<string>(parameters[4], scriptContext), new Expression<string>(parameters[0], scriptContext));
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
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_id;
        private IFunction<string> m_name;
        private IFunction<Element> m_from;
        private IFunction<Element> m_to;
        private IFunction<string> m_initialType;

        public CreateExitScript(ScriptContext scriptContext, IFunction<string> name, IFunction<Element> from, IFunction<Element> to)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_name = name;
            m_from = from;
            m_to = to;
        }

        public CreateExitScript(ScriptContext scriptContext, IFunction<string> name, IFunction<Element> from, IFunction<Element> to, IFunction<string> initialType)
            : this(scriptContext, name, from, to)
        {
            m_initialType = initialType;
        }

        public CreateExitScript(ScriptContext scriptContext, IFunction<string> name, IFunction<Element> from, IFunction<Element> to, IFunction<string> initialType, IFunction<string> id)
            : this(scriptContext, name, from, to, initialType)
        {
            m_id = id;
        }

        protected override ScriptBase CloneScript()
        {
            if (m_initialType == null)
            {
                return new CreateExitScript(m_scriptContext, m_name.Clone(), m_from.Clone(), m_to.Clone());
            }
            if (m_id == null)
            {
                return new CreateExitScript(m_scriptContext, m_name.Clone(), m_from.Clone(), m_to.Clone(), m_initialType.Clone());
            }
            return new CreateExitScript(m_scriptContext, m_name.Clone(), m_from.Clone(), m_to.Clone(), m_initialType.Clone(), m_id.Clone());
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
                    return m_id == null ? null : m_id.Save();
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
                    m_id = (value == null) ? null : new Expression<string>((string)value, m_scriptContext);
                    break;
                case 1:
                    m_name = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 2:
                    m_from = new Expression<Element>((string)value, m_scriptContext);
                    break;
                case 3:
                    m_to = new Expression<Element>((string)value, m_scriptContext);
                    break;
                case 4:
                    m_initialType = (value == null) ? null : new Expression<string>((string)value, m_scriptContext);
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

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new CreateTimerScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class CreateTimerScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_expr;

        public CreateTimerScript(ScriptContext scriptContext, IFunction<string> expr)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_expr = expr;
        }

        protected override ScriptBase CloneScript()
        {
            return new CreateTimerScript(m_scriptContext, m_expr.Clone());
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
            m_expr = new Expression<string>((string)value, m_scriptContext);
        }
    }

    public class CreateTurnScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create turnscript"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new CreateTurnScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class CreateTurnScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_expr;

        public CreateTurnScript(ScriptContext scriptContext, IFunction<string> expr)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_expr = expr;
        }

        protected override ScriptBase CloneScript()
        {
            return new CreateTurnScript(m_scriptContext, m_expr.Clone());
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
            m_expr = new Expression<string>((string)value, m_scriptContext);
        }
    }
}
