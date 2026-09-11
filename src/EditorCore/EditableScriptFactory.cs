using QuestViva.Engine;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

public class EditableScriptData
{
    private readonly Expression<bool>? _visibilityExpression;

    public EditableScriptData(Element editor, WorldModel worldModel, int order)
    {
        Order = order;
        DisplayString = editor.Fields.GetString("display");
        // Only created for editors that have a category - see EditableScriptFactory.IsScriptEditor
        Category = editor.Fields.GetString("category")!;
        CreateString = editor.Fields.GetString("create");
        AdderDisplayString = editor.Fields.GetString("add");
        IsAdvanced = editor.Fields.GetAsType<bool>("advanced");
        CommonButton = editor.Fields.GetString("common");
        var expression = editor.Fields.GetString("onlydisplayif");
        if (expression != null)
        {
            _visibilityExpression = new Expression<bool>(Engine.Utility.EncodeIdentifierSpaces(expression),
                new ScriptContext(worldModel, true));
        }
    }

    public string? DisplayString { get; }
    public string Category { get; }
    public string? CreateString { get; private set; }
    public string? AdderDisplayString { get; private set; }
    public bool IsAdvanced { get; }
    public string? CommonButton { get; private set; }
    public int Order { get; private set; }

    public async Task<bool> IsVisible()
    {
        return _visibilityExpression == null || await _visibilityExpression.ExecuteAsync(new Context());
    }
}

internal class EditableScriptFactory
{
    private readonly Dictionary<IScript, EditableScriptBase> _cache = [];
    private readonly EditorController _controller;
    private readonly ScriptFactory _scriptFactory;
    private readonly WorldModel _worldModel;

    internal EditableScriptFactory(EditorController controller, ScriptFactory factory, WorldModel worldModel)
    {
        _controller = controller;
        _scriptFactory = factory;
        _worldModel = worldModel;

        var order = 0;
        foreach (var editor in worldModel.Elements.GetElements(ElementType.Editor).Where(IsScriptEditor))
        {
            var appliesTo = editor.Fields.GetString("appliesto");
            // Script editors always name the script command they apply to
            ScriptData.Add(appliesTo!, new EditableScriptData(editor, worldModel, order++));
        }
    }

    internal Dictionary<string, EditableScriptData> ScriptData { get; } = [];

    private bool IsScriptEditor(Element editor)
    {
        return !string.IsNullOrEmpty(editor.Fields.GetString("category"));
    }

    internal EditableScriptBase CreateEditableScript(string keyword)
    {
        var script = _scriptFactory.CreateSimpleScript(keyword);
        return CreateEditableScript(script);
    }

    internal EditableScriptBase CreateEditableScript(IScript script)
    {
        if (_cache.TryGetValue(script, out var cachedScript))
        {
            return cachedScript;
        }

        EditableScriptBase newScript;

        if (script.Keyword == "if")
        {
            newScript = new EditableIfScript(_controller, (IIfScript) script, _worldModel.UndoLogger);
        }
        else
        {
            var newEditableScript = new EditableScript(_controller, script, _worldModel.UndoLogger);
            if (script.Keyword != null && ScriptData.TryGetValue(script.Keyword, out var scriptData))
            {
                newEditableScript.DisplayTemplate = scriptData.DisplayString;
            }

            newScript = newEditableScript;
        }

        _cache.Add(script, newScript);
        return newScript;
    }

    internal EditableScriptBase CreateEditableFunctionCallScript()
    {
        var script = _scriptFactory.CreateBlankFunctionCallScript();
        return CreateEditableScript(script);
    }

    internal IScript Clone(IScript script)
    {
        return _scriptFactory.CreateSimpleScript(script.Save());
    }
}