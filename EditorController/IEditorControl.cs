using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IEditorControl
    {
        string ControlType { get; }
        string Caption { get; }
        int? Height { get; }
        string Attribute { get; }
        bool Expand { get; }
        string GetString(string tag);
    }
}
