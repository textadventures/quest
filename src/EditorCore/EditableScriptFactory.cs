using QuestViva.Engine;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

public class EditableScriptData
{
    private readonly Expression<bool> m_visibilityExpression;

    public EditableScriptData(Element editor, WorldModel worldModel, int order)
    {
        Order = order;
        DisplayString = editor.Fields.GetString("display");
        Category = editor.Fields.GetString("category");
        CreateString = editor.Fields.GetString("create");
        AdderDisplayString = editor.Fields.GetString("add");
        IsVisibleInSimpleMode = !editor.Fields.GetAsType<bool>("advanced");
        CommonButton = editor.Fields.GetString("common");
        var expression = editor.Fields.GetString("onlydisplayif");
        if (expression != null)
        {
            m_visibilityExpression = new Expression<bool>(Engine.Utility.ConvertVariablesToFleeFormat(expression),
                new ScriptContext(worldModel, true));
        }
    }

    public string DisplayString { get; }
    public string Category { get; }
    public string CreateString { get; private set; }
    public string AdderDisplayString { get; private set; }
    public bool IsVisibleInSimpleMode { get; }
    public string CommonButton { get; private set; }
    public int Order { get; private set; }

    public bool IsVisible()
    {
        return m_visibilityExpression == null || m_visibilityExpression.Execute(new Context());
    }
}

internal class EditableScriptFactory
{
    private readonly Dictionary<IScript, EditableScriptBase> m_cache = new();
    private readonly EditorController m_controller;
    private readonly ScriptFactory m_scriptFactory;
    private readonly WorldModel m_worldModel;

    internal EditableScriptFactory(EditorController controller, ScriptFactory factory, WorldModel worldModel)
    {
        m_controller = controller;
        m_scriptFactory = factory;
        m_worldModel = worldModel;

        var order = 0;
        foreach (var editor in worldModel.Elements.GetElements(ElementType.Editor).Where(IsScriptEditor))
        {
            var appliesTo = editor.Fields.GetString("appliesto");
            ScriptData.Add(appliesTo, new EditableScriptData(editor, worldModel, order++));
        }
    }

    internal Dictionary<string, EditableScriptData> ScriptData { get; } = new();

    private bool IsScriptEditor(Element editor)
    {
        return !string.IsNullOrEmpty(editor.Fields.GetString("category"));
    }

    internal EditableScriptBase CreateEditableScript(string keyword)
    {
        var script = m_scriptFactory.CreateSimpleScript(keyword);
        return CreateEditableScript(script);
    }

    internal EditableScriptBase CreateEditableScript(IScript script)
    {
        EditableScriptBase newScript;

        if (m_cache.TryGetValue(script, out newScript))
        {
            return newScript;
        }

        if (script.Keyword == "if")
        {
            newScript = new EditableIfScript(m_controller, (IIfScript) script, m_worldModel.UndoLogger);
        }
        else
        {
            var newEditableScript = new EditableScript(m_controller, script, m_worldModel.UndoLogger);
            if (ScriptData.ContainsKey(script.Keyword))
            {
                newEditableScript.DisplayTemplate = ScriptData[script.Keyword].DisplayString;
            }

            newScript = newEditableScript;
        }

        m_cache.Add(script, newScript);
        return newScript;
    }

    internal EditableScriptBase CreateEditableFunctionCallScript()
    {
        var script = m_scriptFactory.CreateBlankFunctionCallScript();
        return CreateEditableScript(script);
    }

    internal IEnumerable<string> GetCategories(bool simpleModeOnly, bool showAll)
    {
        var result = new List<string>();
        IEnumerable<EditableScriptData> values = ScriptData.Values;
        if (!showAll)
        {
            values = values.Where(v => v.IsVisible());
        }

        if (simpleModeOnly)
        {
            values = values.Where(v => v.IsVisibleInSimpleMode);
        }

        foreach (var data in values)
        {
            if (!result.Contains(data.Category))
            {
                result.Add(data.Category);
            }
        }

        return result;
    }

    internal IScript Clone(IScript script)
    {
        return m_scriptFactory.CreateSimpleScript(script.Save());
    }
}