using QuestViva.Engine;

namespace QuestViva.EditorCore;

internal class EditorTab : IEditorTab
{
    private readonly Dictionary<string, IEditorControl> _controls;
    private readonly Element _source;
    private readonly EditorVisibilityHelper _visibilityHelper;

    public EditorTab(EditorDefinition parent, WorldModel worldModel, Element source)
    {
        _controls = [];
        Caption = source.Fields.GetString("caption");
        // Editor definitions live in <library type="editor">, which GameSaver
        // excludes from both package and editor saves - so unlike Core.aslx
        // game libraries, a <helpurl> added here is never frozen into a .quest
        // file and applies to every game, however old.
        HelpUrl = string.IsNullOrWhiteSpace(source.Fields.GetString("helpurl"))
            ? null
            : source.Fields.GetString("helpurl");

        foreach (var e in worldModel.Elements.GetElements(ElementType.EditorControl))
        {
            if (e.Parent == source)
            {
                _controls.Add(e.Name, new EditorControl(parent, worldModel, e));
            }
        }

        _visibilityHelper = new EditorVisibilityHelper(parent, worldModel, source);
        _source = source;
    }

    public string? Caption { get; }

    public string? HelpUrl { get; }

    public IEnumerable<IEditorControl> Controls => _controls.Values;

    public Task<bool> IsTabVisible(IEditorData data)
    {
        return _visibilityHelper.IsVisible(data);
    }
}