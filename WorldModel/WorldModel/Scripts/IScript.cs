using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest.Scripts
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

        internal ScriptUpdatedEventArgs(int index, string newValue)
        {
            Index = index;
            NewValue = newValue;
            IsParameterUpdate = true;
        }

        public IScript RemovedScript { get; private set; }
        public IScript AddedScript { get; private set; }
        public IScript InsertedScript { get; private set; }
        public int Index { get; private set; }
        public string NewValue { get; private set; }
        public bool IsParameterUpdate { get; private set; }
    }

    public interface IScript
    {
        void Execute(Context c);
        string Line { get; set; }
        string Save();
        void SetParameter(int index, string value);
        void SetParameterSilent(int index, string value);
        string GetParameter(int index);
        string Keyword { get; }
        event EventHandler<ScriptUpdatedEventArgs> ScriptUpdated;
    }

    public interface IScriptConstructor
    {
        string Keyword { get; }
        IScript Create(string script, Element proc);
        IScriptFactory ScriptFactory { set; }
        WorldModel WorldModel { set; }      // maybe we need an IWorldModel at some point
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

        protected void NotifyUpdate(int index, string newValue)
        {
            if (ScriptUpdated != null)
            {
                ScriptUpdated(this, new ScriptUpdatedEventArgs(index, newValue));
            }
        }

        protected void NotifyUpdate(IScript added, IScript removed)
        {
            if (ScriptUpdated != null)
            {
                ScriptUpdated(this, new ScriptUpdatedEventArgs(added, removed));
            }
        }

        protected void NotifyUpdate(IScript inserted, int index)
        {
            if (ScriptUpdated != null)
            {
                ScriptUpdated(this, new ScriptUpdatedEventArgs(inserted, index));
            }
        }

        #region IScript Members

        public abstract void Execute(Context c);

        public abstract string Save();

        public virtual string Line
        {
            get { return m_line; }
            set { m_line = value; }
        }

        public void SetParameter(int index, string value)
        {
            string oldValue = GetParameter(index);
            SetParameterSilent(index, value);
            if (UndoLog != null)
            {
                UndoLog.AddUndoAction(new UndoScriptChange(this, index, oldValue, value));
            }
        }

        public void SetParameterSilent(int index, string value)
        {
            SetParameterInternal(index, value);
            NotifyUpdate(index, value);
        }

        public virtual void SetParameterInternal(int index, string value)
        {
            throw new NotImplementedException();
        }

        public virtual string GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public virtual string Keyword
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<ScriptUpdatedEventArgs> ScriptUpdated;

        #endregion

        #region IMutableField Members

        public UndoLogger UndoLog { get; set; }
        public bool Locked
        {
            get { return false; }
            set { }
        }

        // Scripts don't have to support cloning as we only support Undo behaviour in the editor,
        // so we can't have a situation where two objects have the same script anyway

        public virtual IMutableField Clone()
        {
            throw new NotImplementedException();
        }

        public bool RequiresCloning { get { return false; } }

        #endregion

        private class UndoScriptChange : AxeSoftware.Quest.UndoLogger.IUndoAction
        {
            private IScript m_appliesTo;
            private int m_index;
            private string m_oldValue;
            private string m_newValue;

            public UndoScriptChange(IScript appliesTo, int index, string oldValue, string newValue)
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
