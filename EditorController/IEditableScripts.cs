using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class EditableScriptsUpdatedEventArgs : EventArgs
    {
        internal EditableScriptsUpdatedEventArgs()
        {
        }

        internal EditableScriptsUpdatedEventArgs(IEditableScript updatedScript, EditableScriptUpdatedEventArgs args)
        {
            UpdatedScript = updatedScript;
            UpdatedScriptEventArgs = args;
        }        

        public IEditableScript UpdatedScript { get; private set; }
        public EditableScriptUpdatedEventArgs UpdatedScriptEventArgs { get; private set; }
    }

    public interface IEditableScripts : IDataWrapper
    {
        IEnumerable<IEditableScript> Scripts { get; }
        void AddNew(string keyword, string elementName);
        IEditableScript this[int index] { get; }
        event EventHandler<EditableScriptsUpdatedEventArgs> Updated;
        void Remove(int[] indexes);
        string DisplayString(int index, string newValue);
        int Count { get; }
        void Swap(int index1, int index2);
        void Cut(int[] indexes);
        void Copy(int[] indexes);
        void Paste(int index, bool useTransaction);
        IEditableScripts Clone(string parent, string attribute);
        string Owner { get; }
        string Code { get; set; }
    }
}
