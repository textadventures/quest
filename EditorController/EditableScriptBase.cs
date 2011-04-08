using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public abstract class EditableScriptBase : IEditableScript
    {
        public event EventHandler<EditableScriptUpdatedEventArgs> Updated;

        public virtual string DisplayString()
        {
            return DisplayString(-1, string.Empty);
        }

        public abstract string DisplayString(int index, string newValue);
        public abstract string EditorName { get; }
        public abstract object GetParameter(int index);
        public abstract void SetParameter(int index, object value);
        public abstract ScriptType Type { get; }

        private IScript m_script;

        public EditableScriptBase(IScript script, UndoLogger undoLogger)
        {
            Script = script;
            if (script != null)
            {
                ((IMutableField)script).UndoLog = undoLogger;
            }
        }

        internal IScript Script
        {
            get
            {
                return m_script;
            }
            set
            {
                if (m_script != null)
                {
                    m_script.ScriptUpdated -= ScriptUpdated;
                }
                m_script = value;
                if (m_script != null)
                {
                    m_script.ScriptUpdated += ScriptUpdated;
                }
            }
        }

        protected void ScriptUpdated(object sender, ScriptUpdatedEventArgs e)
        {
            if (Updated != null)
            {
                if (e != null && e.IsParameterUpdate)
                {
                    // not sure if casting object NewValue to string will work, but we don't want the raw values bubbling up as EditableScriptUpdatedEventArgs.
                    // Maybe we should be wrapping them instead.
                    Updated(this, new EditableScriptUpdatedEventArgs(e.Index, (string)e.NewValue));
                }
                else if (e != null && e.IsNamedParameterUpdate)
                {
                    Updated(this, new EditableScriptUpdatedEventArgs(e.Id, (string)e.NewValue));
                }
                else
                {
                    Updated(this, new EditableScriptUpdatedEventArgs());
                }
            }
        }

        protected void RaiseUpdated(EditableScriptUpdatedEventArgs e)
        {
            if (Updated != null)
            {
                Updated(this, e);
            }
        }
    }
}
