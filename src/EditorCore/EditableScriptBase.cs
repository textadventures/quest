using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
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
        public abstract object GetParameter(string index);
        public abstract void SetParameter(string index, object value);
        public abstract ScriptType Type { get; }

        private IScript m_script;
        private EditorController m_controller;
        private readonly string m_id;

        private static int s_count = 0;

        public EditableScriptBase(EditorController controller, IScript script, UndoLogger undoLogger)
        {
            Script = script;
            m_controller = controller;
            if (script != null)
            {
                ((IMutableField)script).UndoLog = undoLogger;
            }

            s_count++;
            m_id = "script" + s_count;
        }

        public string Id { get { return m_id; } }

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
                    if (FunctionCallScript != null)
                    {
                        FunctionCallScript.FunctionCallParametersUpdated -= FunctionCallParametersUpdated;
                    }
                }
                m_script = value;
                if (m_script != null)
                {
                    m_script.ScriptUpdated += ScriptUpdated;
                    if (FunctionCallScript != null)
                    {
                        FunctionCallScript.FunctionCallParametersUpdated += FunctionCallParametersUpdated;
                    }
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

        void FunctionCallParametersUpdated(object sender, ScriptUpdatedEventArgs e)
        {
            if (EditorName.StartsWith("(function)"))
            {
                Updated(this, new EditableScriptUpdatedEventArgs(e.Index, m_controller.WrapValue(e.NewValue)));
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
