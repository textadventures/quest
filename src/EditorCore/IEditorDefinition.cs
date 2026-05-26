namespace QuestViva.EditorCore;

public interface IEditorDefinition
{
    IDictionary<string, IEditorTab> Tabs { get; }
    IEnumerable<IEditorControl> Controls { get; }
    string Description { get; }
    string OriginalPattern { get; }
    string GetDefaultFilterName(string filterGroupName, IEditorData data);
}