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

        public abstract string DisplayString();
        public abstract string DisplayString(int index, string newValue);
        public abstract string EditorName { get; }
        public abstract string GetParameter(int index);
        public abstract void SetParameter(int index, string value);
        public abstract ScriptType Type { get; }

        private IScript m_script;

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

        void ScriptUpdated(object sender, ScriptUpdatedEventArgs e)
        {
            if (Updated != null)
            {
                if (e.IsParameterUpdate)
                {
                    Updated(this, new EditableScriptUpdatedEventArgs(e.Index, e.NewValue));
                }
                else
                {
                    Updated(this, new EditableScriptUpdatedEventArgs());
                }
            }
        }
    }
}
