using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// <alsodisplayifset> keeps a tab or control visible while the element has its own value for the
// named attribute, even when its other visibility rules would hide it. Hiding a control doesn't
// stop its attribute taking effect in the game, so without this an author could be left with a
// live value they can't see, such as an openscript left over from the Openable/Closable container
// type that replaces a plain container's open behaviour (#2513). Tabs hidden by a feature checkbox
// are covered by FeatureLeftoversTests instead.
[TestClass]
public class AlsoDisplayIfSetTests
{
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
    public async Task OpenScriptShownAsLeftoverAfterSwitchingFromOpenable()
    {
        _controller.AddInheritedTypeToElement("thing", "openable", false);
        _controller.CreateNewEditableScripts("thing", "openscript", "msg", false);

        var (_, control) = FindTabAndControl("openscript");
        Assert.IsTrue(await control.IsControlVisible(Data()));
        Assert.IsFalse(await control.IsControlVisibleOnlyBecauseSet(Data()));

        _controller.RemoveInheritedTypeFromElement("thing", "openable", false);
        _controller.AddInheritedTypeToElement("thing", "container_open", false);

        Assert.IsTrue(await control.IsControlVisible(Data()));
        Assert.IsTrue(await control.IsControlVisibleOnlyBecauseSet(Data()));
    }

    [TestMethod]
    public async Task InheritedOpenScriptDoesNotCount()
    {
        // container_lockable defines its own openscript, which is the type's behaviour rather than
        // something the author left behind.
        _controller.AddInheritedTypeToElement("thing", "container_open", false);
        _controller.AddInheritedTypeToElement("thing", "container_lockable", false);

        var (_, control) = FindTabAndControl("openscript");
        Assert.IsFalse(await control.IsControlVisible(Data()));
    }

    private IEditorData Data()
    {
        return _controller.GetEditorData("thing")!;
    }

    private (IEditorTab Tab, IEditorControl Control) FindTabAndControl(string attribute)
    {
        var definition = _controller.GetEditorDefinition(_controller.GetElementEditorName("thing")!);
        foreach (var tab in definition.Tabs.Values)
        {
            var control = tab.Controls.FirstOrDefault(c => c.Attribute == attribute);
            if (control != null)
            {
                return (tab, control);
            }
        }

        throw new InvalidOperationException($"No control for '{attribute}'");
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
