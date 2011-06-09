using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class EditableScriptUpdatedEventArgs : EventArgs
    {
        internal EditableScriptUpdatedEventArgs()
        {
            Index = -1;
        }

        internal EditableScriptUpdatedEventArgs(int index, object newValue)
        {
            Index = index;
            NewValue = newValue;
            IsParameterUpdate = true;
        }

        internal EditableScriptUpdatedEventArgs(string id, object newValue)
        {
            Id = id;
            NewValue = newValue;
            IsNamedParameterUpdate = true;
        }

        internal EditableScriptUpdatedEventArgs(object newValue)
        {
            NewValue = newValue;
            IsWholeScriptUpdate = true;
        }

        public int Index { get; private set; }
        public string Id { get; private set; }
        public object NewValue { get; private set; }
        public bool IsParameterUpdate { get; private set; }
        public bool IsNamedParameterUpdate { get; private set; }
        public bool IsWholeScriptUpdate { get; private set; }
        public bool IsNestedScriptUpdate { get; set; }
    }

    public enum ScriptType
    {
        Normal,
        If
    }

    public interface IEditableScript
    {
        string DisplayString();
        string DisplayString(int index, object newValue);
        string EditorName { get; }
        object GetParameter(int index);
        void SetParameter(int index, object value);
        event EventHandler<EditableScriptUpdatedEventArgs> Updated;
        ScriptType Type { get; }
        IEnumerable<string> GetVariablesInScope();
    }
}
