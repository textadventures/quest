using QuestViva.Engine;

namespace QuestViva.EditorCore;

internal class EditorTab : IEditorTab
{
    private readonly Dictionary<string, IEditorControl> m_controls;
    private readonly Element m_source;
    private readonly EditorVisibilityHelper m_visibilityHelper;

    public EditorTab(EditorDefinition parent, WorldModel worldModel, Element source)
    {
        m_controls = new Dictionary<string, IEditorControl>();
        Caption = source.Fields.GetString("caption");
        IsTabVisibleInSimpleMode = !source.Fields.GetAsType<bool>("advanced");

        foreach (var e in worldModel.Elements.GetElements(ElementType.EditorControl))
        {
            if (e.Parent == source)
            {
                m_controls.Add(e.Name, new EditorControl(parent, worldModel, e));
            }
        }

        m_visibilityHelper = new EditorVisibilityHelper(parent, worldModel, source);
        m_source = source;
    }

    public string Caption { get; }

    public IEnumerable<IEditorControl> Controls => m_controls.Values;

    public bool IsTabVisible(IEditorData data)
    {
        return m_visibilityHelper.IsVisible(data);
    }

    public bool IsTabVisibleInSimpleMode { get; }

    public bool GetBool(string tag)
    {
        return m_source.Fields.GetAsType<bool>(tag);
    }
}