using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TextAdventures.Quest.EditorControls
{
    public class DataModifiedEventArgs : EventArgs
    {
        public DataModifiedEventArgs(string attribute, object newValue)
        {
            Attribute = attribute;
            NewValue = newValue;
        }

        public string Attribute { get; set; }
        public object NewValue { get; private set; }
    }

    internal interface IElementEditorControl
    {
        IControlDataHelper Helper { get; }
        void Populate(IEditorData data);
        void Save();
        Control FocusableControl { get; }
    }

    internal interface IMultiAttributeElementEditorControl
         : IElementEditorControl
    {
        void AttributeChanged(string attribute, object value);
    }
}
