using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class ScriptUpdatedEventArgs : EventArgs
    {
        internal ScriptUpdatedEventArgs()
        {
        }

        internal ScriptUpdatedEventArgs(IScript added, IScript removed)
        {
            AddedScript = added;
            RemovedScript = removed;
        }

        internal ScriptUpdatedEventArgs(IScript inserted, int index)
        {
            InsertedScript = inserted;
            Index = index;
        }

        internal ScriptUpdatedEventArgs(int index, object newValue)
        {
            Index = index;
            NewValue = newValue;
            IsParameterUpdate = true;
        }

        internal ScriptUpdatedEventArgs(string id, object newValue)
        {
            Id = id;
            NewValue = newValue;
            IsNamedParameterUpdate = true;
        }

        public IScript RemovedScript { get; private set; }
        public IScript AddedScript { get; private set; }
        public IScript InsertedScript { get; private set; }
        public int Index { get; private set; }
        public object NewValue { get; private set; }
        public bool IsParameterUpdate { get; private set; }
        public bool IsNamedParameterUpdate { get; private set; }
        public string Id { get; private set; }
        public bool ScriptsReplaced { get; set; }
    }

    public interface IScriptParent
    {
        IEnumerable<string> GetVariablesInScope();
    }

    public interface IScript : IMutableField
    {
        void Execute(Context c);
        string Line { get; set; }
        string Save();
        void SetParameter(int index, object value);
        void SetParameterSilent(int index, object value);
        object GetParameter(int index);
        string Keyword { get; }
        event EventHandler<ScriptUpdatedEventArgs> ScriptUpdated;
        IScriptParent Parent { get; set; }
        IEnumerable<string> GetDefinedVariables();
    }

    public interface IFunctionCallScript : IScript
    {
        object GetFunctionCallParameter(int index);
        void SetFunctionCallParameter(int index, object value);
        IScript GetFunctionCallParameterScript();
        event EventHandler<ScriptUpdatedEventArgs> FunctionCallParametersUpdated;
    }

    public interface IScriptConstructor
    {
        string Keyword { get; }
        IScript Create(string script, ScriptContext scriptContext);
        IScriptFactory ScriptFactory { set; }
        WorldModel WorldModel { get; set; }
    }

    public abstract class ScriptBase : IScript, IMutableField
    {
        private string m_line;

        public override string ToString()
        {
            return "Script: " + Line;
        }

        protected string SaveScript(string keyword, params string[] args)
        {
            return keyword + " (" + String.Join(", ", args) + ")";
        }

        protected string SaveScript(string keyword, IScript script, params string[] args)
        {
            string result;
            if (args.Length == 0)
            {
                result = keyword;
            }
            else
            {
                result = SaveScript(keyword, args);
            }
            string scriptString = script != null ? script.Save() : string.Empty;
            return result + " {" + Environment.NewLine + scriptString + Environment.NewLine + "}";
        }

        protected void NotifyUpdate(int index, object newValue)
        {
            NotifyUpdate(new ScriptUpdatedEventArgs(index, newValue));
        }

        protected void NotifyUpdate(IScript added, IScript removed)
        {
            NotifyUpdate(new ScriptUpdatedEventArgs(added, removed));
        }

        protected void NotifyUpdate(IScript inserted, int index)
        {
            NotifyUpdate(new ScriptUpdatedEventArgs(inserted, index));
        }

        protected void NotifyUpdate(string id, object newValue)
        {
            NotifyUpdate(new ScriptUpdatedEventArgs(id, newValue));
        }

        protected void NotifyUpdate(ScriptUpdatedEventArgs args)
        {
            if (ScriptUpdated != null)
            {
                ScriptUpdated(this, args);
            }
        }

        public Element Owner { get; set; }

        protected abstract ScriptBase CloneScript();

        public abstract void Execute(Context c);

        public abstract string Save();

        public virtual string Line
        {
            get { return m_line; }
            set { m_line = value; }
        }

        public void SetParameter(int index, object value)
        {
            object oldValue = GetParameter(index);
            SetParameterSilent(index, value);
            if (UndoLog != null)
            {
                UndoLog.AddUndoAction(new UndoScriptChange(this, index, oldValue, value));
            }
        }

        public void SetParameterSilent(int index, object value)
        {
            SetParameterInternal(index, value);
            NotifyUpdate(index, value);
        }

        public abstract void SetParameterInternal(int index, object value);
        public abstract object GetParameter(int index);

        public abstract string Keyword { get; }

        public event EventHandler<ScriptUpdatedEventArgs> ScriptUpdated;

        private IScriptParent m_parent;
        public IScriptParent Parent
        {
            get { return m_parent; }
            set
            {
                bool changed = (m_parent != value);
                m_parent = value;
                if (changed) ParentUpdated();
            }
        }

        protected virtual void ParentUpdated() { }

        public virtual IEnumerable<string> GetDefinedVariables()
        {
            return null;
        }

        #region IMutableField Members

        public UndoLogger UndoLog { get; set; }
        public bool Locked
        {
            get { return false; }
            set { }
        }

        public virtual IMutableField Clone()
        {
            ScriptBase clone = CloneScript();
            clone.m_line = m_line;
            return clone;
        }

        // Scripts only require cloning in the editor, as they cannot be modified when the game is running

        public bool RequiresCloning { get { return false; } }

        #endregion

        private class UndoScriptChange : TextAdventures.Quest.UndoLogger.IUndoAction
        {
            private IScript m_appliesTo;
            private int m_index;
            private object m_oldValue;
            private object m_newValue;

            public UndoScriptChange(IScript appliesTo, int index, object oldValue, object newValue)
            {
                m_appliesTo = appliesTo;
                m_index = index;
                m_oldValue = oldValue;
                m_newValue = newValue;
            }

            #region IUndoAction Members

            public void DoUndo(WorldModel worldModel)
            {
                m_appliesTo.SetParameterSilent(m_index, m_oldValue);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_appliesTo.SetParameterSilent(m_index, m_newValue);
            }

            #endregion
        }
    }
}
