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
        }

        internal EditableScriptUpdatedEventArgs(int index, string newValue)
        {
            Index = index;
            NewValue = newValue;
            IsParameterUpdate = true;
        }

        public int Index { get; private set; }
        public string NewValue { get; private set; }
        public bool IsParameterUpdate { get; private set; }
    }

    public enum ScriptType
    {
        Normal,
        If
    }

    public interface IEditableScript
    {
        string DisplayString();
        string DisplayString(int index, string newValue);
        string EditorName { get; }
        string GetParameter(int index);
        void SetParameter(int index, string value);
        event EventHandler<EditableScriptUpdatedEventArgs> Updated;
        ScriptType Type { get; }
    }
}
