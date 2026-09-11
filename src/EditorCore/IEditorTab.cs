namespace QuestViva.EditorCore;

public interface IEditorTab
{
    string? Caption { get; }

    /// <summary>
    /// The tab's &lt;helpurl&gt;, if it has one: a site-relative path into the
    /// documentation (e.g. "/howto/world/exits/"). Null when the tab has no
    /// help target, in which case no help affordance is offered for it.
    /// </summary>
    string? HelpUrl { get; }
    IEnumerable<IEditorControl> Controls { get; }
    Task<bool> IsTabVisible(IEditorData data);
}