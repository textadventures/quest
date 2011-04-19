using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IEditorData
    {
        // this will be used to populate the data on an editor, so it's e.g. all the object fields

        string Name { get; }
        object GetAttribute(string attribute);
        void SetAttribute(string attribute, object value);

        event EventHandler Changed;
    }
}
