using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public interface IEditorDefinition
    {
        IDictionary<string, IEditorTab> Tabs { get; }
        IEnumerable<IEditorControl> Controls { get; }
        string GetDefaultFilterName(string filterGroupName, IEditorData data);
    }
}
