using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class UndoScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "undo"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new UndoScript(WorldModel);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
    }

    public class UndoScript : ScriptBase
    {
        private WorldModel m_worldModel;

        public UndoScript(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new UndoScript(m_worldModel);
        }

        public override void Execute(Context c)
        {
            m_worldModel.UndoLogger.RollbackTransaction();
        }

        public override string Save()
        {
            return "undo";
        }

        public override string Keyword
        {
            get
            {
                return "undo";
            }
        }

        public override object GetParameter(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override void SetParameterInternal(int index, object value)
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    public class StartTransactionConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "start transaction"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new StartTransactionScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class StartTransactionScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_command;

        public StartTransactionScript(ScriptContext scriptContext, IFunction<string> command)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_command = command;
        }

        protected override ScriptBase CloneScript()
        {
            return new StartTransactionScript(m_scriptContext, m_command.Clone());
        }

        public override void Execute(Context c)
        {
            m_worldModel.UndoLogger.RollTransaction(m_command.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("start transaction", m_command.Save());
        }

        public override string Keyword
        {
            get
            {
                return "start transaction";
            }
        }

        public override object GetParameter(int index)
        {
            return m_command.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_command = new Expression<string>((string)value, m_scriptContext);
        }
    }
}
