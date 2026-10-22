using System.Text.RegularExpressions;
using QuestViva.Engine;

namespace QuestViva.EditorCore;

internal partial class EditorTab : IEditorTab
{
    private readonly Dictionary<string, IEditorControl> _controls;
    private readonly Element _source;
    private readonly EditorVisibilityHelper _visibilityHelper;
    private readonly WorldModel _worldModel;

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
        _worldModel = worldModel;

        // A feature tab is one shown only while a feature checkbox is ticked, either on the
        // element itself ("this.feature_container") or game-wide ("game.feature_asktell").
        // <editoronly/> marks a feature tab whose values don't affect the game (annotations), so
        // there's nothing to warn about when its feature is turned off.
        var match = FeatureGateRegex().Match(source.Fields.GetString("onlydisplayif") ?? "");
        if (match.Success && !source.Fields.GetAsType<bool>("editoronly"))
        {
            IsGameFeature = match.Groups[1].Value == "game";
            FeatureAttribute = match.Groups[2].Value;
        }
    }

    [GeneratedRegex(@"^\s*(this|game)\.(feature_\w+)\s*$")]
    private static partial Regex FeatureGateRegex();

    public string? FeatureAttribute { get; }

    public bool IsGameFeature { get; }

    public string? Caption { get; }

    public string? HelpUrl { get; }

    public IEnumerable<IEditorControl> Controls => _controls.Values;

    // A feature tab stays visible while the element still has settings for it, even with the
    // feature turned off, so the author can see what's still taking effect in the game.
    public async Task<bool> IsTabVisible(IEditorData data)
    {
        return await _visibilityHelper.IsVisible(data) || GetLeftovers(data) != null;
    }

    public async Task<bool> IsTabVisibleOnlyForLeftovers(IEditorData data)
    {
        return !await _visibilityHelper.IsVisible(data) && GetLeftovers(data) != null;
    }

    private FeatureLeftovers? GetLeftovers(IEditorData data)
    {
        return FeatureAttribute == null ? null : GetLeftovers(_worldModel.Elements.Get(data.Name!));
    }

    // The types (from the tab's type dropdowns) and attributes (from its other controls) that the
    // element has of its own. Inherited ones don't count: they belong to a type, not the element.
    internal FeatureLeftovers? GetLeftovers(Element element)
    {
        if (FeatureAttribute == null || element.MetaFields[MetaFieldDefinitions.Library])
        {
            return null;
        }

        var ownAttributes = element.Fields.GetAttributeNames(false).ToHashSet();
        var types = new List<string>();
        var attributes = new List<string>();
        var labels = new List<string>();

        foreach (var control in Controls)
        {
            if (control.ControlType == "dropdowntypes")
            {
                foreach (var (typeName, typeLabel) in control.GetDictionary("types") ?? new Dictionary<string, string>())
                {
                    if (typeName == "*" || !_worldModel.Elements.TryGetValue(ElementType.ObjectType, typeName, out var type))
                    {
                        continue;
                    }

                    if (element.Fields.InheritsType(type) && !types.Contains(typeName))
                    {
                        types.Add(typeName);
                        labels.Add($"{control.Caption}: {typeLabel}");
                    }
                }
            }
            else if (control.Attribute is { } attribute && attribute != FeatureAttribute &&
                     ownAttributes.Contains(attribute) && !attributes.Contains(attribute))
            {
                attributes.Add(attribute);
                labels.Add(control.Caption ?? attribute);
            }
        }

        return types.Count == 0 && attributes.Count == 0
            ? null
            : new FeatureLeftovers(element.Name, types, attributes, labels);
    }
}