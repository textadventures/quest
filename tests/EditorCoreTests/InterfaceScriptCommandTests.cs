using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// The script editor's Interface category (CoreEditorScriptsInterface.aslx): a command for each
// author hook function in CoreOutput, created from its own "create" text and shown with its own
// editor rather than the generic function call one.
[TestClass]
public class InterfaceScriptCommandTests
{
    private static readonly string[] Functions =
    [
        "SetPaneVisible", "AddPane", "SetPaneContent", "SetCommandPane", "SetCommandPlaceholder",
        "SetAutoScroll", "SetContinueOnClick", "SetFadePreviousOutput", "SetMapInteractive"
    ];

    private static async Task<EditorController> LoadController(string templateName)
    {
        var template = EditorController.GetAvailableTemplates().Values.Single(t => t.TemplateName == templateName);
        var bytes = Encoding.UTF8.GetBytes(EditorController.CreateNewGameFile(template.ResourceName, "Test"));

        var controller = new EditorController();
        controller.ClearTree += (_, _) => { };
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        controller.AddedNode += (_, _) => { };
        Assert.IsTrue(await controller.Initialise(new GameData(bytes, "test.aslx")), $"Initialisation failed for '{templateName}'");
        return controller;
    }

    [TestMethod]
    [DataRow("English", "Interface")]
    [DataRow("Deutsch", "Interface")]
    [DataRow("Español", "Interfaz")]
    public async Task EachHook_HasACommandInTheInterfaceCategory(string templateName, string category)
    {
        using var controller = await LoadController(templateName);
        var scripts = controller.GetScriptEditorData();

        foreach (var function in Functions)
        {
            Assert.IsTrue(scripts.TryGetValue($"(function){function}", out var data), $"no script command for {function}");
            Assert.AreEqual(category, data.Category, function);
            StringAssert.DoesNotMatch(data.AdderDisplayString!, new System.Text.RegularExpressions.Regex(@"\["),
                $"{function}'s caption isn't translated");
        }
    }

    [TestMethod]
    public async Task EachCommand_CreatesACallWithItsOwnEditor()
    {
        using var controller = await LoadController("English");
        var scripts = controller.GetScriptEditorData();

        foreach (var function in Functions)
        {
            var created = controller.CreateNewEditableScripts(null, null, scripts[$"(function){function}"].CreateString, false);

            Assert.AreEqual(1, created.Count, function);
            var script = created[0];
            Assert.AreEqual($"(function){function}", script.EditorName, function);
            var definition = controller.GetEditorDefinition(script);
            Assert.AreEqual($"(function){function}", script.EditorName, $"{function} fell back to the generic call editor");
            Assert.IsTrue(definition.Controls.Any(), function);
        }
    }

    [TestMethod]
    public async Task ShowHide_ReadsBackItsPartAndVisibility()
    {
        using var controller = await LoadController("English");
        var created = controller.CreateNewEditableScripts(null, null, "SetPaneVisible (\"compass\", false)", false);

        Assert.AreEqual("\"compass\"", created[0].GetParameter("0"));
        Assert.AreEqual("false", created[0].GetParameter("1"));
    }
}
