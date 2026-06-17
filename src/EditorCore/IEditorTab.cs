namespace QuestViva.EditorCore;

public interface IEditorTab
{
    string Caption { get; }
    IEnumerable<IEditorControl> Controls { get; }
    bool IsTabVisibleInSimpleMode { get; }
    bool IsTabVisible(IEditorData data);
    bool GetBool(string tag);
}