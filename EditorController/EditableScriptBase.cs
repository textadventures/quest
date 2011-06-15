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

        public abstract string DisplayString(int index, object newValue);
        public abstract string EditorName { get; set; }
        public abstract object GetParameter(int index);
        public abstract void SetParameter(int index, object value);
        public abstract ScriptType Type { get; }

        private IScript m_script;
        private EditorController m_controller;

        public EditableScriptBase(EditorController controller, IScript script, UndoLogger undoLogger)
        {
            Script = script;
            m_controller = controller;
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

        internal IFunctionCallScript FunctionCallScript
        {
            get { return m_script as IFunctionCallScript; }
        }

        protected void ScriptUpdated(object sender, ScriptUpdatedEventArgs e)
        {
            if (Updated != null)
            {
                if (e != null && e.IsParameterUpdate)
                {
                    Updated(this, new EditableScriptUpdatedEventArgs(e.Index, m_controller.WrapValue(e.NewValue)));
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

        protected void RaiseUpdateForNestedScriptChange(EditableScriptsUpdatedEventArgs e)
        {
            RaiseUpdated(new EditableScriptUpdatedEventArgs { IsNestedScriptUpdate = true });
        }

        protected EditorController Controller
        {
            get { return m_controller; }
        }

        public IEnumerable<string> GetVariablesInScope()
        {
            return Script.Parent.GetVariablesInScope();
        }
    }
}
