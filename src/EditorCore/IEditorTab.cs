using System.Collections.Generic;

namespace QuestViva.EditorCore
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
