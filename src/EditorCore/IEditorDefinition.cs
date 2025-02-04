using System.Collections.Generic;

namespace QuestViva.EditorCore
{
    public interface IEditorDefinition
    {
        IDictionary<string, IEditorTab> Tabs { get; }
        IEnumerable<IEditorControl> Controls { get; }
        string GetDefaultFilterName(string filterGroupName, IEditorData data);
    }
}
