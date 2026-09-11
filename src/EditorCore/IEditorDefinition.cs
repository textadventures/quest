namespace QuestViva.EditorCore;

public interface IEditorDefinition
{
    /// <summary>
    /// The definition's &lt;appliesto&gt; value - the script command keyword
    /// ("msg", "(function)OutputTextNoBr") or element type ("object") this
    /// editor definition describes.
    /// </summary>
    string? AppliesTo { get; }

    IDictionary<string, IEditorTab> Tabs { get; }
    IEnumerable<IEditorControl> Controls { get; }
    string? Description { get; }
    string? OriginalPattern { get; }
    string GetDefaultFilterName(string filterGroupName, IEditorData data);
}