using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public interface IEditorTab
    {
        string Caption { get; }
        IEnumerable<IEditorControl> Controls { get; }
        bool IsTabVisible(IEditorData data);
        bool IsTabVisibleInSimpleMode { get; }
        bool GetBool(string tag);
    }
}
