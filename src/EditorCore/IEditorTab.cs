namespace QuestViva.EditorCore;

public interface IEditorTab
{
    string Caption { get; }

    /// <summary>
    /// The tab's &lt;helpurl&gt;, if it has one: a site-relative path into the
    /// documentation (e.g. "/howto/world/exits/"). Null when the tab has no
    /// help target, in which case no help affordance is offered for it.
    /// (Unannotated because EditorCore doesn't enable nullable reference
    /// types, matching Caption and the rest of this interface.)
    /// </summary>
    string HelpUrl { get; }
    IEnumerable<IEditorControl> Controls { get; }
    bool IsTabVisibleInSimpleMode { get; }
    Task<bool> IsTabVisible(IEditorData data);
    bool GetBool(string tag);
}