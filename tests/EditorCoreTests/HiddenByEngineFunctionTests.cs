using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// A game function that returns a value under the name of one of the engine's own functions is never
// reached: an expression calls the engine's instead (#2384). The function editor shows a warning
// label, <onlydisplayifhiddenbyengine/>, while that's the case.
[TestClass]
public class HiddenByEngineFunctionTests
{
    private EditorController _controller = null!;

    [TestInitialize]
    public async Task Init()
    {
        _controller = await LoadTemplateController("English");
        _controller.CreateNewFunction("MyFunction");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _controller.Uninitialise();
    }

    [TestMethod]
    public async Task WarningShownOnlyForANameTheEngineUsesWithAReturnType()
    {
        var warning = WarningControl();
        Assert.IsFalse(await warning.IsControlVisible(Data("MyFunction")));

        SetAttribute("MyFunction", "returntype", "string");
        Assert.IsFalse(await warning.IsControlVisible(Data("MyFunction")));

        // Matched whatever the case, as the expression evaluator matches it.
        SetAttribute("MyFunction", "name", "capfirst");
        Assert.IsTrue(await warning.IsControlVisible(Data("capfirst")));

        // With no return type it's called as a statement, which reaches it.
        SetAttribute("capfirst", "returntype", "");
        Assert.IsFalse(await warning.IsControlVisible(Data("capfirst")));
    }

    private void SetAttribute(string element, string attribute, object value)
    {
        var result = Data(element).SetAttribute(attribute, value);
        Assert.IsTrue(result.Valid, $"Setting {attribute} on {element} failed: {result.Message}");
    }

    private IEditorData Data(string element)
    {
        return _controller.GetEditorData(element)!;
    }

    private IEditorControl WarningControl()
    {
        var definition = _controller.GetEditorDefinition(_controller.GetElementEditorName("MyFunction")!);
        return definition.Tabs.Values
            .SelectMany(t => t.Controls)
            .Single(c => c.ControlType == "label" && c.Caption!.Contains("built-in function with this name"));
    }

    private static async Task<EditorController> LoadTemplateController(string templateName)
    {
        var templates = EditorController.GetAvailableTemplates();
        var template = templates.Values.Single(t => t.TemplateName == templateName);
        var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
        var bytes = Encoding.UTF8.GetBytes(initialFileText);

        var controller = new EditorController();
        controller.ClearTree += (_, _) => { };
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        controller.AddedNode += (_, _) => { };
        controller.RemovedNode += (_, _) => { };
        controller.RenamedNode += (_, _) => { };
        controller.RetitledNode += (_, _) => { };
        controller.ElementsUpdated += (_, _) => { };
        controller.Dirty += (_, _) => { };

        var ok = await controller.Initialise(new GameData(bytes, "test.aslx"));
        Assert.IsTrue(ok, $"Initialisation failed for template '{templateName}'");
        controller.UpdateTree();

        return controller;
    }
}
