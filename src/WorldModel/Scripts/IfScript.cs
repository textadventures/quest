using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public interface IIfScript : IScript
    {
        void SetElse(IScript elseScript);
        IElseIfScript AddElseIf(string expression, IScript script);
        IElseIfScript AddElseIf(IFunction<bool> expression, IScript script);
        IFunction<bool> Expression { get; }
        IScript ThenScript { get; set; }
        event EventHandler<IfScriptUpdatedEventArgs> IfScriptUpdated;
        IScript ElseScript { get; }
        IList<IElseIfScript> ElseIfScripts { get; }
        string ExpressionString { get; set; }
        void RemoveElseIf(IElseIfScript elseIfScript);
    }

    public interface IElseIfScript
    {
        IScript Script { get; }
        string ExpressionString { get; set; }
        string Id { get; }
    }

    public class IfScriptConstructor : IScriptConstructor
    {
        #region IScriptConstructor Members

        public string Keyword
        {
            get { return "if"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            string afterExpr;
            string expr = Utility.GetParameter(script, out afterExpr);

            if (afterExpr.StartsWith(")"))
            {
                // We have a mismatch of brackets in the expression
                throw new Exception("Too many ')'");
            }

            string then = Utility.GetScript(afterExpr);

            IScript thenScript = ScriptFactory.CreateScript(then, scriptContext);

            return new IfScript(new Expression<bool>(expr, scriptContext), thenScript, scriptContext);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        #endregion

        public void AddElse(IScript script, string elseScript, ScriptContext scriptContext)
        {
            IScript add = GetElse(elseScript, scriptContext);
            ((IIfScript)script).SetElse(add);
        }

        public void AddElseIf(IScript script, string elseIfScript, ScriptContext scriptContext)
        {
            IScript add = GetElse(elseIfScript, scriptContext);
            if (add.Line == "") return;

            // GetElse uses the ScriptFactory to parse the "else if" block, so it will return
            // a MultiScript containing an IfScript with one expression and one "then" script block.

            IIfScript elseIf = (IIfScript)((IMultiScript)add).Scripts.First();

            ((IIfScript)script).AddElseIf(elseIf.Expression, elseIf.ThenScript);
        }

        private IScript GetElse(string elseScript, ScriptContext scriptContext)
        {
            elseScript = Utility.GetTextAfter(elseScript, "else");
            return ScriptFactory.CreateScript(elseScript, scriptContext);
        }
    }

    public class IfScript : ScriptBase, IIfScript
    {
        public class ElseIfScript : IElseIfScript
        {
            private IfScript m_parent;

            public ElseIfScript(IFunction<bool> expression, IScript script, IfScript parent, string id)
            {
                Expression = expression;
                Script = script;
                m_parent = parent;
                Id = id;
            }

            internal ElseIfScript Clone(IfScript newParent)
            {
                return new ElseIfScript(Expression.Clone(), (IScript)Script.Clone(), newParent, Id);
            }

            internal IFunction<bool> Expression { get; private set; }
            public IScript Script { get; private set; }
            public string Id { get; private set; }

            public string ExpressionString
            {
                get { return Expression.Save(); }
                set
                {
                    m_parent.UndoLog.AddUndoAction(new UndoChangeExpression(this, Expression.Save(), value));
                    SetExpressionSilent(value);
                }
            }

            internal void SetExpressionSilent(string newValue)
            {
                Expression = new Expression<bool>(newValue, m_parent.m_scriptContext);
                m_parent.NotifyUpdate(Id, newValue);
            }
        }

        private IFunction<bool> m_expression;
        private IScript m_thenScript;
        private IScript m_elseScript;
        private List<IElseIfScript> m_elseIfScript = new List<IElseIfScript>();
        private bool m_hasElse = false;
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;

        public event EventHandler<IfScriptUpdatedEventArgs> IfScriptUpdated;

        public IfScript(IFunction<bool> expression, IScript thenScript, ScriptContext scriptContext)
            : this(expression, thenScript, null, scriptContext)
        {
        }

        public IfScript(IFunction<bool> expression, IScript thenScript, IScript elseScript, ScriptContext scriptContext)
        {
            m_expression = expression;
            m_thenScript = thenScript;
            m_elseScript = elseScript;
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
        }

        protected override ScriptBase CloneScript()
        {
            IfScript clone = new IfScript(m_expression.Clone(), (IScript)m_thenScript.Clone(), m_elseScript == null ? null : (IScript)m_elseScript.Clone(), m_scriptContext);
            clone.m_hasElse = m_hasElse;
            foreach (ElseIfScript elseif in m_elseIfScript)
            {
                clone.m_elseIfScript.Add(elseif.Clone(clone));
            }
            clone.m_lastElseIfId = m_lastElseIfId;
            return clone;
        }

        public void SetElse(IScript elseScript)
        {
            if (base.UndoLog != null)
            {
                System.Diagnostics.Debug.Assert(elseScript == null || !m_hasElse, "UndoSetElse assumes that we only ever set the Else script once");

                base.UndoLog.StartTransaction("Add Else script");
                base.UndoLog.AddUndoAction(new UndoSetElse(this, m_elseScript, elseScript, m_hasElse, true));
            }

            m_hasElse = true;
            SetElseSilent(elseScript);

            if (base.UndoLog != null)
            {
                base.UndoLog.EndTransaction();
            }
        }

        private void SetElseSilent(IScript elseScript)
        {
            m_elseScript = elseScript;

            if (IfScriptUpdated != null)
            {
                IfScriptUpdatedEventArgs.IfScriptUpdatedEventType eventType =
                    (elseScript == null) ? IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElse
                    : IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElse;
                IfScriptUpdated(this, new IfScriptUpdatedEventArgs(eventType));
            }
        }

        private int m_lastElseIfId = 0;

        private string GetNewElseIfID()
        {
            m_lastElseIfId++;
            return "elseif" + m_lastElseIfId;
        }

        public IElseIfScript AddElseIf(string expression, IScript script)
        {
            IFunction<bool> expr = new Expression<bool>(expression, m_scriptContext);
            return AddElseIf(expr, script);
        }

        public IElseIfScript AddElseIf(IFunction<bool> expression, IScript script)
        {
            ElseIfScript elseIfScript = new ElseIfScript(expression, script, this, GetNewElseIfID());

            if (base.UndoLog != null)
            {
                base.UndoLog.StartTransaction("Add Else If script");
                base.UndoLog.AddUndoAction(new UndoAddElseIf(this, elseIfScript));
            }

            AddElseIfSilent(elseIfScript);

            if (base.UndoLog != null)
            {
                base.UndoLog.EndTransaction();
            }

            return elseIfScript;
        }

        private void AddElseIfSilent(IElseIfScript elseIfScript)
        {
            m_elseIfScript.Add(elseIfScript);

            if (IfScriptUpdated != null)
            {
                IfScriptUpdated(this, new IfScriptUpdatedEventArgs(IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.AddedElseIf, elseIfScript));
            }
        }

        public void RemoveElseIf(IElseIfScript elseIfScript)
        {
            if (base.UndoLog != null)
            {
                base.UndoLog.StartTransaction("Remove Else If script");
                base.UndoLog.AddUndoAction(new UndoRemoveElseIf(this, elseIfScript));
            }

            RemoveElseIfSilent(elseIfScript);

            if (base.UndoLog != null)
            {
                base.UndoLog.EndTransaction();
            }
        }

        private void RemoveElseIfSilent(IElseIfScript elseIfScript)
        {
            m_elseIfScript.Remove(elseIfScript);

            if (IfScriptUpdated != null)
            {
                IfScriptUpdated(this, new IfScriptUpdatedEventArgs(IfScriptUpdatedEventArgs.IfScriptUpdatedEventType.RemovedElseIf, elseIfScript));
            }
        }

        public IList<IElseIfScript> ElseIfScripts
        {
            get { return m_elseIfScript.AsReadOnly(); }
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            if (m_expression.Execute(c))
            {
                m_thenScript.Execute(c);
                return;
            }

            if (m_elseIfScript != null)
            {
                foreach (ElseIfScript elseIfScript in m_elseIfScript)
                {
                    if (elseIfScript.Expression.Execute(c))
                    {
                        elseIfScript.Script.Execute(c);
                        return;
                    }
                }
            }

            if (m_elseScript != null) m_elseScript.Execute(c);
        }

        public override string Save()
        {
            string result = SaveScript("if", m_thenScript, m_expression.Save());
            if (m_elseIfScript != null)
            {
                foreach (ElseIfScript elseIf in m_elseIfScript)
                {
                    result += Environment.NewLine + SaveScript("else if", elseIf.Script, elseIf.Expression.Save());
                }
            }
            if (m_elseScript != null) result += "else {" + Environment.NewLine + m_elseScript.Save() + Environment.NewLine + "}";
            return result;
        }

        public override string Keyword
        {
            get
            {
                return "if";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    // expression
                    return m_expression.Save();
                case 1:
                    // "then" script
                    return m_thenScript.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            throw new NotImplementedException();
        }

        #endregion

        public string ExpressionString
        {
            get { return m_expression.Save(); }
            set
            {
                base.UndoLog.AddUndoAction(new UndoChangeExpression(this, m_expression.Save(), value));
                SetExpressionSilent(value);
            }
        }

        public IFunction<bool> Expression
        {
            get { return m_expression; }
        }

        private void SetExpressionSilent(string newValue)
        {
            m_expression = new Expression<bool>(newValue, m_scriptContext);
            NotifyUpdate(0, newValue);
        }

        public IScript ThenScript
        {
            get { return m_thenScript; }
            set { m_thenScript = value; }
        }

        public IScript ElseScript
        {
            get { return m_elseScript; }
        }

        private class UndoChangeExpression : UndoLogger.IUndoAction
        {
            private IfScript m_script;
            private ElseIfScript m_elseIfScript;
            private string m_oldValue;
            private string m_newValue;

            private UndoChangeExpression(string oldValue, string newValue)
            {
                m_oldValue = oldValue;
                m_newValue = newValue;
            }

            public UndoChangeExpression(IfScript script, string oldValue, string newValue)
                : this(oldValue, newValue)
            {
                m_script = script;
            }

            public UndoChangeExpression(ElseIfScript elseIfscript, string oldValue, string newValue)
                : this(oldValue, newValue)
            {
                m_elseIfScript = elseIfscript;
            }

            public void DoUndo(WorldModel worldModel)
            {
                if (m_script != null) m_script.SetExpressionSilent(m_oldValue);
                if (m_elseIfScript != null) m_elseIfScript.SetExpressionSilent(m_oldValue);
            }

            public void DoRedo(WorldModel worldModel)
            {
                if (m_script != null) m_script.SetExpressionSilent(m_newValue);
                if (m_elseIfScript != null) m_elseIfScript.SetExpressionSilent(m_newValue);
            }
        }

        // We only need an UndoSetElse and not an UndoSetThen, because an "if" *always*
        // has a "then". So here we implictly assume that the old value was null, and that
        // m_hasElse was false.

        private class UndoSetElse : UndoLogger.IUndoAction
        {
            private IfScript m_script;
            private IScript m_oldValue;
            private IScript m_newValue;
            private bool m_oldHasElse;
            private bool m_newHasElse;

            public UndoSetElse(IfScript script, IScript oldValue, IScript newValue, bool oldHasElse, bool newHasElse)
            {
                m_script = script;
                m_oldValue = oldValue;
                m_newValue = newValue;
                m_oldHasElse = oldHasElse;
                m_newHasElse = newHasElse;
            }

            public void DoUndo(WorldModel worldModel)
            {
                m_script.m_hasElse = m_oldHasElse;
                m_script.SetElseSilent(m_oldValue);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_script.m_hasElse = m_newHasElse;
                m_script.SetElseSilent(m_newValue);
            }
        }

        private class UndoAddElseIf : UndoLogger.IUndoAction
        {
            private IfScript m_script;
            private IElseIfScript m_elseIf;

            public UndoAddElseIf(IfScript script, IElseIfScript elseIf)
            {
                m_script = script;
                m_elseIf = elseIf;
            }

            public void DoUndo(WorldModel worldModel)
            {
                m_script.RemoveElseIfSilent(m_elseIf);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_script.AddElseIfSilent(m_elseIf);
            }
        }

        private class UndoRemoveElseIf : UndoLogger.IUndoAction
        {
            private IfScript m_script;
            private IElseIfScript m_elseIf;

            public UndoRemoveElseIf(IfScript script, IElseIfScript elseIf)
            {
                m_script = script;
                m_elseIf = elseIf;
            }

            public void DoUndo(WorldModel worldModel)
            {
                m_script.AddElseIfSilent(m_elseIf);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_script.RemoveElseIfSilent(m_elseIf);
            }
        }
    }

    public class IfScriptUpdatedEventArgs : EventArgs
    {
        public enum IfScriptUpdatedEventType
        {
            AddedElse,
            RemovedElse,
            AddedElseIf,
            RemovedElseIf
        }

        internal IfScriptUpdatedEventArgs(IfScriptUpdatedEventType eventType)
        {
            EventType = eventType;
        }

        internal IfScriptUpdatedEventArgs(IfScriptUpdatedEventType eventType, IElseIfScript data)
            : this(eventType)
        {
            Data = data;
        }

        public IfScriptUpdatedEventType EventType { get; private set; }
        public IElseIfScript Data { get; private set; }
    }
}
