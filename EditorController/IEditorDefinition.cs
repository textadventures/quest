using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IEditorDefinition
    {
        IDictionary<string, IEditorTab> Tabs { get; }
        IEnumerable<IEditorControl> Controls { get; }
    }
}
