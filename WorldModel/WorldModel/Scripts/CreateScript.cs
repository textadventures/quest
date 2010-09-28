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
            return new CreateScript(WorldModel, new Expression<string>(parameters[0]));
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

        public override string GetParameter(int index)
        {
            return m_expr.Save();
        }

        public override void SetParameterInternal(int index, string value)
        {
            m_expr = new Expression<string>(value);
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
            return new CreateExitScript(WorldModel, new Expression<string>(parameters[0]), new Expression<Element>(parameters[1]), new Expression<Element>(parameters[2]));
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

        #region IScript Members

        public override void Execute(Context c)
        {
            m_worldModel.ObjectFactory.CreateExit(m_name.Execute(c), m_from.Execute(c), m_to.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("create exit", m_name.Save(), m_from.Save(), m_to.Save());
        }

        #endregion
    }
}
