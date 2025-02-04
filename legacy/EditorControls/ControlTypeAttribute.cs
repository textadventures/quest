using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.EditorControls
{
    [AttributeUsage(AttributeTargets.Class)]
    class ControlTypeAttribute : Attribute
    {
        public ControlTypeAttribute(string controlType)
        {
            ControlType = controlType;
        }

        public string ControlType { get; private set; }
    }
}
