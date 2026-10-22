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

    /// <summary>
    /// The feature checkbox attribute that shows this tab (e.g. "feature_container"), or null
    /// if the tab isn't gated by a feature. <see cref="IsGameFeature"/> says whether the checkbox
    /// is on the game rather than on the element itself.
    /// </summary>
    string? FeatureAttribute { get; }

    bool IsGameFeature { get; }

    /// <summary>
    /// True when the tab's feature is turned off but the element still has settings for it, which
    /// still take effect in the game.
    /// </summary>
    Task<bool> IsTabVisibleOnlyForLeftovers(IEditorData data);
}