using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// The open and close messages and the "after opening/closing" scripts work for any object that
// opens and closes, so the Container tab shows them for an Openable/Closable object as well as for
// a container, but not for a surface, which never opens (#2390).
[TestClass]
public class OpenableControlsTests
{
    private static readonly string[] Attributes = ["openmsg", "closemsg", "onopen", "onclose"];

    private EditorController _controller = null!;

    [TestInitialize]
    public async Task Init()
    {
        _controller = await LoadTemplateController("English");
        _controller.CreateNewObject("thing", null, "Thing");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _controller.Uninitialise();
    }

    [TestMethod]
    [DataRow("openable", true)]
    [DataRow("container_open", true)]
    [DataRow("container_closed", true)]
    [DataRow("surface", false)]
    [DataRow(null, false)]
    public async Task OpenAndCloseControls_ShowForObjectsThatOpen(string? type, bool expected)
    {
        if (type != null)
        {
            _controller.AddInheritedTypeToElement("thing", type, false);
        }

        var data = _controller.GetEditorData("thing")!;
        var definition = _controller.GetEditorDefinition(_controller.GetElementEditorName("thing")!);
        var controls = definition.Tabs.Values.SelectMany(t => t.Controls).ToList();

        foreach (var attribute in Attributes)
        {
            var control = controls.Single(c => c.Attribute == attribute);
            Assert.AreEqual(expected, await control.IsControlVisible(data), $"{attribute} on {type ?? "a plain object"}");
        }
    }

    private static async Task<EditorController> LoadTemplateController(string templateName)
    {
        var template = EditorController.GetAvailableTemplates().Values.Single(t => t.TemplateName == templateName);
        var bytes = Encoding.UTF8.GetBytes(EditorController.CreateNewGameFile(template.ResourceName, "Test"));

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

        Assert.IsTrue(await controller.Initialise(new GameData(bytes, "test.aslx")), $"Initialisation failed for template '{templateName}'");
        controller.UpdateTree();
        return controller;
    }
}
